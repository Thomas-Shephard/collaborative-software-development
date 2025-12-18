using System.Collections.Generic;
using System.Windows;
using Jahoot.Core.Models;

namespace Jahoot.Display.Services
{
    /// <summary>
    /// Interface for dashboard navigation service.
    /// </summary>
    public interface IDashboardNavigationService
    {
        /// <summary>
        /// Navigates to the appropriate dashboard based on user's roles.
        /// Determines the primary role from the list and navigates accordingly.
        /// </summary>
        /// <param name="userRoles">List of roles the user has</param>
        /// <param name="currentWindow">The window to close after navigation</param>
        /// <returns>True if navigation was successful, false otherwise</returns>
        bool NavigateToDashboardByRoles(List<Role> userRoles, Window currentWindow);

        /// <summary>
        /// Navigates to the specified dashboard role, reusing cached instances when possible.
        /// </summary>
        /// <param name="role">The role name (e.g., "Student", "Lecturer")</param>
        /// <param name="currentWindow">The window to close after navigation</param>
        /// <returns>True if navigation was successful, false otherwise</returns>
        bool NavigateToDashboard(string role, Window currentWindow);

        /// <summary>
        /// Clears all cached dashboard instances.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Gets the count of cached dashboard instances (useful for testing).
        /// </summary>
        int CachedDashboardCount { get; }
    }
}
