using SC.Api.Interfaces;
using System.IO;

namespace CgiResourceUpload
{
    public class ItemUpdater
    {
        public void ProcessDirectory(
            ISharpcloudClient2 client,
            string sourceDirectory,
            string processedDirectory)
        {
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
    }
}
