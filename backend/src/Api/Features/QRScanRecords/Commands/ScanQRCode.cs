using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
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

            // TODO: Record scan event (with IP address, user agent, timestamp, etc.)

            var scanRecord = new ScanRecord
            {
                QRCodeId = qrCode.Id,
                BrowserInfo = request.Context.Request.Headers["User-Agent"].ToString().GetBrowserInfo(),
                OperatingSystem = request.Context.Request.Headers["User-Agent"].ToString().GetOperatingSystem(),
                DeviceType = request.Context.Request.Headers["User-Agent"].ToString().GetDeviceType(),
                Country = request.Context.Connection.RemoteIpAddress?.GetCountryName() ?? "Unknown"
            };

            var response = new Response(qrCode.RedirectUrl);

            return Result.Ok(response);
        }
    }

    public record Response(string RedirectUrl);
}
