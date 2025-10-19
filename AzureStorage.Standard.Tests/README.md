# AzureStorage.Standard.Tests

Test project for AzureStorage.Standard library.

## Test Structure

This project contains tests for all Azure Storage services:
- Blob Storage
- Queue Storage
- Table Storage
- File Share Storage

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~QueueClientTests"

# Run tests by category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

## Writing Tests

### Unit Tests with Mocking

All client interfaces (`IBlobClient`, `IQueueClient`, `ITableClient`, `IFileShareClient`) can be mocked for unit testing:

```csharp
using Moq;
using AzureStorage.Standard.Core.Domain.Abstractions;

var mockQueueClient = new Mock<IQueueClient>();
mockQueueClient
    .Setup(x => x.SendMessageAsync("test-queue", "message", null, null, default))
    .Returns(Task.CompletedTask);

// Use mockQueueClient.Object in your tests
```

### Integration Tests with Azurite

For integration tests, use [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite):

```bash
# Install Azurite
npm install -g azurite

# Start Azurite
azurite --silent --location c:\azurite
```

Then use the Azurite connection string:
```csharp
var options = new StorageOptions
{
    ConnectionString = "UseDevelopmentStorage=true"
};
```

## Test Dependencies

- **xUnit** - Test framework
- **Moq** - Mocking library
- **FluentAssertions** - Assertion library

## Creating Tests

See individual test files for examples of:
- Mocking storage clients
- Integration testing with real storage
- Testing error scenarios
- Verifying method calls
