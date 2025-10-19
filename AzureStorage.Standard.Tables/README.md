# AzureStorage.Standard.Tables

A simplified, modern .NET client library for Azure Table Storage with built-in error handling and intuitive APIs for managing tables and entities.

[![NuGet](https://img.shields.io/nuget/v/AzureStorage.Standard.Tables.svg)](https://www.nuget.org/packages/AzureStorage.Standard.Tables/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

 **Simplified API** - Easy-to-use wrapper around Azure.Data.Tables SDK
 **Full CRUD Operations** - Insert, query, update, and delete entities
 **Table Management** - Create, delete, and list tables
 **Flexible Querying** - OData filter support for complex queries
 **Batch Transactions** - Atomic operations on multiple entities
 **Partition Key Queries** - Optimized queries by partition
 **Type-Safe Entities** - Generic support for custom entity types
 **Comprehensive Error Handling** - Detailed exception information
 **Extensive Documentation** - Full XML documentation for IntelliSense

## Installation

```bash
dotnet add package AzureStorage.Standard.Tables
```

Or via Package Manager Console:

```powershell
Install-Package AzureStorage.Standard.Tables
```

## Quick Start

### 1. Configure the Client

```csharp
using AzureStorage.Standard.Tables;
using AzureStorage.Standard.Core;

var options = new StorageOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=..."
};

var tableClient = new TableClient(options);
```

### 2. Define Your Entity

```csharp
using AzureStorage.Standard.Core.Domain.Models;

public class Customer : ITableEntity
{
    public string PartitionKey { get; set; }  // e.g., "USA"
    public string RowKey { get; set; }         // e.g., Customer ID
    public DateTimeOffset? Timestamp { get; set; }
    public string ETag { get; set; }

    // Custom properties
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}
```

### 3. Create a Table

```csharp
await tableClient.CreateTableIfNotExistsAsync("Customers");
```

### 4. Insert an Entity

```csharp
var customer = new Customer
{
    PartitionKey = "USA",
    RowKey = "customer-001",
    Name = "John Doe",
    Email = "john@example.com",
    Age = 30
};

await tableClient.InsertEntityAsync("Customers", customer);
```

### 5. Query Entities

```csharp
// Get a specific entity
var customer = await tableClient.GetEntityAsync<Customer>(
    tableName: "Customers",
    partitionKey: "USA",
    rowKey: "customer-001"
);

// Query by partition key (efficient!)
var usCustomers = await tableClient.QueryByPartitionKeyAsync<Customer>(
    tableName: "Customers",
    partitionKey: "USA"
);

// Query with OData filter
var filter = "Age gt 25 and Email ne null";
var results = await tableClient.QueryEntitiesAsync<Customer>(
    tableName: "Customers",
    filter: filter
);
```

### 6. Update an Entity

```csharp
customer.Age = 31;
await tableClient.UpdateEntityAsync("Customers", customer);
```

### 7. Delete an Entity

```csharp
await tableClient.DeleteEntityAsync(
    tableName: "Customers",
    partitionKey: "USA",
    rowKey: "customer-001"
);
```

## Advanced Usage

### Upsert Operations

```csharp
// Insert or replace (overwrites all properties)
await tableClient.UpsertEntityAsync("Customers", customer);
```

### Batch Transactions

```csharp
var actions = new List<TableTransactionAction>
{
    new TableTransactionAction
    {
        ActionType = TableTransactionActionType.Insert,
        Entity = customer1
    },
    new TableTransactionAction
    {
        ActionType = TableTransactionActionType.Update,
        Entity = customer2,
        ETag = "*"
    },
    new TableTransactionAction
    {
        ActionType = TableTransactionActionType.Delete,
        Entity = customer3,
        ETag = "*"
    }
};

// All operations must be in the same partition
await tableClient.ExecuteBatchAsync("Customers", actions);
```

### Optimistic Concurrency with ETags

```csharp
var customer = await tableClient.GetEntityAsync<Customer>("Customers", "USA", "customer-001");

customer.Age = 32;

// Update only if ETag matches (no concurrent modifications)
await tableClient.UpdateEntityAsync(
    tableName: "Customers",
    entity: customer,
    eTag: customer.ETag  // Will fail if entity was modified by another process
);
```

### Query with Pagination

```csharp
var results = await tableClient.QueryEntitiesAsync<Customer>(
    tableName: "Customers",
    filter: "Age gt 21",
    maxPerPage: 100  // Limit results per page
);
```

### Get All Entities (Use with Caution)

```csharp
// Warning: Retrieves ALL entities - use filtering for large tables
var allCustomers = await tableClient.GetAllEntitiesAsync<Customer>("Customers");
```

## Table Management

```csharp
// List all tables
var tables = await tableClient.ListTablesAsync();

// Check if table exists
bool exists = await tableClient.TableExistsAsync("Customers");

// Delete table
await tableClient.DeleteTableAsync("Customers");
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
    ServiceUri = new Uri("https://myaccount.table.core.windows.net")
};
```

## Best Practices

### 1. Partition Key Design
Choose partition keys that distribute data evenly:
```csharp
// Good: Distributes by country
PartitionKey = "USA"

// Bad: All entities in one partition
PartitionKey = "AllCustomers"
```

### 2. Row Key Design
Use unique identifiers:
```csharp
// Good: Unique customer ID
RowKey = $"customer-{customerId}"

// Good: Timestamp for time-series data
RowKey = DateTime.UtcNow.Ticks.ToString()
```

### 3. Query Optimization
Always use partition key when possible:
```csharp
// Efficient: Queries single partition
var results = await tableClient.QueryByPartitionKeyAsync<Customer>("Customers", "USA");

// Inefficient: Scans all partitions
var results = await tableClient.GetAllEntitiesAsync<Customer>("Customers");
```

## Supported Frameworks

- .NET Standard 2.0
- .NET Standard 2.1
- .NET 7.0
- .NET 8.0
- .NET 9.0

## Error Handling

```csharp
try
{
    await tableClient.InsertEntityAsync("Customers", customer);
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
