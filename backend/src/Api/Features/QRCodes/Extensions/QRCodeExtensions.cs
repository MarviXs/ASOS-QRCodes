using System.Security.Claims;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.QRCodes.Extensions;

public static class QRCodeExtensions
{
    public static bool IsOwner(this QRCode qrCode, ClaimsPrincipal user)
    {
        return qrCode.OwnerId == user.GetUserId();
    }
}
