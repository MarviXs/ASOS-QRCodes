using System.ComponentModel.DataAnnotations;
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

public static class Login
{
    public record Request(string Email, string Password);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "auth/login",
                    async Task<Results<Ok<Response>, UnauthorizedHttpResult, BadRequest, ValidationProblem>> (IMediator mediator, Request request) =>
                    {
                        var loginCommand = new Command(request.Email, request.Password);
                        var result = await mediator.Send(loginCommand);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        if (result.HasError<NotFoundError>() || result.HasError<LoginFailedError>())
                        {
                            return TypedResults.Unauthorized();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(Login))
                .WithTags("Auth")
                .WithOpenApi(o =>
                {
                    o.Summary = "Login";
                    o.Description = "Login with an email and password";
                    return o;
                });
        }
    }

    public record Command(string Email, string Password) : IRequest<Result<Response>>;

    public sealed class Handler(UserManager<ApplicationUser> userManager, TokenService tokenService, IValidator<Command> validator)
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var user = await userManager.FindByEmailAsync(message.Email);
            if (user == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, message.Password);

            if (!passwordValid)
            {
                return Result.Fail(new LoginFailedError());
            }

            var accessToken = await tokenService.CreateAccessToken(user);
            var refreshToken = await tokenService.CreateRefreshToken(user);

            return Result.Ok(new Response(accessToken, refreshToken));
        }
    }

    public record Response(string AccessToken, string RefreshToken);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid email address");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        }
    }
}
