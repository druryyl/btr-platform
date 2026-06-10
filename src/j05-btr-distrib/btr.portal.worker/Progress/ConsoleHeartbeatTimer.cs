using System;
using System.Threading;

namespace btr.portal.worker.Progress
{
    internal sealed class ConsoleHeartbeatTimer : IDisposable
    {
        private readonly Timer _timer;
        private readonly Func<string> _messageFactory;
        private readonly object _sync = new object();
        private DateTime _stepStartedAt;
        private bool _disposed;

        public ConsoleHeartbeatTimer(TimeSpan interval, Func<string> messageFactory)
        {
            _messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            _stepStartedAt = DateTime.UtcNow;
            _timer = new Timer(OnTick, null, interval, interval);
        }

        public void ResetStepStart()
        {
            lock (_sync)
            {
                _stepStartedAt = DateTime.UtcNow;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                lock (_sync)
                {
                    return DateTime.UtcNow - _stepStartedAt;
                }
            }
        }

        private void OnTick(object state)
        {
            if (_disposed)
                return;

            try
            {
                var message = _messageFactory();
                if (!string.IsNullOrWhiteSpace(message))
                    ConsoleColorSupport.WriteLine(ConsoleColor.DarkCyan, message);
            }
            catch
            {
                // Heartbeat must never interrupt processing.
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _timer.Dispose();
        }
    }
}
