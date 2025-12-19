using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Jahoot.Display.Commands;

namespace Jahoot.Display.LecturerViews
{
    public class QuestionViewModel : INotifyPropertyChanged
    {
        private string _questionText = string.Empty;
        public string QuestionText
        {
            get => _questionText;
            set
            {
                _questionText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<QuestionOptionViewModel> Options { get; set; }

        public ICommand AddOptionCommand { get; }
        public ICommand RemoveOptionCommand { get; }

        public QuestionViewModel()
        {
            Options = new ObservableCollection<QuestionOptionViewModel>();
            AddOptionCommand = new RelayCommand(_ => AddOption());
            RemoveOptionCommand = new RelayCommand(RemoveOption, CanRemoveOption);
        }

        private void AddOption()
        {
            Options.Add(new QuestionOptionViewModel());
        }

        private void RemoveOption(object? obj)
        {
            if (obj is QuestionOptionViewModel option)
            {
                Options.Remove(option);
            }
        }

        private bool CanRemoveOption(object? obj)
        {
            return obj is QuestionOptionViewModel;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
