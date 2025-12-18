using Jahoot.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jahoot.Display.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetStudents(bool isApproved);
        Task UpdateStudent(int userId, Student student);
        Task DeleteStudent(int userId);
    }
}
