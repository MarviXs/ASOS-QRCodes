using System;
using System.Security.Claims;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;

namespace Fei.Is.Api.Features.QRCodes.Extensions;

public static class QRCodeExtensions
{
    public static bool IsOwner(this QRCode qrCode, ClaimsPrincipal user)
    {
        return qrCode.OwnerId == user.GetUserId();
    }

    public static string EnsureHttpsScheme(this string redirectUrl)
    {
        if (string.IsNullOrWhiteSpace(redirectUrl))
        {
            return redirectUrl;
        }

        var trimmed = redirectUrl.Trim();

        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return $"https://{trimmed}";
    }
}
