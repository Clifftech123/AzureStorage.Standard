using AzureStorage.Standard.Core.Domain.Abstractions;
using AzureStorage.Standard.Core.Domain.Models;
using AzureStorage.Standard.Tables;
using FluentAssertions;
using Xunit;

namespace AzureStroage.Standard.Tests.IntegrationTests;

/// <summary>
/// Integration tests for Table Client that connect to Azurite (Azure Storage Emulator).
///
/// Prerequisites to run these tests:
/// 1. Install Azurite: npm install -g azurite
/// 2. Start Azurite: azurite --silent --location c:\azurite --debug c:\azurite\debug.log
/// 3. Tests will use connection string: "UseDevelopmentStorage=true"
///
/// Azurite Table service runs on: http://127.0.0.1:10002
/// </summary>
[Collection("Integration Tests")]
public class TableClientIntegrationTests : IAsyncLifetime
{
    private TableClient? _tableClient;
    private const string TestTableName = "TestCustomersIntegration";
    private const string ConnectionString = "UseDevelopmentStorage=true";

    public async Task InitializeAsync()
    {
        var options = new StorageOptions { ConnectionString = ConnectionString };
        _tableClient = new TableClient(options);

        // Create test table
        await _tableClient.CreateTableIfNotExistsAsync(TestTableName);
    }

    public async Task DisposeAsync()
    {
        if (_tableClient != null)
        {
            try
            {
                // Clean up test table
                await _tableClient.DeleteTableAsync(TestTableName);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region Table Operations Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateTableIfNotExistsAsync_ShouldCreateTable_WhenTableDoesNotExist()
    {
        // Arrange
        const string tableName = "TestTableCreate";

        try
        {
            // Act
            var result = await _tableClient!.CreateTableIfNotExistsAsync(tableName);

            // Assert
            result.Should().BeTrue();

            var exists = await _tableClient.TableExistsAsync(tableName);
            exists.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            await _tableClient!.DeleteTableAsync(tableName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateTableIfNotExistsAsync_ShouldReturnFalse_WhenTableAlreadyExists()
    {
        // Act
        var result = await _tableClient!.CreateTableIfNotExistsAsync(TestTableName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TableExistsAsync_ShouldReturnTrue_WhenTableExists()
    {
        // Act
        var exists = await _tableClient!.TableExistsAsync(TestTableName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TableExistsAsync_ShouldReturnFalse_WhenTableDoesNotExist()
    {
        // Act
        var exists = await _tableClient!.TableExistsAsync("NonExistentTable");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteTableAsync_ShouldDeleteTable_WhenTableExists()
    {
        // Arrange
        const string tableName = "TestTableDelete";
        await _tableClient!.CreateTableIfNotExistsAsync(tableName);

        // Act
        var result = await _tableClient.DeleteTableAsync(tableName);

        // Assert
        result.Should().BeTrue();

        var exists = await _tableClient.TableExistsAsync(tableName);
        exists.Should().BeFalse();
    }

    #endregion

    #region Entity Insert Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task InsertEntityAsync_ShouldInsertEntity_Successfully()
    {
        // Arrange
        var customer = new TestCustomerEntity
        {
            PartitionKey = "USA",
            RowKey = Guid.NewGuid().ToString(),
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30
        };

        try
        {
            // Act
            await _tableClient!.InsertEntityAsync(TestTableName, customer);

            // Assert - Verify entity exists
            var retrieved = await _tableClient.GetEntityAsync<TestCustomerEntity>(
                TestTableName,
                customer.PartitionKey,
                customer.RowKey);

            retrieved.Should().NotBeNull();
            retrieved.Name.Should().Be(customer.Name);
            retrieved.Email.Should().Be(customer.Email);
            retrieved.Age.Should().Be(customer.Age);
        }
        finally
        {
            // Cleanup
            await _tableClient!.DeleteEntityAsync(TestTableName, customer.PartitionKey, customer.RowKey);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpsertEntityAsync_ShouldInsertEntity_WhenEntityDoesNotExist()
    {
        // Arrange
        var customer = new TestCustomerEntity
        {
            PartitionKey = "UK",
            RowKey = Guid.NewGuid().ToString(),
            Name = "Jane Smith",
            Email = "jane@example.com",
            Age = 25
        };

        try
        {
            // Act
            await _tableClient!.UpsertEntityAsync(TestTableName, customer);

            // Assert
            var retrieved = await _tableClient.GetEntityAsync<TestCustomerEntity>(
                TestTableName,
                customer.PartitionKey,
                customer.RowKey);

            retrieved.Should().NotBeNull();
            retrieved.Name.Should().Be(customer.Name);
        }
        finally
        {
            await _tableClient!.DeleteEntityAsync(TestTableName, customer.PartitionKey, customer.RowKey);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpsertEntityAsync_ShouldUpdateEntity_WhenEntityExists()
    {
        // Arrange
        var customer = new TestCustomerEntity
        {
            PartitionKey = "Canada",
            RowKey = Guid.NewGuid().ToString(),
            Name = "Bob Johnson",
            Email = "bob@example.com",
            Age = 35
        };

        await _tableClient!.InsertEntityAsync(TestTableName, customer);

        try
        {
            // Act - Update the entity
            customer.Name = "Bob Johnson Updated";
            customer.Age = 36;
            await _tableClient.UpsertEntityAsync(TestTableName, customer);

            // Assert
            var retrieved = await _tableClient.GetEntityAsync<TestCustomerEntity>(
                TestTableName,
                customer.PartitionKey,
                customer.RowKey);

            retrieved.Name.Should().Be("Bob Johnson Updated");
            retrieved.Age.Should().Be(36);
        }
        finally
        {
            await _tableClient!.DeleteEntityAsync(TestTableName, customer.PartitionKey, customer.RowKey);
        }
    }

    #endregion

    #region Entity Retrieve Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetEntityAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        var customer = new TestCustomerEntity
        {
            PartitionKey = "Germany",
            RowKey = Guid.NewGuid().ToString(),
            Name = "Hans Mueller",
            Email = "hans@example.com",
            Age = 40
        };

        await _tableClient!.InsertEntityAsync(TestTableName, customer);

        try
        {
            // Act
            var retrieved = await _tableClient.GetEntityAsync<TestCustomerEntity>(
                TestTableName,
                customer.PartitionKey,
                customer.RowKey);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved.PartitionKey.Should().Be(customer.PartitionKey);
            retrieved.RowKey.Should().Be(customer.RowKey);
            retrieved.Name.Should().Be(customer.Name);
            retrieved.Email.Should().Be(customer.Email);
        }
        finally
        {
            await _tableClient!.DeleteEntityAsync(TestTableName, customer.PartitionKey, customer.RowKey);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task QueryByPartitionKeyAsync_ShouldReturnEntitiesInPartition()
    {
        // Arrange
        var partitionKey = "France";
        var customer1 = new TestCustomerEntity
        {
            PartitionKey = partitionKey,
            RowKey = Guid.NewGuid().ToString(),
            Name = "Pierre Dubois",
            Email = "pierre@example.com",
            Age = 28
        };
        var customer2 = new TestCustomerEntity
        {
            PartitionKey = partitionKey,
            RowKey = Guid.NewGuid().ToString(),
            Name = "Marie Laurent",
            Email = "marie@example.com",
            Age = 32
        };

        await _tableClient!.InsertEntityAsync(TestTableName, customer1);
        await _tableClient.InsertEntityAsync(TestTableName, customer2);

        try
        {
            // Act
            var results = await _tableClient.QueryByPartitionKeyAsync<TestCustomerEntity>(
                TestTableName,
                partitionKey);

            // Assert
            results.Should().HaveCountGreaterOrEqualTo(2);
            results.Should().Contain(c => c.Name == "Pierre Dubois");
            results.Should().Contain(c => c.Name == "Marie Laurent");
            results.Should().OnlyContain(c => c.PartitionKey == partitionKey);
        }
        finally
        {
            await _tableClient!.DeleteEntityAsync(TestTableName, customer1.PartitionKey, customer1.RowKey);
            await _tableClient.DeleteEntityAsync(TestTableName, customer2.PartitionKey, customer2.RowKey);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task QueryEntitiesAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var customer1 = new TestCustomerEntity
        {
            PartitionKey = "Spain",
            RowKey = Guid.NewGuid().ToString(),
            Name = "Carlos Garcia",
            Email = "carlos@example.com",
            Age = 45
        };
        var customer2 = new TestCustomerEntity
        {
            PartitionKey = "Italy",
            RowKey = Guid.NewGuid().ToString(),
            Name = "Giovanni Rossi",
            Email = "giovanni@example.com",
            Age = 38
        };

        await _tableClient!.InsertEntityAsync(TestTableName, customer1);
        await _tableClient.InsertEntityAsync(TestTableName, customer2);

        try
        {
            // Act
            var results = await _tableClient.QueryEntitiesAsync<TestCustomerEntity>(TestTableName);

            // Assert
            results.Should().HaveCountGreaterOrEqualTo(2);
            results.Should().Contain(c => c.Name == "Carlos Garcia");
            results.Should().Contain(c => c.Name == "Giovanni Rossi");
        }
        finally
        {
            await _tableClient!.DeleteEntityAsync(TestTableName, customer1.PartitionKey, customer1.RowKey);
            await _tableClient.DeleteEntityAsync(TestTableName, customer2.PartitionKey, customer2.RowKey);
        }
    }

    #endregion

    #region Entity Update Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpdateEntityAsync_ShouldUpdateEntity_Successfully()
    {
        // Arrange
        var customer = new TestCustomerEntity
        {
            PartitionKey = "Japan",
            RowKey = Guid.NewGuid().ToString(),
            Name = "Yuki Tanaka",
            Email = "yuki@example.com",
            Age = 27
        };

        await _tableClient!.InsertEntityAsync(TestTableName, customer);

        try
        {
            // Retrieve to get ETag
            var retrieved = await _tableClient.GetEntityAsync<TestCustomerEntity>(
                TestTableName,
                customer.PartitionKey,
                customer.RowKey);

            // Act - Update the entity
            retrieved.Name = "Yuki Tanaka Updated";
            retrieved.Age = 28;
            await _tableClient.UpdateEntityAsync(TestTableName, retrieved, retrieved.ETag);

            // Assert
            var updated = await _tableClient.GetEntityAsync<TestCustomerEntity>(
                TestTableName,
                customer.PartitionKey,
                customer.RowKey);

            updated.Name.Should().Be("Yuki Tanaka Updated");
            updated.Age.Should().Be(28);
        }
        finally
        {
            await _tableClient!.DeleteEntityAsync(TestTableName, customer.PartitionKey, customer.RowKey);
        }
    }

    #endregion

    #region Entity Delete Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteEntityAsync_ShouldDeleteEntity_Successfully()
    {
        // Arrange
        var customer = new TestCustomerEntity
        {
            PartitionKey = "Australia",
            RowKey = Guid.NewGuid().ToString(),
            Name = "Jack Wilson",
            Email = "jack@example.com",
            Age = 33
        };

        await _tableClient!.InsertEntityAsync(TestTableName, customer);

        // Act
        await _tableClient.DeleteEntityAsync(TestTableName, customer.PartitionKey, customer.RowKey);

        // Assert - Entity should not exist
        await Assert.ThrowsAsync<Azure.RequestFailedException>(async () =>
        {
            await _tableClient.GetEntityAsync<TestCustomerEntity>(
                TestTableName,
                customer.PartitionKey,
                customer.RowKey);
        });
    }

    #endregion
}

/// <summary>
/// Test entity for integration tests
/// </summary>
public class TestCustomerEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public string ETag { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}
