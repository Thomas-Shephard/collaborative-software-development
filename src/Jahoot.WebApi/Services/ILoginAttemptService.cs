namespace Jahoot.WebApi.Services;

public interface ILoginAttemptService
{
    Task<bool> IsLockedOut(string email, string ipAddress);
    Task RecordFailedLoginAttempt(string email, string ipAddress);
    Task ResetFailedLoginAttempts(string email, string ipAddress);
}
