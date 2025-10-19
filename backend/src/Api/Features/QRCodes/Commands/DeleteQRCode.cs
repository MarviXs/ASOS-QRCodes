using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.QRCodes.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.QRCodes.Commands;

public static class DeleteQRCode
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "qr-codes/{id:guid}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var command = new Command(user, id);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeleteQRCode))
                .WithTags(nameof(QRCode))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a QR code";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid QRCodeId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var qrCode = await context.QRCodes.FirstOrDefaultAsync(d => d.Id == message.QRCodeId, cancellationToken);
            if (qrCode == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!qrCode.IsOwner(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            context.QRCodes.Remove(qrCode);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
