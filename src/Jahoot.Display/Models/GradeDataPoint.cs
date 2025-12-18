namespace Jahoot.Display.Models
{
    /// <summary>
    /// Represents a single data point for the grades chart.
    /// TODO: Use this model to replace hardcoded chart data in GradesChartView.
    /// </summary>
    public class GradeDataPoint
    {
        public required string TestName { get; set; }
        public required double Score { get; set; }
        public DateTime TestDate { get; set; }
    }
}
