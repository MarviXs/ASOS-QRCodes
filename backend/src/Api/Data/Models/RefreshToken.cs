using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Models;

[Index(nameof(Token), IsUnique = true)]
public class RefreshToken
{
    public Guid Id { get; set; }
    public required Guid Token { get; set; }
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public long ExpiresAt { get; set; }
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public long CreatedAt { get; set; }
}
