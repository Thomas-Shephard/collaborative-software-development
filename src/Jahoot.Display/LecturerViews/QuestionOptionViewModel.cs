using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jahoot.Display.LecturerViews
{
    public class QuestionOptionViewModel : INotifyPropertyChanged
    {
        private string _optionText = string.Empty;
        public string OptionText
        {
            get => _optionText;
            set
            {
                _optionText = value;
                OnPropertyChanged();
            }
        }

        private bool _isCorrect;
        public bool IsCorrect
        {
            get => _isCorrect;
            set
            {
                _isCorrect = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
