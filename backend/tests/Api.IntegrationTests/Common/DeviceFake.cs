using Bogus;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class DeviceFake : Faker<Device>
{
    public DeviceFake(Guid ownerId, Guid deviceTemplateId)
    {
        RuleFor(x => x.OwnerId, f => ownerId);
        RuleFor(x => x.DeviceTemplateId, f => deviceTemplateId);
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
        RuleFor(x => x.Mac, f => f.Internet.Mac());
        RuleFor(x => x.AccessToken, f => f.Internet.Mac());
    }
}
