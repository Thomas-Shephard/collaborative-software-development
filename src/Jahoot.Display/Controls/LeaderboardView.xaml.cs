using System.Windows.Controls;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.Controls;

public partial class LeaderboardView : UserControl
{
    public LeaderboardView()
    {
        InitializeComponent();
        DataContext = new LeaderboardViewModel();
    }
}
