using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EMBRS
{
    public class Game
    {
        private string _name;
        private uint _appId;

        public Game(string name, uint appId)
        {
            _name = name;
            _appId = appId;
        }

        public void SetGameName(string name)
        {
            _name = name;
        }

        public string GetGameName()
        {
            return _name;
        }

        public void SetAppId(uint appId)
        {
            _appId = appId;
        }

        public uint GetAppId()
        {
            return _appId;
        }

        public async Task UpdateGameFiles()
        {
            var files = new List<string>();
            ProcessDirectory(Settings.GameFilesLocation, ref files);

            var container = new BlobContainerClient(Settings.AzureString, GetAppId().ToString());
            await container.CreateAsync();

            try
            {
                foreach (var file in files)
                {
                    var blob = container.GetBlobClient(file);
                    await blob.UploadAsync(file);
                }
            }
            finally
            {
                await container.DeleteAsync();
            }
        }

        private void ProcessDirectory(string targetDirectory, ref List<string> files)
        {
            var fileEntries = Directory.GetFiles(targetDirectory);
            files.AddRange(fileEntries);

            var subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (var subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, ref files);
        }

        public async Task GetGameFiles()
        {
            var container = new BlobContainerClient(Settings.AzureString, GetAppId().ToString());
            await container.CreateAsync();

            try
            {
                await foreach (var blobItem in container.GetBlobsAsync())
                {
                    var blob = container.GetBlobClient(blobItem.Name);
                    await blob.DownloadToAsync(blobItem.Name);
                }
            }
            finally
            {
                await container.DeleteAsync();
            }
        }
    }
}
