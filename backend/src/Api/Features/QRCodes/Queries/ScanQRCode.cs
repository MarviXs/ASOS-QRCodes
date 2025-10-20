using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.QRCodes.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.QRCodes.Queries;

public static class ScanQRCode
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "scan/{shortCode}",
                    async Task<Results<RedirectHttpResult, NotFound, ForbidHttpResult>> (IMediator mediator, string shortCode) =>
                    {
                        var query = new Query(shortCode);
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

    public record Query(string ShortCode) : IRequest<Result<Response>>;

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

            var response = new Response(qrCode.RedirectUrl);

            return Result.Ok(response);
        }
    }

    public record Response(string RedirectUrl);
}
