using System.Net;
using Carter;
using DeviceDetectorNET;
using DeviceDetectorNET.Parser;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
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
                            return TypedResults.NotFound();

                        if (result.HasError<ForbiddenError>())
                            return TypedResults.Forbid();

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
            var qrCode = await context.QRCodes.AsNoTracking().FirstOrDefaultAsync(q => q.ShortCode == request.ShortCode, cancellationToken);

            if (qrCode is null)
                return Result.Fail(new NotFoundError());

            var qrCodeId = qrCode.Id;
            var ipAddress = request.Context.GetClientIpAddress();

            var headersSnapshot = request.Context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            _ = Task.Run(async () =>
            {
                try
                {
                    var clientHints = ClientHints.Factory(headersSnapshot);
                    headersSnapshot.TryGetValue("User-Agent", out var userAgent);
                    userAgent ??= string.Empty;

                    var dd = new DeviceDetector(userAgent, clientHints);
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

                    var country = "Unknown";
                    try
                    {
                        country = ipAddress?.GetCountryName() ?? "Unknown";
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to get country from IP Address {IpAddress}", ipAddress);
                    }

                    await using var scope = scopeFactory.CreateAsyncScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    db.ScanRecords.Add(
                        new ScanRecord
                        {
                            QRCodeId = qrCodeId,
                            Country = country,
                            OperatingSystem = osInfo.ToString(),
                            BrowserInfo = browserName,
                            DeviceType = deviceType,
                        }
                    );

                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to persist scan record for QRCodeId {QRCodeId}", qrCodeId);
                }
            });

            return Result.Ok(new Response(qrCode.RedirectUrl));
        }
    }

    public static IPAddress? GetClientIpAddress(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ipString = forwardedFor
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();

            if (IPAddress.TryParse(ipString, out var ip))
            {
                return ip;
            }
        }
        return context.Connection.RemoteIpAddress;
    }

    public record Response(string RedirectUrl);
}
