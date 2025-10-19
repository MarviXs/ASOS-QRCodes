using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Auth.Commands;

public static class RefreshToken
{
    public record Request(string RefreshToken);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "auth/refresh",
                    async Task<Results<Ok<Response>, ProblemHttpResult>> (IMediator mediator, Request request) =>
                    {
                        var refreshCommand = new Command(request.RefreshToken);
                        var result = await mediator.Send(refreshCommand);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.Problem("Invalid refresh token", statusCode: StatusCodes.Status403Forbidden);
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(RefreshToken))
                .WithTags("Auth")
                .WithOpenApi(o =>
                {
                    o.Summary = "Refresh a token";
                    o.Description = "Refresh an access token using a refresh token";
                    return o;
                })
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }
    }

    public record Command(string RefreshToken) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context, TokenService tokenService) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command message, CancellationToken cancellationToken)
        {
            var refreshTokenId = tokenService.RefreshTokenToGuid(message.RefreshToken);
            var refreshToken = await context
                .RefreshTokens.Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == refreshTokenId, cancellationToken);
            if (refreshToken == null || refreshToken.IsExpired || refreshToken.User == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var accessToken = await tokenService.CreateAccessToken(refreshToken.User);
            if (accessToken == null)
            {
                return Result.Fail(new NotFoundError());
            }

            return Result.Ok(new Response(accessToken));
        }
    }

    public record Response(string AccessToken);
}
