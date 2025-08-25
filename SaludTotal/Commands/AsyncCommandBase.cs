using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SaludTotal.Commands
{
    public abstract class AsyncCommandBase : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Func<object?, bool>? _canExecute;
        private readonly HashSet<object?> _executingParameters = new();

        protected AsyncCommandBase(Func<object?, bool>? canExecute = null)
        {
            _canExecute = canExecute;
        }

        private bool IsExecuting(object? parameter)
        {
            lock (_executingParameters)
            {
                return _executingParameters.Contains(parameter);
            }
        }

        public bool CanExecute(object? parameter)
        {
            return !IsExecuting(parameter) && (_canExecute?.Invoke(parameter) ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            lock (_executingParameters)
            {
                _executingParameters.Add(parameter);
            }
            RaiseCanExecuteChanged();

            try
            {
                await ExecuteAsync(parameter);
            }
            finally
            {
                lock (_executingParameters)
                {
                    _executingParameters.Remove(parameter);
                }
                RaiseCanExecuteChanged();
            }
        }

        protected abstract Task ExecuteAsync(object? parameter);

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
