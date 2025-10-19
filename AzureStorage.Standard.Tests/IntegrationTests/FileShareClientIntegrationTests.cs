using AzureStorage.Standard.Core.Domain.Models;
using AzureStorage.Standard.Files;
using FluentAssertions;
using Xunit;

namespace AzureStroage.Standard.Tests.IntegrationTests;

/// <summary>
/// Integration tests for File Share Client that connect to Azurite (Azure Storage Emulator).
///
/// Prerequisites to run these tests:
/// 1. Install Azurite: npm install -g azurite
/// 2. Start Azurite: azurite --silent --location c:\azurite --debug c:\azurite\debug.log
/// 3. Tests will use connection string: "UseDevelopmentStorage=true"
///
/// Note: Azurite has limited File Share support. Some features may not work as expected.
/// For full File Share testing, consider using Azure Storage Account.
/// </summary>
[Collection("Integration Tests")]
public class FileShareClientIntegrationTests : IAsyncLifetime
{
    private FileShareClient? _fileShareClient;
    private const string TestShareName = "test-share-integration";
    private const string ConnectionString = "UseDevelopmentStorage=true";

    public async Task InitializeAsync()
    {
        var options = new StorageOptions { ConnectionString = ConnectionString };
        _fileShareClient = new FileShareClient(options);

        // Create test share
        try
        {
            await _fileShareClient.CreateShareIfNotExistsAsync(TestShareName);
        }
        catch
        {
            // Azurite file share may have limited support
            // Some tests might be skipped if share creation fails
        }
    }

    public async Task DisposeAsync()
    {
        if (_fileShareClient != null)
        {
            try
            {
                // Clean up test share
                await _fileShareClient.DeleteShareAsync(TestShareName);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region Share Operations Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateShareIfNotExistsAsync_ShouldCreateShare_WhenShareDoesNotExist()
    {
        // Arrange
        const string shareName = "test-share-create";

        try
        {
            // Act
            var result = await _fileShareClient!.CreateShareIfNotExistsAsync(shareName);

            // Assert
            result.Should().BeTrue();

            var exists = await _fileShareClient.ShareExistsAsync(shareName);
            exists.Should().BeTrue();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            // Cleanup
            try
            {
                await _fileShareClient!.DeleteShareAsync(shareName);
            }
            catch
            {
                // Ignore
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ShareExistsAsync_ShouldReturnTrue_WhenShareExists()
    {
        try
        {
            // Act
            var exists = await _fileShareClient!.ShareExistsAsync(TestShareName);

            // Assert
            exists.Should().BeTrue();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ShareExistsAsync_ShouldReturnFalse_WhenShareDoesNotExist()
    {
        try
        {
            // Act
            var exists = await _fileShareClient!.ShareExistsAsync("non-existent-share");

            // Assert
            exists.Should().BeFalse();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteShareAsync_ShouldDeleteShare_WhenShareExists()
    {
        // Arrange
        const string shareName = "test-share-delete";

        try
        {
            await _fileShareClient!.CreateShareIfNotExistsAsync(shareName);

            // Act
            var result = await _fileShareClient.DeleteShareAsync(shareName);

            // Assert
            result.Should().BeTrue();

            var exists = await _fileShareClient.ShareExistsAsync(shareName);
            exists.Should().BeFalse();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
    }

    #endregion

    #region Directory Operations Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateDirectoryIfNotExistsAsync_ShouldCreateDirectory_WhenDirectoryDoesNotExist()
    {
        // Arrange
        const string directoryPath = "test-directory";

        try
        {
            // Act
            var result = await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);

            // Assert
            result.Should().BeTrue();

            var exists = await _fileShareClient.DirectoryExistsAsync(TestShareName, directoryPath);
            exists.Should().BeTrue();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            // Cleanup
            try
            {
                await _fileShareClient!.DeleteDirectoryAsync(TestShareName, directoryPath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DirectoryExistsAsync_ShouldReturnTrue_WhenDirectoryExists()
    {
        // Arrange
        const string directoryPath = "existing-directory";

        try
        {
            await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);

            // Act
            var exists = await _fileShareClient.DirectoryExistsAsync(TestShareName, directoryPath);

            // Assert
            exists.Should().BeTrue();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            try
            {
                await _fileShareClient!.DeleteDirectoryAsync(TestShareName, directoryPath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DirectoryExistsAsync_ShouldReturnFalse_WhenDirectoryDoesNotExist()
    {
        try
        {
            // Act
            var exists = await _fileShareClient!.DirectoryExistsAsync(TestShareName, "non-existent-directory");

            // Assert
            exists.Should().BeFalse();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteDirectoryAsync_ShouldDeleteDirectory_WhenDirectoryExists()
    {
        // Arrange
        const string directoryPath = "directory-to-delete";

        try
        {
            await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);

            // Act
            var result = await _fileShareClient.DeleteDirectoryAsync(TestShareName, directoryPath);

            // Assert
            result.Should().BeTrue();

            var exists = await _fileShareClient.DirectoryExistsAsync(TestShareName, directoryPath);
            exists.Should().BeFalse();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
    }

    #endregion

    #region File Upload Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UploadFileAsync_FromStream_ShouldUploadSuccessfully()
    {
        // Arrange
        const string directoryPath = "uploads";
        const string fileName = "test-file-stream.txt";
        const string content = "Test file content for stream upload";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        try
        {
            await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);

            // Act
            await _fileShareClient.UploadFileAsync(TestShareName, directoryPath, fileName, stream);

            // Assert
            var exists = await _fileShareClient.FileExistsAsync(TestShareName, directoryPath, fileName);
            exists.Should().BeTrue();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            try
            {
                await _fileShareClient!.DeleteFileAsync(TestShareName, directoryPath, fileName);
                await _fileShareClient.DeleteDirectoryAsync(TestShareName, directoryPath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UploadFileAsync_FromBytes_ShouldUploadSuccessfully()
    {
        // Arrange
        const string directoryPath = "uploads-bytes";
        const string fileName = "test-file-bytes.txt";
        const string content = "Test file content for byte array upload";
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);

        try
        {
            await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);

            // Act
            await _fileShareClient.UploadFileAsync(TestShareName, directoryPath, fileName, bytes);

            // Assert
            var exists = await _fileShareClient.FileExistsAsync(TestShareName, directoryPath, fileName);
            exists.Should().BeTrue();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            try
            {
                await _fileShareClient!.DeleteFileAsync(TestShareName, directoryPath, fileName);
                await _fileShareClient.DeleteDirectoryAsync(TestShareName, directoryPath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    #endregion

    #region File Download Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DownloadFileAsync_ToStream_ShouldDownloadSuccessfully()
    {
        // Arrange
        const string directoryPath = "downloads";
        const string fileName = "test-download-stream.txt";
        const string content = "Test file content for stream download";
        var uploadBytes = System.Text.Encoding.UTF8.GetBytes(content);

        try
        {
            await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);
            await _fileShareClient.UploadFileAsync(TestShareName, directoryPath, fileName, uploadBytes);

            // Act
            using var downloadStream = new MemoryStream();
            await _fileShareClient.DownloadFileAsync(TestShareName, directoryPath, fileName, downloadStream);

            // Assert
            downloadStream.Length.Should().BeGreaterThan(0);
            var downloadedContent = System.Text.Encoding.UTF8.GetString(downloadStream.ToArray());
            downloadedContent.Should().Be(content);
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            try
            {
                await _fileShareClient!.DeleteFileAsync(TestShareName, directoryPath, fileName);
                await _fileShareClient.DeleteDirectoryAsync(TestShareName, directoryPath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DownloadFileAsync_ToBytes_ShouldDownloadSuccessfully()
    {
        // Arrange
        const string directoryPath = "downloads-bytes";
        const string fileName = "test-download-bytes.txt";
        const string content = "Test file content for byte download";
        var uploadBytes = System.Text.Encoding.UTF8.GetBytes(content);

        try
        {
            await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);
            await _fileShareClient.UploadFileAsync(TestShareName, directoryPath, fileName, uploadBytes);

            // Act
            var downloadedBytes = await _fileShareClient.DownloadFileAsync(TestShareName, directoryPath, fileName);

            // Assert
            downloadedBytes.Should().NotBeEmpty();
            var downloadedContent = System.Text.Encoding.UTF8.GetString(downloadedBytes);
            downloadedContent.Should().Be(content);
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            try
            {
                await _fileShareClient!.DeleteFileAsync(TestShareName, directoryPath, fileName);
                await _fileShareClient.DeleteDirectoryAsync(TestShareName, directoryPath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    #endregion

    #region File Operations Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FileExistsAsync_ShouldReturnTrue_WhenFileExists()
    {
        // Arrange
        const string directoryPath = "file-check";
        const string fileName = "existing-file.txt";
        var content = System.Text.Encoding.UTF8.GetBytes("File exists test");

        try
        {
            await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);
            await _fileShareClient.UploadFileAsync(TestShareName, directoryPath, fileName, content);

            // Act
            var exists = await _fileShareClient.FileExistsAsync(TestShareName, directoryPath, fileName);

            // Assert
            exists.Should().BeTrue();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            try
            {
                await _fileShareClient!.DeleteFileAsync(TestShareName, directoryPath, fileName);
                await _fileShareClient.DeleteDirectoryAsync(TestShareName, directoryPath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FileExistsAsync_ShouldReturnFalse_WhenFileDoesNotExist()
    {
        // Arrange
        const string directoryPath = "file-check-missing";

        try
        {
            await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);

            // Act
            var exists = await _fileShareClient.FileExistsAsync(TestShareName, directoryPath, "non-existent-file.txt");

            // Assert
            exists.Should().BeFalse();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            try
            {
                await _fileShareClient!.DeleteDirectoryAsync(TestShareName, directoryPath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteFileAsync_ShouldDeleteFile_WhenFileExists()
    {
        // Arrange
        const string directoryPath = "file-delete";
        const string fileName = "file-to-delete.txt";
        var content = System.Text.Encoding.UTF8.GetBytes("Delete me");

        try
        {
            await _fileShareClient!.CreateDirectoryIfNotExistsAsync(TestShareName, directoryPath);
            await _fileShareClient.UploadFileAsync(TestShareName, directoryPath, fileName, content);

            // Act
            var result = await _fileShareClient.DeleteFileAsync(TestShareName, directoryPath, fileName);

            // Assert
            result.Should().BeTrue();

            var exists = await _fileShareClient.FileExistsAsync(TestShareName, directoryPath, fileName);
            exists.Should().BeFalse();
        }
        catch (Azure.RequestFailedException ex) when (ex.Message.Contains("not supported"))
        {
            // Skip test if Azurite doesn't support file shares
            return;
        }
        finally
        {
            try
            {
                await _fileShareClient!.DeleteDirectoryAsync(TestShareName, directoryPath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    #endregion
}
