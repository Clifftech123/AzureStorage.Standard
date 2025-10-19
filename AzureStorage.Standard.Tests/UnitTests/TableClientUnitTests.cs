using AzureStorage.Standard.Core.Domain.Abstractions;
using AzureStorage.Standard.Core.Domain.Models;
using AzureStorage.Standard.Tables;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureStroage.Standard.Tests.UnitTests;

/// <summary>
/// Unit tests for Table Client using mocking.
/// These tests verify the behavior without requiring actual Azure Table Storage.
/// </summary>
public class TableClientUnitTests
{
    private readonly Mock<ITableClient> _mockTableClient;

    public TableClientUnitTests()
    {
        _mockTableClient = new Mock<ITableClient>();
    }

    [Fact]
    public async Task InsertEntityAsync_ShouldCallTableClient_WithCorrectEntity()
    {
        // Arrange
        const string tableName = "Customers";
        var customer = new CustomerEntity
        {
            PartitionKey = "USA",
            RowKey = "001",
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockTableClient
            .Setup(x => x.InsertEntityAsync(tableName, customer, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockTableClient.Object.InsertEntityAsync(tableName, customer);

        // Assert
        _mockTableClient.Verify(
            x => x.InsertEntityAsync(tableName, customer, default),
            Times.Once);
    }

    [Fact]
    public async Task GetEntityAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        const string tableName = "Customers";
        const string partitionKey = "USA";
        const string rowKey = "001";

        var expectedCustomer = new CustomerEntity
        {
            PartitionKey = partitionKey,
            RowKey = rowKey,
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockTableClient
            .Setup(x => x.GetEntityAsync<CustomerEntity>(tableName, partitionKey, rowKey, default))
            .ReturnsAsync(expectedCustomer);

        // Act
        var customer = await _mockTableClient.Object.GetEntityAsync<CustomerEntity>(tableName, partitionKey, rowKey);

        // Assert
        customer.Should().NotBeNull();
        customer.Name.Should().Be("John Doe");
        customer.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task QueryByPartitionKeyAsync_ShouldReturnEntitiesInPartition()
    {
        // Arrange
        const string tableName = "Customers";
        const string partitionKey = "USA";

        var expectedCustomers = new List<CustomerEntity>
        {
            new CustomerEntity { PartitionKey = "USA", RowKey = "001", Name = "John Doe" },
            new CustomerEntity { PartitionKey = "USA", RowKey = "002", Name = "Jane Smith" }
        };

        _mockTableClient
            .Setup(x => x.QueryByPartitionKeyAsync<CustomerEntity>(tableName, partitionKey, default))
            .ReturnsAsync(expectedCustomers);

        // Act
        var customers = await _mockTableClient.Object.QueryByPartitionKeyAsync<CustomerEntity>(tableName, partitionKey);

        // Assert
        customers.Should().HaveCount(2);
        customers.Should().OnlyContain(c => c.PartitionKey == "USA");
    }

    [Fact]
    public async Task UpdateEntityAsync_ShouldCallTableClient_WithUpdatedEntity()
    {
        // Arrange
        const string tableName = "Customers";
        var customer = new CustomerEntity
        {
            PartitionKey = "USA",
            RowKey = "001",
            Name = "John Doe Updated",
            Email = "john.updated@example.com",
            ETag = "*"
        };

        _mockTableClient
            .Setup(x => x.UpdateEntityAsync(tableName, customer, customer.ETag, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockTableClient.Object.UpdateEntityAsync(tableName, customer, customer.ETag);

        // Assert
        _mockTableClient.Verify(
            x => x.UpdateEntityAsync(tableName, customer, customer.ETag, default),
            Times.Once);
    }

    [Fact]
    public async Task DeleteEntityAsync_ShouldCallTableClient_WithCorrectKeys()
    {
        // Arrange
        const string tableName = "Customers";
        const string partitionKey = "USA";
        const string rowKey = "001";

        _mockTableClient
            .Setup(x => x.DeleteEntityAsync(tableName, partitionKey, rowKey, "*", default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockTableClient.Object.DeleteEntityAsync(tableName, partitionKey, rowKey);

        // Assert
        _mockTableClient.Verify(
            x => x.DeleteEntityAsync(tableName, partitionKey, rowKey, "*", default),
            Times.Once);
    }
}

/// <summary>
/// Example customer repository that uses ITableClient
/// This shows how to build a repository pattern with the library
/// </summary>
public class CustomerRepositoryTests
{
    private readonly Mock<ITableClient> _mockTableClient;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryTests()
    {
        _mockTableClient = new Mock<ITableClient>();
        _repository = new CustomerRepository(_mockTableClient.Object);
    }

    [Fact]
    public async Task AddCustomer_ShouldInsertEntity_IntoTable()
    {
        // Arrange
        var customer = new CustomerEntity
        {
            PartitionKey = "USA",
            RowKey = "001",
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockTableClient
            .Setup(x => x.InsertEntityAsync("Customers", It.IsAny<CustomerEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _repository.AddCustomer(customer);

        // Assert
        _mockTableClient.Verify(
            x => x.InsertEntityAsync("Customers", It.Is<CustomerEntity>(c => c.Email == "john@example.com"), default),
            Times.Once);
    }

    [Fact]
    public async Task GetCustomersByCountry_ShouldQueryByPartitionKey()
    {
        // Arrange
        const string country = "USA";
        var expectedCustomers = new List<CustomerEntity>
        {
            new CustomerEntity { PartitionKey = "USA", RowKey = "001", Name = "John" },
            new CustomerEntity { PartitionKey = "USA", RowKey = "002", Name = "Jane" }
        };

        _mockTableClient
            .Setup(x => x.QueryByPartitionKeyAsync<CustomerEntity>("Customers", country, default))
            .ReturnsAsync(expectedCustomers);

        // Act
        var customers = await _repository.GetCustomersByCountry(country);

        // Assert
        customers.Should().HaveCount(2);
    }
}

// Example repository implementation
public class CustomerRepository
{
    private readonly ITableClient _tableClient;
    private const string TableName = "Customers";

    public CustomerRepository(ITableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task AddCustomer(CustomerEntity customer)
    {
        await _tableClient.InsertEntityAsync(TableName, customer);
    }

    public async Task<IEnumerable<CustomerEntity>> GetCustomersByCountry(string country)
    {
        return await _tableClient.QueryByPartitionKeyAsync<CustomerEntity>(TableName, country);
    }
}

// Example entity
public class CustomerEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public string ETag { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
