# AzureStorage.Standard

A clean, modern .NET library that simplifies working with Azure Storage services through a unified, intuitive API.

##  What is AzureStorage.Standard?

AzureStorage.Standard is a wrapper library that provides a consistent, developer-friendly interface for all Azure Storage services:

- **Blob Storage** - Store and manage files, images, videos, and unstructured data
- **Queue Storage** - Build reliable messaging and async processing pipelines
- **Table Storage** - Store structured NoSQL data with flexible schemas
- **File Share Storage** - Create SMB-compatible cloud file shares

##  Why Use This Library?

### The Problem
  Working with Azure Storage can be challenging:
- **Multiple SDKs** - Each storage service has its own SDK with different patterns
- **Complex Authentication** - Managing connection strings, account keys, and SAS tokens is error-prone
- **Boilerplate Code** - Repetitive initialization and configuration code across projects
- **Inconsistent APIs** - Different approaches for similar operations across services

### The Solution
AzureStorage.Standard provides:
-  **Unified API** - One consistent interface across all storage services
-  **Flexible Authentication** - Support for connection strings, account keys, and SAS tokens
-  **Factory Pattern** - Create any storage client from a single configuration
-  **Clean Architecture** - Well-organized, testable, and maintainable code
-  **Type Safety** - Interface-based design for easy mocking and testing
-  **Simple Configuration** - Set up once, use everywhere

##  Quick Start

### Installation
```bash
dotnet add package AzureStorage.Standard.Core
dotnet add package AzureStorage.Standard.Blobs   # For Blob Storage
dotnet add package AzureStorage.Standard.Queues  # For Queue Storage
dotnet add package AzureStorage.Standard.Tables  # For Table Storage
dotnet add package AzureStorage.Standard.Files   # For File Share Storage
```

### Basic Usage

```csharp
using AzureStorage.Standard.Core.Domain.Models;
using AzureStorage.Standard.Core.Application;

// Option 1: Using Connection String
var options = StorageOptions.CreateFromConnectionString(
    "DefaultEndpointsProtocol=https;AccountName=...");

// Option 2: Using Account Key
var options = StorageOptions.CreateFromAccountKey(
    accountName: "mystorageaccount",
    accountKey: "your-account-key");

// Option 3: Using SAS Token
var options = StorageOptions.CreateFromSasToken(
    accountName: "mystorageaccount",
    sasToken: "your-sas-token");

// Create the factory
var factory = new AzureStorageFactory(options);

// Create clients as needed
var blobClient = factory.CreateBlobClient();
var queueClient = factory.CreateQueueClient();
var tableClient = factory.CreateTableClient();
var fileClient = factory.CreateFileShareClient();
```



##  Configuration

### Three Authentication Methods

**1. Connection String** (Recommended for development)
```csharp
var options = StorageOptions.CreateFromConnectionString(connectionString);
```

**2. Account Name + Key** (For production with key rotation)
```csharp
var options = StorageOptions.CreateFromAccountKey(accountName, accountKey);
```

**3. Account Name + SAS Token** (For limited access scenarios)
```csharp
var options = StorageOptions.CreateFromSasToken(accountName, sasToken);
```

### Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs
services.AddSingleton<StorageOptions>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["Azure:Storage:ConnectionString"];
    return StorageOptions.CreateFromConnectionString(connectionString);
});

services.AddSingleton<IStorageClientFactory, AzureStorageFactory>();
```

##  Blob Storage

The Blob Storage client provides a simple, powerful API for managing files and unstructured data in Azure.

### Features

-  **Automatic Retries** - Built-in retry logic for transient failures (configurable)
-  **Batch Operations** - Upload/delete multiple files with partial success tracking
-  **Container Management** - Create, delete, and check container existence
-  **Blob Operations** - Upload, download, copy, move, and delete blobs
-  **Metadata & Properties** - Get and set custom metadata and blob properties
-  **SAS Token Generation** - Create secure, time-limited access URLs
-  **Storage Tiers** - Set blob access tiers (Hot, Cool, Archive)
-  **Snapshots** - Create and manage blob snapshots

### Basic Blob Operations

```csharp
using AzureStorage.Standard.Blobs;

// Create blob client
var blobClient = factory.CreateBlobClient();

// Create a container
await blobClient.CreateContainerIfNotExistsAsync("my-container");

// Upload a file
using var fileStream = File.OpenRead("photo.jpg");
await blobClient.UploadBlobAsync(
    containerName: "my-container",
    blobName: "photos/photo.jpg",
    content: fileStream,
    contentType: "image/jpeg");

// Download a file
var stream = await blobClient.DownloadBlobAsync("my-container", "photos/photo.jpg");
await using var outputFile = File.Create("downloaded-photo.jpg");
await stream.CopyToAsync(outputFile);

// Delete a blob
await blobClient.DeleteBlobAsync("my-container", "photos/photo.jpg");
```

### Batch Operations with Partial Success Tracking

```csharp
// Upload multiple files
var files = new Dictionary<string, string>
{
    ["document1.pdf"] = @"C:\files\document1.pdf",
    ["document2.pdf"] = @"C:\files\document2.pdf",
    ["report.xlsx"] = @"C:\files\report.xlsx"
};

try
{
    // Returns comma-separated list of successfully uploaded files
    var uploaded = await blobClient.UploadBlobsAsync("my-container", files);
    Console.WriteLine($"Successfully uploaded: {uploaded}");
}
catch (AzureStorageException ex)
{
    // Exception message includes partial success info
    // Example: "Failed to upload all blobs. Successfully uploaded: document1.pdf, document2.pdf."
    Console.WriteLine(ex.Message);
}

// Delete multiple blobs
var blobsToDelete = new[] { "file1.txt", "file2.txt", "file3.txt" };
var deleted = await blobClient.DeleteBlobsAsync("my-container", blobsToDelete);
Console.WriteLine($"Deleted: {deleted}"); // Returns "file1.txt, file2.txt, file3.txt"
```

### Automatic Retry Configuration

Retries are enabled by default with sensible settings, but you can customize them:

```csharp
// Option 1: Use default retry policy (recommended)
var options = StorageOptions.CreateFromConnectionString(connectionString);
// Defaults: 3 retries, exponential backoff, 1-30 second delays

// Option 2: Disable retries
var options = StorageOptions.CreateFromConnectionString(connectionString);
options.RetryOptions = RetryOptions.None;

// Option 3: Custom retry configuration
var options = StorageOptions.CreateFromConnectionString(connectionString);
options.RetryOptions = new RetryOptions
{
    MaxRetryAttempts = 5,
    InitialDelay = TimeSpan.FromMilliseconds(500),
    MaxDelay = TimeSpan.FromSeconds(15)
};

// Option 4: Aggressive retries for critical operations
var options = StorageOptions.CreateFromConnectionString(connectionString);
options.RetryOptions = RetryOptions.Aggressive; // 5 retries, faster backoff
```

**What gets retried automatically:**
- HTTP 408 (Request Timeout)
- HTTP 500 (Internal Server Error)
- HTTP 502 (Bad Gateway)
- HTTP 503 (Service Unavailable)
- HTTP 504 (Gateway Timeout)
- HTTP 429 (Too Many Requests - throttling)
- Network errors (connection failures, socket errors)
- Azure "ServerBusy" errors

### Container Management

```csharp
// List all containers
var containers = await blobClient.ListContainersAsync();
foreach (var container in containers)
{
    Console.WriteLine(container);
}

// Check if container exists
bool exists = await blobClient.ContainerExistsAsync("my-container");

// Get container properties
var properties = await blobClient.GetContainerPropertiesAsync("my-container");

// Set container metadata
var metadata = new Dictionary<string, string>
{
    ["Department"] = "Sales",
    ["Project"] = "Q4-2024"
};
await blobClient.SetContainerMetadataAsync("my-container", metadata);

// Delete container
await blobClient.DeleteContainerAsync("my-container");
```

### Advanced Blob Operations

```csharp
// List blobs with prefix filter
var blobs = await blobClient.ListBlobsAsync("my-container", prefix: "photos/");
foreach (var blob in blobs)
{
    Console.WriteLine($"{blob.Name} - {blob.Size} bytes");
}

// Copy blob
await blobClient.CopyBlobAsync(
    sourceContainer: "source-container",
    sourceBlobName: "file.txt",
    destinationContainer: "backup-container",
    destinationBlobName: "file-backup.txt");

// Move blob (copy + delete)
await blobClient.MoveBlobAsync(
    sourceContainer: "temp-container",
    sourceBlobName: "upload.txt",
    destinationContainer: "final-container",
    destinationBlobName: "upload.txt");

// Rename blob within same container
await blobClient.RenameBlobAsync("my-container", "old-name.txt", "new-name.txt");

// Create snapshot
var snapshotId = await blobClient.CreateBlobSnapshotAsync("my-container", "important.txt");
Console.WriteLine($"Snapshot created: {snapshotId}");

// List blob snapshots
var snapshots = await blobClient.ListBlobSnapshotsAsync("my-container", "important.txt");
```

### Blob Metadata and Properties

```csharp
// Set blob metadata
var metadata = new Dictionary<string, string>
{
    ["Author"] = "John Doe",
    ["Category"] = "Reports",
    ["Year"] = "2024"
};
await blobClient.SetBlobMetadataAsync("my-container", "report.pdf", metadata);

// Get blob metadata
var blobMetadata = await blobClient.GetBlobMetadataAsync("my-container", "report.pdf");
Console.WriteLine($"Author: {blobMetadata["Author"]}");

// Get blob properties
var blobInfo = await blobClient.GetBlobPropertiesAsync("my-container", "report.pdf");
Console.WriteLine($"Size: {blobInfo.Size} bytes");
Console.WriteLine($"Content-Type: {blobInfo.ContentType}");
Console.WriteLine($"Last Modified: {blobInfo.LastModified}");

// Set content type
await blobClient.SetBlobContentTypeAsync("my-container", "data.json", "application/json");

// Set access tier (Hot, Cool, Archive)
await blobClient.SetBlobAccessTierAsync("my-container", "old-file.txt", "Archive");
```

### SAS Token Generation

```csharp
// Generate SAS token for blob (read-only, expires in 1 hour)
var sasToken = await blobClient.GenerateBlobSasTokenAsync(
    containerName: "my-container",
    blobName: "document.pdf",
    expiresIn: TimeSpan.FromHours(1),
    permissions: BlobSasPermissions.Read);

// Generate complete SAS URL
var sasUrl = await blobClient.GenerateBlobSasUrlAsync(
    containerName: "my-container",
    blobName: "document.pdf",
    expiresIn: TimeSpan.FromHours(1),
    permissions: BlobSasPermissions.Read);
Console.WriteLine($"Share this URL: {sasUrl}");

// Generate SAS token with write permissions
var writeSasToken = await blobClient.GenerateBlobSasTokenAsync(
    containerName: "uploads",
    blobName: "new-file.txt",
    expiresIn: TimeSpan.FromMinutes(15),
    permissions: BlobSasPermissions.Read | BlobSasPermissions.Write);

// Generate container SAS token (list all blobs)
var containerSasToken = await blobClient.GenerateContainerSasTokenAsync(
    containerName: "my-container",
    expiresIn: TimeSpan.FromDays(1),
    permissions: BlobSasPermissions.Read | BlobSasPermissions.List);
```

### Error Handling

```csharp
try
{
    await blobClient.UploadBlobAsync("my-container", "file.txt", stream);
}
catch (AzureStorageException ex)
{
    // All Azure Storage errors are wrapped in AzureStorageException
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
    Console.WriteLine($"HTTP Status: {ex.StatusCode}");
}
```

##  Library Features

-  **Clean, intuitive API** - Easy to learn and use
-  **Automatic retry logic** - Transparent handling of transient failures
-  **Batch operations** - Upload/delete multiple files with progress tracking
-  **Multiple authentication methods** - Connection string, account key, or SAS token
-  **Factory pattern** - Centralized client creation
-  **Testable design** - Interface-based for easy mocking
-  **Comprehensive documentation** - XML docs for IntelliSense
-  **Performance focused** - Minimal overhead wrapper
-  **Validation built-in** - Configuration validated at startup

##  Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## ` License

This project is licensed under the MIT License - see the LICENSE file for details.

##   Issues

Found a bug or have a feature request? Please [open an issue](https://github.com/Clifftech123/AzureStroage.Standard/issues).