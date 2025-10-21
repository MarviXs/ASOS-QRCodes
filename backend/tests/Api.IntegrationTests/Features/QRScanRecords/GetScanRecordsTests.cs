using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.QRScanRecords.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.QRScanRecords;

[Collection("IntegrationTests")]
public class GetScanRecordsTests : BaseIntegrationTest
{
    private readonly IntegrationTestWebAppFactory _factory;

    public GetScanRecordsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetScanRecords_ShouldReturnOwnedRecords()
    {
        // Arrange
        var ownedQrCode = new QRCodeFake(_factory.DefaultUserId).Generate();
        var otherQrCode = new QRCodeFake(_factory.OtherUserId).Generate();

        await AppDbContext.QRCodes.AddRangeAsync(ownedQrCode, otherQrCode);

        var ownedScan = new ScanRecord
        {
            QRCodeId = ownedQrCode.Id,
            BrowserInfo = "Chrome",
            OperatingSystem = "Windows",
            DeviceType = DeviceType.Desktop,
            Country = "United States",
            CreatedAt = DateTime.UtcNow
        };

        var otherScan = new ScanRecord
        {
            QRCodeId = otherQrCode.Id,
            BrowserInfo = "Safari",
            OperatingSystem = "macOS",
            DeviceType = DeviceType.Desktop,
            Country = "United Kingdom",
            CreatedAt = DateTime.UtcNow
        };

        await AppDbContext.ScanRecords.AddRangeAsync(ownedScan, otherScan);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("scan-records");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var records = await response.Content.ReadFromJsonAsync<PagedList<GetScanRecords.Response>>();
        records.Should().NotBeNull();
        records!.Items.Should().ContainSingle();

        var record = records.Items[0];
        record.QRCodeId.Should().Be(ownedQrCode.Id);
        record.DeviceType.Should().Be(DeviceType.Desktop);
        record.QRCodeName.Should().Be(ownedQrCode.DisplayName);
    }

    [Fact]
    public async Task GetScanRecords_ShouldReturnNotFound_WhenFilteringByMissingQRCode()
    {
        // Act
        var response = await Client.GetAsync($"scan-records?QRCodeId={Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetScanRecords_ShouldReturnForbid_WhenFilteringByQRCodeNotOwned()
    {
        // Arrange
        var qrCode = new QRCodeFake(_factory.OtherUserId).Generate();
        await AppDbContext.QRCodes.AddAsync(qrCode);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"scan-records?QRCodeId={qrCode.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetScanRecords_ShouldRespectDateFilters()
    {
        // Arrange
        var qrCode = new QRCodeFake(_factory.DefaultUserId).Generate();
        await AppDbContext.QRCodes.AddAsync(qrCode);

        var now = DateTime.UtcNow;
        var inRange = new ScanRecord
        {
            QRCodeId = qrCode.Id,
            BrowserInfo = "Chrome",
            OperatingSystem = "Windows",
            DeviceType = DeviceType.Desktop,
            Country = "United States",
            CreatedAt = now
        };
        var outOfRange = new ScanRecord
        {
            QRCodeId = qrCode.Id,
            BrowserInfo = "Firefox",
            OperatingSystem = "Windows",
            DeviceType = DeviceType.Desktop,
            Country = "United States",
            CreatedAt = now.AddDays(-10)
        };

        await AppDbContext.ScanRecords.AddRangeAsync(inRange, outOfRange);
        await AppDbContext.SaveChangesAsync();

        var start = now.AddDays(-1).ToString("O");
        var end = now.AddDays(1).ToString("O");

        // Act
        var response = await Client.GetAsync($"scan-records?StartDate={start}&EndDate={end}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var records = await response.Content.ReadFromJsonAsync<PagedList<GetScanRecords.Response>>();
        records.Should().NotBeNull();
        records!.Items.Should().ContainSingle();
        records.Items[0].CreatedAt.Date.Should().Be(now.Date);
    }
}
