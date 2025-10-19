namespace Fei.Is.Api.Common.Utils;

public static class StringUtils
{
    public static string Normalized(string? search)
    {
        return string.IsNullOrWhiteSpace(search) ? string.Empty : search.Trim().ToLower();
    }
}
