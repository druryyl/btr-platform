using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BtrReturGudang.Wpf.Mvvm
{
    /// <summary>
    /// Lightweight, zero-dependency base class for ViewModels implementing INotifyPropertyChanged.
    /// Provides thread-safe, strongly-typed property change notification.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the field to a new value and raises PropertyChanged if the value changed.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="storage">Reference to the backing field.</param>
        /// <param name="value">New value.</param>
        /// <param name="propertyName">Automatically captured property name.</param>
        /// <returns>True if the value changed, false otherwise.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Sets the field to a new value, invokes the callback action if changed, and raises PropertyChanged.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="storage">Reference to the backing field.</param>
        /// <param name="value">New value.</param>
        /// <param name="onChanged">Callback executed when property changes.</param>
        /// <param name="propertyName">Automatically captured property name.</param>
        /// <returns>True if the value changed, false otherwise.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the PropertyChanged event with explicit event args.
        /// </summary>
        /// <param name="args">Property changed event arguments.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }
}
