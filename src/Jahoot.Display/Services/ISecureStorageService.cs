namespace Jahoot.Display.Services;
public interface ISecureStorageService
{
    void SaveToken(string token, bool persist);
    
    string? GetToken();

    void DeleteToken();
}
