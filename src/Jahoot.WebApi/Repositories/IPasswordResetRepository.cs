using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public interface IPasswordResetRepository
{
    Task CreateTokenAsync(int userId, string token);
    Task<PasswordResetToken?> GetPasswordResetTokenByEmail(string email);
    Task UpdatePasswordResetTokenAsync(PasswordResetToken passwordResetToken);
}
