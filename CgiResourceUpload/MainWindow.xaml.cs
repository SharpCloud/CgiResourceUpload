using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SC.Api;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CgiResourceUpload
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Logger _logger;

        public MainWindow()
        {
            InitializeComponent();
            _logger = new Logger(LogTextBox);
        }

        private void BrowseSourceFolderClick(object sender, RoutedEventArgs e)
        {
            OpenFolderPicker(SourceFolderTextBox);
        }

        private void BrowseProcessedFolderClick(object sender, RoutedEventArgs e)
        {
            OpenFolderPicker(ProcessedFolderTextBox);
        }

        private void OpenFolderPicker(TextBox target)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                target.Text = dialog.FileName;
            }
        }

        private async void ProcessClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
            await _logger.Log("Starting resource upload...");
            await Validate();

            var client = new SharpcloudClient(
              uri: UrlTextBox.Text,
              username: UsernameTextBox.Text,
              password: PasswordEntryBox.Password,
              proxyURL: string.Empty,
              sendDefaultProxyCredentials: false,
              proxyUsername: string.Empty,
              proxyPassword: string.Empty);

            var updater = new ItemUpdater();

            updater.ProcessDirectory(
                client,
                SourceFolderTextBox.Text,
                ProcessedFolderTextBox.Text);

            await _logger.Log("Update complete");
        }

        private async Task<bool> Validate()
        {
            var usernameInvalid = string.IsNullOrWhiteSpace(UsernameTextBox.Text);
            var passwordInvalid = PasswordEntryBox.SecurePassword.Length == 0;
            var sourceInvalid = string.IsNullOrWhiteSpace(SourceFolderTextBox.Text);
            var processedInvalid = string.IsNullOrWhiteSpace(ProcessedFolderTextBox.Text);
            var urlInvalid = string.IsNullOrWhiteSpace(UrlTextBox.Text);

            if (usernameInvalid)
            {
                await _logger.LogError("SharpCloud username is empty");
            }

            if (passwordInvalid)
            {
                await _logger.LogError("SharpCloud password is empty");
            }

            if (sourceInvalid)
            {
                await _logger.LogError("Source directory is empty");
            }

            if (processedInvalid)
            {
                await _logger.LogError("Processed directory is empty");
            }

            if (urlInvalid)
            {
                await _logger.LogError("SharpCloud story URL is empty");
            }

            var isValid =
                usernameInvalid &&
                passwordInvalid &&
                sourceInvalid &&
                processedInvalid &&
                urlInvalid;

            return isValid;
        }

        private void LogHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var path = _logger.GetLogFilePath();
            var arg = "/select, \"" + path + "\"";
            System.Diagnostics.Process.Start("explorer.exe", arg);
        }
    }
}
