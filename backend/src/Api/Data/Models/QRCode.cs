namespace Fei.Is.Api.Data.Models;

public class QRCode : BaseModel
{
    public Guid OwnerId { get; set; } = Guid.Empty;
    public ApplicationUser? Owner { get; set; } = null!;
    public ICollection<ScanRecord> ScanRecords { get; set; } = [];
    public required string DisplayName { get; set; }
    public required string RedirectUrl { get; set; }
    public required string ShortCode { get; set; }

    // QR Code styling properties
    public required string DotStyle { get; set; } = "square";
    public required string CornerDotStyle { get; set; } = "square";
    public required string CornerSquareStyle { get; set; } = "square";
    public required string Color { get; set; }
}
