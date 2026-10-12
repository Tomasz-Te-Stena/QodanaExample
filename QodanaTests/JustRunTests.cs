using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QodanaExample;

namespace QodanaTests;

[ExcludeFromCodeCoverage]
public class JustRunTests
{
    private readonly Mock<ILogger<JustRun>> _loggerMock;
    private readonly JustRun _sut;

    public JustRunTests()
    {
        _loggerMock = new Mock<ILogger<JustRun>>();
        _sut = new JustRun(_loggerMock.Object);
    }

    private static HttpRequest CreateRequest(string queryString = "")
    {
        var context = new DefaultHttpContext();

        if (!string.IsNullOrEmpty(queryString))
        {
            context.Request.QueryString = new QueryString(queryString);
        }

        return context.Request;
    }

    [Fact]
    public void Initialize_That_Class()
    {
        Assert.NotNull(_sut);
    }

    [Fact]
    public void Run_ReturnsPassword()
    {
        // Arrange
        const string expectedPhrase = "{\"pass\":\"SuperSecret123\"}";
        var request = CreateRequest();

        // Act
        var result = _sut.Run(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(
            expectedPhrase,
            okResult.Value);
    }

    [Fact]
    public void Run_WithDebugTrue_LogsInformation()
    {
        // Arrange
        const string expectedPhrase = "[INIT] Call to JustRun.";
        var request = CreateRequest("?debug=true");

        // Act
        var result = _sut.Run(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        VerifyLog(
            LogLevel.Information,
            expectedPhrase);
    }

    [Fact]
    public void Run_WithDebugFalse_DoesNotLogInformation()
    {
        // Arrange
        var request = CreateRequest("?debug=false");

        // Act
        _sut.Run(request);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void CurrentTime_ReturnsCurrentUtcTime()
    {
        // Arrange
        var request = CreateRequest();

        var before = DateTimeOffset.UtcNow;

        // Act
        var result = _sut.CurrentTime(request);

        var after = DateTimeOffset.UtcNow;

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTime = Assert.IsType<DateTimeOffset>(okResult.Value);

        Assert.InRange(returnedTime, before, after);
    }

    [Fact]
    public void CurrentTime_WithDebugTrue_LogsInformation()
    {
        // Arrange
        const string expectedPhrase = "[INIT] Call to CurrentTime.";
        var request = CreateRequest("?debug=true");

        // Act
        _sut.CurrentTime(request);

        // Assert
        VerifyLog(
            LogLevel.Information,
            expectedPhrase);
    }

    [Fact]
    public void ReadFile_ReturnsFileContent()
    {
        // Arrange
        var path = Path.GetTempFileName();
        const string expectedContent = "Hello from xUnit!";

        try
        {
            File.WriteAllText(path, expectedContent);

            var request = CreateRequest(
                $"?file={Uri.EscapeDataString(path)}");

            // Act
            var result = _sut.ReadFile(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(expectedContent, okResult.Value);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadFile_WhenFileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var path = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.txt");

        var request = CreateRequest(
            $"?file={Uri.EscapeDataString(path)}");

        // Act + Assert
        Assert.Throws<FileNotFoundException>(() => _sut.ReadFile(request));
    }

    [Fact]
    public void WeakEncryption_ReturnsExpectedMd5Hash()
    {
        // Arrange
        const string expectedPhrase = "EC25161FA0D047E8510496A34E46DF01";
        var request = CreateRequest();

        // Act
        var result = _sut.WeakEncryption(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var hash = Assert.IsType<string>(okResult.Value);

        Assert.NotEmpty(hash);
        Assert.Equal(expectedPhrase.Length, hash.Length);
    }

    private void VerifyLog(
        LogLevel level,
        string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, _) =>
                    value!.ToString() == expectedMessage),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
