
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Blobs
{
    /// <summary>
    /// Interface for Azure Blob Storage operations.
    /// Provides comprehensive methods for managing containers, blobs, metadata, and access control.
    /// </summary>
    public interface IBlobClient : IDisposable
    {
        #region Container Operations

        /// <summary>
        /// Lists all containers in the storage account
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of container names</returns>
        Task<IEnumerable<string>> ListContainersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a container if it doesn't exist
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if created, false if already exists</returns>
        Task<bool> CreateContainerIfNotExistsAsync(string containerName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a container
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a container exists
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if container exists</returns>
        Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets container properties and metadata
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Container properties including metadata</returns>
        Task<IDictionary<string, string>> GetContainerPropertiesAsync(string containerName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets metadata on a container
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="metadata">Metadata key-value pairs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SetContainerMetadataAsync(string containerName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets metadata from a container
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Container metadata</returns>
        Task<IDictionary<string, string>> GetContainerMetadataAsync(string containerName, CancellationToken cancellationToken = default);

        #endregion

        #region Blob Listing & Query Operations

        /// <summary>
        /// Lists all blobs in a container with optional prefix filter
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="prefix">Optional prefix to filter blobs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of blob items</returns>
        Task<IEnumerable<BlobItem>> ListBlobsAsync(string containerName, string prefix = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single blob with its properties and metadata
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Blob item with properties and metadata</returns>
        Task<BlobItem> GetBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets blob properties including metadata
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Blob properties</returns>
        Task<BlobItem> GetBlobPropertiesAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a blob exists
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if blob exists</returns>
        Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

        #endregion

        #region Blob Upload Operations

        /// <summary>
        /// Uploads a blob from a stream with optional content type
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="content">Stream containing blob data</param>
        /// <param name="contentType">Content type (MIME type) of the blob</param>
        /// <param name="overwrite">Whether to overwrite if blob exists</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UploadBlobAsync(
            string containerName, 
            string blobName, 
            Stream content, 
            string contentType = null,
            bool overwrite = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads a blob from a byte array with optional content type
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="content">Byte array containing blob data</param>
        /// <param name="contentType">Content type (MIME type) of the blob</param>
        /// <param name="overwrite">Whether to overwrite if blob exists</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UploadBlobAsync(
            string containerName, 
            string blobName, 
            byte[] content, 
            string contentType = null,
            bool overwrite = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads a blob from a local file path
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="filePath">Path to the local file</param>
        /// <param name="contentType">Content type (MIME type) of the blob</param>
        /// <param name="overwrite">Whether to overwrite if blob exists</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UploadBlobFromFileAsync(
            string containerName, 
            string blobName, 
            string filePath, 
            string contentType = null,
            bool overwrite = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads multiple blobs from local files in batch
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="files">Dictionary of blob names and file paths</param>
        /// <param name="overwrite">Whether to overwrite if blobs exist</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A comma-separated list of successfully uploaded blob names</returns>
        Task<string> UploadBlobsAsync(
            string containerName,
            IDictionary<string, string> files,
            bool overwrite = true,
            CancellationToken cancellationToken = default);

        #endregion

        #region Blob Download Operations

        /// <summary>
        /// Downloads a blob to a stream
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream containing blob data</returns>
        Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a blob as a byte array
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Byte array containing blob data</returns>
        Task<byte[]> DownloadBlobAsBytesAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a blob to a local file path
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="filePath">Path where the file will be saved</param>
        /// <param name="overwrite">Whether to overwrite if file exists</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DownloadBlobToFileAsync(
            string containerName, 
            string blobName, 
            string filePath, 
            bool overwrite = true, 
            CancellationToken cancellationToken = default);

        #endregion

        #region Blob Delete Operations

        /// <summary>
        /// Deletes a single blob
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes multiple blobs in batch
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobNames">Collection of blob names to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of blobs successfully deleted</returns>
        Task<string> DeleteBlobsAsync(string containerName, IEnumerable<string> blobNames, CancellationToken cancellationToken = default);

        #endregion

        #region Blob Copy & Move Operations

        /// <summary>
        /// Copies a blob within the same account or from a URL
        /// </summary>
        /// <param name="sourceContainer">Source container name</param>
        /// <param name="sourceBlobName">Source blob name</param>
        /// <param name="destinationContainer">Destination container name</param>
        /// <param name="destinationBlobName">Destination blob name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CopyBlobAsync(
            string sourceContainer, 
            string sourceBlobName, 
            string destinationContainer, 
            string destinationBlobName, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves a blob (copy then delete source)
        /// </summary>
        /// <param name="sourceContainer">Source container name</param>
        /// <param name="sourceBlobName">Source blob name</param>
        /// <param name="destinationContainer">Destination container name</param>
        /// <param name="destinationBlobName">Destination blob name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task MoveBlobAsync(
            string sourceContainer, 
            string sourceBlobName, 
            string destinationContainer, 
            string destinationBlobName, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Renames a blob within the same container
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="oldBlobName">Current blob name</param>
        /// <param name="newBlobName">New blob name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RenameBlobAsync(string containerName, string oldBlobName, string newBlobName, CancellationToken cancellationToken = default);

        #endregion

        #region Blob Metadata Operations

        /// <summary>
        /// Sets metadata on a blob
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="metadata">Metadata key-value pairs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SetBlobMetadataAsync(
            string containerName, 
            string blobName, 
            IDictionary<string, string> metadata, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets metadata from a blob
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Blob metadata</returns>
        Task<IDictionary<string, string>> GetBlobMetadataAsync(
            string containerName, 
            string blobName, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the content type of a blob
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="contentType">New content type (MIME type)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SetBlobContentTypeAsync(
            string containerName, 
            string blobName, 
            string contentType, 
            CancellationToken cancellationToken = default);

        #endregion

        #region Blob URL Operations

        /// <summary>
        /// Gets the public URL of a blob
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <returns>Public URL of the blob</returns>
        Task<string> GetBlobUrlAsync(string containerName, string blobName);

        /// <summary>
        /// Gets the URI of a blob
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <returns>URI of the blob</returns>
        Task<Uri> GetBlobUriAsync(string containerName, string blobName);

        #endregion

        #region SAS Token Operations

        /// <summary>
        /// Generates a SAS token for a blob with specified permissions
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="expiresIn">Duration until token expires</param>
        /// <param name="permissions">Blob permissions</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>SAS token</returns>
        Task<string> GenerateBlobSasTokenAsync(
            string containerName, 
            string blobName, 
            TimeSpan expiresIn, 
            BlobSasPermissions permissions = BlobSasPermissions.Read, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a SAS URL for a blob
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="expiresIn">Duration until URL expires</param>
        /// <param name="permissions">Blob permissions</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Complete SAS URL</returns>
        Task<string> GenerateBlobSasUrlAsync(
            string containerName, 
            string blobName, 
            TimeSpan expiresIn, 
            BlobSasPermissions permissions = BlobSasPermissions.Read, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a SAS token for a container
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="expiresIn">Duration until token expires</param>
        /// <param name="permissions">Container permissions</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Container SAS token</returns>
        Task<string> GenerateContainerSasTokenAsync(
            string containerName, 
            TimeSpan expiresIn, 
            BlobSasPermissions permissions = BlobSasPermissions.Read, 
            CancellationToken cancellationToken = default);

        #endregion

        #region Blob Storage Tier Operations

        /// <summary>
        /// Sets the access tier of a blob (Hot, Cool, Archive)
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="accessTier">Access tier to set</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SetBlobAccessTierAsync(
            string containerName, 
            string blobName, 
            string accessTier, 
            CancellationToken cancellationToken = default);

        #endregion

        #region Blob Snapshot Operations

        /// <summary>
        /// Creates a snapshot of a blob
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Snapshot identifier</returns>
        Task<string> CreateBlobSnapshotAsync(
            string containerName, 
            string blobName, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists all snapshots of a blob
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of snapshot identifiers</returns>
        Task<IEnumerable<string>> ListBlobSnapshotsAsync(
            string containerName, 
            string blobName, 
            CancellationToken cancellationToken = default);

        #endregion
    }
}
