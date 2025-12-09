using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public interface IStudentRepository
{
    Task CreateStudentAsync(string name, string email, string hashedPassword);
    Task CreateStudentAsync(int userId);
    Task<Student?> GetStudentByUserIdAsync(int userId);
}
