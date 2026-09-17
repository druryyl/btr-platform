using System;
using System.Threading;
using System.Threading.Tasks;

namespace btr.portal.worker.admin.Services
{
    public class HealthPollingService : IDisposable
    {
        private readonly Func<Task> _pollAction;
        private readonly TimeSpan _interval;
        private Timer _timer;
        private bool _isPolling;
        private bool _disposed;

        public bool IsPolling => _isPolling;

        public event EventHandler<string> PollFailed;

        public HealthPollingService(Func<Task> pollAction, TimeSpan interval)
        {
            _pollAction = pollAction ?? throw new ArgumentNullException(nameof(pollAction));
            _interval = interval;
        }

        public void Start()
        {
            if (_isPolling || _disposed)
                return;

            _isPolling = true;
            _timer = new Timer(PollCallback, null, TimeSpan.Zero, _interval);
        }

        public void Stop()
        {
            _isPolling = false;
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private async void PollCallback(object state)
        {
            if (!_isPolling || _disposed)
                return;

            try
            {
                await _pollAction();
            }
            catch (Exception ex)
            {
                PollFailed?.Invoke(this, ex.Message);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _isPolling = false;
            _timer?.Dispose();
        }
    }
}
