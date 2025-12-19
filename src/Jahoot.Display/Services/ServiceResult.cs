using System.Net;

namespace Jahoot.Display.Services
{
    public class ServiceResult<T>
    {
        public T? Data { get; init; }
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
        public HttpStatusCode? StatusCode { get; init; } = null; // Optional: to carry HTTP status

        public static ServiceResult<T> Success(T data) => new ServiceResult<T> { Data = data, IsSuccess = true };
        public static ServiceResult<T> Failure(string errorMessage, HttpStatusCode? statusCode = null) => new ServiceResult<T> { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode };
    }
}
