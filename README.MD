# AzureStorage.Standard

A simplified, modern .NET library that provides easy-to-use wrappers for Azure Storage services with built-in retry policies, comprehensive error handling, and intuitive APIs.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-Standard%202.0%2B%20%7C%207%20%7C%208%20%7C%209-512BD4)](https://dotnet.microsoft.com/)

##  Packages

AzureStorage.Standard is distributed as four separate NuGet packages, each wrapping a specific Azure Storage service:

| Package | Description | NuGet | Documentation |
|---------|-------------|-------|---------------|
| **AzureStorage.Standard.Blobs** | Azure Blob Storage client with retry policies | [![NuGet](https://img.shields.io/nuget/v/AzureStorage.Standard.Blobs.svg)](https://www.nuget.org/packages/AzureStorage.Standard.Blobs/) | [README](./AzureStorage.Standard.Blobs/README.md) |
| **AzureStorage.Standard.Tables** | Azure Table Storage client for NoSQL data | [![NuGet](https://img.shields.io/nuget/v/AzureStorage.Standard.Tables.svg)](https://www.nuget.org/packages/AzureStorage.Standard.Tables/) | [README](./AzureStorage.Standard.Tables/README.md) |
| **AzureStorage.Standard.Queues** | Azure Queue Storage client for messaging | [![NuGet](https://img.shields.io/nuget/v/AzureStorage.Standard.Queues.svg)](https://www.nuget.org/packages/AzureStorage.Standard.Queues/) | [README](./AzureStorage.Standard.Queues/README.md) |
| **AzureStorage.Standard.Files** | Azure File Share client with Polly resilience | [![NuGet](https://img.shields.io/nuget/v/AzureStorage.Standard.Files.svg)](https://www.nuget.org/packages/AzureStorage.Standard.Files/) | [README](./AzureStorage.Standard.Files/README.md) |

##  Key Features

-  **Simplified APIs** - Easy-to-use wrappers around official Azure SDKs
-  **Built-in Resilience** - Automatic retry policies using Polly (Blobs & Files)
-  **Comprehensive Error Handling** - Consistent exception handling across all services
-  **Streaming Support** - Efficient handling of large files and data
-  **Multiple Auth Methods** - Connection string, account key, or managed identity
-  **Full Documentation** - Complete XML docs for IntelliSense
-  **Multi-Framework Support** - .NET Standard 2.0+, .NET 7, 8, and 9
-  **Source Link Enabled** - Step-through debugging support

##  Quick Start

### Installation

Install the packages you need:

```bash
# For Blob Storage
dotnet add package AzureStorage.Standard.Blobs

# For Table Storage
dotnet add package AzureStorage.Standard.Tables

# For Queue Storage
dotnet add package AzureStorage.Standard.Queues

# For File Share Storage
dotnet add package AzureStorage.Standard.Files
```

### Basic Usage

**Blob Storage Example:**
```csharp
using AzureStorage.Standard.Blobs;
using AzureStorage.Standard.Core;

var options = new StorageOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=..."
};

var blobClient = new AzureBlobClient(options);

// Upload a file
await blobClient.UploadBlobAsync("container", "file.txt", "C:\\path\\file.txt");

// Download a file
await blobClient.DownloadBlobAsync("container", "file.txt", "C:\\downloads\\file.txt");
```

**Table Storage Example:**
```csharp
using AzureStorage.Standard.Tables;
using AzureStorage.Standard.Core;

var tableClient = new TableClient(options);

// Insert an entity
var customer = new Customer
{
    PartitionKey = "USA",
    RowKey = "001",
    Name = "John Doe"
};
await tableClient.InsertEntityAsync("Customers", customer);

// Query entities
var customers = await tableClient.QueryByPartitionKeyAsync<Customer>("Customers", "USA");
```

**Queue Storage Example:**
```csharp
using AzureStorage.Standard.Queues;

var queueClient = new QueueClient(options);

// Send a message
await queueClient.SendMessageAsync("orders", "Process order #12345");

// Receive messages
var messages = await queueClient.ReceiveMessagesAsync("orders", maxMessages: 10);
```

**File Share Example:**
```csharp
using AzureStorage.Standard.Files;

var fileClient = new FileShareClient(options);

// Upload a file
await fileClient.UploadFileAsync("documents", "reports/report.pdf", "C:\\report.pdf");

// Download a file
await fileClient.DownloadFileAsync("documents", "reports/report.pdf", "C:\\downloads\\report.pdf");
```

## 📚 Documentation

Each package has its own comprehensive README with detailed examples, best practices, and API documentation:

- **[Blob Storage Documentation](./AzureStorage.Standard.Blobs/README.md)** - Upload/download, container management, SAS tokens, metadata, access tiers
- **[Table Storage Documentation](./AzureStorage.Standard.Tables/README.md)** - CRUD operations, OData queries, batch transactions, partition keys
- **[Queue Storage Documentation](./AzureStorage.Standard.Queues/README.md)** - Message operations, visibility timeout, batch sending, poison messages
- **[File Share Documentation](./AzureStorage.Standard.Files/README.md)** - File operations, directory management, streaming, SMB support

## 🔐 Authentication

All packages support three authentication methods:

**1. Connection String** (recommended for development)
```csharp
var options = new StorageOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=..."
};
```

**2. Account Key**
```csharp
var options = new StorageOptions
{
    AccountName = "myaccount",
    AccountKey = "your-account-key"
};
```

**3. Service URI** (for managed identity)
```csharp
var options = new StorageOptions
{
    ServiceUri = new Uri("https://myaccount.blob.core.windows.net")
};
```

##  Testing

The library includes comprehensive unit tests using xUnit. Tests can use:

- **Mocking**: All packages use interface-based design (`IQueueClient`, `ITableClient`, etc.) making it easy to mock for unit tests
- **Integration Tests**: Connect to Azure Storage Emulator (Azurite) or actual Azure Storage for integration testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test AzureStorage.Standard.Tests/AzureStroage.Standard.Tests.csproj
```

### Mocking Example

```csharp
using Moq;
using AzureStorage.Standard.Core.Domain.Abstractions;

// Mock the queue client
var mockQueueClient = new Mock<IQueueClient>();
mockQueueClient
    .Setup(x => x.SendMessageAsync("orders", It.IsAny<string>(), null, null, default))
    .ReturnsAsync();

// Use the mock in your tests
var service = new OrderService(mockQueueClient.Object);
await service.ProcessOrder(order);

// Verify the message was sent
mockQueueClient.Verify(
    x => x.SendMessageAsync("orders", It.IsAny<string>(), null, null, default),
    Times.Once);
```

### Integration Testing with Azurite

For local integration testing, use [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (Azure Storage Emulator):

```bash
# Install Azurite
npm install -g azurite

# Start Azurite
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

Then use the Azurite connection string in your tests:
```csharp
var options = new StorageOptions
{
    ConnectionString = "UseDevelopmentStorage=true"
};
```

##  Supported Frameworks

- .NET Standard 2.0
- .NET Standard 2.1
- .NET 8.0
- .NET 9.0

##  Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

##  Support

For issues, questions, or feature requests:
- **Issues**: [GitHub Issues](https://github.com/Clifftech123/AzureStroage.Standard/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Clifftech123/AzureStroage.Standard/discussions)

##  Related Resources

- [Azure Storage Documentation](https://learn.microsoft.com/en-us/azure/storage/)
- [Azure Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/)
- [Azure Table Storage](https://learn.microsoft.com/en-us/azure/storage/tables/)
- [Azure Queue Storage](https://learn.microsoft.com/en-us/azure/storage/queues/)
- [Azure Files](https://learn.microsoft.com/en-us/azure/storage/files/)

---

**Built with  by Isaiah Clifford Opoku**
