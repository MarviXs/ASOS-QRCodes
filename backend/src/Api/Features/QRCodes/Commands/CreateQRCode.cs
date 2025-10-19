using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.QRCodes.Commands;

public static class CreateQRCode
{
    public record Request(string DisplayName, string RedirectUrl, string ShortCode, string DotStyle, string CornerDotStyle, string CornerSquareStyle, string Color);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "qr-codes",
                    async Task<Results<Created<Guid>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
                    {
                        var command = new Command(request, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateQRCode))
                .WithTags(nameof(QRCode))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a QR code";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var qrCode = new QRCode
            {
                OwnerId = message.User.GetUserId(),
                DisplayName = message.Request.DisplayName,
                RedirectUrl = message.Request.RedirectUrl,
                ShortCode = message.Request.ShortCode,
                DotStyle = message.Request.DotStyle,
                CornerDotStyle = message.Request.CornerDotStyle,
                CornerSquareStyle = message.Request.CornerSquareStyle,
                Color = message.Request.Color
            };
            await context.QRCodes.AddAsync(qrCode, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(qrCode.Id);
        }
    }

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
