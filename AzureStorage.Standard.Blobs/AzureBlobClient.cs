

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
		/// Creates a new instance of BlobClientWrapper
		/// </summary>

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
		public async Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default) {

			ValidateContainerName(containerName);

			try {
				var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
				return await containerClient.ExistsAsync(cancellationToken);
			}
			catch (RequestFailedException ex) {

				throw new AzureStorageException($"Failed to check if container '{containerName}' exists.", ex.ErrorCode, ex.Status, ex);
			}

		}


		/// <summary>
		///  Check if the blob exit in the container 
		/// </summary> 
		public async Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {

			// Validate the input to avoid exceptions and unnecessary API calls
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);
			try {
				return await blobClient.ExistsAsync(cancellationToken);

			}
			catch (RequestFailedException ex) {

				throw new AzureStorageException($"Failed to check if blob '{blobName}' exists in container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}
    
	     /// <summary>
		///   Check if the container does not exit if exit skip it 
		/// </summary> 
        public async Task<bool> CreateContainerIfNotExistsAsync(string containerName, CancellationToken cancellationToken = default)
        {
            ValidateContainerName(containerName);

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var response = await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                return response != null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to create container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

		public async  Task<bool> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default) {
			
			ValidateBlobName(containerName);

			try
			{
				var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
				  var response = await containerClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                return response.Value;
				
			}
			 catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
            }
		}
		


		public Task<string> CreateBlobSnapshotAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}



		public async Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			ValidateBlobName(blobName);

			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			try {
				var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
				return response.Value;
			}
			catch (RequestFailedException ex) {
				throw new AzureStorageException($"Failed to delete blob '{blobName}' from container '{containerName}'.", ex.ErrorCode, ex.Status, ex);
			}
		}

		// Delete more blobs
		public async Task<string> DeleteBlobsAsync(string containerName, IEnumerable<string> blobNames, CancellationToken cancellationToken = default) {
			ValidateContainerName(containerName);
			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var deleted = new List<string>();

			try {
				foreach (var name in blobNames) {
					var blobClient = containerClient.GetBlobClient(name);
					var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
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

		public async Task<byte[]> DownloadBlobAsBytesAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			using var stream = await DownloadBlobAsync(containerName, blobName, cancellationToken);
			if (stream == null)
				return null;

			using var memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream, cancellationToken);
			return memoryStream.ToArray();
		}
		
		
	   public async Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
        {
            ValidateContainerName(containerName);
            ValidateBlobName(blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
                var memoryStream = new MemoryStream();
                await response.Value.Content.CopyToAsync(memoryStream, cancellationToken);
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

		public Task DownloadBlobToFileAsync(string containerName, string blobName, string filePath, bool overwrite = true, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<string> GenerateBlobSasTokenAsync(string containerName, string blobName, TimeSpan expiresIn, BlobSasPermissions permissions = BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<string> GenerateBlobSasUrlAsync(string containerName, string blobName, TimeSpan expiresIn, BlobSasPermissions permissions = BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<string> GenerateContainerSasTokenAsync(string containerName, TimeSpan expiresIn, BlobSasPermissions permissions = BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<BlobItem> GetBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<IDictionary<string, string>> GetBlobMetadataAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<BlobItem> GetBlobPropertiesAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<Uri> GetBlobUriAsync(string containerName, string blobName) {
			throw new NotImplementedException();
		}

		public Task<string> GetBlobUrlAsync(string containerName, string blobName) {
			throw new NotImplementedException();
		}

		public Task<IDictionary<string, string>> GetContainerMetadataAsync(string containerName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<IDictionary<string, string>> GetContainerPropertiesAsync(string containerName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<IEnumerable<BlobItem>> ListBlobsAsync(string containerName, string prefix = null, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<IEnumerable<string>> ListBlobSnapshotsAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}


		public Task MoveBlobAsync(string sourceContainer, string sourceBlobName, string destinationContainer, string destinationBlobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task RenameBlobAsync(string containerName, string oldBlobName, string newBlobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task SetBlobAccessTierAsync(string containerName, string blobName, string accessTier, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task SetBlobContentTypeAsync(string containerName, string blobName, string contentType, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task SetBlobMetadataAsync(string containerName, string blobName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task SetContainerMetadataAsync(string containerName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType = null, bool overwrite = true, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task UploadBlobAsync(string containerName, string blobName, byte[] content, string contentType = null, bool overwrite = true, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task UploadBlobFromFileAsync(string containerName, string blobName, string filePath, string contentType = null, bool overwrite = true, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task UploadBlobsAsync(string containerName, IDictionary<string, string> files, bool overwrite = true, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}




		public Task CopyBlobAsync(string sourceContainer, string sourceBlobName, string destinationContainer, string destinationBlobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

     
		public Task<string> GenerateBlobSasTokenAsync(string containerName, string blobName, TimeSpan expiresIn, Core.Domain.Models.BlobSasPermissions permissions = Core.Domain.Models.BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<string> GenerateBlobSasUrlAsync(string containerName, string blobName, TimeSpan expiresIn, Core.Domain.Models.BlobSasPermissions permissions = Core.Domain.Models.BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<string> GenerateContainerSasTokenAsync(string containerName, TimeSpan expiresIn, Core.Domain.Models.BlobSasPermissions permissions = Core.Domain.Models.BlobSasPermissions.Read, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}


		#region Private Methods

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

		private static void ValidateBlobName(string blobName) {
			if (string.IsNullOrWhiteSpace(blobName)) {
				throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
			}
		}

		 private static void ValidateContainerName(string containerName) {
            if (string.IsNullOrWhiteSpace(containerName)) {
                throw new ArgumentException("Container name cannot be null or empty.", nameof(containerName));
            }
        }



		#endregion
	}
}