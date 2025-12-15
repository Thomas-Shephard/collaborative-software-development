using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public interface ILecturerRepository
{
    Task CreateLecturerAsync(string name, string email, string hashedPassword, bool isAdmin);
    Task CreateLecturerAsync(int userId, bool isAdmin);
    Task<Lecturer?> GetLecturerByUserIdAsync(int userId);
    Task<IEnumerable<Lecturer>> GetLecturersAsync();
    Task UpdateLecturerAsync(Lecturer lecturer);
}
