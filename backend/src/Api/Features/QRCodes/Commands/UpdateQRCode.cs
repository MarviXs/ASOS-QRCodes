using System;
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

public static class UpdateQRCode
{
    public record Request(
        string DisplayName,
        string RedirectUrl,
        string ShortCode,
        string DotStyle,
        string CornerDotStyle,
        string CornerSquareStyle,
        string Color
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "qr-codes/{id:guid}",
                    async Task<Results<NoContent, NotFound, ValidationProblem, Conflict, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid id,
                        Request request
                    ) =>
                    {
                        var command = new Command(request, id, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        else if (result.HasError<ConcurrencyError>())
                        {
                            return TypedResults.Conflict();
                        }
                        else if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(UpdateQRCode))
                .WithTags(nameof(QRCode))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a QR code";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid QRCodeId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var qrCode = await context.QRCodes.FirstOrDefaultAsync(d => d.Id == message.QRCodeId, cancellationToken);
            if (qrCode == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!qrCode.IsOwner(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var normalizedRedirectUrl = message.Request.RedirectUrl.EnsureHttpsScheme();

            qrCode.DisplayName = message.Request.DisplayName;
            qrCode.RedirectUrl = normalizedRedirectUrl;
            qrCode.ShortCode = message.Request.ShortCode;
            qrCode.DotStyle = message.Request.DotStyle;
            qrCode.CornerDotStyle = message.Request.CornerDotStyle;
            qrCode.CornerSquareStyle = message.Request.CornerSquareStyle;
            qrCode.Color = message.Request.Color;
            context.QRCodes.Update(qrCode);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }

    }

    public record Response(Guid Id, string Name, long? ResponseTime, long? LastResponseTimestamp);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.DisplayName).NotEmpty().WithMessage("Display name is required");
            RuleFor(r => r.Request.RedirectUrl).NotEmpty().WithMessage("Redirect URL is required");
            RuleFor(r => r.Request.ShortCode).NotEmpty().WithMessage("Short code is required");
            RuleFor(r => r.Request.DotStyle).NotEmpty().WithMessage("Dot style is required");
            RuleFor(r => r.Request.CornerDotStyle).NotEmpty().WithMessage("Corner dot style is required");
            RuleFor(r => r.Request.CornerSquareStyle).NotEmpty().WithMessage("Corner square style is required");
            RuleFor(r => r.Request.Color).NotEmpty().WithMessage("Color is required");
        }
    }
}
