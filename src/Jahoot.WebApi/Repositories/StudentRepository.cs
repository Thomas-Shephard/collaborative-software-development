using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class StudentRepository(IDbConnection connection) : IStudentRepository
{
    private const string CreateStudentQuery = "INSERT INTO Student (user_id) VALUES (@UserId);";

    private sealed class StudentData
    {
        public int UserId { get; init; }
        public required string Email { get; init; }
        public required string Name { get; init; }
        public required string PasswordHash { get; init; }
        public DateTime? LastLogin { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int StudentId { get; init; }
        public required string AccountStatus { get; init; }
    }

    public async Task CreateStudentAsync(string name, string email, string hashedPassword)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using IDbTransaction transaction = connection.BeginTransaction();

        const string createUserQuery = "INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @HashedPassword); SELECT LAST_INSERT_ID();";
        int userId = await connection.ExecuteScalarAsync<int>(createUserQuery, new { Name = name, Email = email, HashedPassword = hashedPassword }, transaction);

        await connection.ExecuteAsync(CreateStudentQuery, new { UserId = userId }, transaction);

        transaction.Commit();
    }

    public async Task CreateStudentAsync(int userId)
    {
        await connection.ExecuteAsync(CreateStudentQuery, new { UserId = userId });
    }

    public async Task<Student?> GetStudentByUserIdAsync(int userId)
    {
        const string studentQuery = """
                             SELECT user.*, student.*, lecturer.is_admin
                             FROM Student student
                                      JOIN User user ON student.user_id = user.user_id
                                      LEFT JOIN Lecturer lecturer ON user.user_id = lecturer.user_id
                             WHERE student.user_id = @UserId
                             """;

        Student? student = (await connection.QueryAsync<StudentData, bool?, Student>(studentQuery, MapStudent, new { UserId = userId }, splitOn: "is_admin"))
            .SingleOrDefault();

        if (student is null)
        {
            return null;
        }

        const string subjectQuery = """
                                    SELECT subject.*
                                    FROM Subject subject
                                             JOIN StudentSubject student_subject ON subject.subject_id = student_subject.subject_id
                                    WHERE student_subject.student_id = @StudentId
                                    """;

        IEnumerable<Subject> subjects = await connection.QueryAsync<Subject>(subjectQuery, new { student.StudentId });
        student.Subjects = subjects.ToList();

        return student;
    }

    public async Task<IEnumerable<Student>> GetStudentsByStatusAsync(StudentAccountStatus status)
    {
        string statusString = status switch
        {
            StudentAccountStatus.PendingApproval => "pending_approval",
            StudentAccountStatus.Active => "active",
            StudentAccountStatus.Disabled => "disabled",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

        const string studentQuery = """
                             SELECT user.*, student.*, lecturer.is_admin
                             FROM Student student
                                      JOIN User user ON student.user_id = user.user_id
                                      LEFT JOIN Lecturer lecturer ON user.user_id = lecturer.user_id
                             WHERE student.account_status = @Status
                             """;

        IEnumerable<Student> students = await connection.QueryAsync<StudentData, bool?, Student>(studentQuery, MapStudent, new { Status = statusString }, splitOn: "is_admin");
        List<Student> studentList = students.ToList();

        if (studentList.Count == 0)
        {
            return studentList;
        }

        const string subjectQuery = """
                                    SELECT student_subject.student_id, subject.*
                                    FROM Subject subject
                                             JOIN StudentSubject student_subject ON subject.subject_id = student_subject.subject_id
                                    WHERE student_subject.student_id IN @StudentIds
                                    """;

        IEnumerable<(int StudentId, Subject Subject)> subjects = await connection.QueryAsync<int, Subject, (int StudentId, Subject Subject)>(
            subjectQuery,
            (studentId, subject) => (studentId, subject),
            new { StudentIds = studentList.Select(s => s.StudentId) },
            splitOn: "subject_id");

        Dictionary<int, List<Subject>> subjectsByStudent = subjects.GroupBy(x => x.StudentId)
                                                                   .ToDictionary(g => g.Key, g => g.Select(x => x.Subject).ToList());

        foreach (Student student in studentList.Where(student => subjectsByStudent.ContainsKey(student.StudentId)))
        {
            student.Subjects = subjectsByStudent[student.StudentId];
        }

        return studentList;
    }

    public async Task UpdateStudentAsync(Student student)
    {
        using IDbTransaction transaction = connection.BeginTransaction();

        try
        {
            string statusString = student.AccountStatus switch
            {
                StudentAccountStatus.PendingApproval => "pending_approval",
                StudentAccountStatus.Active => "active",
                StudentAccountStatus.Disabled => "disabled",
                _ => throw new ArgumentOutOfRangeException(nameof(student), student.AccountStatus, null)
            };

            const string updateStudentQuery = "UPDATE Student SET account_status = @AccountStatus WHERE student_id = @StudentId";
            await connection.ExecuteAsync(updateStudentQuery, new { AccountStatus = statusString, student.StudentId }, transaction);

            const string deleteSubjectsQuery = "DELETE FROM StudentSubject WHERE student_id = @StudentId";
            await connection.ExecuteAsync(deleteSubjectsQuery, new { student.StudentId }, transaction);

            if (student.Subjects.Any())
            {
                const string insertSubjectsQuery = "INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)";
                await connection.ExecuteAsync(insertSubjectsQuery, student.Subjects.Select(s => new { student.StudentId, s.SubjectId }), transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static Student MapStudent(StudentData studentData, bool? isAdmin)
    {
        List<Role> roles = [Role.Student];

        if (isAdmin.HasValue)
        {
            roles.Add(Role.Lecturer);
            if (isAdmin.Value)
            {
                roles.Add(Role.Admin);
            }
        }

        // Map database enum string to C# enum
        // The database enum values are 'pending_approval', 'active', 'disabled'
        // The C# enum values are PendingApproval, Active, Disabled
        StudentAccountStatus status = studentData.AccountStatus switch
        {
            "pending_approval" => StudentAccountStatus.PendingApproval,
            "active" => StudentAccountStatus.Active,
            "disabled" => StudentAccountStatus.Disabled,
            _ => throw new InvalidOperationException($"Unknown account status: {studentData.AccountStatus}")
        };

        return new Student
        {
            UserId = studentData.UserId,
            Email = studentData.Email,
            Name = studentData.Name,
            PasswordHash = studentData.PasswordHash,
            Roles = roles,
            LastLogin = studentData.LastLogin,
            CreatedAt = studentData.CreatedAt,
            UpdatedAt = studentData.UpdatedAt,
            StudentId = studentData.StudentId,
            AccountStatus = status,
            Subjects = []
        };
    }
}
