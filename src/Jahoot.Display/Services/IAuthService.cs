using Jahoot.Core.Models;

namespace Jahoot.Display.Services;

/// <summary>
/// This is the plan for how our app handles logging people in and out.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Tries to log a user in using their email and password.
    /// </summary>
    /// <param name="loginRequest">The user's email and password.</param>
    /// <returns>
    /// True if it worked, false if it didn't.
    /// </returns>
    Task<Tuple<bool, string>> Login(LoginRequest loginRequest);

    /// <summary>
    /// Logs the user out and gets rid of their login token.
    /// </summary>
    void Logout();
}
