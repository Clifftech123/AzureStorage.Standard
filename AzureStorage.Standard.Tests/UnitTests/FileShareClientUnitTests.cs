using AzureStorage.Standard.Files;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureStroage.Standard.Tests.UnitTests;

/// <summary>
/// Unit tests for File Share Client using mocking.
/// These tests demonstrate how to mock IFileShareClient for unit testing.
/// </summary>
public class FileShareClientUnitTests
{
    private readonly Mock<IFileShareClient> _mockFileClient;

    public FileShareClientUnitTests()
    {
        _mockFileClient = new Mock<IFileShareClient>();
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
    public async Task CreateDirectoryIfNotExistsAsync_ShouldReturnTrue()
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
}
