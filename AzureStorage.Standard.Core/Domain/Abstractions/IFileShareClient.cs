using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Files
{
   /// <summary>
    /// Interface for Azure File Share Storage operations
    /// </summary>
    public interface IFileShareClient : IDisposable
    {
        // Share Operations
        /// <summary>
        /// Lists all file shares in the storage account
        /// </summary>
        Task<IEnumerable<string>> ListSharesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a file share if it doesn't exist
        /// </summary>
        Task<bool> CreateShareIfNotExistsAsync(string shareName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file share
        /// </summary>
        Task<bool> DeleteShareAsync(string shareName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a file share exists
        /// </summary>
        Task<bool> ShareExistsAsync(string shareName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the quota (size limit) for a file share in GB
        /// </summary>
        Task SetShareQuotaAsync(string shareName, int quotaInGB, CancellationToken cancellationToken = default);

        // Directory Operations
        /// <summary>
        /// Creates a directory in a share
        /// </summary>
        Task<bool> CreateDirectoryIfNotExistsAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a directory from a share
        /// </summary>
        Task<bool> DeleteDirectoryAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a directory exists
        /// </summary>
        Task<bool> DirectoryExistsAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists files and directories in a path
        /// </summary>
        Task<IEnumerable<FileShareItem>> ListFilesAndDirectoriesAsync(string shareName, string directoryPath = null, CancellationToken cancellationToken = default);

        // File Operations
        /// <summary>
        /// Uploads a file from a stream
        /// </summary>
        Task UploadFileAsync(string shareName, string filePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads a file from byte array
        /// </summary>
        Task UploadFileAsync(string shareName, string filePath, byte[] content, bool overwrite = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file to a stream
        /// </summary>
        Task<Stream> DownloadFileAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file as byte array
        /// </summary>
        Task<byte[]> DownloadFileAsBytesAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file
        /// </summary>
        Task<bool> DeleteFileAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a file exists
        /// </summary>
        Task<bool> FileExistsAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets file properties
        /// </summary>
        Task<FileShareItem> GetFilePropertiesAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Copies a file within the same account
        /// </summary>
        Task CopyFileAsync(string sourceShare, string sourceFilePath, string destinationShare, string destinationFilePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets file metadata
        /// </summary>
        Task SetFileMetadataAsync(string shareName, string filePath, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);
    }
}
