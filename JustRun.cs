using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;


namespace QodanaExample;

public class JustRun(ILogger<JustRun> logger)
{
    private readonly ILogger<JustRun> _logger = logger;

    private string password = "SuperSecret123";

    [Function("JustRun")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        // Log debug infomration
        var debug = bool.TryParse(req.Query["debug"], out var debugValue)
                 && debugValue;

        if (debug)
        {
            _logger.LogInformation("[INIT] Call to JustRun.");
        }

        return new OkObjectResult("{\"pass\":\""+password+"\"}");
    }

    [Function("CurrentTime")]
    public IActionResult CurrentTime(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        // Log debug infomration
        var debug = bool.TryParse(req.Query["debug"], out var debugValue)
                 && debugValue;

        if (debug)
        {
            _logger.LogInformation("[INIT] Call to CurrentTime.");
        }

        return new OkObjectResult(DateTimeOffset.UtcNow);
    }

    [Function("ReadFile")]
    public IActionResult ReadFile(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        var fileName = req.Query["file"].ToString();

        // INTENTIONALLY VULNERABLE:
        // Untrusted HTTP input is used directly as a filesystem path.
        var content = File.ReadAllText(fileName);

        return new OkObjectResult(content);
    }
}