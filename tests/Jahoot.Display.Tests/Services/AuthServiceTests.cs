using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jahoot.Display.Services;
using Jahoot.Core.Models.Requests;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Jahoot.Display.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<ISecureStorageService> _secureStorageServiceMock;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _secureStorageServiceMock = new Mock<ISecureStorageService>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new System.Uri("http://localhost/")
        };

        _authService = new AuthService(httpClient, _secureStorageServiceMock.Object);
    }

    [Test]
    public async Task Register_ReturnsSuccess_WhenResponseIsSuccess()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);
        var request = new StudentRegistrationRequestModel 
        { 
            Email = "test@example.com", 
            Password = "Password123!", 
            Name = "Test User" 
        };

        // Act
        var result = await _authService.Register(request);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ErrorMessage, Is.Empty);
    }

    [Test]
    public async Task Register_ReturnsErrorMessage_WhenJsonHasMessageProperty()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "{\"message\": \"User already exists\"}");
        var request = new StudentRegistrationRequestModel 
        { 
            Email = "test@example.com", 
            Password = "Password123!", 
            Name = "Test User" 
        };

        // Act
        var result = await _authService.Register(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("User already exists"));
    }

    [Test]
    public async Task Register_ReturnsGenericError_WhenJsonHasNoMessageProperty()
    {
        // This tests the fix!
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "{\"other\": \"value\"}");
        var request = new StudentRegistrationRequestModel 
        { 
            Email = "test@example.com", 
            Password = "Password123!", 
            Name = "Test User" 
        };

        // Act
        var result = await _authService.Register(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Registration failed."));
    }

    [Test]
    public async Task Register_ReturnsRawContent_WhenResponseIsNotJson()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Critical Server Error");
        var request = new StudentRegistrationRequestModel 
        { 
            Email = "test@example.com", 
            Password = "Password123!", 
            Name = "Test User" 
        };

        // Act
        var result = await _authService.Register(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Critical Server Error"));
    }
    
    [Test]
    public async Task Register_ReturnsGenericError_WhenResponseEmpty()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "");
        var request = new StudentRegistrationRequestModel 
        { 
            Email = "test@example.com", 
            Password = "Password123!", 
            Name = "Test User" 
        };

        // Act
        var result = await _authService.Register(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Registration failed."));
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "")
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}
