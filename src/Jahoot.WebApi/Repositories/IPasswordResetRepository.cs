using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public interface IPasswordResetRepository
{
    Task CreateTokenAsync(int userId, string token);
    Task<PasswordResetToken?> GetTokenByTokenAsync(string token);
    Task UpdateTokenAsync(PasswordResetToken token);
}
