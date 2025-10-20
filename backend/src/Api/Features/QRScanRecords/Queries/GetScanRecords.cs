using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.QRCodes.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.QRScanRecords.Queries;

public static class GetScanRecords
{
    public class QueryParameters : SearchParameters
    {
        public Guid? QRCodeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "scan-records",
                    async Task<Results<Ok<PagedList<Response>>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(user, parameters);
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
                .WithName(nameof(GetScanRecords))
                .WithTags(nameof(ScanRecord))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated scan records";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var qrCodeParameters = message.Parameters;

            var qrCode = await context
                .QRCodes.AsNoTracking()
                .FirstOrDefaultAsync(qrCode => qrCode.Id == qrCodeParameters.QRCodeId, cancellationToken);
            if (qrCode == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!qrCode.IsOwner(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var query = context.ScanRecords.AsNoTracking();
            if (qrCodeParameters.QRCodeId.HasValue)
            {
                query = query.Where(d => d.QRCodeId == qrCodeParameters.QRCodeId.Value);
            }

            if (qrCodeParameters.StartDate.HasValue)
            {
                query = query.Where(d => d.CreatedAt >= qrCodeParameters.StartDate.Value);
            }
            if (qrCodeParameters.EndDate.HasValue)
            {
                query = query.Where(d => d.CreatedAt <= qrCodeParameters.EndDate.Value);
            }

            query = query.Sort(qrCodeParameters.SortBy ?? nameof(ScanRecord.CreatedAt), qrCodeParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var scans = await query.Paginate(qrCodeParameters).ToListAsync(cancellationToken);

            var responseItems = scans.Select(scan => new Response(
                scan.Id,
                qrCode.DisplayName,
                scan.QRCodeId,
                scan.Country,
                scan.OperatingSystem,
                scan.BrowserInfo,
                scan.DeviceType,
                scan.CreatedAt
            ));

            return responseItems.ToPagedList(totalCount, qrCodeParameters.PageNumber, qrCodeParameters.PageSize);
        }
    }

    public record Response(
        Guid Id,
        string QRCodeName,
        Guid QRCodeId,
        string Country,
        string OperatingSystem,
        string BrowserInfo,
        DeviceType DeviceType,
        DateTime CreatedAt
    );
}
