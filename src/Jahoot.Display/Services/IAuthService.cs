using Jahoot.Core.Models;

namespace Jahoot.Display.Services;
public interface IAuthService
{
    Task<Tuple<bool, string>> Login(LoginRequest loginRequest);

    void Logout();
}
