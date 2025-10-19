using AzureStorage.Standard.Blobs;
using AzureStorage.Standard.Core.Domain.Models;
using FluentAssertions;
using Xunit;

namespace AzureStroage.Standard.Tests.IntegrationTests;

/// <summary>
/// Integration tests for Blob Client that connect to Azurite (Azure Storage Emulator).
///
/// Prerequisites to run these tests:
/// 1. Install Azurite: npm install -g azurite
/// 2. Start Azurite: azurite --silent --location c:\azurite --debug c:\azurite\debug.log
/// 3. Tests will use connection string: "UseDevelopmentStorage=true"
///
/// These tests verify actual Azure Blob Storage operations against a local emulator.
/// </summary>
[Collection("Integration Tests")]
public class BlobClientIntegrationTests : IAsyncLifetime
{
    private AzureBlobClient? _blobClient;
    private const string TestContainerName = "test-container-integration";

    // Azurite connection string (local emulator)
    private const string ConnectionString = "UseDevelopmentStorage=true";

    public async Task InitializeAsync()
    {
        // Skip tests if Azurite is not running
        if (string.IsNullOrEmpty(ConnectionString))
        {
            return;
        }

        var options = new StorageOptions
        {
            ConnectionString = ConnectionString
        };

        _blobClient = new AzureBlobClient(options);

        // Create test container
        await _blobClient.CreateContainerIfNotExistsAsync(TestContainerName);
    }

    public async Task DisposeAsync()
    {
        if (_blobClient != null)
        {
            // Clean up test container
            try
            {
                await _blobClient.DeleteContainerAsync(TestContainerName);
            }
            catch
            {
                // Ignore cleanup errors
            }
            _blobClient.Dispose();
        }
    }

    #region Container Operations Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateContainerIfNotExistsAsync_ShouldCreateNewContainer()
    {
        // Arrange
        const string containerName = "test-create-container";

        try
        {
            // Act
            var result = await _blobClient!.CreateContainerIfNotExistsAsync(containerName);

            // Assert
            result.Should().BeTrue();

            var exists = await _blobClient.ContainerExistsAsync(containerName);
            exists.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            await _blobClient!.DeleteContainerAsync(containerName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ListContainersAsync_ShouldReturnContainers()
    {
        // Act
        var containers = await _blobClient!.ListContainersAsync();

        // Assert
        containers.Should().Contain(TestContainerName);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ContainerExistsAsync_ShouldReturnTrue_WhenContainerExists()
    {
        // Act
        var exists = await _blobClient!.ContainerExistsAsync(TestContainerName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ContainerExistsAsync_ShouldReturnFalse_WhenContainerDoesNotExist()
    {
        // Act
        var exists = await _blobClient!.ContainerExistsAsync("non-existent-container-12345");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SetAndGetContainerMetadata_ShouldWorkCorrectly()
    {
        // Arrange
        var metadata = new Dictionary<string, string>
        {
            { "Environment", "Testing" },
            { "Owner", "IntegrationTests" }
        };

        // Act
        await _blobClient!.SetContainerMetadataAsync(TestContainerName, metadata);
        var retrievedMetadata = await _blobClient.GetContainerMetadataAsync(TestContainerName);

        // Assert
        retrievedMetadata.Should().ContainKey("environment"); // Note: Azure lowercases metadata keys
        retrievedMetadata.Should().ContainKey("owner");
    }

    #endregion

    #region Blob Upload Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UploadBlobAsync_FromStream_ShouldUploadSuccessfully()
    {
        // Arrange
        const string blobName = "test-upload-stream.txt";
        const string content = "Test content for integration test from stream";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        try
        {
            // Act
            await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream);

            // Assert
            var exists = await _blobClient.BlobExistsAsync(TestContainerName, blobName);
            exists.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            await _blobClient!.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UploadBlobAsync_FromByteArray_ShouldUploadSuccessfully()
    {
        // Arrange
        const string blobName = "test-upload-bytes.txt";
        var content = System.Text.Encoding.UTF8.GetBytes("Test content from byte array");

        try
        {
            // Act
            await _blobClient!.UploadBlobAsync(TestContainerName, blobName, content);

            // Assert
            var exists = await _blobClient.BlobExistsAsync(TestContainerName, blobName);
            exists.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            await _blobClient!.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UploadBlobAsync_WithContentType_ShouldSetContentType()
    {
        // Arrange
        const string blobName = "test-content-type.json";
        const string content = "{\"test\": \"data\"}";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        try
        {
            // Act
            await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream, "application/json");

            // Assert
            var properties = await _blobClient.GetBlobPropertiesAsync(TestContainerName, blobName);
            properties.ContentType.Should().Be("application/json");
        }
        finally
        {
            // Cleanup
            await _blobClient!.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    #endregion

    #region Blob Download Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DownloadBlobAsync_ShouldDownloadCorrectContent()
    {
        // Arrange
        const string blobName = "test-download.txt";
        const string content = "Test content for download";
        using var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _blobClient!.UploadBlobAsync(TestContainerName, blobName, uploadStream);

        try
        {
            // Act
            var downloadStream = await _blobClient.DownloadBlobAsync(TestContainerName, blobName);

            // Assert
            using var reader = new StreamReader(downloadStream);
            var downloadedContent = await reader.ReadToEndAsync();
            downloadedContent.Should().Be(content);
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DownloadBlobAsBytesAsync_ShouldDownloadCorrectBytes()
    {
        // Arrange
        const string blobName = "test-download-bytes.txt";
        var content = System.Text.Encoding.UTF8.GetBytes("Test content");
        await _blobClient!.UploadBlobAsync(TestContainerName, blobName, content);

        try
        {
            // Act
            var downloadedBytes = await _blobClient.DownloadBlobAsBytesAsync(TestContainerName, blobName);

            // Assert
            downloadedBytes.Should().BeEquivalentTo(content);
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    #endregion

    #region Blob List Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ListBlobsAsync_ShouldReturnUploadedBlobs()
    {
        // Arrange
        var blobNames = new[] { "blob1.txt", "blob2.txt", "blob3.txt" };
        foreach (var blobName in blobNames)
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes($"Content of {blobName}"));
            await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream);
        }

        try
        {
            // Act
            var blobs = await _blobClient!.ListBlobsAsync(TestContainerName);

            // Assert
            var blobList = blobs.ToList();
            blobList.Should().HaveCountGreaterOrEqualTo(3);
            blobList.Select(b => b.Name).Should().Contain(blobNames);
        }
        finally
        {
            // Cleanup
            foreach (var blobName in blobNames)
            {
                await _blobClient!.DeleteBlobAsync(TestContainerName, blobName);
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ListBlobsAsync_WithPrefix_ShouldReturnFilteredBlobs()
    {
        // Arrange
        var blobNames = new[] { "reports/report1.pdf", "reports/report2.pdf", "data/data.csv" };
        foreach (var blobName in blobNames)
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes($"Content of {blobName}"));
            await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream);
        }

        try
        {
            // Act
            var blobs = await _blobClient!.ListBlobsAsync(TestContainerName, "reports/");

            // Assert
            var blobList = blobs.ToList();
            blobList.Should().HaveCountGreaterOrEqualTo(2);
            blobList.Should().OnlyContain(b => b.Name.StartsWith("reports/"));
        }
        finally
        {
            // Cleanup
            foreach (var blobName in blobNames)
            {
                await _blobClient!.DeleteBlobAsync(TestContainerName, blobName);
            }
        }
    }

    #endregion

    #region Blob Delete Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteBlobAsync_ShouldRemoveBlob()
    {
        // Arrange
        const string blobName = "test-delete.txt";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Content to delete"));
        await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream);

        // Act
        var result = await _blobClient.DeleteBlobAsync(TestContainerName, blobName);

        // Assert
        result.Should().BeTrue();
        var exists = await _blobClient.BlobExistsAsync(TestContainerName, blobName);
        exists.Should().BeFalse();
    }

    #endregion

    #region Blob Copy Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CopyBlobAsync_ShouldCreateCopyOfBlob()
    {
        // Arrange
        const string sourceBlobName = "source-blob.txt";
        const string destBlobName = "dest-blob.txt";
        const string content = "Content to copy";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _blobClient!.UploadBlobAsync(TestContainerName, sourceBlobName, stream);

        try
        {
            // Act
            await _blobClient.CopyBlobAsync(TestContainerName, sourceBlobName, TestContainerName, destBlobName);

            // Assert
            var destExists = await _blobClient.BlobExistsAsync(TestContainerName, destBlobName);
            destExists.Should().BeTrue();

            var destStream = await _blobClient.DownloadBlobAsync(TestContainerName, destBlobName);
            using var reader = new StreamReader(destStream);
            var destContent = await reader.ReadToEndAsync();
            destContent.Should().Be(content);
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, sourceBlobName);
            await _blobClient.DeleteBlobAsync(TestContainerName, destBlobName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task MoveBlobAsync_ShouldMoveBlob()
    {
        // Arrange
        const string sourceBlobName = "move-source.txt";
        const string destBlobName = "move-dest.txt";
        const string content = "Content to move";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _blobClient!.UploadBlobAsync(TestContainerName, sourceBlobName, stream);

        try
        {
            // Act
            await _blobClient.MoveBlobAsync(TestContainerName, sourceBlobName, TestContainerName, destBlobName);

            // Assert
            var sourceExists = await _blobClient.BlobExistsAsync(TestContainerName, sourceBlobName);
            sourceExists.Should().BeFalse(); // Source should be deleted

            var destExists = await _blobClient.BlobExistsAsync(TestContainerName, destBlobName);
            destExists.Should().BeTrue(); // Destination should exist
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, destBlobName);
        }
    }

    #endregion

    #region Blob Metadata Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SetAndGetBlobMetadata_ShouldWorkCorrectly()
    {
        // Arrange
        const string blobName = "metadata-test.txt";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Content with metadata"));
        await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream);

        var metadata = new Dictionary<string, string>
        {
            { "Author", "John Doe" },
            { "Department", "IT" },
            { "Version", "1.0" }
        };

        try
        {
            // Act
            await _blobClient.SetBlobMetadataAsync(TestContainerName, blobName, metadata);
            var retrievedMetadata = await _blobClient.GetBlobMetadataAsync(TestContainerName, blobName);

            // Assert
            retrievedMetadata.Should().ContainKey("author"); // Azure lowercases keys
            retrievedMetadata.Should().ContainKey("department");
            retrievedMetadata.Should().ContainKey("version");
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SetBlobContentTypeAsync_ShouldUpdateContentType()
    {
        // Arrange
        const string blobName = "content-type-test.txt";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}"));
        await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream);

        try
        {
            // Act
            await _blobClient.SetBlobContentTypeAsync(TestContainerName, blobName, "application/json");

            // Assert
            var properties = await _blobClient.GetBlobPropertiesAsync(TestContainerName, blobName);
            properties.ContentType.Should().Be("application/json");
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    #endregion

    #region Blob Properties Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetBlobPropertiesAsync_ShouldReturnCorrectProperties()
    {
        // Arrange
        const string blobName = "properties-test.txt";
        const string content = "Content for properties test";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream, "text/plain");

        try
        {
            // Act
            var properties = await _blobClient.GetBlobPropertiesAsync(TestContainerName, blobName);

            // Assert
            properties.Should().NotBeNull();
            properties.Name.Should().Be(blobName);
            properties.Size.Should().Be(content.Length);
            properties.ContentType.Should().Be("text/plain");
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task BlobExistsAsync_ShouldReturnTrue_WhenBlobExists()
    {
        // Arrange
        const string blobName = "exists-test.txt";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Content"));
        await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream);

        try
        {
            // Act
            var exists = await _blobClient.BlobExistsAsync(TestContainerName, blobName);

            // Assert
            exists.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    #endregion

    #region Blob URL Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetBlobUrlAsync_ShouldReturnValidUrl()
    {
        // Arrange
        const string blobName = "url-test.txt";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Content"));
        await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream);

        try
        {
            // Act
            var url = await _blobClient.GetBlobUrlAsync(TestContainerName, blobName);

            // Assert
            url.Should().NotBeNullOrEmpty();
            url.Should().Contain(TestContainerName);
            url.Should().Contain(blobName);
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetBlobUriAsync_ShouldReturnValidUri()
    {
        // Arrange
        const string blobName = "uri-test.txt";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Content"));
        await _blobClient!.UploadBlobAsync(TestContainerName, blobName, stream);

        try
        {
            // Act
            var uri = await _blobClient.GetBlobUriAsync(TestContainerName, blobName);

            // Assert
            uri.Should().NotBeNull();
            uri.ToString().Should().Contain(TestContainerName);
            uri.ToString().Should().Contain(blobName);
        }
        finally
        {
            // Cleanup
            await _blobClient.DeleteBlobAsync(TestContainerName, blobName);
        }
    }

    #endregion
}
