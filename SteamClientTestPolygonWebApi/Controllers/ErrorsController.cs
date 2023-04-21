using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SteamClientTestPolygonWebApi.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorsController : ControllerBase
{
    private readonly ILogger<ErrorsController> _logger;

    public ErrorsController(ILogger<ErrorsController> logger) =>
        _logger = logger;

    [Route("/error")]
    public IActionResult Error()
    {
        Exception? exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is not null)
        {
            _logger.LogError(
                exception,
                "An error occurred while processing user request. {@ErrorMessage}, {@DateTimeUtc}",
                exception.Message, DateTime.UtcNow);
        }

        return Problem();
    }
}