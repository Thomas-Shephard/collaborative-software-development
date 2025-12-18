using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Authorization;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Jahoot.WebApi.Tests.Authorization;

public class RoleAuthorizationHandlerTests
{
    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private Mock<IServiceScope> _serviceScopeMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private RoleAuthorizationHandler _handler;

    [SetUp]
    public void Setup()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserRepository))).Returns(_userRepositoryMock.Object);

        _handler = new RoleAuthorizationHandler(_serviceScopeFactoryMock.Object);
    }

    private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal user, IAuthorizationRequirement requirement)
    {
        return new AuthorizationHandlerContext([requirement], user, null);
    }

    private static ClaimsPrincipal CreateUser(bool isAuthenticated, int userId)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Sub, userId.ToString())
        ];
        return new ClaimsPrincipal(new ClaimsIdentity(claims, isAuthenticated ? "TestAuth" : null));
    }

    private void SetupUserInRepo(int userId, bool isDisabled, params Role[] roles)
    {
        User user = new()
        {
            UserId = userId,
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hash",
            IsDisabled = isDisabled,
            Roles = roles.ToList()
        };

        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(user);
    }

    [Test]
    public async Task HandleRequirementAsync_UnauthenticatedUser_DoesNotSucceed()
    {
        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(false, 1);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task HandleRequirementAsync_UserWithMatchingRole_Succeeds()
    {
        const int userId = 1;
        SetupUserInRepo(userId, false, Role.Admin, Role.Lecturer);

        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(true, userId);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.True);
    }

    [Test]
    public async Task HandleRequirementAsync_DisabledUserWithMatchingRole_DoesNotSucceed()
    {
        const int userId = 1;
        SetupUserInRepo(userId, true, Role.Admin);

        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(true, userId);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task HandleRequirementAsync_UserWithoutMatchingRole_DoesNotSucceed()
    {
        const int userId = 1;
        SetupUserInRepo(userId, false, Role.Student);

        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(true, userId);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task HandleRequirementAsync_UserNotFound_DoesNotSucceed()
    {
        const int userId = 1;
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(true, userId);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.False);
    }
}
