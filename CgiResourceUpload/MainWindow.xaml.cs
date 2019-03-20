using CgiResourceUpload.Helpers;
using CgiResourceUpload.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using SC.API.ComInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
        private const string UsernameKey = "Username";
        private const string PasswordKey = "Password";
        private const string SourceFolderKey = "SourceFolder";
        private const string ProcessedFolderKey = "ProcessedFolder";
        private const string UnprocessedFolderKey = "UnprocessedFolder";
        private const string UrlKey = "Url";
        private const string DryRunKey = "IsDryRun";

        private readonly Logger _logger;

        private readonly Regex _urlRegex = new Regex(
            @"(https?://.*/)html/#/story/([0-9a-z-]*)",
            RegexOptions.IgnoreCase);

        private IList<ValidationCheck> _validationChecks;

        public string AppName => $"CGI Resource Upload v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

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
            const string noUpdate = "Update will not be performed";

            LogTextBox.Clear();
            await _logger.Log($"{AppName}: Starting resource upload...");
            var isValid = await Validate();

            if (!isValid)
            {
                await _logger.Log(noUpdate);
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

                await _logger.Log($"Loading story with ID '{storyId}'");
                var story = api.LoadStory(storyId);
                await _logger.Log($"Story '{story.Name}' loaded");

                var sharepermission = story.StoryAsRoadmap.SharedUsers.FirstOrDefault(su =>
                    su.User.Username.ToLower() == UsernameTextBox.Text.ToLower())
                    .Action.ToString();

                if (sharepermission != null && (sharepermission == "admin" || sharepermission == "owner"))
                {
                    var updater = new ItemUpdater(_logger);

                    await updater.ProcessDirectory(
                        story,
                        SourceFolderTextBox.Text,
                        ProcessedFolderTextBox.Text,
                        UnprocessedFolderTextBox.Text,
                        storyId,
                        DryRunCheckBox.IsChecked.GetValueOrDefault());

                    await _logger.Log("Update complete");
                }
                else
                {
                    await _logger.Log($"'{UsernameTextBox.Text}' does not have permissions to update the specified story");
                    await _logger.Log(noUpdate);
                }
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

        private async void WindowClosing(object sender, CancelEventArgs e)
        {
            var helper = new RegistryHelper(_logger);
            await helper.RegWrite(UsernameKey, UsernameTextBox.Text);

            await helper.RegWrite(PasswordKey,
                Convert.ToBase64String(Encoding.Default.GetBytes(PasswordEntryBox.Password)));

            await helper.RegWrite(SourceFolderKey, SourceFolderTextBox.Text);
            await helper.RegWrite(ProcessedFolderKey, ProcessedFolderTextBox.Text);
            await helper.RegWrite(UnprocessedFolderKey, UnprocessedFolderTextBox.Text);
            await helper.RegWrite(UrlKey, UrlTextBox.Text);
            await helper.RegWrite(DryRunKey, DryRunCheckBox.IsChecked.GetValueOrDefault());
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var helper = new RegistryHelper(_logger);
            UsernameTextBox.Text = await helper.RegRead(UsernameKey, string.Empty);

            PasswordEntryBox.Password = Encoding.Default.GetString(
                Convert.FromBase64String(await helper.RegRead(PasswordKey, string.Empty)));

            SourceFolderTextBox.Text = await helper.RegRead(SourceFolderKey, string.Empty);
            ProcessedFolderTextBox.Text = await helper.RegRead(ProcessedFolderKey, string.Empty);
            UnprocessedFolderTextBox.Text = await helper.RegRead(UnprocessedFolderKey, string.Empty);
            UrlTextBox.Text = await helper.RegRead(UrlKey, string.Empty);
            DryRunCheckBox.IsChecked = bool.Parse(await helper.RegRead(DryRunKey, "False"));
        }
    }
}
