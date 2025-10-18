

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using AzureStorage.Standard.Blobs.Internal;
using AzureStorage.Standard.Core;
using AzureStorage.Standard.Core.Domain.Models;
using BlobSasPermissions = Azure.Storage.Sas.BlobSasPermissions;

namespace AzureStorage.Standard.Blobs
{
	  /// <summary>
      /// Azure Blob Storage client implementation
	  /// This wraps the Azure.Storage.Blobs library
      /// </summary>
	
      public class AzureBlobClient : IBlobClient {

		private readonly BlobServiceClient _blobServiceClient;
		private readonly StorageOptions _options;

		/// <summary>
		/// Initializes a new instance of the <see cref="AzureBlobClient"/> class.
		/// </summary>
		/// <param name="options">Configuration options for connecting to Azure Blob Storage. Supports connection string, service URI, or account name/key authentication.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when required options are invalid or missing.</exception>
		public AzureBlobClient(StorageOptions options) {
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_options.Validate();

			if (!string.IsNullOrWhiteSpace(_options.ConnectionString)) {
				_blobServiceClient = new BlobServiceClient(_options.ConnectionString);
			}
			else if (options.ServiceUri != null) {
				_blobServiceClient = new BlobServiceClient(options.ServiceUri);
			}

			else {
				var accountURL = new Uri($"https://{_options.AccountName}.blob.core.windows.net/");
				var credential = new StorageSharedKeyCredential(_options.AccountName, _options.AccountKey);
				_blobServiceClient = new BlobServiceClient(accountURL, credential);
			}
		}

		/// <summary>
		/// Lists all blob containers in the storage account.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>A collection of container names.</returns>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<IEnumerable<string>> ListContainersAsync(CancellationToken cancellationToken = default) {
			var containers = new List<string>();
			await foreach (var container in _blobServiceClient.GetBlobContainersAsync(cancellationToken: cancellationToken)) {
				containers.Add(container.Name);
			}
			return containers;
		}


		/// <summary>
		/// Checks if a blob container exists.
		/// </summary>
		/// <param name="containerName">The name of the container to check.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>True if the container exists; otherwise, false.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default) {

			ValidateContainerName(containerName);

			try {
				var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
				return await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => containerClient.ExistsAsync(cancellationToken));
			}
			catch (RequestFailedException ex) {

				throw new AzureStorageException($"Failed to check if container '{containerName}' exists.", ex.ErrorCode, ex.Status, ex);
			}

		}


		/// <summary>
		/// Checks if a blob exists in the specified container.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>True if the blob exists; otherwise, false.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {

			// Validate the input to avoid exceptions and unnecessary API calls
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);
			try {
				return await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.ExistsAsync(cancellationToken));

			}
			catch (RequestFailedException ex) {

				throw new AzureStorageException($"Failed to check if blob '{blobName}' exists in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}
    
		/// <summary>
		/// Creates a container if it does not already exist.
		/// </summary>
		/// <param name="containerName">The name of the container to create.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>True if the container was created; false if it already existed.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<bool> CreateContainerIfNotExistsAsync(string containerName, CancellationToken cancellationToken = default)
        {
            ValidateContainerName(containerName);

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken));
                return response != null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to create container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

		/// <summary>
		/// Deletes a container if it exists.
		/// </summary>
		/// <param name="containerName">The name of the container to delete.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>True if the container was deleted; false if it did not exist.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async  Task<bool> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default) {

			ValidateContainerName(containerName);

			try
			{
				var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
				  var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => containerClient.DeleteIfExistsAsync(cancellationToken: cancellationToken));
                return response.Value;
				
			}
			 catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
            }
		}
		


		/// <summary>
		/// Creates a snapshot of an existing blob.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The snapshot identifier.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<string> CreateBlobSnapshotAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.CreateSnapshotAsync(cancellationToken: cancellationToken));
				return response.Value.Snapshot;
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to create snapshot for blob '{blobName}' in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}



		/// <summary>
		/// Deletes a blob if it exists.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>True if the blob was deleted; false if it did not exist.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken));
				return response.Value;
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to delete blob '{blobName}' from container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Deletes multiple blobs in a container.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobNames">The collection of blob names to delete.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>A comma-separated list of successfully deleted blob names.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<string> DeleteBlobsAsync(string containerName, IEnumerable<string> blobNames, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var deleted = new List<string>();

			try {
				foreach (var name in blobNames) {
					var blobClient = containerClient.GetBlobClient(name);
					var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken));
					if (response.Value) {
						deleted.Add(name);
					}
				}

				// Return a comma-separated list of deleted blobs
				return string.Join(", ", deleted);
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to delete blobs '{string.Join(", ", blobNames)}' from container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
			
			
		}

		/// <summary>
		/// Downloads a blob as a byte array.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The blob content as a byte array, or null if the blob does not exist.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<byte[]> DownloadBlobAsBytesAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			using var stream = await DownloadBlobAsync(containerName, blobName, cancellationToken);
			if (stream == null)
				return null;

			using var memoryStream = new MemoryStream();
#if NETSTANDARD2_0
			await stream.CopyToAsync(memoryStream);
#else
			await stream.CopyToAsync(memoryStream, cancellationToken);
#endif
			return memoryStream.ToArray();
		}
		
		
		/// <summary>
		/// Downloads a blob to a stream.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>A stream containing the blob content, or null if the blob does not exist.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
        {
            ValidateContainerName(containerName);
            ValidateBlobName(blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                var response = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken));
                var memoryStream = new MemoryStream();
#if NETSTANDARD2_0
                await response.Value.Content.CopyToAsync(memoryStream);
#else
                await response.Value.Content.CopyToAsync(memoryStream, cancellationToken);
#endif
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
            {
                return null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to download blob '{blobName}' from container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

		/// <summary>
		/// Downloads a blob to a local file path.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="filePath">The local file path where the blob will be saved.</param>
		/// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/>, <paramref name="blobName"/>, or <paramref name="filePath"/> is null or empty.</exception>
		/// <exception cref="IOException">Thrown when the file already exists and overwrite is false.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task DownloadBlobToFileAsync(string containerName, string blobName, string filePath, bool overwrite = true, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			if (string.IsNullOrWhiteSpace(filePath)) {
				throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
			}

			if (!overwrite && File.Exists(filePath)) {
				throw new IOException($"File '{filePath}' already exists and overwrite is set to false.");
			}

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.DownloadToAsync(filePath, cancellationToken));
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to download blob '{blobName}' from container '{containerName}' to file '{filePath}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Generates a SAS token for a blob with specified permissions.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="expiresIn">The duration until the token expires.</param>
		/// <param name="permissions">The permissions to grant.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The SAS token.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the storage account is not configured for SAS token generation.</exception>
		public Task<string> GenerateBlobSasTokenAsync(string containerName, string blobName, TimeSpan expiresIn, BlobSasPermissions permissions = BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			if (string.IsNullOrWhiteSpace(_options.AccountName) || string.IsNullOrWhiteSpace(_options.AccountKey)) {
				throw new InvalidOperationException("Account name and key are required to generate SAS tokens.");
			}

			var sasBuilder = new BlobSasBuilder {
				BlobContainerName = containerName,
				BlobName = blobName,
				Resource = "b",
				StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
				ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn)
			};

			SetSasPermissions(sasBuilder, permissions);

			var credential = new StorageSharedKeyCredential(_options.AccountName, _options.AccountKey);
			var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

			return Task.FromResult(sasToken);
		}

		/// <summary>
		/// Generates a SAS URL for a blob.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="expiresIn">The duration until the URL expires.</param>
		/// <param name="permissions">The permissions to grant.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The complete SAS URL.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the storage account is not configured for SAS token generation.</exception>
		public async Task<string> GenerateBlobSasUrlAsync(string containerName, string blobName, TimeSpan expiresIn, BlobSasPermissions permissions = BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			var sasToken = await GenerateBlobSasTokenAsync(containerName, blobName, expiresIn, permissions, cancellationToken);
			var blobUrl = await GetBlobUrlAsync(containerName, blobName);

			return $"{blobUrl}?{sasToken}";
		}

		/// <summary>
		/// Generates a SAS token for a container with specified permissions.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="expiresIn">The duration until the token expires.</param>
		/// <param name="permissions">The permissions to grant.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The container SAS token.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the storage account is not configured for SAS token generation.</exception>
		public Task<string> GenerateContainerSasTokenAsync(string containerName, TimeSpan expiresIn, BlobSasPermissions permissions = BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);

			if (string.IsNullOrWhiteSpace(_options.AccountName) || string.IsNullOrWhiteSpace(_options.AccountKey)) {
				throw new InvalidOperationException("Account name and key are required to generate SAS tokens.");
			}

			var sasBuilder = new BlobSasBuilder {
				BlobContainerName = containerName,
				Resource = "c",
				StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
				ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn)
			};

			SetSasPermissions(sasBuilder, permissions);

			var credential = new StorageSharedKeyCredential(_options.AccountName, _options.AccountKey);
			var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

			return Task.FromResult(sasToken);
		}

		/// <summary>
		/// Gets a blob with its properties and metadata.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The blob item with properties and metadata.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<BlobItem> GetBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			return await GetBlobPropertiesAsync(containerName, blobName, cancellationToken);
		}

		/// <summary>
		/// Gets metadata from a blob.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The blob metadata as a dictionary.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<IDictionary<string, string>> GetBlobMetadataAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				var properties = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.GetPropertiesAsync(cancellationToken: cancellationToken));
				return new Dictionary<string, string>(properties.Value.Metadata, StringComparer.OrdinalIgnoreCase);
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to get metadata for blob '{blobName}' in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Gets blob properties including metadata.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The blob properties.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<BlobItem> GetBlobPropertiesAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				var properties = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.GetPropertiesAsync(cancellationToken: cancellationToken));
				return new BlobItem {
					Name = blobName,
					Container = containerName,
					Size = properties.Value.ContentLength,
					ContentType = properties.Value.ContentType,
					ContentMD5 = properties.Value.ContentHash != null ? Convert.ToBase64String(properties.Value.ContentHash) : null,
					CreatedOn = properties.Value.CreatedOn,
					LastModified = properties.Value.LastModified,
					ETag = properties.Value.ETag.ToString(),
					Metadata = new Dictionary<string, string>(properties.Value.Metadata, StringComparer.OrdinalIgnoreCase),
					IsDirectory = false,
					BlobType = properties.Value.BlobType.ToString()
				};
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to get properties for blob '{blobName}' in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Gets the URI of a blob.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <returns>The URI of the blob.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		public Task<Uri> GetBlobUriAsync(string containerName, string blobName) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			return Task.FromResult(blobClient.Uri);
		}

		/// <summary>
		/// Gets the URL of a blob.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <returns>The URL of the blob.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		public Task<string> GetBlobUrlAsync(string containerName, string blobName) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			return Task.FromResult(blobClient.Uri.ToString());
		}

		/// <summary>
		/// Gets metadata from a container.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The container metadata as a dictionary.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<IDictionary<string, string>> GetContainerMetadataAsync(string containerName, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

			try {
				var properties = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => containerClient.GetPropertiesAsync(cancellationToken: cancellationToken));
				return new Dictionary<string, string>(properties.Value.Metadata, StringComparer.OrdinalIgnoreCase);
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to get metadata for container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Gets container properties and metadata.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The container properties including metadata.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<IDictionary<string, string>> GetContainerPropertiesAsync(string containerName, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

			try {
				var properties = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => containerClient.GetPropertiesAsync(cancellationToken: cancellationToken));
				var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
					["ETag"] = properties.Value.ETag.ToString(),
					["LastModified"] = properties.Value.LastModified.ToString("O"),
					["LeaseStatus"] = properties.Value.LeaseStatus.ToString(),
					["LeaseState"] = properties.Value.LeaseState.ToString(),
					["HasImmutabilityPolicy"] = properties.Value.HasImmutabilityPolicy.ToString(),
					["HasLegalHold"] = properties.Value.HasLegalHold.ToString()
				};

				foreach (var metadata in properties.Value.Metadata) {
					result[metadata.Key] = metadata.Value;
				}

				return result;
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to get properties for container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Lists all blobs in a container with optional prefix filter.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="prefix">Optional prefix to filter blobs.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>A collection of blob items.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<IEnumerable<BlobItem>> ListBlobsAsync(string containerName, string prefix = null, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobs = new List<BlobItem>();

			try {
				await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken)) {
					blobs.Add(MapToBlobItem(blobItem, containerName));
				}
				return blobs;
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to list blobs in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Lists all snapshots of a blob.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>A collection of snapshot identifiers.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task<IEnumerable<string>> ListBlobSnapshotsAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var snapshots = new List<string>();

			try {
				await foreach (var blobItem in containerClient.GetBlobsAsync(
					prefix: blobName,
					states: Azure.Storage.Blobs.Models.BlobStates.Snapshots,
					cancellationToken: cancellationToken)) {
					if (blobItem.Name == blobName && blobItem.Snapshot != null) {
						snapshots.Add(blobItem.Snapshot);
					}
				}
				return snapshots;
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to list snapshots for blob '{blobName}' in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}


		/// <summary>
		/// Moves a blob from one location to another (copy then delete source).
		/// </summary>
		/// <param name="sourceContainer">The source container name.</param>
		/// <param name="sourceBlobName">The source blob name.</param>
		/// <param name="destinationContainer">The destination container name.</param>
		/// <param name="destinationBlobName">The destination blob name.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task MoveBlobAsync(string sourceContainer, string sourceBlobName, string destinationContainer, string destinationBlobName, CancellationToken cancellationToken = default) {
			await CopyBlobAsync(sourceContainer, sourceBlobName, destinationContainer, destinationBlobName, cancellationToken);
			await DeleteBlobAsync(sourceContainer, sourceBlobName, cancellationToken);
		}

		/// <summary>
		/// Renames a blob within the same container.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="oldBlobName">The current blob name.</param>
		/// <param name="newBlobName">The new blob name.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task RenameBlobAsync(string containerName, string oldBlobName, string newBlobName, CancellationToken cancellationToken = default) {
			await CopyBlobAsync(containerName, oldBlobName, containerName, newBlobName, cancellationToken);
			await DeleteBlobAsync(containerName, oldBlobName, cancellationToken);
		}

		/// <summary>
		/// Sets the access tier of a blob (Hot, Cool, or Archive).
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="accessTier">The access tier to set (Hot, Cool, Archive).</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task SetBlobAccessTierAsync(string containerName, string blobName, string accessTier, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			if (string.IsNullOrWhiteSpace(accessTier)) {
				throw new ArgumentException("Access tier cannot be null or empty.", nameof(accessTier));
			}

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.SetAccessTierAsync(new Azure.Storage.Blobs.Models.AccessTier(accessTier), cancellationToken: cancellationToken));
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to set access tier '{accessTier}' for blob '{blobName}' in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Sets the content type of a blob.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="contentType">The new content type (MIME type).</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task SetBlobContentTypeAsync(string containerName, string blobName, string contentType, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			if (string.IsNullOrWhiteSpace(contentType)) {
				throw new ArgumentException("Content type cannot be null or empty.", nameof(contentType));
			}

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				var properties = await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.GetPropertiesAsync(cancellationToken: cancellationToken));
				var headers = new Azure.Storage.Blobs.Models.BlobHttpHeaders {
					ContentType = contentType,
					ContentEncoding = properties.Value.ContentEncoding,
					ContentLanguage = properties.Value.ContentLanguage,
					ContentDisposition = properties.Value.ContentDisposition,
					CacheControl = properties.Value.CacheControl
				};
				await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.SetHttpHeadersAsync(headers, cancellationToken: cancellationToken));
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to set content type for blob '{blobName}' in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Sets metadata on a blob.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="metadata">The metadata key-value pairs to set.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is null.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task SetBlobMetadataAsync(string containerName, string blobName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			if (metadata == null) {
				throw new ArgumentNullException(nameof(metadata));
			}

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken));
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to set metadata for blob '{blobName}' in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Sets metadata on a container.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="metadata">The metadata key-value pairs to set.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is null.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task SetContainerMetadataAsync(string containerName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);

			if (metadata == null) {
				throw new ArgumentNullException(nameof(metadata));
			}

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

			try {
				await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => containerClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken));
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to set metadata for container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Uploads a blob from a stream.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="content">The stream containing the blob data.</param>
		/// <param name="contentType">The content type (MIME type) of the blob.</param>
		/// <param name="overwrite">Whether to overwrite the blob if it already exists.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType = null, bool overwrite = true, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			if (content == null) {
				throw new ArgumentNullException(nameof(content));
			}

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				var options = new Azure.Storage.Blobs.Models.BlobUploadOptions();
				if (!string.IsNullOrWhiteSpace(contentType)) {
					options.HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders {
						ContentType = contentType
					};
				}

				await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => blobClient.UploadAsync(content, options, cancellationToken));
			}
			catch (RequestFailedException ex) when (!overwrite && ex.ErrorCode == "BlobAlreadyExists") {
				throw new AzureStorageException($"Blob '{blobName}' already exists in container '{containerName}' and overwrite is set to false.", ex.ErrorCode, ex.Status, ex);
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to upload blob '{blobName}' to container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Uploads a blob from a byte array.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="content">The byte array containing the blob data.</param>
		/// <param name="contentType">The content type (MIME type) of the blob.</param>
		/// <param name="overwrite">Whether to overwrite the blob if it already exists.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task UploadBlobAsync(string containerName, string blobName, byte[] content, string contentType = null, bool overwrite = true, CancellationToken cancellationToken = default) {
			if (content == null) {
				throw new ArgumentNullException(nameof(content));
			}

			using var memoryStream = new MemoryStream(content);
			await UploadBlobAsync(containerName, blobName, memoryStream, contentType, overwrite, cancellationToken);
		}

		/// <summary>
		/// Uploads a blob from a local file.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="filePath">The local file path to upload.</param>
		/// <param name="contentType">The content type (MIME type) of the blob.</param>
		/// <param name="overwrite">Whether to overwrite the blob if it already exists.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/>, <paramref name="blobName"/>, or <paramref name="filePath"/> is null or empty.</exception>
		/// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task UploadBlobFromFileAsync(string containerName, string blobName, string filePath, string contentType = null, bool overwrite = true, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			if (string.IsNullOrWhiteSpace(filePath)) {
				throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
			}

			if (!File.Exists(filePath)) {
				throw new FileNotFoundException($"File '{filePath}' does not exist.", filePath);
			}

			using var fileStream = File.OpenRead(filePath);
			await UploadBlobAsync(containerName, blobName, fileStream, contentType, overwrite, cancellationToken);
		}

		/// <summary>
		/// Uploads multiple blobs from local files in batch.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="files">A dictionary where keys are blob names and values are local file paths.</param>
		/// <param name="overwrite">Whether to overwrite blobs if they already exist.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>A comma-separated list of successfully uploaded blob names.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="files"/> is null.</exception>
		/// <exception cref="AzureStorageException">Thrown when any upload operation fails.</exception>
		public async Task<string> UploadBlobsAsync(string containerName, IDictionary<string, string> files, bool overwrite = true, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);

			if (files == null) {
				throw new ArgumentNullException(nameof(files));
			}

			var uploaded = new List<string>();

			try {
				foreach (var file in files) {
					await UploadBlobFromFileAsync(containerName, file.Key, file.Value, null, overwrite, cancellationToken);
					uploaded.Add(file.Key);
				}

				// Return a comma-separated list of uploaded blobs
				return string.Join(", ", uploaded);
			}
			catch (Exception ex) {
				// If we have partial uploads, include that info in the error message
				var successMessage = uploaded.Count > 0
					? $" Successfully uploaded: {string.Join(", ", uploaded)}."
					: string.Empty;

				throw new AzureStorageException(
					$"Failed to upload all blobs to container '{containerName}'.{successMessage}",
					ex is RequestFailedException reqEx ? reqEx.ErrorCode : null,
					ex is RequestFailedException reqEx2 ? reqEx2.Status : 0,
					ex);
			}
		}




		/// <summary>
		/// Copies a blob from one location to another within the same storage account.
		/// </summary>
		/// <param name="sourceContainer">The source container name.</param>
		/// <param name="sourceBlobName">The source blob name.</param>
		/// <param name="destinationContainer">The destination container name.</param>
		/// <param name="destinationBlobName">The destination blob name.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
		/// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
		public async Task CopyBlobAsync(string sourceContainer, string sourceBlobName, string destinationContainer, string destinationBlobName, CancellationToken cancellationToken = default) {
			ValidateContainerName(sourceContainer);
			ValidateBlobName(sourceBlobName);
			ValidateContainerName(destinationContainer);
			ValidateBlobName(destinationBlobName);

			var sourceContainerClient = _blobServiceClient.GetBlobContainerClient(sourceContainer);
			var sourceBlobClient = sourceContainerClient.GetBlobClient(sourceBlobName);

			var destinationContainerClient = _blobServiceClient.GetBlobContainerClient(destinationContainer);
			var destinationBlobClient = destinationContainerClient.GetBlobClient(destinationBlobName);

			try {
				await RetryPolicyHelper.ExecuteWithRetryAsync(_options.RetryOptions, () => destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri));
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to copy blob '{sourceBlobName}' from container '{sourceContainer}' to '{destinationBlobName}' in container '{destinationContainer}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		/// <summary>
		/// Disposes the resources used by this client.
		/// </summary>
		/// <remarks>
		/// The BlobServiceClient does not implement IDisposable, so this method is a no-op.
		/// It's included to satisfy the IDisposable interface requirement.
		/// </remarks>
		public void Dispose() {
			// BlobServiceClient does not require disposal
			// This method is implemented to satisfy the IDisposable interface
		}


		/// <summary>
		/// Generates a SAS token for a blob with specified permissions.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="expiresIn">The duration until the token expires.</param>
		/// <param name="permissions">The permissions to grant.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The SAS token.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the storage account is not configured for SAS token generation.</exception>
		public Task<string> GenerateBlobSasTokenAsync(string containerName, string blobName, TimeSpan expiresIn, Core.Domain.Models.BlobSasPermissions permissions = Core.Domain.Models.BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			// Convert Core.Domain.Models.BlobSasPermissions to Azure.Storage.Sas.BlobSasPermissions
			var azurePermissions = ConvertToAzureSasPermissions(permissions);
			return GenerateBlobSasTokenAsync(containerName, blobName, expiresIn, azurePermissions, cancellationToken);
		}

		/// <summary>
		/// Generates a SAS URL for a blob.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <param name="expiresIn">The duration until the URL expires.</param>
		/// <param name="permissions">The permissions to grant.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The complete SAS URL.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> or <paramref name="blobName"/> is null or empty.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the storage account is not configured for SAS token generation.</exception>
		public Task<string> GenerateBlobSasUrlAsync(string containerName, string blobName, TimeSpan expiresIn, Core.Domain.Models.BlobSasPermissions permissions = Core.Domain.Models.BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			// Convert Core.Domain.Models.BlobSasPermissions to Azure.Storage.Sas.BlobSasPermissions
			var azurePermissions = ConvertToAzureSasPermissions(permissions);
			return GenerateBlobSasUrlAsync(containerName, blobName, expiresIn, azurePermissions, cancellationToken);
		}

		/// <summary>
		/// Generates a SAS token for a container with specified permissions.
		/// </summary>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="expiresIn">The duration until the token expires.</param>
		/// <param name="permissions">The permissions to grant.</param>
		/// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
		/// <returns>The container SAS token.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="containerName"/> is null or empty.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the storage account is not configured for SAS token generation.</exception>
		public Task<string> GenerateContainerSasTokenAsync(string containerName, TimeSpan expiresIn, Core.Domain.Models.BlobSasPermissions permissions = Core.Domain.Models.BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			// Convert Core.Domain.Models.BlobSasPermissions to Azure.Storage.Sas.BlobSasPermissions
			var azurePermissions = ConvertToAzureSasPermissions(permissions);
			return GenerateContainerSasTokenAsync(containerName, expiresIn, azurePermissions, cancellationToken);
		}


		#region Private Methods

		/// <summary>
		/// Converts custom BlobSasPermissions to Azure SDK BlobSasPermissions.
		/// </summary>
		private static BlobSasPermissions ConvertToAzureSasPermissions(Core.Domain.Models.BlobSasPermissions permissions) {
			BlobSasPermissions azurePermissions = default;

			if (permissions.HasFlag(Core.Domain.Models.BlobSasPermissions.Read))
				azurePermissions |= BlobSasPermissions.Read;
			if (permissions.HasFlag(Core.Domain.Models.BlobSasPermissions.Add))
				azurePermissions |= BlobSasPermissions.Add;
			if (permissions.HasFlag(Core.Domain.Models.BlobSasPermissions.Create))
				azurePermissions |= BlobSasPermissions.Create;
			if (permissions.HasFlag(Core.Domain.Models.BlobSasPermissions.Write))
				azurePermissions |= BlobSasPermissions.Write;
			if (permissions.HasFlag(Core.Domain.Models.BlobSasPermissions.Delete))
				azurePermissions |= BlobSasPermissions.Delete;
			if (permissions.HasFlag(Core.Domain.Models.BlobSasPermissions.List))
				azurePermissions |= BlobSasPermissions.List;

			return azurePermissions;
		}

		/// <summary>
		/// Maps Azure SDK BlobItem to custom BlobItem model.
		/// </summary>
		private static BlobItem MapToBlobItem(Azure.Storage.Blobs.Models.BlobItem blobItem, string containerName)
		{
			return new BlobItem
			{
				Name = blobItem.Name,
				Container = containerName,
				Size = blobItem.Properties.ContentLength.GetValueOrDefault(),
				ContentType = blobItem.Properties.ContentType,
				ContentMD5 = blobItem.Properties.ContentHash != null ? Convert.ToBase64String(blobItem.Properties.ContentHash) : null,
				CreatedOn = blobItem.Properties.CreatedOn,
				LastModified = blobItem.Properties.LastModified,
				ETag = blobItem.Properties.ETag?.ToString(),
				Metadata = blobItem.Metadata?.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
				IsDirectory = false,
				BlobType = blobItem.Properties.BlobType?.ToString()
			};
		}

		/// <summary>
		/// Sets the SAS permissions on a BlobSasBuilder.
		/// </summary>
		private static void SetSasPermissions(BlobSasBuilder sasBuilder, BlobSasPermissions permissions)
		{
			// Build a combined BlobSasPermissions value and set it once on the builder.
			BlobSasPermissions combined = default(BlobSasPermissions);

			if (permissions.HasFlag(BlobSasPermissions.Read))
				combined |= BlobSasPermissions.Read;
			if (permissions.HasFlag(BlobSasPermissions.Add))
				combined |= BlobSasPermissions.Add;
			if (permissions.HasFlag(BlobSasPermissions.Create))
				combined |= BlobSasPermissions.Create;
			if (permissions.HasFlag(BlobSasPermissions.Write))
				combined |= BlobSasPermissions.Write;
			if (permissions.HasFlag(BlobSasPermissions.Delete))
				combined |= BlobSasPermissions.Delete;
			if (permissions.HasFlag(BlobSasPermissions.List))
				combined |= BlobSasPermissions.List;

			sasBuilder.SetPermissions(combined);
		}
		#endregion




		#region Private Helpers

		/// <summary>
		/// Validates that the blob name is not null or empty.
		/// </summary>
		/// <param name="blobName">The blob name to validate.</param>
		/// <exception cref="ArgumentException">Thrown when the blob name is null or empty.</exception>
		private static void ValidateBlobName(string blobName) {
			if (string.IsNullOrWhiteSpace(blobName)) {
				throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
			}
		}

		/// <summary>
		/// Validates that the container name is not null or empty.
		/// </summary>
		/// <param name="containerName">The container name to validate.</param>
		/// <exception cref="ArgumentException">Thrown when the container name is null or empty.</exception>
		private static void ValidateContainerName(string containerName) {
            if (string.IsNullOrWhiteSpace(containerName)) {
                throw new ArgumentException("Container name cannot be null or empty.", nameof(containerName));
            }
        }



		#endregion
	}
}