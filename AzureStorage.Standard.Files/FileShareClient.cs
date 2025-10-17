using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using AzureStorage.Standard.Core;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Files
{
    public class FileShareClient: IFileShareClient
    {
      private readonly ShareServiceClient _shareServiceClient;
        private readonly StorageOptions _options;

        /// <summary>
        /// Creates a new instance of FileShareClientWrapper
        /// </summary>
        public FileShareClient(StorageOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.Validate();

            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                _shareServiceClient = new ShareServiceClient(options.ConnectionString);
            }
            else if (options.ServiceUri != null)
            {
                _shareServiceClient = new ShareServiceClient(options.ServiceUri);
            }
            else
            {
                var accountUri = new Uri($"https://{options.AccountName}.file.core.windows.net");
                var credential = new Azure.Storage.StorageSharedKeyCredential(options.AccountName, options.AccountKey);
                _shareServiceClient = new ShareServiceClient(accountUri, credential);
            }
        }

        #region Share Operations

        public async Task<IEnumerable<string>> ListSharesAsync(CancellationToken cancellationToken = default)
        {
            var shares = new List<string>();

            try
            {
                await foreach (var share in _shareServiceClient.GetSharesAsync(cancellationToken: cancellationToken))
                {
                    shares.Add(share.Name);
                }

                return shares;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException("Failed to list file shares.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task<bool> CreateShareIfNotExistsAsync(string shareName, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var response = await shareClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                return response != null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to create file share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task<bool> DeleteShareAsync(string shareName, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var response = await shareClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete file share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task<bool> ShareExistsAsync(string shareName, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                return await shareClient.ExistsAsync(cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to check if file share '{shareName}' exists.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task SetShareQuotaAsync(string shareName, int quotaInGB, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);

            if (quotaInGB < 1 || quotaInGB > 102400)
                throw new ArgumentOutOfRangeException(nameof(quotaInGB), "Quota must be between 1 and 102400 GB.");

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                await shareClient.SetQuotaAsync(quotaInGB, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to set quota for file share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        #endregion

        #region Directory Operations

        public async Task<bool> CreateDirectoryIfNotExistsAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(directoryPath, nameof(directoryPath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var directoryClient = shareClient.GetDirectoryClient(directoryPath);
                var response = await directoryClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                return response != null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to create directory '{directoryPath}' in share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task<bool> DeleteDirectoryAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(directoryPath, nameof(directoryPath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var directoryClient = shareClient.GetDirectoryClient(directoryPath);
                var response = await directoryClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete directory '{directoryPath}' from share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task<bool> DirectoryExistsAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(directoryPath, nameof(directoryPath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var directoryClient = shareClient.GetDirectoryClient(directoryPath);
                return await directoryClient.ExistsAsync(cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to check if directory '{directoryPath}' exists in share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task<IEnumerable<FileShareItem>> ListFilesAndDirectoriesAsync(string shareName, string directoryPath = null, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);

            var items = new List<FileShareItem>();

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var directoryClient = string.IsNullOrWhiteSpace(directoryPath)
                    ? shareClient.GetRootDirectoryClient()
                    : shareClient.GetDirectoryClient(directoryPath);

                await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync(cancellationToken: cancellationToken))
                {
                    items.Add(MapToFileShareItem(item, shareName, directoryPath));
                }

                return items;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to list files and directories in share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        #endregion

        #region File Operations

        public async Task UploadFileAsync(string shareName, string filePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var fileClient = GetFileClient(shareClient, filePath);

                // Create parent directories if needed
                await CreateParentDirectoriesAsync(shareClient, filePath, cancellationToken);

                await fileClient.CreateAsync(content.Length, cancellationToken: cancellationToken);
                await fileClient.UploadAsync(content, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to upload file '{filePath}' to share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task UploadFileAsync(string shareName, string filePath, byte[] content, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream(content);
            await UploadFileAsync(shareName, filePath, stream, overwrite, cancellationToken);
        }

        public async Task<Stream> DownloadFileAsync(string shareName, string filePath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var fileClient = GetFileClient(shareClient, filePath);

                var response = await fileClient.DownloadAsync(cancellationToken: cancellationToken);
                var memoryStream = new MemoryStream();
                await response.Value.Content.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "ResourceNotFound")
            {
                return null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to download file '{filePath}' from share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task<byte[]> DownloadFileAsBytesAsync(string shareName, string filePath, CancellationToken cancellationToken = default)
        {
            using var stream = await DownloadFileAsync(shareName, filePath, cancellationToken);
            if (stream == null)
                return null;

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }

        public async Task<bool> DeleteFileAsync(string shareName, string filePath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var fileClient = GetFileClient(shareClient, filePath);

                var response = await fileClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete file '{filePath}' from share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task<bool> FileExistsAsync(string shareName, string filePath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var fileClient = GetFileClient(shareClient, filePath);

                return await fileClient.ExistsAsync(cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to check if file '{filePath}' exists in share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task<FileShareItem> GetFilePropertiesAsync(string shareName, string filePath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var fileClient = GetFileClient(shareClient, filePath);

                var properties = await fileClient.GetPropertiesAsync(cancellationToken: cancellationToken);

                return new FileShareItem
                {
                    Name = Path.GetFileName(filePath),
                    Path = filePath,
                    ShareName = shareName,
                    IsDirectory = false,
                    Size = properties.Value.ContentLength,
                    ContentType = properties.Value.ContentType,
                    ContentMD5 = properties.Value.ContentHash != null ? Convert.ToBase64String(properties.Value.ContentHash) : null,
                    CreatedOn = properties.Value.SmbProperties.FileCreatedOn,
                    LastModified = properties.Value.LastModified,
                    ETag = properties.Value.ETag.ToString(),
                    Metadata = properties.Value.Metadata.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase)
                };
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "ResourceNotFound")
            {
                return null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to get properties for file '{filePath}' in share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task CopyFileAsync(string sourceShare, string sourceFilePath, string destinationShare, string destinationFilePath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(sourceShare);
            ValidatePath(sourceFilePath, nameof(sourceFilePath));
            ValidateShareName(destinationShare);
            ValidatePath(destinationFilePath, nameof(destinationFilePath));

            try
            {
                var sourceShareClient = _shareServiceClient.GetShareClient(sourceShare);
                var sourceFileClient = GetFileClient(sourceShareClient, sourceFilePath);

                var destShareClient = _shareServiceClient.GetShareClient(destinationShare);
                var destFileClient = GetFileClient(destShareClient, destinationFilePath);

                await CreateParentDirectoriesAsync(destShareClient, destinationFilePath, cancellationToken);

                await destFileClient.StartCopyAsync(sourceFileClient.Uri, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to copy file from '{sourceShare}/{sourceFilePath}' to '{destinationShare}/{destinationFilePath}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        public async Task SetFileMetadataAsync(string shareName, string filePath, IDictionary<string, string> metadata, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var fileClient = GetFileClient(shareClient, filePath);

                await fileClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to set metadata for file '{filePath}' in share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        #endregion

        #region Helper Methods

        private static ShareFileClient GetFileClient(ShareClient shareClient, string filePath)
        {
            var parts = filePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return shareClient.GetRootDirectoryClient().GetFileClient(parts[0]);
            }

            var directoryPath = string.Join("/", parts.Take(parts.Length - 1));
            var fileName = parts.Last();
            return shareClient.GetDirectoryClient(directoryPath).GetFileClient(fileName);
        }

        private static async Task CreateParentDirectoriesAsync(ShareClient shareClient, string filePath, CancellationToken cancellationToken)
        {
            var parts = filePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 1)
                return;

            var currentPath = "";
            for (int i = 0; i < parts.Length - 1; i++)
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? parts[i] : $"{currentPath}/{parts[i]}";
                var directoryClient = shareClient.GetDirectoryClient(currentPath);
                await directoryClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            }
        }

        private static FileShareItem MapToFileShareItem(ShareFileItem item, string shareName, string basePath)
        {
            var fullPath = string.IsNullOrWhiteSpace(basePath) ? item.Name : $"{basePath}/{item.Name}";

            return new FileShareItem
            {
                Name = item.Name,
                Path = fullPath,
                ShareName = shareName,
                IsDirectory = item.IsDirectory,
                Size = item.IsDirectory ? null : item.FileSize,
                LastModified = item.Properties?.LastModified,
                ETag = item.Properties?.ETag.ToString()
            };
        }

        private static void ValidateShareName(string shareName)
        {
            if (string.IsNullOrWhiteSpace(shareName))
                throw new ArgumentNullException(nameof(shareName), "Share name cannot be null or empty.");
        }

        private static void ValidatePath(string path, string paramName)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null or empty.");
        }

        #endregion

        public void Dispose()
        {
            // ShareServiceClient doesn't implement IDisposable, so nothing to dispose
        }
    }
    }
