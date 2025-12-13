using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Auth;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Auth;

public class RefreshControllerTests
{
    private RefreshController _refreshController;
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<ITokenDenyService> _tokenDenyServiceMock;
    private Mock<ITokenService> _tokenServiceMock;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenDenyServiceMock = new Mock<ITokenDenyService>();
        _tokenServiceMock = new Mock<ITokenService>();

        _refreshController = new RefreshController(_userRepositoryMock.Object, _tokenDenyServiceMock.Object, _tokenServiceMock.Object);
    }

    [Test]
    public async Task Refresh_ValidToken_InvalidatesOldAndReturnsNewToken()
    {
        string jti = Guid.NewGuid().ToString();
        long exp = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
        const string email = "test@example.com";

        ClaimsPrincipal userPrincipal = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email)
        ], "mock"));

        _refreshController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        User user = new()
        {
            UserId = 1,
            Name = "Test User",
            Email = email,
            PasswordHash = "hash",
            Roles = [Role.Student]
        };

        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email)).ReturnsAsync(user);

        const string expectedToken = "mocked_new_token";
        _tokenServiceMock.Setup(s => s.GenerateToken(user)).Returns(expectedToken);

        IActionResult result = await _refreshController.Refresh();

        _tokenDenyServiceMock.Verify(s => s.DenyAsync(jti, It.IsAny<DateTime>()), Times.Once);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        string? tokenString = okResult.Value?.GetType().GetProperty("Token")?.GetValue(okResult.Value, null) as string;
        Assert.That(tokenString, Is.EqualTo(expectedToken));

        _tokenServiceMock.Verify(s => s.GenerateToken(user), Times.Once);
    }

    [Test]
    public async Task Refresh_MissingJti_ReturnsBadRequest()
    {
        ClaimsPrincipal userPrincipal = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Exp, "12345"),
            new Claim(JwtRegisteredClaimNames.Email, "test@example.com")
        ], "mock"));

        _refreshController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        IActionResult result = await _refreshController.Refresh();

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("Token does not contain a JTI claim."));
    }

    [Test]
    public async Task Refresh_MissingExp_ReturnsBadRequest()
    {
        ClaimsPrincipal userPrincipal = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, "jti"),
            new Claim(JwtRegisteredClaimNames.Email, "test@example.com")
        ], "mock"));

        _refreshController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        IActionResult result = await _refreshController.Refresh();

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("Token does not contain a valid EXP claim."));
    }

    [Test]
    public async Task Refresh_MissingEmail_ReturnsBadRequest()
    {
        long exp = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
        ClaimsPrincipal userPrincipal = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, "jti"),
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString())
        ], "mock"));

        _refreshController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        IActionResult result = await _refreshController.Refresh();

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("Token does not contain an Email claim."));
    }

    [Test]
    public async Task Refresh_UserNotFound_ReturnsUnauthorized()
    {
        string jti = Guid.NewGuid().ToString();
        long exp = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
        const string email = "test@example.com";

        ClaimsPrincipal userPrincipal = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email)
        ], "mock"));

        _refreshController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email)).ReturnsAsync(() => null);

        IActionResult result = await _refreshController.Refresh();

        _tokenDenyServiceMock.Verify(s => s.DenyAsync(jti, It.IsAny<DateTime>()), Times.Once);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }
}
