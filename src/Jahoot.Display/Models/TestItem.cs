namespace Jahoot.Display.Models
{
    /// <summary>
    /// Represents a test item for display in lists and views.
    /// Used by TestListView control and various dashboards.
    /// </summary>
    public class TestItem
    {
        public required string Icon { get; set; }
        public required string TestName { get; set; }
        public required string SubjectName { get; set; }
        public required string DateInfo { get; set; }
        public required string StatusOrScore { get; set; }
        public required string StatusLabel { get; set; }
    }
}
