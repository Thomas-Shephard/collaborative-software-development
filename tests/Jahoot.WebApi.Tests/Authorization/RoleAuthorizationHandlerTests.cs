using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Jahoot.WebApi.Tests.Authorization;

public class RoleAuthorizationHandlerTests
{
    private RoleAuthorizationHandler _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new RoleAuthorizationHandler();
    }

    private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal user, IAuthorizationRequirement requirement)
    {
        return new AuthorizationHandlerContext([requirement], user, null);
    }

    private static ClaimsPrincipal CreateUser(bool isAuthenticated, Role[] roles)
    {
        List<Claim> claims = [..roles.Select(role => new Claim(ClaimTypes.Role, role.ToString()))];
        return new ClaimsPrincipal(new ClaimsIdentity(claims, isAuthenticated ? "TestAuth" : null));
    }

    [Test]
    public async Task HandleRequirementAsync_UnauthenticatedUser_DoesNotSucceed()
    {
        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(false, []);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task HandleRequirementAsync_UserWithMatchingRole_Succeeds()
    {
        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(true, [Role.Admin, Role.Lecturer]);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.True);
    }

    [Test]
    public async Task HandleRequirementAsync_UserWithoutMatchingRole_DoesNotSucceed()
    {
        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(true, [Role.Student]);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task HandleRequirementAsync_LecturerUserForAdminRole_DoesNotSucceed()
    {
        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(true, [Role.Lecturer]);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task HandleRequirementAsync_UserWithInvalidRoleClaim_DoesNotSucceed()
    {
        RoleRequirement requirement = new(Role.Admin);
        ClaimsPrincipal user = CreateUser(true, [(Role)(-1)]);
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task HandleRequirementAsync_UserWithUnparsableRoleClaim_DoesNotSucceed()
    {
        RoleRequirement requirement = new(Role.Admin);
        List<Claim> claims = [new(ClaimTypes.Role, "NotARealRole")];
        ClaimsPrincipal user = new(new ClaimsIdentity(claims, "TestAuth"));
        AuthorizationHandlerContext context = CreateContext(user, requirement);

        await _handler.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.False);
    }
}
