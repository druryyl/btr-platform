using System;

namespace BtrReturGudang.Wpf.Services
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class StatusNotificationEventArgs : EventArgs
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public int DurationMs { get; set; }

        public StatusNotificationEventArgs(string message, NotificationType type = NotificationType.Info, int durationMs = 3500)
        {
            Message = message;
            Type = type;
            DurationMs = durationMs;
        }
    }

    /// <summary>
    /// Service for decoupled application status and non-intrusive notification dispatch.
    /// Used to communicate state changes from child views/viewmodels to the main window status bar and toast overlays.
    /// </summary>
    public interface IStatusNotificationService
    {
        event EventHandler<StatusNotificationEventArgs> NotificationReceived;
        void ShowNotification(string message, NotificationType type = NotificationType.Info, int durationMs = 3500);
    }

    public class StatusNotificationService : IStatusNotificationService
    {
        public event EventHandler<StatusNotificationEventArgs> NotificationReceived;

        public void ShowNotification(string message, NotificationType type = NotificationType.Info, int durationMs = 3500)
        {
            NotificationReceived?.Invoke(this, new StatusNotificationEventArgs(message, type, durationMs));
        }
    }
}
