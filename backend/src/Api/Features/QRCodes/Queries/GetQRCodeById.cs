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

public static class GetQRCodeById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "qr-codes/{id:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var query = new Query(id, user);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetQRCodeById))
                .WithTags(nameof(QRCode))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a QR code by id";
                    return o;
                });
        }
    }

    public record Query(Guid QRCodeId, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var qrCode = await context.QRCodes.AsNoTracking().FirstOrDefaultAsync(qrCode => qrCode.Id == request.QRCodeId, cancellationToken);

            if (qrCode == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!qrCode.IsOwner(request.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var response = new Response(
                qrCode.Id,
                qrCode.DisplayName,
                qrCode.RedirectUrl,
                qrCode.ShortCode,
                qrCode.DotStyle,
                qrCode.CornerDotStyle,
                qrCode.CornerSquareStyle,
                qrCode.Color
            );

            return Result.Ok(response);
        }
    }

    public record Response(
        Guid Id,
        string DisplayName,
        string RedirectUrl,
        string ShortCode,
        string DotStyle,
        string CornerDotStyle,
        string CornerSquareStyle,
        string Color
    );
}
