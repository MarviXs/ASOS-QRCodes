using System.Security.Claims;
using Carter;
using DeviceDetectorNET;
using DeviceDetectorNET.Parser;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.QRCodes.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.QRScanRecords.Commands;

public static class ScanQRCode
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "scan/{shortCode}",
                    async Task<Results<RedirectHttpResult, NotFound, ForbidHttpResult>> (HttpContext context, IMediator mediator, string shortCode) =>
                    {
                        var query = new Query(shortCode, context);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Redirect(result.Value.RedirectUrl);
                    }
                )
                .WithName(nameof(ScanQRCode))
                .WithTags(nameof(QRCode))
                .AllowAnonymous()
                .WithOpenApi(o =>
                {
                    o.Summary = "Redirect to the URL";
                    return o;
                });
        }
    }

    public record Query(string ShortCode, HttpContext Context) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context, IServiceScopeFactory scopeFactory, ILogger<Handler> logger)
        : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var qrCode = await context.QRCodes.AsNoTracking().FirstOrDefaultAsync(qrCode => qrCode.ShortCode == request.ShortCode, cancellationToken);

            if (qrCode == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var headers = request.Context.Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToArray().FirstOrDefault());

            var clientHints = ClientHints.Factory(headers);
            var dd = new DeviceDetector(request.Context.Request.Headers["User-Agent"], clientHints);
            dd.SkipBotDetection();
            dd.Parse();

            var clientInfo = dd.GetClient();
            var osInfo = OperatingSystemParser.GetOsFamily(dd.GetOs().Match?.Name ?? "Unknown");
            var browserName = clientInfo.Match?.Name ?? "Unknown";

            var deviceType = DeviceType.Other;
            if (dd.IsDesktop())
                deviceType = DeviceType.Desktop;
            else if (dd.IsTablet())
                deviceType = DeviceType.Tablet;
            else if (dd.IsMobile())
                deviceType = DeviceType.Mobile;

            var qrCodeId = qrCode.Id;
            var ipAddress = request.Context.Connection.RemoteIpAddress;
            var operatingSystem = osInfo.ToString();
            var browserInfo = browserName;
            var capturedDeviceType = deviceType;

            _ = Task.Run(async () =>
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var country = ipAddress?.GetCountryName() ?? "Unknown";

                    db.ScanRecords.Add(
                        new ScanRecord
                        {
                            QRCodeId = qrCodeId,
                            Country = country,
                            OperatingSystem = operatingSystem,
                            BrowserInfo = browserInfo,
                            DeviceType = capturedDeviceType,
                        }
                    );

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Swallow to never affect redirect, but log so you can diagnose failures
                    logger.LogWarning(ex, "Failed to persist scan record for QRCodeId {QRCodeId}", qrCodeId);
                }
            });

            return Result.Ok(new Response(qrCode.RedirectUrl));
        }
    }

    public record Response(string RedirectUrl);
}
