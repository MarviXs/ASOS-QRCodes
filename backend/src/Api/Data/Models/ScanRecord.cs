using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class ScanRecord : BaseModel
{
    public Guid QRCodeId { get; set; } = Guid.Empty;
    public QRCode? QRCode { get; set; } = null!;
    public string BrowserInfo { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
