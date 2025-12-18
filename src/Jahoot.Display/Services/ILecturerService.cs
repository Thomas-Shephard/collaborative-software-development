using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Display.Services;

public interface ILecturerService
{
    Task<IEnumerable<Lecturer>> GetAllLecturersAsync();
    Task<Result> CreateLecturerAsync(CreateLecturerRequestModel request);
    Task<Result> UpdateLecturerAsync(int id, UpdateLecturerRequestModel request);
    Task<Result> AssignLecturerRoleAsync(AssignLecturerRoleRequestModel request);
    Task<Result> DeleteLecturerAsync(int id);
    Task<Result> ResetLecturerPasswordAsync(string email);
}
