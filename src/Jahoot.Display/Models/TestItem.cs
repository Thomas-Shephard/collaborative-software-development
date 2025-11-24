using System;

namespace Jahoot.Display.Models
{
    public class TestItem
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? DueDate { get; set; }
        public double? Score { get; set; } // null for upcoming
        public string Course { get; set; } = string.Empty;
        public string TestInfo { get; set; } = string.Empty; // e.g., "10 questions • 30 mins"
        public int QuestionCount { get; set; }
        public double? PercentageCorrect { get; set; }
        public int? TotalPoints { get; set; }
    }
}
