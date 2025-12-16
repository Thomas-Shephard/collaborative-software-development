using System.Data;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email, IDbTransaction? transaction = null);
    Task UpdateUserAsync(User user, IDbTransaction? transaction = null);
    Task DeleteUserAsync(int userId);
}
