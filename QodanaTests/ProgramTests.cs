using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker.Builder;

namespace QodanaTests
{
    [ExcludeFromCodeCoverage]
    public class ProgramTests
    {
        [Fact]
        public void ConfigureApplicationInsights_WithoutConnectionString_DoesNotThrow()
        {
            // Arrange
            var expectedConnStr = "APPLICATIONINSIGHTS_CONNECTION_STRING";
            var originalValue = Environment.GetEnvironmentVariable(
                expectedConnStr);

            try
            {
                Environment.SetEnvironmentVariable(
                    expectedConnStr,
                    null);

                var builder = FunctionsApplication.CreateBuilder([]);

                // Act
                var exception = Record.Exception(() =>
                    Program.ConfigureApplicationInsights(builder));

                // Assert
                Assert.Null(exception);
            }
            finally
            {
                Environment.SetEnvironmentVariable(
                    expectedConnStr,
                    originalValue);
            }
        }

        [Fact]
        public void ConfigureApplicationInsights_WithConnectionString_DoesNotThrow()
        {
            // Arrange
            var expectedConnStr = "APPLICATIONINSIGHTS_CONNECTION_STRING";
            var originalValue = Environment.GetEnvironmentVariable(
                expectedConnStr);

            try
            {
                Environment.SetEnvironmentVariable(
                    expectedConnStr,
                    "InstrumentationKey=00000000-0000-0000-0000-000000000000");

                var builder = FunctionsApplication.CreateBuilder([]);

                // Act
                var exception = Record.Exception(() =>
                    Program.ConfigureApplicationInsights(builder));

                // Assert
                Assert.Null(exception);
            }
            finally
            {
                Environment.SetEnvironmentVariable(
                    expectedConnStr,
                    originalValue);
            }
        }
    }
}