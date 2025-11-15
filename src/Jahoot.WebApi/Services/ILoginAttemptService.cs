namespace Jahoot.WebApi.Services;

public interface ILoginAttemptService
{
    Task<bool> IsLockedOut(string username, string ipAddress);
    Task RecordFailedLoginAttempt(string username, string ipAddress);
    void ResetFailedLoginAttempts(string username, string ipAddress);
}
