using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SaludTotal.Commands
{
    public class RelayCommand : ICommand

    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action<object> _Execute;
        private readonly Predicate<object> _CanExecute;
        private readonly List<string> _dependentProperties = new();

        public RelayCommand(Action<object> ExecuteMethod, Predicate<object> CanExecuteMethod, INotifyPropertyChanged? notifier = null,
            params string[] dependentProperties)
        {
            _Execute = ExecuteMethod;
            _CanExecute = CanExecuteMethod;

            if (notifier != null && dependentProperties != null && dependentProperties.Length > 0)
            {
                _dependentProperties.AddRange(dependentProperties);
                notifier.PropertyChanged += (s, e) =>
                {
                    if (_dependentProperties.Contains(e.PropertyName))
                        RaiseCanExecuteChanged();
                };
            }
        }

        public bool CanExecute(object? parameter)
        {
            return _CanExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _Execute(parameter);
        }

        public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
