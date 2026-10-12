using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

Program.Main(args);

public partial class Program
{
    [ExcludeFromCodeCoverage]
    public static void Main(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        builder.ConfigureFunctionsWebApplication();

        ConfigureApplicationInsights(builder);

        builder.Build().Run();
    }

    public static void ConfigureApplicationInsights(
        FunctionsApplicationBuilder builder)
    {
        if (!string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable(
                    "APPLICATIONINSIGHTS_CONNECTION_STRING")))
        {
            builder.Services
                .AddOpenTelemetry()
                .UseFunctionsWorkerDefaults()
                .UseAzureMonitorExporter();
        }
    }
}