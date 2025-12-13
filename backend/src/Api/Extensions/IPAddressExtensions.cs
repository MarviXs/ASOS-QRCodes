using System.Collections.Concurrent;
using System.Net;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;

namespace Fei.Is.Api.Extensions;

public static class IPAddressExtensions
{
    private static readonly string DefaultDatabaseRelativePath = Path.Combine("Data", "GeoLite", "GeoLite2-Country.mmdb");

    private static readonly ConcurrentDictionary<string, Lazy<DatabaseReader>> ReaderCache = new(StringComparer.OrdinalIgnoreCase);

    public static string? GetCountryName(this IPAddress ipAddress, string? databasePath = null)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (IPAddress.IsLoopback(ipAddress))
        {
            return null;
        }

        var lookupAddress = ipAddress.IsIPv4MappedToIPv6 ? ipAddress.MapToIPv4() : ipAddress;

        var reader = GetReader(databasePath);

        try
        {
            var response = reader.Country(lookupAddress);
            return response?.Country?.Name;
        }
        catch (AddressNotFoundException)
        {
            return null;
        }
        catch (GeoIP2Exception)
        {
            return null;
        }
    }

    private static DatabaseReader GetReader(string? databasePath)
    {
        var path = ResolveDatabasePath(databasePath);
        var lazyReader = ReaderCache.GetOrAdd(
            path,
            key => new Lazy<DatabaseReader>(() => CreateReader(key), LazyThreadSafetyMode.ExecutionAndPublication)
        );

        return lazyReader.Value;
    }

    private static DatabaseReader CreateReader(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"GeoLite2 database not found at path '{path}'.", path);
        }

        return new DatabaseReader(path);
    }

    private static string ResolveDatabasePath(string? databasePath)
    {
        var basePath = AppContext.BaseDirectory;

        if (string.IsNullOrWhiteSpace(databasePath))
        {
            return Path.GetFullPath(Path.Combine(basePath, DefaultDatabaseRelativePath));
        }

        return Path.IsPathRooted(databasePath) ? Path.GetFullPath(databasePath) : Path.GetFullPath(Path.Combine(basePath, databasePath));
    }
}
