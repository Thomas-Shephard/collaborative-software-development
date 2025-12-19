namespace Jahoot.Display.Models
{
    /// <summary>
    /// Represents a recent activity item for display in the lecturer dashboard.
    /// </summary>
    public class RecentActivityItem
    {
        public required string StudentInitials { get; set; }
        public required string DescriptionPrefix { get; set; }
        public required string TestName { get; set; }
        public required string TimeAgo { get; set; }
        public required string Result { get; set; }
    }
}
