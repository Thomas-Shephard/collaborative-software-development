using Jahoot.Core.Models;

namespace Jahoot.WebApi.Models.Responses;

public class StudentResponseDto
{
    public int UserId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<Role> Roles { get; init; }
    public DateTime? LastLogin { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int StudentId { get; init; }
    public required StudentAccountStatus AccountStatus { get; init; }

    public static StudentResponseDto FromStudent(Student student)
    {
        return new StudentResponseDto
        {
            UserId = student.UserId,
            Email = student.Email,
            Name = student.Name,
            Roles = student.Roles,
            LastLogin = student.LastLogin,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            StudentId = student.StudentId,
            AccountStatus = student.AccountStatus
        };
    }
}
