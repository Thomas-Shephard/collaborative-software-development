using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.Controls;

public partial class HeaderControl : UserControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(HeaderControl), new PropertyMetadata(string.Empty));

    public string Header
    {
        get { return (string)GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }

    public static readonly DependencyProperty SubheaderProperty =
        DependencyProperty.Register("Subheader", typeof(string), typeof(HeaderControl), new PropertyMetadata(string.Empty));

    public string Subheader
    {
        get { return (string)GetValue(SubheaderProperty); }
        set { SetValue(SubheaderProperty, value); }
    }

    public HeaderControl()
    {
        InitializeComponent();
    }
}
