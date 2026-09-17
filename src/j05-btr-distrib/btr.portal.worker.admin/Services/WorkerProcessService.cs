using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace btr.portal.worker.admin.Services
{
    public class WorkerProcessService
    {
        public event EventHandler<string> OutputReceived;
        public event EventHandler<int> Exited;

        private Process _currentProcess;

        public bool IsRunning => _currentProcess != null && !_currentProcess.HasExited;

        public void StartWorker(string exePath, string arguments)
        {
            if (IsRunning)
                throw new InvalidOperationException("Worker process is already running.");

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = System.IO.Path.GetDirectoryName(exePath)
            };

            _currentProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            _currentProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    OutputReceived?.Invoke(this, e.Data);
            };
            _currentProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    OutputReceived?.Invoke(this, $"[ERROR] {e.Data}");
            };
            _currentProcess.Exited += (s, e) =>
            {
                Exited?.Invoke(this, _currentProcess.ExitCode);
            };

            _currentProcess.Start();
            _currentProcess.BeginOutputReadLine();
            _currentProcess.BeginErrorReadLine();
        }

        public void CancelWorker()
        {
            if (!IsRunning)
                return;

            try
            {
                _currentProcess.CancelOutputRead();
                _currentProcess.Kill();
            }
            catch (InvalidOperationException)
            {
                // Process already exiting
            }
        }

        public void Dispose()
        {
            if (_currentProcess != null)
            {
                if (!_currentProcess.HasExited)
                {
                    try { _currentProcess.Kill(); } catch { }
                }
                _currentProcess.Dispose();
                _currentProcess = null;
            }
        }
    }
}
