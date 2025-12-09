using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.QRScanRecords.Queries;

namespace Fei.Is.Api.UnitTests.Features.QRScanRecords;

public class GetScanAnalyticsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsValidationError_WhenQrCodeIdIsEmpty()
    {
        using var context = TestUtils.CreateInMemoryDbContext();
        var handler = new GetScanAnalytics.Handler(context);
        var user = TestUtils.CreateUserPrincipal(Guid.NewGuid());

        var query = new GetScanAnalytics.Query(
            user,
            new GetScanAnalytics.QueryParameters { QRCodeId = Guid.Empty }
        );

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.HasError<ValidationError>());
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenQrCodeIsOwnedByAnotherUser()
    {
        using var context = TestUtils.CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var qrCode = new QRCode
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            DisplayName = "Other User Code",
            RedirectUrl = "https://other-user.com",
            ShortCode = "other",
            DotStyle = "square",
            CornerDotStyle = "square",
            CornerSquareStyle = "square",
            Color = "#123456"
        };
        context.QRCodes.Add(qrCode);
        await context.SaveChangesAsync();

        var handler = new GetScanAnalytics.Handler(context);
        var query = new GetScanAnalytics.Query(
            TestUtils.CreateUserPrincipal(requesterId),
            new GetScanAnalytics.QueryParameters { QRCodeId = qrCode.Id }
        );

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task Handle_ComputesAnalyticsForOwnedQrCodes()
    {
        using var context = TestUtils.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var qrCodeOne = new QRCode
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            DisplayName = "Landing Page",
            RedirectUrl = "https://example.com",
            ShortCode = "ex1",
            DotStyle = "dots",
            CornerDotStyle = "dots",
            CornerSquareStyle = "square",
            Color = "#000000"
        };
        var qrCodeTwo = new QRCode
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            DisplayName = "Product Page",
            RedirectUrl = "https://example.com/product",
            ShortCode = "ex2",
            DotStyle = "dots",
            CornerDotStyle = "dots",
            CornerSquareStyle = "square",
            Color = "#ffffff"
        };
        var foreignQrCode = new QRCode
        {
            Id = Guid.NewGuid(),
            OwnerId = otherUserId,
            DisplayName = "Foreign",
            RedirectUrl = "https://foreign.com",
            ShortCode = "for",
            DotStyle = "dots",
            CornerDotStyle = "dots",
            CornerSquareStyle = "square",
            Color = "#aaaaaa"
        };

        context.QRCodes.AddRange(qrCodeOne, qrCodeTwo, foreignQrCode);

        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 3);

        var scans = new[]
        {
            new ScanRecord
            {
                QRCodeId = qrCodeOne.Id,
                CreatedAt = startDate,
                Country = "USA",
                OperatingSystem = "Windows",
                BrowserInfo = "Edge",
                DeviceType = DeviceType.Desktop
            },
            new ScanRecord
            {
                QRCodeId = qrCodeOne.Id,
                CreatedAt = startDate.AddDays(1),
                Country = string.Empty,
                OperatingSystem = string.Empty,
                BrowserInfo = string.Empty,
                DeviceType = DeviceType.Mobile
            },
            new ScanRecord
            {
                QRCodeId = qrCodeTwo.Id,
                CreatedAt = startDate.AddDays(2),
                Country = "USA",
                OperatingSystem = "Android",
                BrowserInfo = "Chrome",
                DeviceType = DeviceType.Mobile
            },
            new ScanRecord
            {
                QRCodeId = qrCodeOne.Id,
                CreatedAt = startDate.AddDays(31),
                Country = "Canada",
                OperatingSystem = "iOS",
                BrowserInfo = "Safari",
                DeviceType = DeviceType.Tablet
            },
            new ScanRecord
            {
                QRCodeId = foreignQrCode.Id,
                CreatedAt = startDate,
                Country = "France",
                OperatingSystem = "Linux",
                BrowserInfo = "Firefox",
                DeviceType = DeviceType.Desktop
            }
        };

        context.ScanRecords.AddRange(scans);
        await context.SaveChangesAsync();

        var handler = new GetScanAnalytics.Handler(context);
        var query = new GetScanAnalytics.Query(
            TestUtils.CreateUserPrincipal(userId),
            new GetScanAnalytics.QueryParameters { StartDate = startDate, EndDate = endDate }
        );

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var response = result.Value;

        Assert.Equal(3, response.DailyScans.Count);
        Assert.Contains(response.DailyScans, daily => daily.Date == startDate && daily.Count == 1);
        Assert.Contains(response.DailyScans, daily => daily.Date == startDate.AddDays(1) && daily.Count == 1);
        Assert.Contains(response.DailyScans, daily => daily.Date == startDate.AddDays(2) && daily.Count == 1);

        Assert.Equal(3, response.TotalScansInPeriod);
        Assert.Equal(4, response.LifetimeScans);

        Assert.Contains(response.Countries, country => country.Name == "USA" && country.Count == 2);
        Assert.Contains(response.Countries, country => country.Name == "Unknown" && country.Count == 1);
        Assert.DoesNotContain(response.Countries, country => country.Name == "France");

        Assert.Contains(response.OperatingSystems, os => os.Name == "Unknown" && os.Count == 1);
        Assert.Contains(response.Browsers, browser => browser.Name == "Unknown" && browser.Count == 1);

        Assert.Contains(response.DeviceTypes, device => device.Name == DeviceType.Mobile.ToString() && device.Count == 2);
        Assert.Contains(response.DeviceTypes, device => device.Name == DeviceType.Desktop.ToString() && device.Count == 1);
    }
}
