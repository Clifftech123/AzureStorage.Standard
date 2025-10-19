# AzureStorage.Standard.Blobs

A simplified, modern .NET client library for Azure Blob Storage with built-in retry policies, comprehensive error handling, and intuitive APIs.

[![NuGet](https://img.shields.io/nuget/v/AzureStorage.Standard.Blobs.svg)](https://www.nuget.org/packages/AzureStorage.Standard.Blobs/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

 **Simplified API** - Easy-to-use wrapper around Azure.Storage.Blobs SDK
 **Built-in Resilience** - Automatic retry policies using Polly
 **Comprehensive Operations** - Upload, download, delete, copy, and more
 **Streaming Support** - Efficient handling of large files
 **Container Management** - Full CRUD operations for containers
 **Metadata Management** - Easy blob and container metadata handling
 **Access Tier Management** - Hot, Cool, and Archive tier support
 **Lease Operations** - Blob leasing for distributed locking
 **SAS Token Generation** - Secure delegated access
 **Extensive Documentation** - Full XML documentation for IntelliSense

## Installation

```bash
dotnet add package AzureStorage.Standard.Blobs
```

Or via Package Manager Console:

```powershell
Install-Package AzureStorage.Standard.Blobs
```

## Quick Start

### 1. Configure the Client

```csharp
using AzureStorage.Standard.Blobs;
using AzureStorage.Standard.Core;

var options = new StorageOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=..."
};

var blobClient = new AzureBlobClient(options);
```

### 2. Upload a Blob

```csharp
// Upload from file
await blobClient.UploadBlobAsync(
    containerName: "documents",
    blobName: "report.pdf",
    filePath: "C:\\reports\\report.pdf"
);

// Upload from stream
using var stream = File.OpenRead("image.jpg");
await blobClient.UploadBlobAsync(
    containerName: "images",
    blobName: "profile.jpg",
    content: stream
);
```

### 3. Download a Blob

```csharp
// Download to file
await blobClient.DownloadBlobAsync(
    containerName: "documents",
    blobName: "report.pdf",
    filePath: "C:\\downloads\\report.pdf"
);

// Download to stream
using var downloadStream = await blobClient.DownloadBlobToStreamAsync(
    containerName: "images",
    blobName: "profile.jpg"
);
```

### 4. Container Operations

```csharp
// Create container
await blobClient.CreateContainerIfNotExistsAsync("my-container");

// List containers
var containers = await blobClient.ListContainersAsync();

// Delete container
await blobClient.DeleteContainerAsync("my-container");
```

### 5. List Blobs

```csharp
var blobs = await blobClient.ListBlobsAsync(
    containerName: "documents",
    prefix: "reports/"
);

foreach (var blob in blobs)
{
    Console.WriteLine($"Blob: {blob.Name}, Size: {blob.Size} bytes");
}
```

## Advanced Usage

### Metadata Management

```csharp
// Set blob metadata
var metadata = new Dictionary<string, string>
{
    { "Author", "John Doe" },
    { "Department", "Engineering" }
};
await blobClient.SetBlobMetadataAsync("container", "document.pdf", metadata);

// Get blob metadata
var retrievedMetadata = await blobClient.GetBlobMetadataAsync("container", "document.pdf");
```

### Access Tier Management

```csharp
// Change blob access tier
await blobClient.SetBlobAccessTierAsync(
    containerName: "archives",
    blobName: "old-data.zip",
    accessTier: AccessTier.Archive
);
```

### Copy Blobs

```csharp
// Copy blob within storage account
await blobClient.CopyBlobAsync(
    sourceContainerName: "source-container",
    sourceBlobName: "original.pdf",
    destinationContainerName: "backup-container",
    destinationBlobName: "backup-original.pdf"
);
```

### Generate SAS Token

```csharp
var sasToken = await blobClient.GenerateBlobSasTokenAsync(
    containerName: "documents",
    blobName: "report.pdf",
    permissions: BlobSasPermissions.Read,
    expiresOn: DateTimeOffset.UtcNow.AddHours(2)
);

// Use the SAS token
var sasUrl = $"https://mystorageaccount.blob.core.windows.net/documents/report.pdf?{sasToken}";
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
    ServiceUri = new Uri("https://myaccount.blob.core.windows.net")
};
```

## Supported Frameworks

- .NET Standard 2.0
- .NET Standard 2.1
- .NET 8.0
- .NET 9.0


## Error Handling

All operations throw `AzureStorageException` with detailed error information:

```csharp
try
{
    await blobClient.DownloadBlobAsync("container", "missing-file.pdf", "output.pdf");
}
catch (AzureStorageException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
}
```

## Related Packages

- **[AzureStorage.Standard.Tables](https://www.nuget.org/packages/AzureStorage.Standard.Tables/)** - Azure Table Storage client
- **[AzureStorage.Standard.Queues](https://www.nuget.org/packages/AzureStorage.Standard.Queues/)** - Azure Queue Storage client
- **[AzureStorage.Standard.Files](https://www.nuget.org/packages/AzureStorage.Standard.Files/)** - Azure File Share client

## Documentation

For complete documentation, visit the [GitHub repository](https://github.com/Clifftech123/AzureStroage.Standard).

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or suggestions, please [open an issue](https://github.com/Clifftech123/AzureStroage.Standard/issues) on GitHub.
