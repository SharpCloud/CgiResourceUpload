using CgiResourceUpload.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using SC.API.ComInterop;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        private readonly Regex _urlRegex = new Regex(
            @"(https?://.*/)html/#/story/([0-9a-z-]*)",
            RegexOptions.IgnoreCase);

        private IList<ValidationCheck> _validationChecks;

        public MainWindow()
        {
            InitializeComponent();
            _logger = new Logger(LogTextBox);
            _validationChecks = CreateValidationChecks();
        }

        private void BrowseSourceFolderClick(object sender, RoutedEventArgs e)
        {
            OpenFolderPicker(SourceFolderTextBox);
        }

        private void BrowseProcessedFolderClick(object sender, RoutedEventArgs e)
        {
            OpenFolderPicker(ProcessedFolderTextBox);
        }

        private void BrowseUnprocessedFolderClick(object sender, RoutedEventArgs e)
        {
            OpenFolderPicker(UnprocessedFolderTextBox);
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
            var isValid = await Validate();

            if (!isValid)
            {
                await _logger.Log("Update not performed");
                return;
            }

            try
            {
                var match = _urlRegex.Match(UrlTextBox.Text);
                var url = match.Groups[1].Value;
                var storyId = match.Groups[2].Value;

                var api = new SharpCloudApi(
                   username: UsernameTextBox.Text,
                   password: PasswordEntryBox.Password,
                   url: url,
                   proxyURL: string.Empty,
                   useDefaultProxyCredentials: false,
                   proxyUsername: string.Empty,
                   proxyPassword: string.Empty);

                var updater = new ItemUpdater(_logger);

                await updater.ProcessDirectory(
                    api,
                    SourceFolderTextBox.Text,
                    ProcessedFolderTextBox.Text,
                    UnprocessedFolderTextBox.Text,
                    storyId,
                    DryRunCheckBox.IsChecked.GetValueOrDefault());

                await _logger.Log("Update complete");
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex);
            }
        }

        private async Task<bool> Validate()
        {
            var validationSuccess = true;

            foreach (var check in _validationChecks)
            {
                var isValid = check.Validate();
                if (!isValid)
                {
                    validationSuccess = false;
                    await _logger.LogError(check.FailMessage);
                }
            }

            return validationSuccess;
        }

        private IList<ValidationCheck> CreateValidationChecks()
        {
            var validationChecks = new[]
            {
                new ValidationCheck(
                    () => !string.IsNullOrWhiteSpace(UsernameTextBox.Text),
                    "SharpCloud username is empty"),

                new ValidationCheck(
                    () => PasswordEntryBox.SecurePassword.Length > 0,
                    "SharpCloud password is empty"),

                new ValidationCheck(
                    () => !string.IsNullOrWhiteSpace(SourceFolderTextBox.Text),
                    "Source directory is empty"),

                new ValidationCheck(
                    () => !string.IsNullOrWhiteSpace(ProcessedFolderTextBox.Text),
                    "Processed directory is empty"),

                new ValidationCheck(
                    () => !string.IsNullOrWhiteSpace(UnprocessedFolderTextBox.Text),
                    "Unprocessed directory is empty"),

                new ValidationCheck(
                    () => !string.IsNullOrWhiteSpace(UrlTextBox.Text),
                    "SharpCloud story URL is empty")
            };

            return validationChecks;
        }

        private void LogHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var path = _logger.GetLogFilePath();
            var arg = "/select, \"" + path + "\"";
            System.Diagnostics.Process.Start("explorer.exe", arg);
        }
    }
}
