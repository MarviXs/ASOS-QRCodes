using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.QRScanRecords.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.QRScanRecords;

[Collection("IntegrationTests")]
public class GetScanAnalyticsTests : BaseIntegrationTest
{
    private readonly IntegrationTestWebAppFactory _factory;

    public GetScanAnalyticsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetScanAnalytics_ShouldReturnAggregatedStatistics()
    {
        // Arrange
        var qrCode = new QRCodeFake(_factory.DefaultUserId).Generate();
        await AppDbContext.QRCodes.AddAsync(qrCode);

        var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextDate = startDate.AddDays(1);
        var outsideDate = startDate.AddDays(10);

        var scanRecords = new[]
        {
            new ScanRecord
            {
                QRCodeId = qrCode.Id,
                BrowserInfo = "Chrome",
                OperatingSystem = "Windows",
                DeviceType = DeviceType.Desktop,
                Country = "United States",
                CreatedAt = startDate
            },
            new ScanRecord
            {
                QRCodeId = qrCode.Id,
                BrowserInfo = "Chrome",
                OperatingSystem = "Windows",
                DeviceType = DeviceType.Desktop,
                Country = "United States",
                CreatedAt = nextDate
            },
            new ScanRecord
            {
                QRCodeId = qrCode.Id,
                BrowserInfo = "Firefox",
                OperatingSystem = "Linux",
                DeviceType = DeviceType.Mobile,
                Country = "Canada",
                CreatedAt = outsideDate
            }
        };

        await AppDbContext.ScanRecords.AddRangeAsync(scanRecords);
        await AppDbContext.SaveChangesAsync();

        var url = $"scan-records/analytics?QRCodeId={qrCode.Id}&StartDate={startDate:O}&EndDate={nextDate:O}";

        // Act
        var response = await Client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var analytics = await response.Content.ReadFromJsonAsync<GetScanAnalytics.Response>();
        analytics.Should().NotBeNull();
        analytics!.TotalScansInPeriod.Should().Be(2);
        analytics.LifetimeScans.Should().Be(3);
        analytics.StartDate.Date.Should().Be(startDate.Date);
        analytics.EndDate.Date.Should().Be(nextDate.Date);

        analytics.DailyScans.Should().HaveCount(2);
        analytics.DailyScans.Should().ContainEquivalentOf(new GetScanAnalytics.DailyScan(startDate.Date, 1));
        analytics.DailyScans.Should().ContainEquivalentOf(new GetScanAnalytics.DailyScan(nextDate.Date, 1));

        analytics.Countries.Should().ContainSingle(c => c.Name == "United States" && c.Count == 2);
        analytics.DeviceTypes.Should().ContainSingle(d => d.Name == DeviceType.Desktop.ToString() && d.Count == 2);
    }
}
