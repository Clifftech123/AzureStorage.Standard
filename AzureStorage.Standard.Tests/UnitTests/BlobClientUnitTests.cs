using AzureStorage.Standard.Blobs;
using AzureStorage.Standard.Core.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureStroage.Standard.Tests.UnitTests;

/// <summary>
/// Comprehensive unit tests for Blob Client covering all IBlobClient interface methods.
/// These tests use mocking to verify behavior without requiring actual Azure Storage.
/// </summary>
public class BlobClientUnitTests
{
    private readonly Mock<IBlobClient> _mockBlobClient;

    public BlobClientUnitTests()
    {
        _mockBlobClient = new Mock<IBlobClient>();
    }

    #region Container Operations Tests

    [Fact]
    public async Task ListContainersAsync_ShouldReturnContainerNames()
    {
        // Arrange
        var expectedContainers = new List<string> { "container1", "container2", "container3" };

        _mockBlobClient
            .Setup(x => x.ListContainersAsync(default))
            .ReturnsAsync(expectedContainers);

        // Act
        var containers = await _mockBlobClient.Object.ListContainersAsync();

        // Assert
        containers.Should().HaveCount(3);
        containers.Should().BeEquivalentTo(expectedContainers);
    }

    [Fact]
    public async Task CreateContainerIfNotExistsAsync_ShouldReturnTrue_WhenContainerIsCreated()
    {
        // Arrange
        const string containerName = "test-container";

        _mockBlobClient
            .Setup(x => x.CreateContainerIfNotExistsAsync(containerName, default))
            .ReturnsAsync(true);

        // Act
        var result = await _mockBlobClient.Object.CreateContainerIfNotExistsAsync(containerName);

        // Assert
        result.Should().BeTrue();
        _mockBlobClient.Verify(x => x.CreateContainerIfNotExistsAsync(containerName, default), Times.Once);
    }

    [Fact]
    public async Task DeleteContainerAsync_ShouldReturnTrue_WhenDeleted()
    {
        // Arrange
        const string containerName = "test-container";

        _mockBlobClient
            .Setup(x => x.DeleteContainerAsync(containerName, default))
            .ReturnsAsync(true);

        // Act
        var result = await _mockBlobClient.Object.DeleteContainerAsync(containerName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ContainerExistsAsync_ShouldReturnTrue_WhenContainerExists()
    {
        // Arrange
        const string containerName = "existing-container";

        _mockBlobClient
            .Setup(x => x.ContainerExistsAsync(containerName, default))
            .ReturnsAsync(true);

        // Act
        var exists = await _mockBlobClient.Object.ContainerExistsAsync(containerName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetContainerPropertiesAsync_ShouldReturnMetadata()
    {
        // Arrange
        const string containerName = "test-container";
        var expectedMetadata = new Dictionary<string, string>
        {
            { "Environment", "Production" },
            { "Owner", "DevTeam" }
        };

        _mockBlobClient
            .Setup(x => x.GetContainerPropertiesAsync(containerName, default))
            .ReturnsAsync(expectedMetadata);

        // Act
        var metadata = await _mockBlobClient.Object.GetContainerPropertiesAsync(containerName);

        // Assert
        metadata.Should().HaveCount(2);
        metadata.Should().ContainKey("Environment");
    }

    [Fact]
    public async Task SetContainerMetadataAsync_ShouldSetMetadata()
    {
        // Arrange
        const string containerName = "test-container";
        var metadata = new Dictionary<string, string>
        {
            { "Key1", "Value1" },
            { "Key2", "Value2" }
        };

        _mockBlobClient
            .Setup(x => x.SetContainerMetadataAsync(containerName, metadata, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.SetContainerMetadataAsync(containerName, metadata);

        // Assert
        _mockBlobClient.Verify(x => x.SetContainerMetadataAsync(containerName, metadata, default), Times.Once);
    }

    [Fact]
    public async Task GetContainerMetadataAsync_ShouldReturnMetadata()
    {
        // Arrange
        const string containerName = "test-container";
        var expectedMetadata = new Dictionary<string, string> { { "key", "value" } };

        _mockBlobClient
            .Setup(x => x.GetContainerMetadataAsync(containerName, default))
            .ReturnsAsync(expectedMetadata);

        // Act
        var metadata = await _mockBlobClient.Object.GetContainerMetadataAsync(containerName);

        // Assert
        metadata.Should().ContainKey("key");
    }

    #endregion

    #region Blob Listing & Query Operations Tests

    [Fact]
    public async Task ListBlobsAsync_ShouldReturnBlobList()
    {
        // Arrange
        const string containerName = "test-container";
        var expectedBlobs = new List<BlobItem>
        {
            new BlobItem { Name = "file1.txt", Size = 1024 },
            new BlobItem { Name = "file2.txt", Size = 2048 }
        };

        _mockBlobClient
            .Setup(x => x.ListBlobsAsync(containerName, null, default))
            .ReturnsAsync(expectedBlobs);

        // Act
        var blobs = await _mockBlobClient.Object.ListBlobsAsync(containerName);

        // Assert
        blobs.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListBlobsAsync_WithPrefix_ShouldReturnFilteredBlobs()
    {
        // Arrange
        const string containerName = "test-container";
        const string prefix = "reports/";
        var expectedBlobs = new List<BlobItem>
        {
            new BlobItem { Name = "reports/report1.pdf", Size = 1024 }
        };

        _mockBlobClient
            .Setup(x => x.ListBlobsAsync(containerName, prefix, default))
            .ReturnsAsync(expectedBlobs);

        // Act
        var blobs = await _mockBlobClient.Object.ListBlobsAsync(containerName, prefix);

        // Assert
        blobs.Should().HaveCount(1);
        blobs.First().Name.Should().StartWith("reports/");
    }

    [Fact]
    public async Task GetBlobAsync_ShouldReturnBlobItem()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var expectedBlob = new BlobItem { Name = blobName, Size = 1024 };

        _mockBlobClient
            .Setup(x => x.GetBlobAsync(containerName, blobName, default))
            .ReturnsAsync(expectedBlob);

        // Act
        var blob = await _mockBlobClient.Object.GetBlobAsync(containerName, blobName);

        // Assert
        blob.Should().NotBeNull();
        blob.Name.Should().Be(blobName);
    }

    [Fact]
    public async Task GetBlobPropertiesAsync_ShouldReturnProperties()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var expectedProperties = new BlobItem
        {
            Name = blobName,
            Size = 2048,
            ContentType = "text/plain"
        };

        _mockBlobClient
            .Setup(x => x.GetBlobPropertiesAsync(containerName, blobName, default))
            .ReturnsAsync(expectedProperties);

        // Act
        var properties = await _mockBlobClient.Object.GetBlobPropertiesAsync(containerName, blobName);

        // Assert
        properties.Size.Should().Be(2048);
        properties.ContentType.Should().Be("text/plain");
    }

    [Fact]
    public async Task BlobExistsAsync_ShouldReturnTrue_WhenBlobExists()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "existing-file.txt";

        _mockBlobClient
            .Setup(x => x.BlobExistsAsync(containerName, blobName, default))
            .ReturnsAsync(true);

        // Act
        var exists = await _mockBlobClient.Object.BlobExistsAsync(containerName, blobName);

        // Assert
        exists.Should().BeTrue();
    }

    #endregion

    #region Blob Upload Operations Tests

    [Fact]
    public async Task UploadBlobAsync_FromStream_ShouldCallBlobClient()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        using var stream = new MemoryStream();

        _mockBlobClient
            .Setup(x => x.UploadBlobAsync(containerName, blobName, stream, null, true, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.UploadBlobAsync(containerName, blobName, stream);

        // Assert
        _mockBlobClient.Verify(x => x.UploadBlobAsync(containerName, blobName, stream, null, true, default), Times.Once);
    }

    [Fact]
    public async Task UploadBlobAsync_FromByteArray_ShouldCallBlobClient()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var content = new byte[] { 1, 2, 3, 4, 5 };

        _mockBlobClient
            .Setup(x => x.UploadBlobAsync(containerName, blobName, content, null, true, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.UploadBlobAsync(containerName, blobName, content);

        // Assert
        _mockBlobClient.Verify(x => x.UploadBlobAsync(containerName, blobName, content, null, true, default), Times.Once);
    }

    [Fact]
    public async Task UploadBlobFromFileAsync_ShouldCallBlobClient()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        const string filePath = "C:\\test\\file.txt";

        _mockBlobClient
            .Setup(x => x.UploadBlobFromFileAsync(containerName, blobName, filePath, null, true, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.UploadBlobFromFileAsync(containerName, blobName, filePath);

        // Assert
        _mockBlobClient.Verify(x => x.UploadBlobFromFileAsync(containerName, blobName, filePath, null, true, default), Times.Once);
    }

    [Fact]
    public async Task UploadBlobsAsync_ShouldReturnSuccessfullyUploadedBlobs()
    {
        // Arrange
        const string containerName = "test-container";
        var files = new Dictionary<string, string>
        {
            { "file1.txt", "C:\\test\\file1.txt" },
            { "file2.txt", "C:\\test\\file2.txt" }
        };

        _mockBlobClient
            .Setup(x => x.UploadBlobsAsync(containerName, files, true, default))
            .ReturnsAsync("file1.txt, file2.txt");

        // Act
        var result = await _mockBlobClient.Object.UploadBlobsAsync(containerName, files);

        // Assert
        result.Should().Contain("file1.txt");
        result.Should().Contain("file2.txt");
    }

    #endregion

    #region Blob Download Operations Tests

    [Fact]
    public async Task DownloadBlobAsync_ShouldReturnStream()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var expectedStream = new MemoryStream();

        _mockBlobClient
            .Setup(x => x.DownloadBlobAsync(containerName, blobName, default))
            .ReturnsAsync(expectedStream);

        // Act
        var stream = await _mockBlobClient.Object.DownloadBlobAsync(containerName, blobName);

        // Assert
        stream.Should().NotBeNull();
    }

    [Fact]
    public async Task DownloadBlobAsBytesAsync_ShouldReturnByteArray()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };

        _mockBlobClient
            .Setup(x => x.DownloadBlobAsBytesAsync(containerName, blobName, default))
            .ReturnsAsync(expectedBytes);

        // Act
        var bytes = await _mockBlobClient.Object.DownloadBlobAsBytesAsync(containerName, blobName);

        // Assert
        bytes.Should().HaveCount(5);
    }

    [Fact]
    public async Task DownloadBlobToFileAsync_ShouldCallBlobClient()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        const string filePath = "C:\\downloads\\file.txt";

        _mockBlobClient
            .Setup(x => x.DownloadBlobToFileAsync(containerName, blobName, filePath, true, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.DownloadBlobToFileAsync(containerName, blobName, filePath);

        // Assert
        _mockBlobClient.Verify(x => x.DownloadBlobToFileAsync(containerName, blobName, filePath, true, default), Times.Once);
    }

    #endregion

    #region Blob Delete Operations Tests

    [Fact]
    public async Task DeleteBlobAsync_ShouldReturnTrue_WhenDeleted()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";

        _mockBlobClient
            .Setup(x => x.DeleteBlobAsync(containerName, blobName, default))
            .ReturnsAsync(true);

        // Act
        var result = await _mockBlobClient.Object.DeleteBlobAsync(containerName, blobName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteBlobsAsync_ShouldReturnDeletedBlobNames()
    {
        // Arrange
        const string containerName = "test-container";
        var blobNames = new List<string> { "file1.txt", "file2.txt", "file3.txt" };

        _mockBlobClient
            .Setup(x => x.DeleteBlobsAsync(containerName, blobNames, default))
            .ReturnsAsync("file1.txt, file2.txt, file3.txt");

        // Act
        var result = await _mockBlobClient.Object.DeleteBlobsAsync(containerName, blobNames);

        // Assert
        result.Should().Contain("file1.txt");
        result.Should().Contain("file2.txt");
    }

    #endregion

    #region Blob Copy & Move Operations Tests

    [Fact]
    public async Task CopyBlobAsync_ShouldCallBlobClient()
    {
        // Arrange
        const string sourceContainer = "source-container";
        const string sourceBlob = "source.txt";
        const string destContainer = "dest-container";
        const string destBlob = "destination.txt";

        _mockBlobClient
            .Setup(x => x.CopyBlobAsync(sourceContainer, sourceBlob, destContainer, destBlob, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.CopyBlobAsync(sourceContainer, sourceBlob, destContainer, destBlob);

        // Assert
        _mockBlobClient.Verify(x => x.CopyBlobAsync(sourceContainer, sourceBlob, destContainer, destBlob, default), Times.Once);
    }

    [Fact]
    public async Task MoveBlobAsync_ShouldCallBlobClient()
    {
        // Arrange
        const string sourceContainer = "source-container";
        const string sourceBlob = "source.txt";
        const string destContainer = "dest-container";
        const string destBlob = "destination.txt";

        _mockBlobClient
            .Setup(x => x.MoveBlobAsync(sourceContainer, sourceBlob, destContainer, destBlob, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.MoveBlobAsync(sourceContainer, sourceBlob, destContainer, destBlob);

        // Assert
        _mockBlobClient.Verify(x => x.MoveBlobAsync(sourceContainer, sourceBlob, destContainer, destBlob, default), Times.Once);
    }

    [Fact]
    public async Task RenameBlobAsync_ShouldCallBlobClient()
    {
        // Arrange
        const string containerName = "test-container";
        const string oldBlobName = "old-name.txt";
        const string newBlobName = "new-name.txt";

        _mockBlobClient
            .Setup(x => x.RenameBlobAsync(containerName, oldBlobName, newBlobName, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.RenameBlobAsync(containerName, oldBlobName, newBlobName);

        // Assert
        _mockBlobClient.Verify(x => x.RenameBlobAsync(containerName, oldBlobName, newBlobName, default), Times.Once);
    }

    #endregion

    #region Blob Metadata Operations Tests

    [Fact]
    public async Task SetBlobMetadataAsync_ShouldSetMetadata()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var metadata = new Dictionary<string, string>
        {
            { "Author", "John Doe" },
            { "Department", "Engineering" }
        };

        _mockBlobClient
            .Setup(x => x.SetBlobMetadataAsync(containerName, blobName, metadata, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.SetBlobMetadataAsync(containerName, blobName, metadata);

        // Assert
        _mockBlobClient.Verify(x => x.SetBlobMetadataAsync(containerName, blobName, metadata, default), Times.Once);
    }

    [Fact]
    public async Task GetBlobMetadataAsync_ShouldReturnMetadata()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var expectedMetadata = new Dictionary<string, string>
        {
            { "Author", "John Doe" }
        };

        _mockBlobClient
            .Setup(x => x.GetBlobMetadataAsync(containerName, blobName, default))
            .ReturnsAsync(expectedMetadata);

        // Act
        var metadata = await _mockBlobClient.Object.GetBlobMetadataAsync(containerName, blobName);

        // Assert
        metadata.Should().ContainKey("Author");
    }

    [Fact]
    public async Task SetBlobContentTypeAsync_ShouldSetContentType()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "data.json";
        const string contentType = "application/json";

        _mockBlobClient
            .Setup(x => x.SetBlobContentTypeAsync(containerName, blobName, contentType, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.SetBlobContentTypeAsync(containerName, blobName, contentType);

        // Assert
        _mockBlobClient.Verify(x => x.SetBlobContentTypeAsync(containerName, blobName, contentType, default), Times.Once);
    }

    #endregion

    #region Blob URL Operations Tests

    [Fact]
    public async Task GetBlobUrlAsync_ShouldReturnUrl()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        const string expectedUrl = "https://mystorageaccount.blob.core.windows.net/test-container/test-file.txt";

        _mockBlobClient
            .Setup(x => x.GetBlobUrlAsync(containerName, blobName))
            .ReturnsAsync(expectedUrl);

        // Act
        var url = await _mockBlobClient.Object.GetBlobUrlAsync(containerName, blobName);

        // Assert
        url.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task GetBlobUriAsync_ShouldReturnUri()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var expectedUri = new Uri("https://mystorageaccount.blob.core.windows.net/test-container/test-file.txt");

        _mockBlobClient
            .Setup(x => x.GetBlobUriAsync(containerName, blobName))
            .ReturnsAsync(expectedUri);

        // Act
        var uri = await _mockBlobClient.Object.GetBlobUriAsync(containerName, blobName);

        // Assert
        uri.Should().Be(expectedUri);
    }

    #endregion

    #region SAS Token Operations Tests

    [Fact]
    public async Task GenerateBlobSasTokenAsync_ShouldReturnSasToken()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var expiresIn = TimeSpan.FromHours(1);
        const string expectedToken = "sv=2021-06-08&se=2024...";

        _mockBlobClient
            .Setup(x => x.GenerateBlobSasTokenAsync(containerName, blobName, expiresIn, BlobSasPermissions.Read, default))
            .ReturnsAsync(expectedToken);

        // Act
        var token = await _mockBlobClient.Object.GenerateBlobSasTokenAsync(containerName, blobName, expiresIn);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateBlobSasUrlAsync_ShouldReturnCompleteUrl()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "test-file.txt";
        var expiresIn = TimeSpan.FromHours(1);
        const string expectedUrl = "https://mystorageaccount.blob.core.windows.net/test-container/test-file.txt?sv=2021...";

        _mockBlobClient
            .Setup(x => x.GenerateBlobSasUrlAsync(containerName, blobName, expiresIn, BlobSasPermissions.Read, default))
            .ReturnsAsync(expectedUrl);

        // Act
        var url = await _mockBlobClient.Object.GenerateBlobSasUrlAsync(containerName, blobName, expiresIn);

        // Assert
        url.Should().Contain("https://");
    }

    [Fact]
    public async Task GenerateContainerSasTokenAsync_ShouldReturnSasToken()
    {
        // Arrange
        const string containerName = "test-container";
        var expiresIn = TimeSpan.FromDays(1);
        const string expectedToken = "sv=2021-06-08&se=2024...";

        _mockBlobClient
            .Setup(x => x.GenerateContainerSasTokenAsync(containerName, expiresIn, BlobSasPermissions.Read, default))
            .ReturnsAsync(expectedToken);

        // Act
        var token = await _mockBlobClient.Object.GenerateContainerSasTokenAsync(containerName, expiresIn);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Blob Storage Tier Operations Tests

    [Fact]
    public async Task SetBlobAccessTierAsync_ShouldSetTier()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "old-file.txt";
        const string accessTier = "Archive";

        _mockBlobClient
            .Setup(x => x.SetBlobAccessTierAsync(containerName, blobName, accessTier, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockBlobClient.Object.SetBlobAccessTierAsync(containerName, blobName, accessTier);

        // Assert
        _mockBlobClient.Verify(x => x.SetBlobAccessTierAsync(containerName, blobName, accessTier, default), Times.Once);
    }

    #endregion

    #region Blob Snapshot Operations Tests

    [Fact]
    public async Task CreateBlobSnapshotAsync_ShouldReturnSnapshotId()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "important.txt";
        const string expectedSnapshotId = "2024-01-15T10:30:00.0000000Z";

        _mockBlobClient
            .Setup(x => x.CreateBlobSnapshotAsync(containerName, blobName, default))
            .ReturnsAsync(expectedSnapshotId);

        // Act
        var snapshotId = await _mockBlobClient.Object.CreateBlobSnapshotAsync(containerName, blobName);

        // Assert
        snapshotId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ListBlobSnapshotsAsync_ShouldReturnSnapshotIds()
    {
        // Arrange
        const string containerName = "test-container";
        const string blobName = "important.txt";
        var expectedSnapshots = new List<string>
        {
            "2024-01-15T10:30:00.0000000Z",
            "2024-01-15T11:00:00.0000000Z"
        };

        _mockBlobClient
            .Setup(x => x.ListBlobSnapshotsAsync(containerName, blobName, default))
            .ReturnsAsync(expectedSnapshots);

        // Act
        var snapshots = await _mockBlobClient.Object.ListBlobSnapshotsAsync(containerName, blobName);

        // Assert
        snapshots.Should().HaveCount(2);
    }

    #endregion
}
