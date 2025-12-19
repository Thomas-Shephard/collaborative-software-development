using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jahoot.Display.LecturerViews
{
    public class QuestionOptionViewModel : INotifyPropertyChanged
    {
        private readonly QuestionViewModel _parent;

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
                if (_isCorrect)
                {
                    _parent.SetSelectedOption(this);
                }
            }
        }

        public QuestionOptionViewModel(QuestionViewModel parent)
        {
            _parent = parent;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void SetIsCorrect(bool value)
        {
            _isCorrect = value;
            OnPropertyChanged(nameof(IsCorrect));
        }
    }
}
