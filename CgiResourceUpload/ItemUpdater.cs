using CgiResourceUpload.Models;
using ExcelDataReader;
using SC.Api.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CgiResourceUpload
{
    public class ItemUpdater
    {
        public void ProcessDirectory(
            ISharpcloudClient2 client,
            string sourceDirectory,
            string processedDirectory,
            string spreadsheetPath)
        {
            var spreadsheetData = ReadSpreadsheet(spreadsheetPath);
            var directories = Directory.EnumerateDirectories(sourceDirectory);

            foreach (var path in directories)
            {
                var success = ProcessSubdirectory(path);

                if (success)
                {
                    var dirName = Path.GetFileName(path);
                    var destination = Path.Combine(processedDirectory, dirName);
                    Directory.Move(path, destination);
                }
            }
        }

        private bool ProcessSubdirectory(string path)
        {
            return true;
        }

        private IList<ItemMetadata> ReadSpreadsheet(string spreadsheetPath)
        {
            using (var spreadsheetFile = File.Open(spreadsheetPath, FileMode.Open, FileAccess.Read))
            {
                var readerConfig = new ExcelReaderConfiguration
                {
                    FallbackEncoding = Encoding.GetEncoding(1252)
                };

                using (var reader = ExcelReaderFactory.CreateReader(spreadsheetFile, readerConfig))
                {
                    //while (reader.Read())
                    //{
                    //}
                }
            }

            return new List<ItemMetadata>();
        }
    }
}
