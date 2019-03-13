using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace CgiResourceUpload
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

        private void BrowseSpreadsheetClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                SpreadsheetTextBox.Text = dialog.FileName;
            }
        }

        private void ProcessClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
