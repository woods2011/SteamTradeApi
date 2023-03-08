namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxyAnonymityJudges;

public interface IProxyAnonymityJudge
{
    string ContentUri { get; }
    Task<ProxyAnonymityLevel> Judge(HttpContent responseMessage);
}

public enum ProxyAnonymityLevel
{
    Transparent,
    Anonymous,
    Elite
}