# AzureStorage.Standard.Files

A simplified, modern .NET client library for Azure File Share (Azure Files) with built-in retry policies, comprehensive error handling, and intuitive APIs.

[![NuGet](https://img.shields.io/nuget/v/AzureStorage.Standard.Files.svg)](https://www.nuget.org/packages/AzureStorage.Standard.Files/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

 **Simplified API** - Easy-to-use wrapper around Azure.Storage.Files.Shares SDK
 **Built-in Resilience** - Automatic retry policies using Polly
 **File Share Management** - Create, delete, and list shares
 **Directory Operations** - Create, delete, and navigate directories
 **File Operations** - Upload, download, copy, and delete files
 **Streaming Support** - Efficient handling of large files
 **Metadata Management** - File and directory metadata support
 **SAS Token Generation** - Secure delegated access
 **Comprehensive Error Handling** - Detailed exception information
 **Extensive Documentation** - Full XML documentation for IntelliSense

## Installation

```bash
dotnet add package AzureStorage.Standard.Files
```

Or via Package Manager Console:

```powershell
Install-Package AzureStorage.Standard.Files
```

## Quick Start

### 1. Configure the Client

```csharp
using AzureStorage.Standard.Files;
using AzureStorage.Standard.Core;

var options = new StorageOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=..."
};

var fileClient = new FileShareClient(options);
```

### 2. Create a File Share

```csharp
await fileClient.CreateShareIfNotExistsAsync("documents");
```

### 3. Upload a File

```csharp
// Upload from local file
await fileClient.UploadFileAsync(
    shareName: "documents",
    filePath: "reports/annual-report.pdf",
    localFilePath: "C:\\reports\\2024-annual-report.pdf"
);

// Upload from stream
using var stream = File.OpenRead("data.csv");
await fileClient.UploadFileAsync(
    shareName: "documents",
    filePath: "data/data.csv",
    stream: stream
);
```

### 4. Download a File

```csharp
// Download to local file
await fileClient.DownloadFileAsync(
    shareName: "documents",
    filePath: "reports/annual-report.pdf",
    localFilePath: "C:\\downloads\\annual-report.pdf"
);

// Download to stream
using var downloadStream = await fileClient.DownloadFileToStreamAsync(
    shareName: "documents",
    filePath: "data/data.csv"
);
```

### 5. Directory Operations

```csharp
// Create directory
await fileClient.CreateDirectoryIfNotExistsAsync(
    shareName: "documents",
    directoryPath: "reports/2024"
);

// List directory contents
var files = await fileClient.ListFilesAndDirectoriesAsync(
    shareName: "documents",
    directoryPath: "reports"
);

foreach (var item in files)
{
    if (item.IsDirectory)
        Console.WriteLine($"Directory: {item.Name}");
    else
        Console.WriteLine($"File: {item.Name}, Size: {item.Size} bytes");
}

// Delete directory
await fileClient.DeleteDirectoryAsync(
    shareName: "documents",
    directoryPath: "reports/2024"
);
```

## Advanced Usage

### File Metadata

```csharp
// Set file metadata
var metadata = new Dictionary<string, string>
{
    { "Author", "John Doe" },
    { "Department", "Finance" },
    { "Year", "2024" }
};

await fileClient.SetFileMetadataAsync(
    shareName: "documents",
    filePath: "reports/annual-report.pdf",
    metadata: metadata
);

// Get file metadata
var fileMetadata = await fileClient.GetFileMetadataAsync(
    shareName: "documents",
    filePath: "reports/annual-report.pdf"
);
```

### Copy Files

```csharp
// Copy file within the same share
await fileClient.CopyFileAsync(
    shareName: "documents",
    sourceFilePath: "original.pdf",
    destinationFilePath: "backup/original-copy.pdf"
);
```

### File Properties

```csharp
// Get file properties (size, last modified, etc.)
var properties = await fileClient.GetFilePropertiesAsync(
    shareName: "documents",
    filePath: "reports/annual-report.pdf"
);

Console.WriteLine($"Size: {properties.Size} bytes");
Console.WriteLine($"Last Modified: {properties.LastModified}");
Console.WriteLine($"Content Type: {properties.ContentType}");
```

### Check File Existence

```csharp
bool fileExists = await fileClient.FileExistsAsync(
    shareName: "documents",
    filePath: "reports/annual-report.pdf"
);

if (fileExists)
{
    Console.WriteLine("File exists!");
}
```

### Delete Files

```csharp
await fileClient.DeleteFileAsync(
    shareName: "documents",
    filePath: "reports/old-report.pdf"
);
```

## Share Management

```csharp
// List all shares
var shares = await fileClient.ListSharesAsync();
foreach (var share in shares)
{
    Console.WriteLine($"Share: {share}");
}

// Check if share exists
bool exists = await fileClient.ShareExistsAsync("documents");

// Get share properties
var properties = await fileClient.GetSharePropertiesAsync("documents");
Console.WriteLine($"Quota: {properties.QuotaInGB} GB");

// Delete share
await fileClient.DeleteShareAsync("documents");
```

## Generate SAS Token

```csharp
// Generate SAS token for a file
var sasToken = await fileClient.GenerateFileSasTokenAsync(
    shareName: "documents",
    filePath: "reports/annual-report.pdf",
    permissions: ShareFileSasPermissions.Read,
    expiresOn: DateTimeOffset.UtcNow.AddHours(24)
);

// Use the SAS URL
var sasUrl = $"https://myaccount.file.core.windows.net/documents/reports/annual-report.pdf?{sasToken}";
```

## Authentication Options

### Connection String (Development)

```csharp
var options = new StorageOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=..."
};
```

### Account Key

```csharp
var options = new StorageOptions
{
    AccountName = "myaccount",
    AccountKey = "your-account-key"
};
```

### Service URI (Managed Identity)

```csharp
var options = new StorageOptions
{
    ServiceUri = new Uri("https://myaccount.file.core.windows.net")
};
```

## Working with Nested Directories

```csharp
// Create nested directory structure
await fileClient.CreateDirectoryIfNotExistsAsync("documents", "reports");
await fileClient.CreateDirectoryIfNotExistsAsync("documents", "reports/2024");
await fileClient.CreateDirectoryIfNotExistsAsync("documents", "reports/2024/Q1");

// Upload file to nested directory
await fileClient.UploadFileAsync(
    shareName: "documents",
    filePath: "reports/2024/Q1/financial-report.pdf",
    localFilePath: "C:\\reports\\Q1-report.pdf"
);
```

## Best Practices

### 1. File Path Format
Use forward slashes for paths:
```csharp
// Correct
"reports/2024/Q1/report.pdf"

// Incorrect
"reports\\2024\\Q1\\report.pdf"
```

### 2. Create Directories First
Ensure parent directories exist before uploading:
```csharp
await fileClient.CreateDirectoryIfNotExistsAsync("documents", "reports/2024");
await fileClient.UploadFileAsync("documents", "reports/2024/report.pdf", localPath);
```

### 3. Large File Uploads
For files > 100 MB, use streaming:
```csharp
using var fileStream = File.OpenRead(largeFilePath);
await fileClient.UploadFileAsync("documents", "large-file.zip", fileStream);
```

### 4. SMB Mount Support
Azure Files supports SMB protocol - mount as network drive:
```bash
# Windows
net use Z: \\myaccount.file.core.windows.net\documents /u:AZURE\myaccount accountkey

# Linux
sudo mount -t cifs //myaccount.file.core.windows.net/documents /mnt/documents -o username=myaccount,password=accountkey
```

## Use Cases

### Lift and Shift Applications
Replace on-premises file shares with Azure Files:
```csharp
// Migration pattern
await fileClient.CreateShareIfNotExistsAsync("legacy-app-data");
await fileClient.UploadFileAsync("legacy-app-data", "config/settings.ini", localPath);
```

### Shared Configuration
Store application configuration files:
```csharp
await fileClient.UploadFileAsync("config", "appsettings.json", configPath);
var configStream = await fileClient.DownloadFileToStreamAsync("config", "appsettings.json");
```

### Log Aggregation
Centralized logging:
```csharp
var logPath = $"logs/{DateTime.UtcNow:yyyy-MM-dd}/app.log";
await fileClient.UploadFileAsync("logs", logPath, logFilePath);
```

## Supported Frameworks

- .NET Standard 2.0
- .NET Standard 2.1
- .NET 8.0
- .NET 9.0

## Error Handling

```csharp
try
{
    await fileClient.DownloadFileAsync("documents", "missing-file.pdf", "output.pdf");
}
catch (AzureStorageException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
}
```

## Related Packages

- **[AzureStorage.Standard.Blobs](https://www.nuget.org/packages/AzureStorage.Standard.Blobs/)** - Azure Blob Storage client
- **[AzureStorage.Standard.Tables](https://www.nuget.org/packages/AzureStorage.Standard.Tables/)** - Azure Table Storage client
- **[AzureStorage.Standard.Queues](https://www.nuget.org/packages/AzureStorage.Standard.Queues/)** - Azure Queue Storage client

## Documentation

For complete documentation, visit the [GitHub repository](https://github.com/Clifftech123/AzureStroage.Standard).

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or suggestions, please [open an issue](https://github.com/Clifftech123/AzureStroage.Standard/issues) on GitHub.
