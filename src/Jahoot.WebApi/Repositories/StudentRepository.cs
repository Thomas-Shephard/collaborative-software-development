using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class StudentRepository(IDbConnection connection, IUserRepository userRepository) : IStudentRepository
{
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
        public bool IsApproved { get; init; }
        public bool IsDisabled { get; init; }
    }

    private const string CreateStudentQuery = "INSERT INTO Student (user_id) VALUES (@UserId);";

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
                             SELECT user.*, student.*
                             FROM Student student
                                      JOIN User user ON student.user_id = user.user_id
                             WHERE student.user_id = @UserId
                             """;

        StudentData? studentData = await connection.QuerySingleOrDefaultAsync<StudentData>(studentQuery, new { UserId = userId });

        if (studentData is null)
        {
            return null;
        }

        List<Role> roles = await userRepository.GetRolesByUserIdAsync(studentData.UserId);
        Student student = MapStudent(studentData, roles);

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

    public async Task<IEnumerable<Student>> GetStudentsByApprovalStatusAsync(bool isApproved)
    {
        const string studentQuery = """
                                    SELECT user.*, student.*
                                    FROM Student student
                                             JOIN User user ON student.user_id = user.user_id
                                    WHERE student.is_approved = @IsApproved
                                    """;

        IEnumerable<StudentData> studentsData = await connection.QueryAsync<StudentData>(studentQuery, new { IsApproved = isApproved });
        List<Student> studentList = [];

        foreach (StudentData studentData in studentsData)
        {
            List<Role> roles = await userRepository.GetRolesByUserIdAsync(studentData.UserId);
            studentList.Add(MapStudent(studentData, roles));
        }

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
            const string updateStudentQuery = "UPDATE Student SET is_approved = @IsApproved WHERE student_id = @StudentId";
            await connection.ExecuteAsync(updateStudentQuery, student, transaction);

            const string deleteSubjectsQuery = "DELETE FROM StudentSubject WHERE student_id = @StudentId";
            await connection.ExecuteAsync(deleteSubjectsQuery, student, transaction);

            if (student.Subjects.Any())
            {
                const string insertSubjectsQuery = "INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)";
                await connection.ExecuteAsync(insertSubjectsQuery, student.Subjects.Select(subject => new { student.StudentId, subject.SubjectId }), transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static Student MapStudent(StudentData studentData, List<Role> roles)
    {
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
            IsApproved = studentData.IsApproved,
            IsDisabled = studentData.IsDisabled,
            Subjects = []
        };
    }
}
