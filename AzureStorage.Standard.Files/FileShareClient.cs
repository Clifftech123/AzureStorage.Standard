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
using AzureStorage.Standard.Files.Internal;

namespace AzureStorage.Standard.Files
{
    /// <summary>
    /// Azure File Share Storage client implementation that wraps the Azure.Storage.Files.Shares SDK.
    /// Provides simplified access to Azure File Share operations including shares, directories, and files.
    /// <para>
    /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/files/storage-files-introduction">Azure Files Overview</see>
    /// </para>
    /// <para>
    /// SDK Reference: <see href="https://learn.microsoft.com/en-us/dotnet/api/azure.storage.files.shares">Azure.Storage.Files.Shares Namespace</see>
    /// </para>
    /// </summary>
    public class FileShareClient: IFileShareClient
    {
      private readonly ShareServiceClient _shareServiceClient;
        private readonly StorageOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileShareClient"/> class.
        /// <para>
        /// Supports multiple authentication methods:
        /// - Connection string (recommended for development)
        /// - Service URI (for managed identity scenarios)
        /// - Account name and key (for explicit credential scenarios)
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/files/storage-how-to-use-files-dotnet">Use Azure Files with .NET</see>
        /// </para>
        /// </summary>
        /// <param name="options">Configuration options for connecting to Azure File Share Storage.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when required options are invalid or missing.</exception>
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

        /// <summary>
        /// Lists all file shares in the storage account.
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/files/storage-how-to-create-file-share">Create an Azure file share</see>
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of file share names.</returns>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
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

        /// <summary>
        /// Creates a file share if it does not already exist.
        /// <para>
        /// File share names must be lowercase, 3-63 characters, and contain only letters, numbers, and hyphens.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-shares--directories--files--and-metadata">Naming and Referencing Shares, Directories, Files, and Metadata</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share to create.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the share was created; false if it already existed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> CreateShareIfNotExistsAsync(string shareName, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => shareClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken));
                return response != null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to create file share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Deletes a file share if it exists.
        /// <para>
        /// Warning: Deleting a share permanently removes all files and directories within it.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/files/storage-how-to-create-file-share">Create an Azure file share</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the share was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> DeleteShareAsync(string shareName, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => shareClient.DeleteIfExistsAsync(cancellationToken: cancellationToken));
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete file share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Checks if a file share exists in the storage account.
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/dotnet/api/azure.storage.files.shares.shareclient.existsasync">ShareClient.ExistsAsync Method</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the share exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> ShareExistsAsync(string shareName, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                return await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => shareClient.ExistsAsync(cancellationToken));
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to check if file share '{shareName}' exists.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Sets the quota (maximum size) for a file share.
        /// <para>
        /// The quota determines the maximum size of the file share in gigabytes (GB).
        /// Valid range: 1 GB to 102,400 GB (100 TiB) for premium and standard shares.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/files/storage-files-scale-targets">Azure Files scalability and performance targets</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="quotaInGB">The quota size in gigabytes (1-102400 GB).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when quota is not between 1 and 102400 GB.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task SetShareQuotaAsync(string shareName, int quotaInGB, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);

            if (quotaInGB < 1 || quotaInGB > 102400)
                throw new ArgumentOutOfRangeException(nameof(quotaInGB), "Quota must be between 1 and 102400 GB.");

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => shareClient.SetQuotaAsync(quotaInGB, cancellationToken));
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to set quota for file share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        #endregion

        #region Directory Operations

        /// <summary>
        /// Creates a directory in a file share if it does not already exist.
        /// <para>
        /// Supports nested directory paths (e.g., "folder1/folder2/folder3").
        /// Parent directories are automatically created as needed.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/create-directory">Create Directory (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="directoryPath">The path of the directory to create (use forward slashes for nested paths).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the directory was created; false if it already existed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="directoryPath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> CreateDirectoryIfNotExistsAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(directoryPath, nameof(directoryPath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var directoryClient = shareClient.GetDirectoryClient(directoryPath);
                var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => directoryClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken));
                return response != null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to create directory '{directoryPath}' in share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Deletes a directory from a file share if it exists.
        /// <para>
        /// Warning: The directory must be empty before deletion. Delete all files and subdirectories first.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/delete-directory">Delete Directory (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="directoryPath">The path of the directory to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the directory was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="directoryPath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails (e.g., directory not empty).</exception>
        public async Task<bool> DeleteDirectoryAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(directoryPath, nameof(directoryPath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var directoryClient = shareClient.GetDirectoryClient(directoryPath);
                var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => directoryClient.DeleteIfExistsAsync(cancellationToken: cancellationToken));
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete directory '{directoryPath}' from share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Checks if a directory exists in a file share.
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/dotnet/api/azure.storage.files.shares.sharedirectoryclient.existsasync">ShareDirectoryClient.ExistsAsync Method</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="directoryPath">The path of the directory to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the directory exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="directoryPath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> DirectoryExistsAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(directoryPath, nameof(directoryPath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var directoryClient = shareClient.GetDirectoryClient(directoryPath);
                return await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => directoryClient.ExistsAsync(cancellationToken));
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to check if directory '{directoryPath}' exists in share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Lists all files and subdirectories within a directory in a file share.
        /// <para>
        /// If no directory path is specified, lists items in the root directory.
        /// Use the IsDirectory property on returned items to distinguish between files and directories.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/list-directories-and-files">List Directories and Files (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="directoryPath">The directory path to list (optional, defaults to root directory).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of <see cref="FileShareItem"/> objects representing files and directories.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
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

        /// <summary>
        /// Uploads a file to an Azure file share from a stream.
        /// <para>
        /// Automatically creates parent directories if they don't exist.
        /// The file size is determined from the stream length.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/create-file">Create File (REST API)</see>
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/put-range">Put Range (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path where the file will be stored (e.g., "folder/file.txt").</param>
        /// <param name="content">The stream containing the file content.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists (currently not implemented).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/>, <paramref name="filePath"/>, or <paramref name="content"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
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

                // Check if file exists when overwrite is false
                if (!overwrite)
                {
                    bool exists = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => fileClient.ExistsAsync(cancellationToken));
                    if (exists)
                    {
                        throw new AzureStorageException($"File '{filePath}' already exists in share '{shareName}' and overwrite is set to false.", "FileAlreadyExists", 409, null);
                    }
                }

                await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => fileClient.CreateAsync(content.Length, cancellationToken: cancellationToken));
                await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => fileClient.UploadAsync(content, cancellationToken: cancellationToken));
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to upload file '{filePath}' to share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Uploads a file to an Azure file share from a byte array.
        /// <para>
        /// This is a convenience method that wraps the byte array in a MemoryStream.
        /// Automatically creates parent directories if they don't exist.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/create-file">Create File (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path where the file will be stored (e.g., "folder/file.txt").</param>
        /// <param name="content">The byte array containing the file content.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists (currently not implemented).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/>, <paramref name="filePath"/>, or <paramref name="content"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task UploadFileAsync(string shareName, string filePath, byte[] content, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using var stream = new MemoryStream(content);
            await UploadFileAsync(shareName, filePath, stream, overwrite, cancellationToken);
        }

        /// <summary>
        /// Downloads a file from an Azure file share to a stream.
        /// <para>
        /// The returned stream is a MemoryStream positioned at the beginning.
        /// The caller is responsible for disposing the returned stream.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/get-file">Get File (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file to download.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A stream containing the file content, or null if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<Stream> DownloadFileAsync(string shareName, string filePath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var fileClient = GetFileClient(shareClient, filePath);

                var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => fileClient.DownloadAsync(cancellationToken: cancellationToken));
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

        /// <summary>
        /// Downloads a file from an Azure file share as a byte array.
        /// <para>
        /// This is a convenience method that downloads the file and converts the stream to a byte array.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/get-file">Get File (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file to download.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A byte array containing the file content, or null if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<byte[]> DownloadFileAsBytesAsync(string shareName, string filePath, CancellationToken cancellationToken = default)
        {
            using var stream = await DownloadFileAsync(shareName, filePath, cancellationToken);
            if (stream == null)
                return null;

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Deletes a file from an Azure file share if it exists.
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/delete-file2">Delete File (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the file was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> DeleteFileAsync(string shareName, string filePath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var fileClient = GetFileClient(shareClient, filePath);

                var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => fileClient.DeleteIfExistsAsync(cancellationToken: cancellationToken));
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete file '{filePath}' from share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Checks if a file exists in an Azure file share.
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/dotnet/api/azure.storage.files.shares.sharefileclient.existsasync">ShareFileClient.ExistsAsync Method</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> FileExistsAsync(string shareName, string filePath, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                var fileClient = GetFileClient(shareClient, filePath);

                return await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => fileClient.ExistsAsync(cancellationToken));
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to check if file '{filePath}' exists in share '{shareName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Gets the properties and metadata of a file in an Azure file share.
        /// <para>
        /// Returns a <see cref="FileShareItem"/> containing file size, content type, MD5 hash,
        /// creation time, last modified time, ETag, and custom metadata.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/get-file-properties">Get File Properties (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="FileShareItem"/> with file properties and metadata, or null if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
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

        /// <summary>
        /// Copies a file from one location to another within the same storage account.
        /// <para>
        /// The copy operation is asynchronous on the server side. This method initiates the copy
        /// but may return before the copy completes. Parent directories in the destination are
        /// automatically created if they don't exist.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/copy-file">Copy File (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="sourceShare">The name of the source file share.</param>
        /// <param name="sourceFilePath">The path of the source file.</param>
        /// <param name="destinationShare">The name of the destination file share.</param>
        /// <param name="destinationFilePath">The path of the destination file.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
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

        /// <summary>
        /// Sets custom metadata on a file in an Azure file share.
        /// <para>
        /// Metadata is a collection of name-value pairs associated with the file.
        /// Metadata names must adhere to C# identifier naming rules.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/set-file-metadata">Set File Metadata (REST API)</see>
        /// </para>
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file.</param>
        /// <param name="metadata">A dictionary of metadata key-value pairs.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/>, <paramref name="filePath"/>, or <paramref name="metadata"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task SetFileMetadataAsync(string shareName, string filePath, IDictionary<string, string> metadata, CancellationToken cancellationToken = default)
        {
            ValidateShareName(shareName);
            ValidatePath(filePath, nameof(filePath));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

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

        /// <summary>
        /// Gets a ShareFileClient for the specified file path.
        /// Handles both root-level files and files in nested directories.
        /// </summary>
        /// <param name="shareClient">The share client.</param>
        /// <param name="filePath">The file path (supports forward slashes and backslashes).</param>
        /// <returns>A ShareFileClient for the specified file.</returns>
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

        /// <summary>
        /// Recursively creates all parent directories for a given file path if they don't exist.
        /// This ensures that a file can be uploaded even if its parent directories don't exist yet.
        /// </summary>
        /// <param name="shareClient">The share client.</param>
        /// <param name="filePath">The file path whose parent directories should be created.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
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

        /// <summary>
        /// Maps an Azure SDK ShareFileItem to the wrapper's FileShareItem model.
        /// </summary>
        /// <param name="item">The Azure SDK ShareFileItem.</param>
        /// <param name="shareName">The name of the share containing the item.</param>
        /// <param name="basePath">The base directory path (for constructing the full path).</param>
        /// <returns>A FileShareItem with mapped properties.</returns>
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

        /// <summary>
        /// Validates that a share name is not null or empty.
        /// </summary>
        /// <param name="shareName">The share name to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when the share name is null or whitespace.</exception>
        private static void ValidateShareName(string shareName)
        {
            if (string.IsNullOrWhiteSpace(shareName))
                throw new ArgumentNullException(nameof(shareName), "Share name cannot be null or empty.");
        }

        /// <summary>
        /// Validates that a path parameter is not null or empty.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <param name="paramName">The parameter name for error messaging.</param>
        /// <exception cref="ArgumentNullException">Thrown when the path is null or whitespace.</exception>
        private static void ValidatePath(string path, string paramName)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null or empty.");
        }

        #endregion

        /// <summary>
        /// Disposes the resources used by this client.
        /// <para>
        /// Note: The underlying ShareServiceClient does not implement IDisposable,
        /// so this method is a no-op included to satisfy the IDisposable interface requirement.
        /// </para>
        /// </summary>
        public void Dispose()
        {
            // ShareServiceClient doesn't implement IDisposable, so nothing to dispose
        }
    }
    }
