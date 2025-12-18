using Jahoot.Core.Models;

namespace Jahoot.WebApi.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
