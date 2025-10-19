using Bogus;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class CommandFake : Faker<Command>
{
    public CommandFake(Guid deviceTemplateId)
    {
        RuleFor(x => x.DeviceTemplateId, f => deviceTemplateId);
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
        RuleFor(x => x.DisplayName, f => f.Commerce.ProductAdjective());
        RuleFor(x => x.Params, f => [f.Random.Double(), f.Random.Double()]);
    }
}
