using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CityLibrary.Services
{
    public class FileService
    {
        private readonly ShareClient _shareClient;
        private readonly ShareClient _recycleBinClient;

        public FileService(string connectionString, string shareName)
        {
            _shareClient = new ShareClient(connectionString, shareName);
            _shareClient.CreateIfNotExists();

            _recycleBinClient = new ShareClient(connectionString, shareName + "-recyclebin");
            _recycleBinClient.CreateIfNotExists();
        }

        public async Task UploadFileAsync(string localFilePath, string remoteFileName)
        {
            ShareDirectoryClient directoryClient = _shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directoryClient.GetFileClient(remoteFileName);

            if (await fileClient.ExistsAsync())
            {
                await fileClient.DeleteAsync();
            }

            using FileStream fs = File.OpenRead(localFilePath);
            await fileClient.CreateAsync(fs.Length);
            await fileClient.UploadAsync(fs);
        }

        public async Task DownloadFileAsync(string remoteFileName, string localFilePath)
        {
            ShareDirectoryClient directoryClient = _shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directoryClient.GetFileClient(remoteFileName);

            ShareFileDownloadInfo download = (await fileClient.DownloadAsync()).Value;

            using FileStream fs = File.Create(localFilePath);
            await download.Content.CopyToAsync(fs);
        }

        public async Task<IEnumerable<string>> ListFilesAsync()
        {
            List<string> fileNames = new List<string>();
            ShareDirectoryClient directoryClient = _shareClient.GetRootDirectoryClient();
            await foreach (ShareFileItem fileItem in directoryClient.GetFilesAndDirectoriesAsync())
            {
                fileNames.Add(fileItem.Name);
            }
            return fileNames;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            ShareDirectoryClient directoryClient = _shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directoryClient.GetFileClient(fileName);

            if (await fileClient.ExistsAsync())
            {
                using MemoryStream ms = new MemoryStream();
                var download = (await fileClient.DownloadAsync()).Value;
                await download.Content.CopyToAsync(ms);
                ms.Position = 0;

                ShareDirectoryClient recycleBinClient = _recycleBinClient.GetRootDirectoryClient();
                ShareFileClient recycleFileClient = recycleBinClient.GetFileClient(fileName);
                await recycleFileClient.CreateAsync(ms.Length);
                ms.Position = 0;
                await recycleFileClient.UploadAsync(ms);

                await fileClient.DeleteAsync();
            }
        }

        public async Task<IEnumerable<string>> ListDeletedFilesAsync()
        {
            List<string> fileNames = new List<string>();
            ShareDirectoryClient directoryClient = _recycleBinClient.GetRootDirectoryClient();
            await foreach (ShareFileItem fileItem in directoryClient.GetFilesAndDirectoriesAsync())
            {
                fileNames.Add(fileItem.Name);
            }
            return fileNames;
        }

        public async Task RestoreFileAsync(string fileName)
        {
            ShareDirectoryClient recycleBinClient = _recycleBinClient.GetRootDirectoryClient();
            ShareFileClient recycleFileClient = recycleBinClient.GetFileClient(fileName);

            if (await recycleFileClient.ExistsAsync())
            {
                using MemoryStream ms = new MemoryStream();
                var download = (await recycleFileClient.DownloadAsync()).Value;
                await download.Content.CopyToAsync(ms);
                ms.Position = 0;

                ShareDirectoryClient originalDirectoryClient = _shareClient.GetRootDirectoryClient();
                ShareFileClient originalFileClient = originalDirectoryClient.GetFileClient(fileName);
                await originalFileClient.CreateAsync(ms.Length);
                ms.Position = 0;
                await originalFileClient.UploadAsync(ms);

                await recycleFileClient.DeleteAsync();
            }
        }
    }
}

