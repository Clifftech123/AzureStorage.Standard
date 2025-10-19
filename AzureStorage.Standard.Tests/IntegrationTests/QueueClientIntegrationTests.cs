using AzureStorage.Standard.Core.Domain.Models;
using AzureStorage.Standard.Queues;
using FluentAssertions;
using Xunit;

namespace AzureStroage.Standard.Tests.IntegrationTests;

/// <summary>
/// Integration tests for Queue Client that connect to Azurite (Azure Storage Emulator).
///
/// Prerequisites to run these tests:
/// 1. Install Azurite: npm install -g azurite
/// 2. Start Azurite: azurite --silent --location c:\azurite --debug c:\azurite\debug.log
/// 3. Tests will use connection string: "UseDevelopmentStorage=true"
///
/// Azurite Queue service runs on: http://127.0.0.1:10001
/// </summary>
[Collection("Integration Tests")]
public class QueueClientIntegrationTests : IAsyncLifetime
{
    private QueueClient? _queueClient;
    private const string TestQueueName = "test-queue-integration";
    private const string ConnectionString = "UseDevelopmentStorage=true";

    public async Task InitializeAsync()
    {
        var options = new StorageOptions { ConnectionString = ConnectionString };
        _queueClient = new QueueClient(options);

        // Create test queue
        await _queueClient.CreateQueueIfNotExistsAsync(TestQueueName);
    }

    public async Task DisposeAsync()
    {
        if (_queueClient != null)
        {
            try
            {
                // Clean up test queue
                await _queueClient.DeleteQueueAsync(TestQueueName);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region Queue Operations Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateQueueIfNotExistsAsync_ShouldCreateQueue_WhenQueueDoesNotExist()
    {
        // Arrange
        const string queueName = "test-queue-create";

        try
        {
            // Act
            var result = await _queueClient!.CreateQueueIfNotExistsAsync(queueName);

            // Assert
            result.Should().BeTrue();

            var exists = await _queueClient.QueueExistsAsync(queueName);
            exists.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            await _queueClient!.DeleteQueueAsync(queueName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateQueueIfNotExistsAsync_ShouldReturnFalse_WhenQueueAlreadyExists()
    {
        // Act
        var result = await _queueClient!.CreateQueueIfNotExistsAsync(TestQueueName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task QueueExistsAsync_ShouldReturnTrue_WhenQueueExists()
    {
        // Act
        var exists = await _queueClient!.QueueExistsAsync(TestQueueName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task QueueExistsAsync_ShouldReturnFalse_WhenQueueDoesNotExist()
    {
        // Act
        var exists = await _queueClient!.QueueExistsAsync("non-existent-queue");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteQueueAsync_ShouldDeleteQueue_WhenQueueExists()
    {
        // Arrange
        const string queueName = "test-queue-delete";
        await _queueClient!.CreateQueueIfNotExistsAsync(queueName);

        // Act
        var result = await _queueClient.DeleteQueueAsync(queueName);

        // Assert
        result.Should().BeTrue();

        var exists = await _queueClient.QueueExistsAsync(queueName);
        exists.Should().BeFalse();
    }

    #endregion

    #region Message Operations Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SendMessageAsync_ShouldSendMessage_Successfully()
    {
        // Arrange
        const string message = "Test message for integration test";

        // Act
        await _queueClient!.SendMessageAsync(TestQueueName, message);

        // Assert - Message should be retrievable
        var messages = await _queueClient.ReceiveMessagesAsync(TestQueueName, 1);
        messages.Should().ContainSingle();
        messages.First().MessageText.Should().Be(message);

        // Cleanup
        await _queueClient.DeleteMessageAsync(TestQueueName, messages.First().MessageId, messages.First().PopReceipt);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SendMessageAsync_WithVisibilityTimeout_ShouldDelayMessageVisibility()
    {
        // Arrange
        const string message = "Delayed message";
        var visibilityTimeout = TimeSpan.FromSeconds(5);

        // Act
        await _queueClient!.SendMessageAsync(TestQueueName, message, visibilityTimeout);

        // Assert - Message should not be immediately visible
        var messages = await _queueClient.ReceiveMessagesAsync(TestQueueName, 1);
        messages.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ReceiveMessagesAsync_ShouldReturnMessages_WhenMessagesExist()
    {
        // Arrange
        const string message1 = "Message 1";
        const string message2 = "Message 2";
        await _queueClient!.SendMessageAsync(TestQueueName, message1);
        await _queueClient.SendMessageAsync(TestQueueName, message2);

        // Act
        var messages = await _queueClient.ReceiveMessagesAsync(TestQueueName, 10);

        // Assert
        messages.Should().HaveCountGreaterOrEqualTo(2);
        messages.Should().Contain(m => m.MessageText == message1);
        messages.Should().Contain(m => m.MessageText == message2);

        // Cleanup
        foreach (var msg in messages)
        {
            await _queueClient.DeleteMessageAsync(TestQueueName, msg.MessageId, msg.PopReceipt);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ReceiveMessagesAsync_ShouldReturnEmpty_WhenNoMessages()
    {
        // Arrange
        const string emptyQueueName = "test-queue-empty";
        await _queueClient!.CreateQueueIfNotExistsAsync(emptyQueueName);

        try
        {
            // Act
            var messages = await _queueClient.ReceiveMessagesAsync(emptyQueueName, 10);

            // Assert
            messages.Should().BeEmpty();
        }
        finally
        {
            await _queueClient.DeleteQueueAsync(emptyQueueName);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task PeekMessagesAsync_ShouldReturnMessages_WithoutDequeueing()
    {
        // Arrange
        const string message = "Peek test message";
        await _queueClient!.SendMessageAsync(TestQueueName, message);

        // Act
        var peekedMessages = await _queueClient.PeekMessagesAsync(TestQueueName, 10);

        // Assert
        peekedMessages.Should().ContainSingle(m => m.MessageText == message);

        // Verify message is still in queue
        var receivedMessages = await _queueClient.ReceiveMessagesAsync(TestQueueName, 10);
        receivedMessages.Should().Contain(m => m.MessageText == message);

        // Cleanup
        foreach (var msg in receivedMessages)
        {
            await _queueClient.DeleteMessageAsync(TestQueueName, msg.MessageId, msg.PopReceipt);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteMessageAsync_ShouldDeleteMessage_Successfully()
    {
        // Arrange
        const string message = "Message to delete";
        await _queueClient!.SendMessageAsync(TestQueueName, message);
        var messages = await _queueClient.ReceiveMessagesAsync(TestQueueName, 1);
        var messageToDelete = messages.First();

        // Act
        await _queueClient.DeleteMessageAsync(TestQueueName, messageToDelete.MessageId, messageToDelete.PopReceipt);

        // Assert - Try to receive again, should not get the same message ID
        var remainingMessages = await _queueClient.ReceiveMessagesAsync(TestQueueName, 10);
        remainingMessages.Should().NotContain(m => m.MessageId == messageToDelete.MessageId);

        // Cleanup
        foreach (var msg in remainingMessages)
        {
            await _queueClient.DeleteMessageAsync(TestQueueName, msg.MessageId, msg.PopReceipt);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ClearMessagesAsync_ShouldDeleteAllMessages()
    {
        // Arrange
        await _queueClient!.SendMessageAsync(TestQueueName, "Message 1");
        await _queueClient.SendMessageAsync(TestQueueName, "Message 2");
        await _queueClient.SendMessageAsync(TestQueueName, "Message 3");

        // Act
        await _queueClient.ClearMessagesAsync(TestQueueName);

        // Assert - Queue should be empty
        var messages = await _queueClient.ReceiveMessagesAsync(TestQueueName, 10);
        messages.Should().BeEmpty();
    }

    #endregion

    #region Queue Properties Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMessageCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        const string queueName = "test-queue-count";
        await _queueClient!.CreateQueueIfNotExistsAsync(queueName);

        try
        {
            // Clear queue first
            await _queueClient.ClearMessagesAsync(queueName);

            // Add known number of messages
            await _queueClient.SendMessageAsync(queueName, "Message 1");
            await _queueClient.SendMessageAsync(queueName, "Message 2");
            await _queueClient.SendMessageAsync(queueName, "Message 3");

            // Act
            var count = await _queueClient.GetMessageCountAsync(queueName);

            // Assert
            count.Should().Be(3);
        }
        finally
        {
            await _queueClient!.DeleteQueueAsync(queueName);
        }
    }

    #endregion

    #region Update Message Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpdateMessageAsync_ShouldUpdateMessageContent()
    {
        // Arrange
        const string originalMessage = "Original message";
        const string updatedMessage = "Updated message";

        await _queueClient!.SendMessageAsync(TestQueueName, originalMessage);
        var messages = await _queueClient.ReceiveMessagesAsync(TestQueueName, 1);
        var message = messages.First();

        // Act
        await _queueClient.UpdateMessageAsync(
            TestQueueName,
            message.MessageId,
            message.PopReceipt,
            updatedMessage,
            TimeSpan.FromSeconds(1),
            System.Threading.CancellationToken.None);

        // Assert - Peek to see updated content (after visibility timeout)
        await Task.Delay(1000); // Wait for visibility timeout
        var peekedMessages = await _queueClient.PeekMessagesAsync(TestQueueName, 10);
        peekedMessages.Should().Contain(m => m.MessageText == updatedMessage);

        // Cleanup
        var receivedMessages = await _queueClient.ReceiveMessagesAsync(TestQueueName, 10);
        foreach (var msg in receivedMessages)
        {
            await _queueClient.DeleteMessageAsync(TestQueueName, msg.MessageId, msg.PopReceipt);
        }
    }

    #endregion
}
