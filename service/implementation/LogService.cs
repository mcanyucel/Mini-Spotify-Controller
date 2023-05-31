using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mini_Spotify_Controller.service.implementation
{
    internal class LogService : ILogService
    {
        private const string LOG_FILE_NAME = "logs/log.txt";
        private static readonly SemaphoreSlim m_Semaphore = new(1, 1);
        public void LogError(string message)
        {
            _ = Task.Run(() =>
            {
                m_Semaphore.Wait();
                try
                {
                    if (!Directory.Exists("logs"))
                    {
                        Directory.CreateDirectory("logs");
                    }
                    File.AppendAllText(LOG_FILE_NAME, $"[{DateTime.Now}] [ERROR] {message}\n");
                }
                finally
                {
                    m_Semaphore.Release();
                }
            });
        }
    }
}
