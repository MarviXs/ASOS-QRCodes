using Bogus;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class QRCodeFake : Faker<QRCode>
{
    public QRCodeFake(Guid ownerId)
    {
        RuleFor(x => x.OwnerId, f => ownerId);
        RuleFor(x => x.DisplayName, f => f.Lorem.Word());
        RuleFor(x => x.RedirectUrl, f => f.Internet.Url());
        RuleFor(x => x.ShortCode, f => f.Random.AlphaNumeric(8));
        RuleFor(x => x.DotStyle, f => "square");
        RuleFor(x => x.CornerDotStyle, f => "square");
        RuleFor(x => x.CornerSquareStyle, f => "square");
        RuleFor(x => x.Color, f => f.Internet.Color());
    }
}
