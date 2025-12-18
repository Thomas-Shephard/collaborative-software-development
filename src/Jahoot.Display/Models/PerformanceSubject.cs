namespace Jahoot.Display.Models
{
    /// <summary>
    /// Represents performance data for a subject.
    /// </summary>
    public class PerformanceSubject
    {
        public required string SubjectName { get; set; }
        public required string ScoreText { get; set; }
        public required double ScoreValue { get; set; }
    }
}
