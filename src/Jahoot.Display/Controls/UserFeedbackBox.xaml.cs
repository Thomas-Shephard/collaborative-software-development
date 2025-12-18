using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Jahoot.Display.Controls
{
    public partial class UserFeedbackBox : UserControl
    {
        private static readonly SolidColorBrush SuccessBackgroundBrush = CreateFrozenBrush("#D4EDDA");
        private static readonly SolidColorBrush SuccessBorderBrush = CreateFrozenBrush("#C3E6CB");
        private static readonly SolidColorBrush SuccessTextBrush = CreateFrozenBrush("#155724");

        private static readonly SolidColorBrush ErrorBackgroundBrush = CreateFrozenBrush("#F8D7DA");
        private static readonly SolidColorBrush ErrorBorderBrush = CreateFrozenBrush("#F5C6CB");
        private static readonly SolidColorBrush ErrorTextBrush = CreateFrozenBrush("#721C24");

        private static SolidColorBrush CreateFrozenBrush(string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }

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
                ContainerBorder.Background = SuccessBackgroundBrush;
                ContainerBorder.BorderBrush = SuccessBorderBrush;
                MessageText.Foreground = SuccessTextBrush;
                IconText.Foreground = SuccessTextBrush;
                IconText.Text = "✓";
            }
            else
            {
                // Error Styles
                ContainerBorder.Background = ErrorBackgroundBrush;
                ContainerBorder.BorderBrush = ErrorBorderBrush;
                MessageText.Foreground = ErrorTextBrush;
                IconText.Foreground = ErrorTextBrush;
                IconText.Text = "⚠";
            }
        }
    }
}