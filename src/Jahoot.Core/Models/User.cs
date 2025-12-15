using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jahoot.Core.Models;

public class User : INotifyPropertyChanged
{
    public int UserId { get; init; }
    private string _email = null!;
    public required string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _name = null!;
    public required string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public required string PasswordHash { get; set; }
    public required IReadOnlyList<Role> Roles { get; init; }

    private DateTime? _lastLogin;
    public DateTime? LastLogin
    {
        get => _lastLogin;
        set => SetProperty(ref _lastLogin, value);
    }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        field = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }
}
