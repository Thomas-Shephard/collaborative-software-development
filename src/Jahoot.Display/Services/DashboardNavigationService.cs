using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Jahoot.Core.Models;

namespace Jahoot.Display.Services
{
    /// <summary>
    /// Service responsible for managing dashboard navigation and caching dashboard instances
    /// to avoid unnecessary object creation during role switching.
    /// </summary>
    public class DashboardNavigationService : IDashboardNavigationService
    {
        private readonly Dictionary<string, Window> _dashboardCache = new();
        private readonly Dictionary<Window, bool> _windowClosedFlags = new();
        private readonly IServiceProvider _serviceProvider;

        public DashboardNavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Navigates to the appropriate dashboard based on user's roles.
        /// Determines the primary role from the list and navigates accordingly.
        /// </summary>
        /// <param name="userRoles">List of roles the user has</param>
        /// <param name="currentWindow">The window to close after navigation</param>
        /// <returns>True if navigation was successful, false otherwise</returns>
        public bool NavigateToDashboardByRoles(List<Role> userRoles, Window currentWindow)
        {
            string targetRole = DetermineTargetRoleFromRoles(userRoles);
            return NavigateToDashboard(targetRole, currentWindow);
        }

        /// <summary>
        /// Determines which dashboard to navigate to based on user's roles.
        /// Priority order: Admin -> Lecturer -> Student
        /// </summary>
        private string DetermineTargetRoleFromRoles(List<Role> roles)
        {
            // Priority order: Admin -> Lecturer -> Student
            if (roles.Contains(Role.Admin))
            {
                return "Admin";
            }
            else if (roles.Contains(Role.Lecturer))
            {
                return "Lecturer";
            }
            else if (roles.Contains(Role.Student))
            {
                return "Student";
            }
            
            // Default to Student if no roles found
            return "Student";
        }

        /// <summary>
        /// Navigates to the specified dashboard role, reusing cached instances when possible.
        /// Only allows navigation to roles the user actually has access to.
        /// </summary>
        /// <param name="role">The role name (e.g., "Student", "Lecturer", "Admin")</param>
        /// <param name="currentWindow">The window to close after navigation</param>
        /// <returns>True if navigation was successful, false otherwise</returns>
        public bool NavigateToDashboard(string role, Window currentWindow)
        {
            if (currentWindow == null || !currentWindow.IsLoaded)
                return false;

            var dashboard = GetOrCreateDashboard(role, currentWindow);
            
            if (dashboard == null || dashboard == currentWindow)
                return false;

            if (!_windowClosedFlags.ContainsKey(dashboard))
            {
                _windowClosedFlags[dashboard] = false;
                dashboard.Closed += (s, e) =>
                {
                    _windowClosedFlags[dashboard] = true;
                    OnDashboardClosed(role);
                };
            }

            dashboard.Show();
            
            RemoveFromCacheByWindow(currentWindow);
            
            currentWindow.Close();
            
            return true;
        }

        /// <summary>
        /// Gets an existing dashboard from cache or creates a new one.
        /// </summary>
        private Window? GetOrCreateDashboard(string role, Window? currentWindow = null)
        {
            // Check if we already have a cached instance
            if (_dashboardCache.TryGetValue(role, out var cachedDashboard))
            {
                // Don't return the cached dashboard if it's the current window
                if (cachedDashboard == currentWindow)
                {
                    _dashboardCache.Remove(role);
                }
                // Verify the cached window is still valid
                else if (cachedDashboard != null && _windowClosedFlags.TryGetValue(cachedDashboard, out var isClosed) && isClosed)
                {
                    // Window was closed externally, remove from cache
                    _dashboardCache.Remove(role);
                    _windowClosedFlags.Remove(cachedDashboard);
                }
                else
                {
                    return cachedDashboard;
                }
            }

            // Create a new dashboard instance based on role
            var dashboard = CreateDashboard(role);
            
            if (dashboard != null)
            {
                _dashboardCache[role] = dashboard;
                _windowClosedFlags[dashboard] = false;
            }

            return dashboard;
        }

        /// <summary>
        /// Creates a new dashboard instance for the specified role.
        /// </summary>
        private Window? CreateDashboard(string role)
        {
            return role switch
            {
                "Student" => _serviceProvider.GetService(typeof(StudentViews.StudentDashboard)) as Window,
                "Lecturer" => _serviceProvider.GetService(typeof(LecturerViews.LecturerDashboard)) as Window,
                "Admin" => _serviceProvider.GetService(typeof(Pages.AdminDashboard)) as Window,
                _ => null
            };
        }

        /// <summary>
        /// Handles dashboard closed event to remove it from cache.
        /// </summary>
        private void OnDashboardClosed(string role)
        {
            if (_dashboardCache.TryGetValue(role, out var window))
            {
                _windowClosedFlags.Remove(window);
                _dashboardCache.Remove(role);
            }
        }

        /// <summary>
        /// Removes a window from cache by its instance.
        /// </summary>
        private void RemoveFromCacheByWindow(Window window)
        {
            string? keyToRemove = null;
            foreach (var kvp in _dashboardCache)
            {
                if (kvp.Value == window)
                {
                    keyToRemove = kvp.Key;
                    break;
                }
            }

            if (keyToRemove != null)
            {
                _windowClosedFlags.Remove(window);
                _dashboardCache.Remove(keyToRemove);
            }
        }

        /// <summary>
        /// Clears all cached dashboard instances.
        /// </summary>
        public void ClearCache()
        {
            _windowClosedFlags.Clear();
            _dashboardCache.Clear();
        }

        /// <summary>
        /// Gets the count of cached dashboard instances (useful for testing).
        /// </summary>
        public int CachedDashboardCount => _dashboardCache.Count;
    }
}
