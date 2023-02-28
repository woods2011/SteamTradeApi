using SteamClientTestPolygonWebApi.ProxyInfrastructure.BackGroundServices;

namespace SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker.ProxyAnonymityJudges;

public class ProxyAnonymityByHeadersChecker
{
    private readonly SelfIpAddressProvider _selfIpAddressProvider;
    private readonly IEnumerable<string> _transparentProxyHeaders;
    private readonly IEnumerable<string> _anonymousProxyHeaders;

    public ProxyAnonymityByHeadersChecker(SelfIpAddressProvider selfIpAddressProvider)
    {
        _selfIpAddressProvider = selfIpAddressProvider;

        _transparentProxyHeaders = new List<string>()
        {
            //ToDo
        };

        _anonymousProxyHeaders = new List<string>()
        {
            "HTTP_X_FORWARDED_FOR",
            "HTTP_FORWARDED_FOR",
            "HTTP_X_FORWARDED",
            "HTTP_CLIENT_IP",
            "HTTP_FORWARDED_FOR_IP",
            "MT-PROXY-ID",
            "X-TINYPROXY",
            "X_FORWARDED_FOR",
            "FORWARDED_FOR",
            "CLIENT-IP",
            "X-PROXY-ID",
            "PROXY-AGENT",
            "FORWARDED_FOR_IP",
            "X_HTTP_FORWARDED_FOR",

            "X-FORWARDED-FOR",
            "FORWARDED-FOR-IP",


            "REMOTE_ADDR",    // ToDo
            "HTTP_FORWARDED", // ToDo
            "VIA",
            "HTTP-VIA",
            "HTTP_VIA",
            "HTTP_PROXY_CONNECTION",
            "HTTP_COMING_FROM",
            "HTTP_X_COMING_FROM"
        };
    }


    public ProxyAnonymityLevel CheckProxyAnonymity(HeaderDictionary headers)
    {
        var isTransparent =
            headers.Any(header => IsTransparentHeader(header.Key) || IsContainIp(header.Value.ToString()));
        if (isTransparent) return ProxyAnonymityLevel.Transparent;

        var isAnonymous = headers.Any(header => IsAnonymousHeader(header.Key));
        if (isAnonymous) return ProxyAnonymityLevel.Anonymous;

        return ProxyAnonymityLevel.Elite;
    }

    private bool IsTransparentHeader(string line) =>
        _transparentProxyHeaders.Any(match => line.Contains(match, StringComparison.InvariantCultureIgnoreCase));

    private bool IsAnonymousHeader(string line) =>
        _anonymousProxyHeaders.Any(match => line.Contains(match, StringComparison.InvariantCultureIgnoreCase));

    private bool IsContainIp(string value) =>
        value.Contains(_selfIpAddressProvider.Ip, StringComparison.InvariantCultureIgnoreCase);
}