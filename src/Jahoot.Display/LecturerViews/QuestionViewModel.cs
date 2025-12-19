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

        private ObservableCollection<QuestionOptionViewModel> _options;
        public ObservableCollection<QuestionOptionViewModel> Options
        {
            get => _options;
            set
            {
                _options = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddOptionCommand { get; }
        public ICommand RemoveOptionCommand { get; }

                        public QuestionViewModel()

                        {

                            _options = new ObservableCollection<QuestionOptionViewModel>

                            {

                                new QuestionOptionViewModel(this),

                                new QuestionOptionViewModel(this)

                            };

                            AddOptionCommand = new RelayCommand(_ => AddOption(), _ => CanAddOption());

                            RemoveOptionCommand = new RelayCommand(RemoveOption, CanRemoveOption);

                        }

                

                        private void AddOption()

                        {

                            _options.Add(new QuestionOptionViewModel(this));

                            CommandManager.InvalidateRequerySuggested();

                        }

                

                        private bool CanAddOption()

                        {

                            return _options.Count < 4;

                        }

                

                        private void RemoveOption(object? obj)

                        {

                            if (obj is QuestionOptionViewModel option)

                            {

                                _options.Remove(option);

                                CommandManager.InvalidateRequerySuggested();

                            }

                        }

                

                        private bool CanRemoveOption(object? obj)

                        {

                            return obj is QuestionOptionViewModel && _options.Count > 2;

                        }

                

                        public void SetSelectedOption(QuestionOptionViewModel selectedOption)

                        {

                            foreach (var option in _options)

                            {

                                if (option != selectedOption)

                                {

                                    option.SetIsCorrect(false);

                                }

                            }

                        }

        

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
