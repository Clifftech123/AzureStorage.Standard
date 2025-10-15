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

##  Features

-  **Clean, intuitive API** - Easy to learn and use
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