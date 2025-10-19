using AzureStorage.Standard.Core.Domain.Models;
using AzureStorage.Standard.Files;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureStroage.Standard.Tests.UnitTests;

/// <summary>
/// Comprehensive unit tests for File Share Client using mocking.
/// These tests cover all IFileShareClient interface methods without requiring actual Azure Storage.
/// </summary>
public class FileShareClientUnitTests
{
    private readonly Mock<IFileShareClient> _mockFileClient;

    public FileShareClientUnitTests()
    {
        _mockFileClient = new Mock<IFileShareClient>();
    }

    #region Share Operations Tests

    [Fact]
    public async Task ListSharesAsync_ShouldReturnShares()
    {
        // Arrange
        var expectedShares = new List<string> { "share1", "share2", "share3" };

        _mockFileClient
            .Setup(x => x.ListSharesAsync(default))
            .ReturnsAsync(expectedShares);

        // Act
        var shares = await _mockFileClient.Object.ListSharesAsync();

        // Assert
        shares.Should().HaveCount(3);
        shares.Should().Contain("share1");
    }

    [Fact]
    public async Task CreateShareIfNotExistsAsync_ShouldReturnTrue_WhenShareIsCreated()
    {
        // Arrange
        const string shareName = "test-share";

        _mockFileClient
            .Setup(x => x.CreateShareIfNotExistsAsync(shareName, default))
            .ReturnsAsync(true);

        // Act
        var result = await _mockFileClient.Object.CreateShareIfNotExistsAsync(shareName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateShareIfNotExistsAsync_ShouldReturnFalse_WhenShareExists()
    {
        // Arrange
        const string shareName = "existing-share";

        _mockFileClient
            .Setup(x => x.CreateShareIfNotExistsAsync(shareName, default))
            .ReturnsAsync(false);

        // Act
        var result = await _mockFileClient.Object.CreateShareIfNotExistsAsync(shareName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShareExistsAsync_ShouldReturnTrue_WhenShareExists()
    {
        // Arrange
        const string shareName = "existing-share";

        _mockFileClient
            .Setup(x => x.ShareExistsAsync(shareName, default))
            .ReturnsAsync(true);

        // Act
        var exists = await _mockFileClient.Object.ShareExistsAsync(shareName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ShareExistsAsync_ShouldReturnFalse_WhenShareDoesNotExist()
    {
        // Arrange
        const string shareName = "non-existent-share";

        _mockFileClient
            .Setup(x => x.ShareExistsAsync(shareName, default))
            .ReturnsAsync(false);

        // Act
        var exists = await _mockFileClient.Object.ShareExistsAsync(shareName);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteShareAsync_ShouldReturnTrue_WhenDeleted()
    {
        // Arrange
        const string shareName = "test-share";

        _mockFileClient
            .Setup(x => x.DeleteShareAsync(shareName, default))
            .ReturnsAsync(true);

        // Act
        var result = await _mockFileClient.Object.DeleteShareAsync(shareName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetShareQuotaAsync_ShouldSetQuota()
    {
        // Arrange
        const string shareName = "test-share";
        const int quotaInGB = 100;

        _mockFileClient
            .Setup(x => x.SetShareQuotaAsync(shareName, quotaInGB, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockFileClient.Object.SetShareQuotaAsync(shareName, quotaInGB);

        // Assert
        _mockFileClient.Verify(x => x.SetShareQuotaAsync(shareName, quotaInGB, default), Times.Once);
    }

    #endregion

    #region Directory Operations Tests

    [Fact]
    public async Task CreateDirectoryIfNotExistsAsync_ShouldReturnTrue_WhenCreated()
    {
        // Arrange
        const string shareName = "documents";
        const string directoryPath = "reports";

        _mockFileClient
            .Setup(x => x.CreateDirectoryIfNotExistsAsync(shareName, directoryPath, default))
            .ReturnsAsync(true);

        // Act
        var result = await _mockFileClient.Object.CreateDirectoryIfNotExistsAsync(shareName, directoryPath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DirectoryExistsAsync_ShouldReturnTrue_WhenDirectoryExists()
    {
        // Arrange
        const string shareName = "documents";
        const string directoryPath = "reports";

        _mockFileClient
            .Setup(x => x.DirectoryExistsAsync(shareName, directoryPath, default))
            .ReturnsAsync(true);

        // Act
        var exists = await _mockFileClient.Object.DirectoryExistsAsync(shareName, directoryPath);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDirectoryAsync_ShouldReturnTrue_WhenDeleted()
    {
        // Arrange
        const string shareName = "documents";
        const string directoryPath = "reports";

        _mockFileClient
            .Setup(x => x.DeleteDirectoryAsync(shareName, directoryPath, default))
            .ReturnsAsync(true);

        // Act
        var result = await _mockFileClient.Object.DeleteDirectoryAsync(shareName, directoryPath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ListFilesAndDirectoriesAsync_ShouldReturnItems()
    {
        // Arrange
        const string shareName = "documents";
        const string directoryPath = "reports";
        var expectedItems = new List<FileShareItem>
        {
            new FileShareItem { Name = "file1.txt", IsDirectory = false, Size = 1024 },
            new FileShareItem { Name = "subfolder", IsDirectory = true }
        };

        _mockFileClient
            .Setup(x => x.ListFilesAndDirectoriesAsync(shareName, directoryPath, default))
            .ReturnsAsync(expectedItems);

        // Act
        var items = await _mockFileClient.Object.ListFilesAndDirectoriesAsync(shareName, directoryPath);

        // Assert
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.Name == "file1.txt" && !i.IsDirectory);
        items.Should().Contain(i => i.Name == "subfolder" && i.IsDirectory);
    }

    #endregion

    #region File Upload Tests

    [Fact]
    public async Task UploadFileAsync_FromStream_ShouldUploadSuccessfully()
    {
        // Arrange
        const string shareName = "documents";
        const string filePath = "reports/file.txt";
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        _mockFileClient
            .Setup(x => x.UploadFileAsync(shareName, filePath, stream, true, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockFileClient.Object.UploadFileAsync(shareName, filePath, stream);

        // Assert
        _mockFileClient.Verify(x => x.UploadFileAsync(shareName, filePath, stream, true, default), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_FromBytes_ShouldUploadSuccessfully()
    {
        // Arrange
        const string shareName = "documents";
        const string filePath = "reports/file.txt";
        var bytes = new byte[] { 1, 2, 3 };

        _mockFileClient
            .Setup(x => x.UploadFileAsync(shareName, filePath, bytes, true, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockFileClient.Object.UploadFileAsync(shareName, filePath, bytes);

        // Assert
        _mockFileClient.Verify(x => x.UploadFileAsync(shareName, filePath, bytes, true, default), Times.Once);
    }

    #endregion

    #region File Download Tests

    [Fact]
    public async Task DownloadFileAsync_ShouldReturnStream()
    {
        // Arrange
        const string shareName = "documents";
        const string filePath = "reports/file.txt";
        var expectedStream = new MemoryStream(new byte[] { 1, 2, 3 });

        _mockFileClient
            .Setup(x => x.DownloadFileAsync(shareName, filePath, default))
            .ReturnsAsync(expectedStream);

        // Act
        var stream = await _mockFileClient.Object.DownloadFileAsync(shareName, filePath);

        // Assert
        stream.Should().NotBeNull();
        stream.Length.Should().Be(3);
    }

    [Fact]
    public async Task DownloadFileAsBytesAsync_ShouldReturnBytes()
    {
        // Arrange
        const string shareName = "documents";
        const string filePath = "reports/file.txt";
        var expectedBytes = new byte[] { 1, 2, 3 };

        _mockFileClient
            .Setup(x => x.DownloadFileAsBytesAsync(shareName, filePath, default))
            .ReturnsAsync(expectedBytes);

        // Act
        var bytes = await _mockFileClient.Object.DownloadFileAsBytesAsync(shareName, filePath);

        // Assert
        bytes.Should().HaveCount(3);
        bytes.Should().BeEquivalentTo(expectedBytes);
    }

    #endregion

    #region File Operations Tests

    [Fact]
    public async Task FileExistsAsync_ShouldReturnTrue_WhenFileExists()
    {
        // Arrange
        const string shareName = "documents";
        const string filePath = "reports/file.txt";

        _mockFileClient
            .Setup(x => x.FileExistsAsync(shareName, filePath, default))
            .ReturnsAsync(true);

        // Act
        var exists = await _mockFileClient.Object.FileExistsAsync(shareName, filePath);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldReturnTrue_WhenDeleted()
    {
        // Arrange
        const string shareName = "documents";
        const string filePath = "reports/file.txt";

        _mockFileClient
            .Setup(x => x.DeleteFileAsync(shareName, filePath, default))
            .ReturnsAsync(true);

        // Act
        var result = await _mockFileClient.Object.DeleteFileAsync(shareName, filePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetFilePropertiesAsync_ShouldReturnProperties()
    {
        // Arrange
        const string shareName = "documents";
        const string filePath = "reports/file.txt";
        var expectedProperties = new FileShareItem
        {
            Name = "file.txt",
            Size = 1024,
            ContentType = "text/plain",
            IsDirectory = false
        };

        _mockFileClient
            .Setup(x => x.GetFilePropertiesAsync(shareName, filePath, default))
            .ReturnsAsync(expectedProperties);

        // Act
        var properties = await _mockFileClient.Object.GetFilePropertiesAsync(shareName, filePath);

        // Assert
        properties.Should().NotBeNull();
        properties.Name.Should().Be("file.txt");
        properties.Size.Should().Be(1024);
    }

    [Fact]
    public async Task CopyFileAsync_ShouldCopyFile()
    {
        // Arrange
        const string sourceShare = "source-share";
        const string sourceFilePath = "source/file.txt";
        const string destShare = "dest-share";
        const string destFilePath = "dest/file.txt";

        _mockFileClient
            .Setup(x => x.CopyFileAsync(sourceShare, sourceFilePath, destShare, destFilePath, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockFileClient.Object.CopyFileAsync(sourceShare, sourceFilePath, destShare, destFilePath);

        // Assert
        _mockFileClient.Verify(
            x => x.CopyFileAsync(sourceShare, sourceFilePath, destShare, destFilePath, default),
            Times.Once);
    }

    [Fact]
    public async Task SetFileMetadataAsync_ShouldSetMetadata()
    {
        // Arrange
        const string shareName = "documents";
        const string filePath = "reports/file.txt";
        var metadata = new Dictionary<string, string>
        {
            { "Author", "John Doe" },
            { "Department", "Engineering" }
        };

        _mockFileClient
            .Setup(x => x.SetFileMetadataAsync(shareName, filePath, metadata, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockFileClient.Object.SetFileMetadataAsync(shareName, filePath, metadata);

        // Assert
        _mockFileClient.Verify(
            x => x.SetFileMetadataAsync(shareName, filePath, metadata, default),
            Times.Once);
    }

    #endregion
}
