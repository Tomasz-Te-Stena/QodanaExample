using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace QodanaExample;

public class JustRun(ILogger<JustRun> logger)
{
    private readonly ILogger<JustRun> _logger = logger;

    private const string Password = "SuperSecret123";

    [Function("JustRun")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        // Log debug information
        var debug = bool.TryParse(req.Query["debug"], out var debugValue)
                 && debugValue;

        if (debug)
        {
            _logger.LogInformation("[INIT] Call to JustRun.");
        }

        return new OkObjectResult("{\"pass\":\""+Password+"\"}");
    }

    [Function("CurrentTime")]
    public IActionResult CurrentTime(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        // Log debug information
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

    [Function("WeakEncryption")]
    public IActionResult WeakEncryption(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        // Log debug information
        var debug = bool.TryParse(req.Query["debug"], out var debugValue)
                    && debugValue;

        if (debug)
        {
            _logger.LogInformation("[INIT] Call to WeakEncryption.");
        }

        // INTENTIONALLY INSECURE - Qodana security demonstration
        using var md5 = System.Security.Cryptography.MD5.Create();

        var input = System.Text.Encoding.UTF8.GetBytes("SensitivePassword123");

        var hash = md5.ComputeHash(input);

        return new OkObjectResult(Convert.ToHexString(hash));
    }

    public IActionResult InsecureDeserialization(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        using var reader = new StreamReader(req.Body);
        var json = reader.ReadToEnd();

        // INTENTIONALLY VULNERABLE:
        // TypeNameHandling.All allows type information supplied in JSON
        // to influence object creation.
        var settings = new Newtonsoft.Json.JsonSerializerSettings
        {
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All
        };

        var result =
            Newtonsoft.Json.JsonConvert.DeserializeObject<object>(
                json,
                settings);

        return new OkObjectResult(result);
    }

    public HttpClient CreateUnsafeHttpClient()
    {
        var handler = new HttpClientHandler();

        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        return new HttpClient(handler);
    }
}