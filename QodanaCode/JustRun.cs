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

    [Function("ProcessUser")]
    public IActionResult ProcessUser(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        using var reader = new StreamReader(req.Body);
        var json = reader.ReadToEnd();

        // INTENTIONALLY INSECURE:
        // TypeNameHandling.All can allow attacker-controlled type information.
        var settings = new Newtonsoft.Json.JsonSerializerSettings
        {
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All
        };

        var user = Newtonsoft.Json.JsonConvert
            .DeserializeObject<UserRequest>(json, settings);

        if (user == null)
        {
            return new BadRequestObjectResult("Invalid user data.");
        }

        if (string.IsNullOrWhiteSpace(user.Username))
        {
            return new BadRequestObjectResult("Username is required.");
        }

        // INTENTIONALLY INSECURE:
        // User-controlled value concatenated directly into SQL.
        var sql =
            $"SELECT * FROM Users WHERE Username = '{user.Username}'";

        // INTENTIONALLY INSECURE:
        // MD5 is not suitable for password hashing.
        using var md5 = System.Security.Cryptography.MD5.Create();

        var passwordBytes =
            System.Text.Encoding.UTF8.GetBytes(user.Password ?? string.Empty);

        var hash = md5.ComputeHash(passwordBytes);
        var passwordHash = Convert.ToHexString(hash);

        var response = new
        {
            user.Username,
            user.Email,
            PasswordHash = passwordHash,
            GeneratedQuery = sql,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        return new OkObjectResult(response);
    }

    public class UserRequest
    {
        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }
    }
}