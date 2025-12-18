using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Jahoot.Display.Controls
{
    public partial class UserFeedbackBox : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(UserFeedbackBox), new PropertyMetadata(string.Empty, OnMessageChanged));

        public static readonly DependencyProperty IsSuccessProperty =
            DependencyProperty.Register("IsSuccess", typeof(bool), typeof(UserFeedbackBox), new PropertyMetadata(false, OnIsSuccessChanged));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public bool IsSuccess
        {
            get { return (bool)GetValue(IsSuccessProperty); }
            set { SetValue(IsSuccessProperty, value); }
        }

        public UserFeedbackBox()
        {
            InitializeComponent();
            UpdateVisuals();
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UserFeedbackBox)d;
            control.MessageText.Text = (string)e.NewValue;
            control.Visibility = string.IsNullOrEmpty(control.Message) ? Visibility.Collapsed : Visibility.Visible;
        }

        private static void OnIsSuccessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UserFeedbackBox)d;
            control.UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (IsSuccess)
            {
                // Success Styles
                ContainerBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4EDDA"));
                ContainerBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C3E6CB"));
                MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#155724"));
                IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#155724"));
                IconText.Text = "✓";
            }
            else
            {
                // Error Styles
                ContainerBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8D7DA"));
                ContainerBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5C6CB"));
                MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#721C24"));
                IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#721C24"));
                IconText.Text = "⚠";
            }
        }
    }
}