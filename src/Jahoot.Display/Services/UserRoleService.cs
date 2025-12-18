using Jahoot.Core.Models;
using System.Collections.Generic;

namespace Jahoot.Display.Services;

/// <summary>
/// Service for managing user roles and access control
/// </summary>
public interface IUserRoleService
{
    /// <summary>
    /// Sets the current user's roles after login
    /// </summary>
    void SetUserRoles(List<Role> roles);
    
    /// <summary>
    /// Gets the available dashboards based on user's roles
    /// </summary>
    List<string> GetAvailableDashboards();
    
    /// <summary>
    /// Checks if user has access to a specific dashboard
    /// </summary>
    bool HasAccessToDashboard(string dashboardName);
    
    /// <summary>
    /// Gets all user roles
    /// </summary>
    List<Role> GetUserRoles();
    
    /// <summary>
    /// Clears user roles on logout
    /// </summary>
    void ClearRoles();
}

/// <summary>
/// Implementation of user role management service
/// </summary>
public class UserRoleService : IUserRoleService
{
    private List<Role> _userRoles = new();

    public void SetUserRoles(List<Role> roles)
    {
        _userRoles = roles ?? new List<Role>();
    }

    public List<string> GetAvailableDashboards()
    {
        var dashboards = new List<string>();

        foreach (var role in _userRoles)
        {
            switch (role)
            {
                case Role.Student:
                    if (!dashboards.Contains("Student"))
                        dashboards.Add("Student");
                    break;
                case Role.Lecturer:
                    if (!dashboards.Contains("Lecturer"))
                        dashboards.Add("Lecturer");
                    break;
                case Role.Admin:
                    if (!dashboards.Contains("Admin"))
                        dashboards.Add("Admin");
                    break;
            }
        }

        return dashboards;
    }

    public bool HasAccessToDashboard(string dashboardName)
    {
        return dashboardName switch
        {
            "Student" => _userRoles.Contains(Role.Student),
            "Lecturer" => _userRoles.Contains(Role.Lecturer),
            "Admin" => _userRoles.Contains(Role.Admin),
            _ => false
        };
    }

    public List<Role> GetUserRoles()
    {
        return new List<Role>(_userRoles);
    }

    public void ClearRoles()
    {
        _userRoles.Clear();
    }
}
