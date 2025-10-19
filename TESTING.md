# Testing Guide for v1.0.0 Release

This guide helps you test the AzureStorage.Standard packages before releasing v1.0.0.

## Prerequisites

- .NET 9.0 SDK installed
- Azure Storage Account OR Azurite (Azure Storage Emulator)
- Visual Studio Code or Visual Studio

## Step 1: Create a Test Project

```bash
# Create a new test console application
mkdir AzureStorage.Test
cd AzureStorage.Test
dotnet new console
```

## Step 2: Install Alpha Packages

Install the current alpha versions from NuGet.org:

```bash
# Install individual packages
dotnet add package AzureStorage.Standard.Blobs --version 0.0.0-alpha.0.8
dotnet add package AzureStorage.Standard.Queues --version 0.0.0-alpha.0.8
dotnet add package AzureStorage.Standard.Tables --version 0.0.0-alpha.0.8
dotnet add package AzureStorage.Standard.Files --version 0.0.0-alpha.0.8

# Note: Core package is automatically installed as a dependency
```

## Step 3: Set Up Azure Storage Connection

### Option A: Use Azurite (Local Emulator)

```bash
# Install Azurite globally
npm install -g azurite

# Start Azurite
azurite --silent --location c:/azurite
```

Use connection string: `UseDevelopmentStorage=true`

### Option B: Use Azure Storage Account

Get your connection string from Azure Portal:
1. Go to your Storage Account
2. Navigate to **Access keys**
3. Copy **Connection string**

## Step 4: Test Each Package

### Test Blob Storage

```csharp
using AzureStorage.Standard.Blobs;
using AzureStorage.Standard.Core.Domain.Models;

var options = new StorageOptions
{
    ConnectionString = "UseDevelopmentStorage=true" // Or your Azure connection string
};

var blobClient = new AzureBlobClient(options);

// Test 1: Upload a blob
var testData = "Hello, AzureStorage.Standard!"u8.ToArray();
await blobClient.UploadBlobAsync("test-container", "test.txt", testData, "text/plain");
Console.WriteLine("✅ Blob uploaded successfully");

// Test 2: Download the blob
var downloaded = await blobClient.DownloadBlobAsync("test-container", "test.txt");
Console.WriteLine($"✅ Blob downloaded: {System.Text.Encoding.UTF8.GetString(downloaded)}");

// Test 3: List blobs
var blobs = await blobClient.ListBlobsAsync("test-container");
Console.WriteLine($"✅ Found {blobs.Count()} blob(s)");

// Test 4: Delete blob
await blobClient.DeleteBlobAsync("test-container", "test.txt");
Console.WriteLine("✅ Blob deleted successfully");
```

### Test Queue Storage

```csharp
using AzureStorage.Standard.Queues;

var queueClient = new QueueClient(options);

// Test 1: Send message
await queueClient.SendMessageAsync("test-queue", "Hello from queue!");
Console.WriteLine("✅ Message sent to queue");

// Test 2: Receive message
var messages = await queueClient.ReceiveMessagesAsync("test-queue", maxMessages: 1);
var message = messages.FirstOrDefault();
if (message != null)
{
    Console.WriteLine($"✅ Message received: {message.MessageText}");

    // Test 3: Delete message
    await queueClient.DeleteMessageAsync("test-queue", message.MessageId, message.PopReceipt);
    Console.WriteLine("✅ Message deleted from queue");
}
```

### Test Table Storage

```csharp
using AzureStorage.Standard.Tables;
using Azure.Data.Tables;

var tableClient = new TableClient(options);

// Test 1: Create entity
var entity = new TableEntity("TestPartition", "TestRow")
{
    { "Name", "Test User" },
    { "Email", "test@example.com" }
};

await tableClient.UpsertEntityAsync("TestTable", entity);
Console.WriteLine("✅ Entity created in table");

// Test 2: Retrieve entity
var retrieved = await tableClient.GetEntityAsync("TestTable", "TestPartition", "TestRow");
Console.WriteLine($"✅ Entity retrieved: {retrieved.GetString("Name")}");

// Test 3: Delete entity
await tableClient.DeleteEntityAsync("TestTable", "TestPartition", "TestRow");
Console.WriteLine("✅ Entity deleted from table");
```

### Test File Share Storage

```csharp
using AzureStorage.Standard.Files;

var fileClient = new FileShareClient(options);

// Test 1: Upload file
var fileContent = "File content test"u8.ToArray();
await fileClient.UploadFileAsync("test-share", "test-directory", "test.txt", fileContent);
Console.WriteLine("✅ File uploaded to share");

// Test 2: Download file
var downloadedFile = await fileClient.DownloadFileAsync("test-share", "test-directory/test.txt");
Console.WriteLine($"✅ File downloaded: {System.Text.Encoding.UTF8.GetString(downloadedFile)}");

// Test 3: Delete file
await fileClient.DeleteFileAsync("test-share", "test-directory/test.txt");
Console.WriteLine("✅ File deleted from share");
```

## Step 5: Run the Tests

```bash
dotnet run
```

## Expected Results

All tests should pass with ✅ checkmarks. If you see any errors:

1. **Connection errors**: Verify Azurite is running or connection string is correct
2. **Package errors**: Check package versions are installed correctly
3. **API errors**: Report issues on GitHub: https://github.com/Clifftech123/AzureStorage.Standard/issues

## Step 6: Verify Package Quality

Check that:
- [ ] All operations complete without exceptions
- [ ] Error messages are clear and helpful
- [ ] IntelliSense shows proper documentation
- [ ] No NuGet warnings during package installation
- [ ] Packages restore correctly

## Ready for v1.0.0?

If all tests pass and everything works as expected, the packages are ready for v1.0.0 release!

## Reporting Issues

Found a bug? Please report it before v1.0.0 release:
- GitHub Issues: https://github.com/Clifftech123/AzureStorage.Standard/issues
- Include error messages, stack traces, and reproduction steps

## Next Steps

After testing is complete, follow the [RELEASE.md](RELEASE.md) guide to release v1.0.0.
