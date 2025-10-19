using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class QRCode : BaseModel
{
    public Guid OwnerId { get; set; } = Guid.Empty;
    public ApplicationUser? Owner { get; set; } = null!;
    public required string DisplayName { get; set; }
    public required string RedirectUrl { get; set; }
    public required string ShortCode { get; set; }
}
