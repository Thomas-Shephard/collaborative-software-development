using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    Task UpdateUserAsync(User user);
}
