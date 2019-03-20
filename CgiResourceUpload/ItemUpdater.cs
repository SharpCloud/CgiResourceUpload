using SC.API.ComInterop.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Directory = System.IO.Directory;

namespace CgiResourceUpload
{
    public class ItemUpdater
    {
        private readonly Regex _directoryRegex = new Regex(
            @"([0-9]+)_([0-9]+)_(.+)_(.+)",
            RegexOptions.IgnoreCase);

        private readonly string[] _resourceFileExtensions = new[] { ".pptx" };
        private readonly Logger _logger;

        public ItemUpdater(Logger logger)
        {
            _logger = logger;
        }

        public async Task ProcessDirectory(
            Story story,
            string sourceDirectory,
            string processedDirectory,
            string unprocessedDirectory,
            string storyId,
            bool isDryRun)
        {
            var directories = Directory.EnumerateDirectories(sourceDirectory);
            foreach (var path in directories)
            {
                var dirName = Path.GetFileName(path);
                var success = true;

                try
                {
                    await _logger.Log($"Processing '{path}'...");
                    success = await ProcessSubdirectory(path, dirName, story);
                }
                catch (Exception e)
                {
                    success = false;
                    await _logger.LogError(e);
                }
                finally
                {
                    var toCombine = success ? processedDirectory : unprocessedDirectory;
                    var destination = Path.Combine(toCombine, dirName);

                    var logPrefix = isDryRun
                        ? "Performing dry run: would move"
                        : "Moving";

                    await _logger.Log($"{logPrefix} directory to '{destination}'");

                    if (!isDryRun)
                    {
                        Directory.Move(path, destination);
                    }
                }
            }

            if (isDryRun)
            {
                await _logger.Log($"Performing dry run: skipping story synchronization");
            }
            else
            {
                await _logger.Log($"Synchronizing story...");
                story.Save();
            }
        }

        private async Task<bool> ProcessSubdirectory(string dirPath, string dirName, Story story)
        {
            var match = _directoryRegex.Match(dirName);
            var year = match.Groups[1].Value;
            var monthDay = match.Groups[2].Value;
            var extId = match.Groups[3].Value;
            var title = match.Groups[4].Value;

            var folderNameFormatMismatch =
                string.IsNullOrWhiteSpace(year) ||
                string.IsNullOrWhiteSpace(monthDay)||
                string.IsNullOrWhiteSpace(extId) ||
                string.IsNullOrWhiteSpace(title);

            if (folderNameFormatMismatch)
            {
                await _logger.LogWarning($"'{dirPath}' does not have the expected name format of '{{Year}}_{{MMDD}}_{{Unique ID}}_{{Title}}'");
                return false;
            }

            var item = story.Item_FindByExternalId(extId);

            if (item == null)
            {
                item = story.Item_AddNew(title);
                item.ExternalId = extId;
                await _logger.Log($"Item created with name '{title}' and external ID '{extId}'");
            }
            else
            {
                await _logger.Log($"Item with external ID '{extId}' found. Updating...");
                var resourceIds = item.Resources.Select(r => r.Id).ToList();
                await _logger.Log($"Removing all existing resources from item with external ID '{extId}'...");
                foreach (var id in resourceIds)
                {
                    item.Resource_DeleteById(id);
                }
            }

            var dirFiles = Directory.EnumerateFiles(dirPath);
            var resources = dirFiles.Where(f => _resourceFileExtensions.Contains(Path.GetExtension(f)));

            foreach (var resource in resources)
            {
                var name = Path.GetFileNameWithoutExtension(resource);
                item.Resource_AddFile(resource, name);
                await _logger.Log($"Adding resource found at '{resource}' as '{name}'");
            }

            await _logger.Log($"Item update complete");
            return true;
        }
    }
}
