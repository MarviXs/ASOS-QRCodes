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

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
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
            {
                deviceType = DeviceType.Desktop;
            }
            else if (dd.IsTablet())
            {
                deviceType = DeviceType.Tablet;
            }
            else if (dd.IsMobile())
            {
                deviceType = DeviceType.Mobile;
            }

            var scanRecord = new ScanRecord
            {
                QRCodeId = qrCode.Id,
                Country = request.Context.Connection.RemoteIpAddress?.GetCountryName() ?? "Unknown",
                OperatingSystem = osInfo.ToString(),
                BrowserInfo = browserName,
                DeviceType = deviceType,
            };
            context.ScanRecords.Add(scanRecord);
            await context.SaveChangesAsync(cancellationToken);

            var response = new Response(qrCode.RedirectUrl);

            return Result.Ok(response);
        }
    }

    public record Response(string RedirectUrl);
}
