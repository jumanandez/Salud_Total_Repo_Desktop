using SaludTotal.Commands;
using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace SaludTotal.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {
        private string _message;
        private DispatcherTimer? _timer;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMessage));

                if (!string.IsNullOrEmpty(_message))
                    StartTimer();
                else
                    StopTimer();
            }
        }

        public bool HasMessage => !string.IsNullOrEmpty(Message);

        public ICommand ClearMessage => new RelayCommand(_ => ClearMessageExecute(), _ => true);

        public void ClearMessageExecute()
        {
            Message = string.Empty;
        }

        private void StartTimer()
        {
            StopTimer();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _timer.Tick += (s, e) =>
            {
                ClearMessageExecute();
                StopTimer();
            };
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }
    }
}
