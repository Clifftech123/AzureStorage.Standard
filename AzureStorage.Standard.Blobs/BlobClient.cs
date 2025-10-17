

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Blobs
{
	public class BlobClient : IBlobClient {
		public Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task CopyBlobAsync(string sourceContainer, string sourceBlobName, string destinationContainer, string destinationBlobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<string> CreateBlobSnapshotAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<bool> CreateContainerIfNotExistsAsync(string containerName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<int> DeleteBlobsAsync(string containerName, IEnumerable<string> blobNames, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<bool> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		public Task<byte[]> DownloadBlobAsBytesAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
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

		public Task<IEnumerable<string>> ListContainersAsync(CancellationToken cancellationToken = default) {
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
	}
}