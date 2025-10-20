using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.QRCodes.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.QRScanRecords.Queries;

public static class GetScanAnalytics
{
    public class QueryParameters
    {
        public Guid QRCodeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "scan-records/analytics",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult, ValidationProblem>> (
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
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetScanAnalytics))
                .WithTags(nameof(ScanRecord))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get analytics for scan records";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        private const int DefaultRangeDays = 30;

        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var parameters = message.Parameters;

            if (parameters.QRCodeId == Guid.Empty)
            {
                return Result.Fail(new ValidationError(nameof(parameters.QRCodeId), "QRCodeId is required."));
            }

            var qrCode = await context.QRCodes.AsNoTracking().FirstOrDefaultAsync(qr => qr.Id == parameters.QRCodeId, cancellationToken);
            if (qrCode == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!qrCode.IsOwner(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var startDate = parameters.StartDate?.Date ?? DateTime.UtcNow.Date.AddDays(-(DefaultRangeDays - 1));
            var endDate = parameters.EndDate?.Date ?? DateTime.UtcNow.Date;

            if (endDate < startDate)
            {
                return Result.Fail(new ValidationError(nameof(parameters.EndDate), "EndDate must be on or after StartDate."));
            }

            var periodEndExclusive = endDate.AddDays(1);

            var baseQuery = context.ScanRecords.AsNoTracking().Where(scan => scan.QRCodeId == parameters.QRCodeId);

            var lifetimeScans = await baseQuery.CountAsync(cancellationToken);

            var periodQuery = baseQuery.Where(scan => scan.CreatedAt >= startDate && scan.CreatedAt < periodEndExclusive);

            var totalScansInPeriod = await periodQuery.CountAsync(cancellationToken);

            var dailyCountsRaw = await periodQuery
                .GroupBy(scan => scan.CreatedAt.Date)
                .Select(group => new DailyScan(group.Key, group.Count()))
                .ToListAsync(cancellationToken);

            var totalDays = (endDate - startDate).Days;
            var dailyLookup = dailyCountsRaw.ToDictionary(daily => daily.Date);
            var dailySeries = new List<DailyScan>(totalDays + 1);

            for (var dayOffset = 0; dayOffset <= totalDays; dayOffset++)
            {
                var date = startDate.AddDays(dayOffset);
                if (!dailyLookup.TryGetValue(date, out var dailyValue))
                {
                    dailySeries.Add(new DailyScan(date, 0));
                    continue;
                }

                dailySeries.Add(dailyValue);
            }

            var countryStatsRaw = await periodQuery
                .GroupBy(scan => scan.Country)
                .Select(group => new { group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken);

            var osStatsRaw = await periodQuery
                .GroupBy(scan => scan.OperatingSystem)
                .Select(group => new { group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken);

            var browserStatsRaw = await periodQuery
                .GroupBy(scan => scan.BrowserInfo)
                .Select(group => new { group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken);

            var deviceTypeStatsRaw = await periodQuery
                .GroupBy(scan => scan.DeviceType)
                .Select(group => new { group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken);

            var normalizedCountries = NormalizeCategoryCounts(
                countryStatsRaw.Select(item => new CategoryCount(item.Key ?? string.Empty, item.Count))
            );
            var normalizedOperatingSystems = NormalizeCategoryCounts(
                osStatsRaw.Select(item => new CategoryCount(item.Key ?? string.Empty, item.Count))
            );
            var normalizedBrowsers = NormalizeCategoryCounts(browserStatsRaw.Select(item => new CategoryCount(item.Key ?? string.Empty, item.Count)));
            var normalizedDeviceTypes = NormalizeCategoryCounts(
                deviceTypeStatsRaw.Select(item => new CategoryCount(item.Key.ToString(), item.Count))
            );

            var response = new Response(
                dailySeries,
                normalizedCountries,
                normalizedOperatingSystems,
                normalizedBrowsers,
                normalizedDeviceTypes,
                totalScansInPeriod,
                lifetimeScans,
                startDate,
                endDate
            );

            return Result.Ok(response);
        }

        private static List<CategoryCount> NormalizeCategoryCounts(IEnumerable<CategoryCount> items)
        {
            return items.Select(item => item with { Name = NormalizeLabel(item.Name) }).OrderByDescending(item => item.Count).ToList();
        }

        private static string NormalizeLabel(string? label)
        {
            return string.IsNullOrWhiteSpace(label) ? "Unknown" : label!;
        }
    }

    public record Response(
        IReadOnlyList<DailyScan> DailyScans,
        IReadOnlyList<CategoryCount> Countries,
        IReadOnlyList<CategoryCount> OperatingSystems,
        IReadOnlyList<CategoryCount> Browsers,
        IReadOnlyList<CategoryCount> DeviceTypes,
        int TotalScansInPeriod,
        int LifetimeScans,
        DateTime StartDate,
        DateTime EndDate
    );

    public record DailyScan(DateTime Date, int Count);

    public record CategoryCount(string Name, int Count);
}
