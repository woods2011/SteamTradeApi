using MediatR;

namespace SteamClientTestPolygonWebApi.Core.Application.Behaviors;

public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken token)
    {
        _logger.LogInformation(
            "Request Handling {@RequestName} {@DateTimeUtc}", typeof(TRequest).Name, DateTime.UtcNow);

        TResponse response = await next();
        
        _logger.LogInformation(
            "Request Completed {@RequestName} {@DateTimeUtc}", typeof(TRequest).Name, DateTime.UtcNow);
        
        return response;
    }
}