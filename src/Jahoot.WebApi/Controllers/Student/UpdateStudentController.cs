using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/{userId:int}")]
[ApiController]
[Tags("Student")]
public class UpdateStudentController(IStudentRepository studentRepository, IUserRepository userRepository, ISubjectRepository subjectRepository) : ControllerBase
{
    [HttpPut]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> UpdateStudent(int userId, [FromBody] UpdateStudentRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Core.Models.Student? student = await studentRepository.GetStudentByUserIdAsync(userId);
        if (student is null)
        {
            return NotFound($"Student with user ID {userId} not found.");
        }

        User? existingUser = await userRepository.GetUserByEmailAsync(requestModel.Email);
        if (existingUser is not null && existingUser.UserId != userId)
        {
            return Conflict("A user with this email address already exists.");
        }

        List<Jahoot.Core.Models.Subject> subjects = [];
        List<Core.Models.Subject> retrievedSubjects = (await subjectRepository.GetSubjectsByIdsAsync(requestModel.SubjectIds)).ToList();

        if (retrievedSubjects.Count != requestModel.SubjectIds.Count)
        {
            HashSet<int> retrievedIds = retrievedSubjects.Select(s => s.SubjectId).ToHashSet();
            IEnumerable<int> missingIds = requestModel.SubjectIds.Where(id => !retrievedIds.Contains(id));
            return BadRequest($"Subject with IDs {string.Join(", ", missingIds)} not found.");
        }

        Dictionary<int, Core.Models.Subject> subjectMap = retrievedSubjects.ToDictionary(s => s.SubjectId);
        subjects.AddRange(requestModel.SubjectIds.Select(id => subjectMap[id]));

        student.AccountStatus = requestModel.AccountStatus;
        student.Name = requestModel.Name;
        student.Email = requestModel.Email;
        student.Subjects = subjects.AsReadOnly();

        await userRepository.UpdateUserAsync(student);
        await studentRepository.UpdateStudentAsync(student);

        return Ok();
    }
}
