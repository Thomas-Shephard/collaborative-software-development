using Jahoot.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Display.Services
{
    public class StudentService : IStudentService
    {
        private readonly IHttpService _httpService;

        public StudentService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IEnumerable<Student>> GetStudents(bool isApproved)
        {
            return await _httpService.GetAsync<IEnumerable<Student>>($"api/student/list?isApproved={isApproved}") ?? [];
        }

        public async Task UpdateStudent(int userId, Student student)
        {
            var request = new UpdateStudentRequestModel
            {
                Name = student.Name,
                Email = student.Email,
                IsApproved = student.IsApproved,
                IsDisabled = student.IsDisabled,
                SubjectIds = student.Subjects.ToList().ConvertAll(s => s.SubjectId)
            };
            await _httpService.PutAsync($"api/student/{userId}", request);
        }

        public async Task DeleteStudent(int userId)
        {
            await _httpService.DeleteAsync($"api/student/{userId}");
        }
    }
}
