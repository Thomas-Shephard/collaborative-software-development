using System.Threading.Tasks;

namespace Jahoot.Display.Services;

public interface IHttpService
{
    Task<T?> GetAsync<T>(string uri);
    Task<Result> PostAsync<T>(string uri, T data);
    Task<Result> PutAsync<T>(string uri, T data);
    Task<Result> DeleteAsync(string uri);
}
