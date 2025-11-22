using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class UserRepository(IDbConnection connection) : IUserRepository
{
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        const string query = """
                             SELECT user.*, lecturer.is_admin, student.student_id
                             FROM User user
                             LEFT JOIN Lecturer lecturer ON user.user_id = lecturer.user_id
                             LEFT JOIN Student student ON user.user_id = student.user_id
                             WHERE user.email = @Email
                             """;

        IEnumerable<User> user = await connection.QueryAsync<User, bool?, int?, User>(
                                                                                      query,
                                                                                      (user, isAdmin, studentId) =>
                                                                                      {
                                                                                          if (isAdmin.HasValue)
                                                                                          {
                                                                                              user.Roles.Add(Role.Lecturer);
                                                                                              if (isAdmin.Value)
                                                                                              {
                                                                                                  user.Roles.Add(Role.Admin);
                                                                                              }
                                                                                          }

                                                                                          if (studentId.HasValue)
                                                                                          {
                                                                                              user.Roles.Add(Role.Student);
                                                                                          }

                                                                                          return user;
                                                                                      },
                                                                                      new { Email = email },
                                                                                      splitOn: "is_admin,student_id");

        return user.SingleOrDefault();
    }

    public async Task UpdateUserAsync(User user)
    {
        await connection.ExecuteAsync("UPDATE User SET email = @Email, name = @Name, password_hash = @PasswordHash, last_login = @LastLogin WHERE user_id = @Id", user);
    }
}
