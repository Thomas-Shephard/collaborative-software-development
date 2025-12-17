using System.Data;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email, IDbTransaction? transaction = null);
    Task<User?> GetUserByIdAsync(int userId);
    Task<List<Role>> GetRolesByUserIdAsync(int userId, IDbTransaction? transaction = null);
    Task<Dictionary<int, List<Role>>> GetRolesByUserIdsAsync(IEnumerable<int> userIds, IDbTransaction? transaction = null);
    Task UpdateUserAsync(User user, IDbTransaction? transaction = null);
    Task DeleteUserAsync(int userId);
}
