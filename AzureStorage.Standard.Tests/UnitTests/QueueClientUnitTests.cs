using AzureSt.Storage.Standard.Queues;
using AzureStorage.Standard.Core.Domain.Abstractions;
using AzureStorage.Standard.Core.Domain.Models;
using AzureStorage.Standard.Queues;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureStroage.Standard.Tests.UnitTests;

/// <summary>
/// Unit tests for Queue Client using mocking.
/// These tests don't require Azure Storage connection - they use mocks to verify behavior.
/// </summary>
public class QueueClientUnitTests
{
    private readonly Mock<IQueueClient> _mockQueueClient;

    public QueueClientUnitTests()
    {
        _mockQueueClient = new Mock<IQueueClient>();
    }

    [Fact]
    public async Task SendMessageAsync_ShouldCallQueueClient_WithCorrectParameters()
    {
        // Arrange
        const string queueName = "test-queue";
        const string message = "Test message";

        _mockQueueClient
            .Setup(x => x.SendMessageAsync(queueName, message, null, null, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockQueueClient.Object.SendMessageAsync(queueName, message);

        // Assert
        _mockQueueClient.Verify(
            x => x.SendMessageAsync(queueName, message, null, null, default),
            Times.Once);
    }

    [Fact]
    public async Task ReceiveMessagesAsync_ShouldReturnMessages_WhenMessagesExist()
    {
        // Arrange
        const string queueName = "test-queue";
        var expectedMessages = new List<QueueMessageItem>
        {
            new QueueMessageItem
            {
                MessageId = "msg-1",
                MessageText = "Message 1",
                PopReceipt = "receipt-1",
                DequeueCount = 1
            },
            new QueueMessageItem
            {
                MessageId = "msg-2",
                MessageText = "Message 2",
                PopReceipt = "receipt-2",
                DequeueCount = 1
            }
        };

        _mockQueueClient
            .Setup(x => x.ReceiveMessagesAsync(queueName, 10, null, default))
            .ReturnsAsync(expectedMessages);

        // Act
        var messages = await _mockQueueClient.Object.ReceiveMessagesAsync(queueName, 10);

        // Assert
        messages.Should().HaveCount(2);
        messages.Should().BeEquivalentTo(expectedMessages);
    }

    [Fact]
    public async Task DeleteMessageAsync_ShouldCallQueueClient_WithCorrectParameters()
    {
        // Arrange
        const string queueName = "test-queue";
        const string messageId = "msg-123";
        const string popReceipt = "receipt-456";

        _mockQueueClient
            .Setup(x => x.DeleteMessageAsync(queueName, messageId, popReceipt, default))
            .Returns(Task.CompletedTask);

        // Act
        await _mockQueueClient.Object.DeleteMessageAsync(queueName, messageId, popReceipt);

        // Assert
        _mockQueueClient.Verify(
            x => x.DeleteMessageAsync(queueName, messageId, popReceipt, default),
            Times.Once);
    }

    [Fact]
    public async Task CreateQueueIfNotExistsAsync_ShouldReturnTrue_WhenQueueIsCreated()
    {
        // Arrange
        const string queueName = "new-queue";

        _mockQueueClient
            .Setup(x => x.CreateQueueIfNotExistsAsync(queueName, default))
            .ReturnsAsync(true);

        // Act
        var result = await _mockQueueClient.Object.CreateQueueIfNotExistsAsync(queueName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetMessageCountAsync_ShouldReturnCount()
    {
        // Arrange
        const string queueName = "test-queue";
        const int expectedCount = 42;

        _mockQueueClient
            .Setup(x => x.GetMessageCountAsync(queueName, default))
            .ReturnsAsync(expectedCount);

        // Act
        var count = await _mockQueueClient.Object.GetMessageCountAsync(queueName);

        // Assert
        count.Should().Be(expectedCount);
    }
}

/// <summary>
/// Example of testing a service that uses IQueueClient
/// This demonstrates how consumers of the library would write their tests
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IQueueClient> _mockQueueClient;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockQueueClient = new Mock<IQueueClient>();
        _orderService = new OrderService(_mockQueueClient.Object);
    }

    [Fact]
    public async Task ProcessOrder_ShouldSendMessageToQueue()
    {
        // Arrange
        var order = new Order { Id = "ORD-123", Amount = 99.99m };

        _mockQueueClient
            .Setup(x => x.SendMessageAsync("orders", It.IsAny<string>(), null, null, default))
            .Returns(Task.CompletedTask);

        // Act
        await _orderService.ProcessOrder(order);

        // Assert
        _mockQueueClient.Verify(
            x => x.SendMessageAsync("orders", It.Is<string>(msg => msg.Contains("ORD-123")), null, null, default),
            Times.Once);
    }
}

// Example service that uses IQueueClient
public class OrderService
{
    private readonly IQueueClient _queueClient;

    public OrderService(IQueueClient queueClient)
    {
        _queueClient = queueClient;
    }

    public async Task ProcessOrder(Order order)
    {
        var message = $"Process order {order.Id} with amount {order.Amount}";
        await _queueClient.SendMessageAsync("orders", message);
    }
}

public class Order
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
