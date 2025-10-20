using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.QRCodes.Queries;

public static class GetQRCodes
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "qr-codes",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetQRCodes))
                .WithTags(nameof(QRCode))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated QR codes";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var qrCodeParameters = message.Parameters;

            var query = context
                .QRCodes.AsNoTracking()
                .Where(qrCode => qrCode.OwnerId == message.User.GetUserId())
                .Where(d => d.DisplayName.ToLower().Contains(StringUtils.Normalized(qrCodeParameters.SearchTerm)))
                .Sort(qrCodeParameters.SortBy ?? nameof(QRCode.UpdatedAt), qrCodeParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var qrCodes = await query.Paginate(qrCodeParameters).ToListAsync(cancellationToken);

            var responseItems = qrCodes
                .Select(qrCode => new Response(
                    qrCode.Id,
                    qrCode.DisplayName,
                    qrCode.RedirectUrl,
                    qrCode.ShortCode,
                    qrCode.CreatedAt,
                    qrCode.UpdatedAt,
                    qrCode.DotStyle,
                    qrCode.CornerDotStyle,
                    qrCode.CornerSquareStyle,
                    qrCode.Color
                ))
                .ToList();

            return responseItems.ToPagedList(totalCount, qrCodeParameters.PageNumber, qrCodeParameters.PageSize);
        }
    }

    public record Response(
        Guid Id,
        string DisplayName,
        string RedirectUrl,
        string ShortCode,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string DotStyle,
        string CornerDotStyle,
        string CornerSquareStyle,
        string Color
    );
}
