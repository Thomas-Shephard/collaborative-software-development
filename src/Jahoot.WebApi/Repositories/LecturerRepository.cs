using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class LecturerRepository(IDbConnection connection, IUserRepository userRepository) : ILecturerRepository
{
    private sealed class LecturerData
    {
        public int UserId { get; init; }
        public required string Email { get; init; }
        public required string Name { get; init; }
        public required string PasswordHash { get; init; }
        public bool IsDisabled { get; init; }
        public DateTime? LastLogin { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int LecturerId { get; init; }
        public bool IsAdmin { get; init; }
    }

    private const string CreateLecturerQuery = "INSERT INTO Lecturer (user_id, is_admin) VALUES (@UserId, @IsAdmin);";

    public async Task CreateLecturerAsync(string name, string email, string hashedPassword, bool isAdmin)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using IDbTransaction transaction = connection.BeginTransaction();

        const string createUserQuery = "INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @HashedPassword); SELECT LAST_INSERT_ID();";
        int userId = await connection.ExecuteScalarAsync<int>(createUserQuery, new { Name = name, Email = email, HashedPassword = hashedPassword }, transaction);

        await connection.ExecuteAsync(CreateLecturerQuery, new { UserId = userId, IsAdmin = isAdmin }, transaction);

        transaction.Commit();
    }

    public async Task CreateLecturerAsync(int userId, bool isAdmin)
    {
        await connection.ExecuteAsync(CreateLecturerQuery, new { UserId = userId, IsAdmin = isAdmin });
    }

    public async Task<Lecturer?> GetLecturerByUserIdAsync(int userId)
    {
        const string lecturerQuery = """
                                     SELECT user.*, lecturer.*
                                     FROM Lecturer lecturer
                                              JOIN User user ON lecturer.user_id = user.user_id
                                     WHERE lecturer.user_id = @UserId
                                     """;

        LecturerData? lecturerData = await connection.QuerySingleOrDefaultAsync<LecturerData>(lecturerQuery, new { UserId = userId });

        if (lecturerData is null)
        {
            return null;
        }

        List<Role> roles = await userRepository.GetRolesByUserIdAsync(lecturerData.UserId);
        return MapLecturer(lecturerData, roles);
    }

    public async Task<IEnumerable<Lecturer>> GetLecturersAsync()
    {
        const string lecturerQuery = """
                                     SELECT user.*, lecturer.*
                                     FROM Lecturer lecturer
                                              JOIN User user ON lecturer.user_id = user.user_id
                                     """;

        IEnumerable<LecturerData> lecturersData = (await connection.QueryAsync<LecturerData>(lecturerQuery)).ToList();
        
        if (!lecturersData.Any())
        {
            return [];
        }

        Dictionary<int, List<Role>> rolesByUserId = await userRepository.GetRolesByUserIdsAsync(lecturersData.Select(x => x.UserId));
        
        return lecturersData.Select(lecturerData => 
            MapLecturer(lecturerData, rolesByUserId.GetValueOrDefault(lecturerData.UserId, []))).ToList();
    }

    public async Task UpdateLecturerAsync(Lecturer lecturer)
    {
        const string query = "UPDATE Lecturer SET is_admin = @IsAdmin WHERE lecturer_id = @LecturerId";

        await connection.ExecuteAsync(query, new { lecturer.IsAdmin, lecturer.LecturerId });
    }

    private static Lecturer MapLecturer(LecturerData data, List<Role> roles)
    {
        return new Lecturer
        {
            UserId = data.UserId,
            Email = data.Email,
            Name = data.Name,
            PasswordHash = data.PasswordHash,
            IsDisabled = data.IsDisabled,
            Roles = roles,
            LastLogin = data.LastLogin,
            CreatedAt = data.CreatedAt,
            UpdatedAt = data.UpdatedAt,
            LecturerId = data.LecturerId,
            IsAdmin = data.IsAdmin
        };
    }
}
