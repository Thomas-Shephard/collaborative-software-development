using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.Views;

/// <summary>
/// Interaction logic for StudentDashboard.xaml
/// </summary>
public partial class StudentDashboard : UserControl
{
    public StudentDashboard()
    {
        InitializeComponent();
        var vm = new StudentDashboardViewModel();
        DataContext = vm;
        Loaded += StudentDashboard_Loaded;
    }

    private void StudentDashboard_Loaded(object? sender, RoutedEventArgs e)
    {
        DrawGradesPlot();
    }

    private void DrawGradesPlot()
    {
        if (DataContext is not StudentDashboardViewModel vm) return;

        var scores = vm.CompletedTests.Where(t => t.Score.HasValue).OrderBy(t => t.Date).ToList();
        GradesCanvas.Children.Clear();
        if (!scores.Any()) return;

        double w = GradesCanvas.ActualWidth;
        double h = GradesCanvas.ActualHeight;
        if (w <= 0) w = GradesCanvas.Width = 400; // fallback
        if (h <= 0) h = GradesCanvas.Height = 140;

        double minScore = scores.Min(s => s.Score!.Value);
        double maxScore = scores.Max(s => s.Score!.Value);
        if (Math.Abs(maxScore - minScore) < 0.1)
        {
            minScore -= 5; maxScore += 5;
        }

        var poly = new Polyline { Stroke = new SolidColorBrush(Color.FromRgb(42,122,226)), StrokeThickness = 2 };

        for (int i = 0; i < scores.Count; i++)
        {
            double x = (i / (double)(scores.Count - 1)) * (w - 20) + 10;
            double normalized = (scores[i].Score!.Value - minScore) / (maxScore - minScore);
            double y = (1 - normalized) * (h - 20) + 10;
            poly.Points.Add(new Point(x, y));

            // draw point
            var dot = new Ellipse { Width = 6, Height = 6, Fill = Brushes.White, Stroke = poly.Stroke, StrokeThickness = 2 };
            Canvas.SetLeft(dot, x - 3);
            Canvas.SetTop(dot, y - 3);
            GradesCanvas.Children.Add(dot);
        }

        GradesCanvas.Children.Add(poly);
    }
}
