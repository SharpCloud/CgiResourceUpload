using System;
using System.IO;
using System.Reflection;
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

        public void Log(string message)
        {
            var formatted = FormatMessage(message);
            _logWindow.Text += formatted;
            LogToFile(formatted);
        }

        public void LogError(string message)
        {
            Log($"ERROR: {message}");
        }

        private string FormatMessage(string message)
        {
            return $"[{DateTime.UtcNow}] {message}{Environment.NewLine}";
        }

        private void LogToFile(string message)
        {
            const string logFile = "cgi-resource-upload-log.txt";

            var path = GetAbsolutePath(logFile);
            File.AppendAllText(path, message);
        }

        private string GetAbsolutePath(string path)
        {
            string output;

            try
            {
                var uri = new Uri(path, UriKind.Absolute);
                output = path;
            }
            catch (UriFormatException)
            {
                var dllPath = Assembly.GetExecutingAssembly().Location;
                var dir = Path.GetDirectoryName(dllPath);
                output = Path.Combine(dir, path);
            }

            return output;
        }
    }
}
