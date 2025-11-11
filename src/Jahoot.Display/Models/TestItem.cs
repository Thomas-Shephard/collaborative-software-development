using System;

namespace Jahoot.Display.Models
{
    public class TestItem
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double? Score { get; set; } // null for upcoming
        public string Course { get; set; } = string.Empty;
    }
}
