using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CgiResourceUpload
{
    public class Logger
    {
        private readonly TextBox _logWindow;

        public Logger(TextBox logWindow)
        {
            _logWindow = logWindow;
        }

        public async Task Log(string message)
        {
            var formatted = FormatMessage(message);
            await LogToUI(formatted);
            LogToFile(formatted);
        }

        public async Task LogError(string message)
        {
            await Log($"ERROR: {message}");
        }

        private string FormatMessage(string message)
        {
            return $"[{DateTime.UtcNow}] {message}{Environment.NewLine}";
        }

        private void LogToFile(string message)
        {
            var path = GetLogFilePath();
            File.AppendAllText(path, message);
        }

        private async Task LogToUI(string message)
        {
            _logWindow.Text += message;
            _logWindow.ScrollToEnd();
            await Task.Delay(1);
        }

        public string GetLogFilePath()
        {
            const string logFile = "cgi-resource-upload-log.txt";
            string output;

            try
            {
                var uri = new Uri(logFile, UriKind.Absolute);
                output = logFile;
            }
            catch (UriFormatException)
            {
                var dllPath = Assembly.GetExecutingAssembly().Location;
                var dir = Path.GetDirectoryName(dllPath);
                output = Path.Combine(dir, logFile);
            }

            return output;
        }
    }
}
