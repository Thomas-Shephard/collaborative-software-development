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

        // Admin has access to all dashboards
        if (_userRoles.Contains(Role.Admin))
        {
            dashboards.Add("Admin");
            dashboards.Add("Lecturer");
            dashboards.Add("Student");
            return dashboards;
        }

        // Lecturer has access to Lecturer and Student dashboards
        if (_userRoles.Contains(Role.Lecturer))
        {
            dashboards.Add("Lecturer");
            dashboards.Add("Student");
            return dashboards;
        }

        // Student only has access to Student dashboard
        if (_userRoles.Contains(Role.Student))
        {
            dashboards.Add("Student");
        }

        return dashboards;
    }

    public bool HasAccessToDashboard(string dashboardName)
    {
        return dashboardName switch
        {
            // Admin dashboard: only Admin role
            "Admin" => _userRoles.Contains(Role.Admin),
            
            // Lecturer dashboard: Admin or Lecturer roles
            "Lecturer" => _userRoles.Contains(Role.Admin) || _userRoles.Contains(Role.Lecturer),
            
            // Student dashboard: Any role (Admin, Lecturer, or Student)
            "Student" => _userRoles.Contains(Role.Admin) || _userRoles.Contains(Role.Lecturer) || _userRoles.Contains(Role.Student),
            
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
