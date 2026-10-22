using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BtrReturGudang.Wpf.Mvvm
{
    /// <summary>
    /// Asynchronous RelayCommand implementation for WPF ICommand binding.
    /// Manages execution state (IsExecuting) to prevent accidental concurrent re-entrancy.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeWithParam;
        private readonly Predicate<object> _canExecuteWithParam;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _executeWithParam = _ => execute();
            _canExecuteWithParam = canExecute != null ? _ => canExecute() : (Predicate<object>)null;
        }

        public AsyncRelayCommand(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecuteWithParam = canExecute;
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (_isExecuting != value)
                {
                    _isExecuting = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return !IsExecuting && (_canExecuteWithParam == null || _canExecuteWithParam(parameter));
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                IsExecuting = true;
                await _executeWithParam(parameter).ConfigureAwait(true);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Generic Asynchronous RelayCommand strongly typed to parameter type T.
    /// </summary>
    /// <typeparam name="T">Parameter type.</typeparam>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Predicate<T> _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (_isExecuting != value)
                {
                    _isExecuting = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            if (IsExecuting) return false;
            if (_canExecute == null) return true;

            if (parameter == null && typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                return false;
            }

            return _canExecute((T)parameter);
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync((T)parameter);
        }

        public async Task ExecuteAsync(T parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                IsExecuting = true;
                await _execute(parameter).ConfigureAwait(true);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
