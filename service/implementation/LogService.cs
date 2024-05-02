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
                m_Semaphore.Wait();
                try
                {
                    if (!Directory.Exists("logs"))
                    {
                        Directory.CreateDirectory("logs");
                    }
                    File.AppendAllText(m_LogFilePath, $"[{DateTime.Now}] [ERROR] {message}\n");
                }
                finally
                {
                    m_Semaphore.Release();
                }
            });
        }

        private readonly string m_LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiniSpotifyController", "application.log");
        private static readonly SemaphoreSlim m_Semaphore = new(1, 1);
    }
}
