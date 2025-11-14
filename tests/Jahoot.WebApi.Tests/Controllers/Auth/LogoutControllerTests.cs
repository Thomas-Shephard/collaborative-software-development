using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jahoot.WebApi.Controllers.Auth;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Auth;

public class LogoutControllerTests
{
    private LogoutController _logoutController;
    private Mock<ITokenDenyService> _tokenDenyServiceMock;

    [SetUp]
    public void Setup()
    {
        _tokenDenyServiceMock = new Mock<ITokenDenyService>();
        _logoutController = new LogoutController(_tokenDenyServiceMock.Object);
    }

    [Test]
    public async Task Logout_ValidToken_DeniesTokenAndReturnsOk()
    {
        string jti = Guid.NewGuid().ToString();
        long exp = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString())
        ], "mock"));

        _logoutController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        IActionResult result = await _logoutController.Logout();

        Assert.That(result, Is.TypeOf<OkResult>());
        _tokenDenyServiceMock.Verify(tokenDenyService => tokenDenyService.DenyAsync(jti, It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public async Task Logout_TokenWithoutJti_ReturnsBadRequest()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity([
            // No JTI claim
        ], "mock"));

        _logoutController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        IActionResult result = await _logoutController.Logout();

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("Token does not contain a JTI claim."));
    }

    [Test]
    public async Task Logout_TokenWithoutExp_ReturnsBadRequest()
    {
        string jti = Guid.NewGuid().ToString();
        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, jti)
            // No Exp claim
        ], "mock"));

        _logoutController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        IActionResult result = await _logoutController.Logout();

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("Token does not contain a valid EXP claim."));
        _tokenDenyServiceMock.Verify(tokenDenyService => tokenDenyService.DenyAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task Logout_TokenWithInvalidExp_ReturnsBadRequest()
    {
        string jti = Guid.NewGuid().ToString();
        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Exp, "invalid_exp_value") // Invalid EXP claim
        ], "mock"));

        _logoutController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        IActionResult result = await _logoutController.Logout();

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("Token does not contain a valid EXP claim."));
        _tokenDenyServiceMock.Verify(tokenDenyService => tokenDenyService.DenyAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }
}
