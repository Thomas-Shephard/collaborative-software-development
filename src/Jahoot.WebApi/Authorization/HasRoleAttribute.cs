using Jahoot.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace Jahoot.WebApi.Authorization;

public class HasRoleAttribute(Role role) : AuthorizeAttribute(role.ToString());
