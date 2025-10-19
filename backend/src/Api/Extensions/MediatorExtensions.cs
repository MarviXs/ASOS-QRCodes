using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using MediatR;

namespace Fei.Is.Api.Extensions;

public static class MediatorExtensions
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, AppDbContext ctx)
    {
        var domainEntities = ctx.ChangeTracker.Entries<BaseModel>().Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

        var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents!).ToList();
        domainEntities.ToList().ForEach(entity => entity.Entity.DomainEvents!.Clear());

        var tasks = domainEvents.Select(
            async (domainEvent) =>
            {
                await mediator.Publish(domainEvent);
            }
        );

        await Task.WhenAll(tasks);
    }
}
