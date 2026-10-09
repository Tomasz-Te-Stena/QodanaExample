using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace QodanaExample;

public class JustRun
{
    private readonly ILogger<JustRun> _logger;

    public JustRun(ILogger<JustRun> logger)
    {
        _logger = logger;
    }

    [Function("JustRun")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        // Log debug infomration
        bool debug = bool.TryParse(req.Query["debug"], out var debugValue)
                 && debugValue;

        if (debug)
        {
            _logger.LogInformation("[INIT] Call to JustRun.");
        }

        return new OkObjectResult("Welcome to Azure Functions!");
    }

    [Function("CurrentTime")]
    public IActionResult CurrentTime(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        // Log debug infomration
        bool debug = bool.TryParse(req.Query["debug"], out var debugValue)
                 && debugValue;

        if (debug)
        {
            _logger.LogInformation("[INIT] Call to CurrentTime.");
        }

        return new OkObjectResult(DateTimeOffset.UtcNow);
    }
}