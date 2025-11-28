namespace Jahoot.WebApi.Repositories;

public interface IStudentRepository
{
    Task CreateStudentAsync(string name, string email, string hashedPassword);
}
