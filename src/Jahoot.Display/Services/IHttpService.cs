using Jahoot.Core.Models;
using System.Threading.Tasks;

namespace Jahoot.Display.Services;

public interface IHttpService
{
    Task<T?> GetAsync<T>(string uri);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data);
    Task<Result> PostAsync<T>(string uri, T data);
    Task<Result> PutAsync<T>(string uri, T data);
    Task<Result> DeleteAsync(string uri);
}
