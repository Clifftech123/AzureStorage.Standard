using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Files
{
   /// <summary>
    /// Interface for Azure File Share Storage operations.
    /// Provides comprehensive methods for managing file shares, directories, and files.
    /// This is a simplified wrapper around the Azure.Storage.Files.Shares SDK.
    /// </summary>
    public interface IFileShareClient : IDisposable
    {
        #region Share Operations

        /// <summary>
        /// Lists all file shares in the storage account.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of file share names.</returns>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<IEnumerable<string>> ListSharesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a file share if it does not already exist.
        /// File share names must be lowercase, 3-63 characters, and contain only letters, numbers, and hyphens.
        /// </summary>
        /// <param name="shareName">The name of the file share to create.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the share was created; false if it already existed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> CreateShareIfNotExistsAsync(string shareName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file share if it exists.
        /// Warning: Deleting a share permanently removes all files and directories within it.
        /// </summary>
        /// <param name="shareName">The name of the file share to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the share was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> DeleteShareAsync(string shareName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a file share exists in the storage account.
        /// </summary>
        /// <param name="shareName">The name of the file share to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the share exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> ShareExistsAsync(string shareName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the quota (maximum size) for a file share.
        /// The quota determines the maximum size of the file share in gigabytes (GB).
        /// Valid range: 1 GB to 102,400 GB (100 TiB) for premium and standard shares.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="quotaInGB">The quota size in gigabytes (1-102400 GB).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when quota is not between 1 and 102400 GB.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task SetShareQuotaAsync(string shareName, int quotaInGB, CancellationToken cancellationToken = default);

        #endregion

        #region Directory Operations

        /// <summary>
        /// Creates a directory in a file share if it does not already exist.
        /// Supports nested directory paths (e.g., "folder1/folder2/folder3").
        /// Parent directories are automatically created as needed.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="directoryPath">The path of the directory to create (use forward slashes for nested paths).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the directory was created; false if it already existed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="directoryPath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> CreateDirectoryIfNotExistsAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a directory from a file share if it exists.
        /// Warning: The directory must be empty before deletion. Delete all files and subdirectories first.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="directoryPath">The path of the directory to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the directory was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="directoryPath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails (e.g., directory not empty).</exception>
        Task<bool> DeleteDirectoryAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a directory exists in a file share.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="directoryPath">The path of the directory to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the directory exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="directoryPath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> DirectoryExistsAsync(string shareName, string directoryPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists all files and subdirectories within a directory in a file share.
        /// If no directory path is specified, lists items in the root directory.
        /// Use the IsDirectory property on returned items to distinguish between files and directories.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="directoryPath">The directory path to list (optional, defaults to root directory).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of <see cref="FileShareItem"/> objects representing files and directories.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<IEnumerable<FileShareItem>> ListFilesAndDirectoriesAsync(string shareName, string directoryPath = null, CancellationToken cancellationToken = default);

        #endregion

        #region File Operations

        /// <summary>
        /// Uploads a file to an Azure file share from a stream.
        /// Automatically creates parent directories if they don't exist.
        /// The file size is determined from the stream length.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path where the file will be stored (e.g., "folder/file.txt").</param>
        /// <param name="content">The stream containing the file content.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists (currently not implemented).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/>, <paramref name="filePath"/>, or <paramref name="content"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task UploadFileAsync(string shareName, string filePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads a file to an Azure file share from a byte array.
        /// This is a convenience method that wraps the byte array in a MemoryStream.
        /// Automatically creates parent directories if they don't exist.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path where the file will be stored (e.g., "folder/file.txt").</param>
        /// <param name="content">The byte array containing the file content.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists (currently not implemented).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/>, <paramref name="filePath"/>, or <paramref name="content"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task UploadFileAsync(string shareName, string filePath, byte[] content, bool overwrite = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file from an Azure file share to a stream.
        /// The returned stream is a MemoryStream positioned at the beginning.
        /// The caller is responsible for disposing the returned stream.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file to download.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A stream containing the file content, or null if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<Stream> DownloadFileAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file from an Azure file share as a byte array.
        /// This is a convenience method that downloads the file and converts the stream to a byte array.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file to download.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A byte array containing the file content, or null if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<byte[]> DownloadFileAsBytesAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file from an Azure file share if it exists.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the file was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> DeleteFileAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a file exists in an Azure file share.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> FileExistsAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the properties and metadata of a file in an Azure file share.
        /// Returns a <see cref="FileShareItem"/> containing file size, content type, MD5 hash,
        /// creation time, last modified time, ETag, and custom metadata.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="FileShareItem"/> with file properties and metadata, or null if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/> or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<FileShareItem> GetFilePropertiesAsync(string shareName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Copies a file from one location to another within the same storage account.
        /// The copy operation is asynchronous on the server side. This method initiates the copy
        /// but may return before the copy completes. Parent directories in the destination are
        /// automatically created if they don't exist.
        /// </summary>
        /// <param name="sourceShare">The name of the source file share.</param>
        /// <param name="sourceFilePath">The path of the source file.</param>
        /// <param name="destinationShare">The name of the destination file share.</param>
        /// <param name="destinationFilePath">The path of the destination file.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task CopyFileAsync(string sourceShare, string sourceFilePath, string destinationShare, string destinationFilePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets custom metadata on a file in an Azure file share.
        /// Metadata is a collection of name-value pairs associated with the file.
        /// Metadata names must adhere to C# identifier naming rules.
        /// </summary>
        /// <param name="shareName">The name of the file share.</param>
        /// <param name="filePath">The path of the file.</param>
        /// <param name="metadata">A dictionary of metadata key-value pairs.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shareName"/>, <paramref name="filePath"/>, or <paramref name="metadata"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task SetFileMetadataAsync(string shareName, string filePath, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);

        #endregion
    }
}
