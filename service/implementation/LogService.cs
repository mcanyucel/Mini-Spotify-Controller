using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MiniSpotifyController.service.implementation
{
    internal sealed class LogService : ILogService
    {
        public void LogError(string message)
        {
            _ = Task.Run(() =>
            {
                Semaphore.Wait();
                try
                {
                    if (!Directory.Exists("logs"))
                    {
                        Directory.CreateDirectory("logs");
                    }
                    File.AppendAllText(_logFilePath, $"[{DateTime.Now}] [ERROR] {message}\n");
                }
                finally
                {
                    Semaphore.Release();
                }
            });
        }

        private readonly string _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiniSpotifyController", "application.log");
        private static readonly SemaphoreSlim Semaphore = new(1, 1);
    }
}
