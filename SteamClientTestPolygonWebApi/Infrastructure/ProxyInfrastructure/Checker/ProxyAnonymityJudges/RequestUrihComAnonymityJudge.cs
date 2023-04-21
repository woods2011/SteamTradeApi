namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxyAnonymityJudges;

public class RequestUrihComAnonymityJudge : IProxyAnonymityJudge
{
    public string ContentUri => "https://request.urih.com/";

    private readonly ProxyAnonymityByHeadersChecker _proxyAnonymityByHeadersChecker;

    public RequestUrihComAnonymityJudge(ProxyAnonymityByHeadersChecker proxyAnonymityByHeadersChecker) =>
        _proxyAnonymityByHeadersChecker = proxyAnonymityByHeadersChecker;

    public async Task<ProxyAnonymityLevel> Judge(HttpContent responseMessage)
    {
        HeaderDictionary headersFromContent = await ParseHttpHeadersFromContent(responseMessage);
        return _proxyAnonymityByHeadersChecker.CheckProxyAnonymity(headersFromContent);
    }

    private static async Task<HeaderDictionary> ParseHttpHeadersFromContent(HttpContent responseMessage)
    {
        var content = await responseMessage.ReadAsStringAsync();
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        var headersFromContent = new HeaderDictionary();

        foreach (var line in lines)
        {
            var headerParts = line.Split(new[] { ':', '=' }, 2, StringSplitOptions.TrimEntries);
            if (headerParts.Length != 2) continue;
            var (key, value) = (headerParts[0], headerParts[1]);
            headersFromContent.Add(key, value);
        }
        
        return headersFromContent;
    }
}