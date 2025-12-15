namespace Jahoot.Core.Models;

public class Student : User
{
    public int StudentId { get; init; }
    private StudentAccountStatus _accountStatus;
    public required StudentAccountStatus AccountStatus
    {
        get => _accountStatus;
        set => SetProperty(ref _accountStatus, value);
    }
}
