using Bogus;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class DeviceTemplateFake : Faker<DeviceTemplate>
{
    public DeviceTemplateFake(Guid OwnerId)
    {
        RuleFor(x => x.OwnerId, f => OwnerId);
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
    }
}
