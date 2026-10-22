using System;
using System.Windows.Input;

namespace BtrReturGudang.Wpf.Mvvm
{
    /// <summary>
    /// Lightweight RelayCommand implementation for WPF ICommand binding.
    /// Supports parameterless and parameterized actions with optional execution predicates.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executeWithParam;
        private readonly Predicate<object> _canExecuteWithParam;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _executeWithParam = _ => execute();
            _canExecuteWithParam = canExecute != null ? _ => canExecute() : (Predicate<object>)null;
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecuteWithParam = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteWithParam == null || _canExecuteWithParam(parameter);
        }

        public void Execute(object parameter)
        {
            _executeWithParam(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Generic RelayCommand strongly typed to parameter type T.
    /// </summary>
    /// <typeparam name="T">Parameter type.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;

            if (parameter == null && typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                return false;
            }

            return _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
