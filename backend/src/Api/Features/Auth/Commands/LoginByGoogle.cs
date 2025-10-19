using System.ComponentModel.DataAnnotations;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Fei.Is.Api.Features.Auth.Commands;

public static class LoginByGoogle
{
    public record Request(string GoogleToken);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "auth/google",
                    async Task<Results<Ok<Response>, UnauthorizedHttpResult, BadRequest, ValidationProblem>> (IMediator mediator, Request request) =>
                    {
                        var loginCommand = new Command(request.GoogleToken);
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
                .WithName(nameof(LoginByGoogle))
                .WithTags("Auth")
                .WithOpenApi(o =>
                {
                    o.Summary = "Login";
                    o.Description = "Login with Google";
                    return o;
                });
        }
    }

    public record Command(string GoogleToken) : IRequest<Result<Response>>;

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

            Payload payload = await ValidateAsync(message.GoogleToken);
            var user = await userManager.FindByEmailAsync(payload.Email);

            if (user == null) // Register user if not exists
            {
                user = new ApplicationUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    RegistrationDate = DateTimeOffset.UtcNow
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return Result.Fail(new ValidationError(createResult));
                }
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
            RuleFor(x => x.GoogleToken).NotEmpty().WithMessage("GoogleToken is required");
        }
    }
}
