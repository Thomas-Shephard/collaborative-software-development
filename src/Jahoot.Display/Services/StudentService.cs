using System.Collections.Generic;
using System.Threading.Tasks;
using Jahoot.Core.Models;
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

        public async Task<Result> UpdateStudent(int userId, Student student)
        {
            var request = new UpdateStudentRequestModel
            {
                Name = student.Name,
                Email = student.Email,
                IsApproved = student.IsApproved,
                IsDisabled = student.IsDisabled,
                SubjectIds = (student.Subjects?.ToList() ?? new List<Jahoot.Core.Models.Subject>()).ConvertAll(s => s.SubjectId)
            };
            var result = await _httpService.PutAsync($"api/student/{userId}", request);
            if (result is Result operationResult && !operationResult.Success)
            {
                throw new InvalidOperationException(operationResult.ErrorMessage ?? "Failed to update student.");
            }
            return result;
        }

        public async Task<Result> DeleteStudent(int userId)
        {
            var result = await _httpService.DeleteAsync($"api/student/{userId}");
            if (result is Result operationResult && !operationResult.Success)
            {
                throw new InvalidOperationException(operationResult.ErrorMessage ?? "Failed to delete student.");
            }
            return result;
        }
    }
}
