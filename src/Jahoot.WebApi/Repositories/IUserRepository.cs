using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

/// <summary>
/// This is the plan for how we get users from the database.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// This gets a user from the database by their email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>The user if they exist.</returns>
    Task<User?> GetUserByEmailAsync(string email);
    /// <summary>
    /// This updates a user in the database.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateUserAsync(User user);
}
