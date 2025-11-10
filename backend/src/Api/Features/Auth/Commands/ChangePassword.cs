using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Features.Auth.Commands;

public static class ChangePassword
{
    public record Request(string CurrentPassword, string NewPassword);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "auth/change-password",
                    async Task<Results<NoContent, ValidationProblem, NotFound>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
                    {
                        var command = new Command(user, request.CurrentPassword, request.NewPassword);
                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(ChangePassword))
                .WithTags("Auth")
                .WithOpenApi(o =>
                {
                    o.Summary = "Change the current user password";
                    o.Description = "Allows an authenticated user to update their password by providing the current and new password.";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, string CurrentPassword, string NewPassword) : IRequest<Result>;

    public sealed class Handler(UserManager<ApplicationUser> userManager, IValidator<Command> validator)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var user = await userManager.FindByIdAsync(message.User.GetUserId().ToString());
            if (user == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var changePasswordResult = await userManager.ChangePasswordAsync(user, message.CurrentPassword, message.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                return Result.Fail(new ValidationError(changePasswordResult));
            }

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required");
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters long");
        }
    }
}
