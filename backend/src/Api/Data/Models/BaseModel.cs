

using MediatR;

namespace Fei.Is.Api.Data.Models;

public class BaseModel
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<INotification>? DomainEvents { get; private set; }

    public void AddDomainEvent(INotification eventItem)
    {
        DomainEvents ??= [];
        DomainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
        DomainEvents?.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        DomainEvents?.Clear();
    }
}
