namespace Jahoot.Display.Services;
public interface ISecureStorageService
{
    void SaveToken(string token);
    
    string? GetToken();

    void DeleteToken();
}
