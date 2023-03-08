using System.IO.Abstractions;

namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources;

public class FileProxySource : IProxySource
{
    private readonly IFileSystem _fileSystem;
    public string? Scheme { get; init; } = SupportedProxiesSchemes.Socks5;


    public FileProxySource(IFileSystem fileSystem) => _fileSystem = fileSystem;

    public FileProxySource() : this(new FileSystem()) { }


    public async Task<IEnumerable<Uri>> GetProxiesAsync(CancellationToken token)
    {
        var proxyFileName = Scheme is not null ? $"ProxyPool_{Scheme}.txt" : "ProxyPool.txt";
        var proxyFilePath = $"{_fileSystem.Directory.GetCurrentDirectory()}/Files/{proxyFileName}";
        var fileAllLines = await _fileSystem.File.ReadAllLinesAsync(proxyFilePath, token);
        return fileAllLines.Select(line => ProxyParser.Parse(line, Scheme));
    }

    public static FileProxySource AutoScheme => new() {Scheme = null};
    public static FileProxySource Http => new() {Scheme = SupportedProxiesSchemes.Http};
    public static FileProxySource Socks4 => new() {Scheme = SupportedProxiesSchemes.Socks4};
    public static FileProxySource Socks5 => new() {Scheme = SupportedProxiesSchemes.Socks5};

    public static FileProxySource[] GetAllSchemesLoaders
        => new[] {AutoScheme, Http, Socks4, Socks5};
}