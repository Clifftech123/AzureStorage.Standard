using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureSt.Storage.Standard.Queues
{
    /// <summary>
    /// Interface for Azure Queue Storage operations.
    /// Provides comprehensive methods for managing queues and messages.
    /// This is a simplified wrapper around the Azure.Storage.Queues SDK.
    /// </summary>
    public interface IQueueClient : IDisposable
    {
        #region Queue Operations

        /// <summary>
        /// Lists all queues in the storage account.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of queue names.</returns>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<IEnumerable<string>> ListQueuesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a queue if it does not already exist.
        /// Queue names must be lowercase, 3-63 characters, and contain only letters, numbers, and hyphens.
        /// </summary>
        /// <param name="queueName">The name of the queue to create.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the queue was created; false if it already existed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> CreateQueueIfNotExistsAsync(string queueName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a queue if it exists.
        /// Warning: Deleting a queue permanently removes all messages within it.
        /// </summary>
        /// <param name="queueName">The name of the queue to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the queue was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a queue exists in the storage account.
        /// </summary>
        /// <param name="queueName">The name of the queue to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the queue exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> QueueExistsAsync(string queueName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the approximate number of messages in a queue.
        /// Note: The count is approximate and may not be exact due to the distributed nature of Azure Storage.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>The approximate number of messages in the queue.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<int> GetMessageCountAsync(string queueName, CancellationToken cancellationToken = default);

        #endregion

        #region Message Operations

        /// <summary>
        /// Sends a text message to a queue.
        /// Messages can be up to 64 KB in size.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="message">The message text to send.</param>
        /// <param name="visibilityTimeout">Optional. The time the message should be invisible after being retrieved. Maximum 7 days.</param>
        /// <param name="timeToLive">Optional. The time-to-live for the message. Maximum 7 days.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> or <paramref name="message"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task SendMessageAsync(string queueName, string message, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a binary message to a queue.
        /// The byte array is automatically converted to Base64 encoding for transmission.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="message">The binary message content to send.</param>
        /// <param name="visibilityTimeout">Optional. The time the message should be invisible after being retrieved.</param>
        /// <param name="timeToLive">Optional. The time-to-live for the message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task SendMessageAsync(string queueName, byte[] message, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends multiple messages to a queue in sequence.
        /// Note: Messages are sent one at a time.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="messages">The collection of messages to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> or <paramref name="messages"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task SendMessagesAsync(string queueName, IEnumerable<string> messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives and removes messages from a queue, making them invisible to other consumers.
        /// Retrieved messages must be deleted explicitly after processing. Maximum 32 messages can be retrieved at once.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="maxMessages">The maximum number of messages to retrieve (1-32). Defaults to 1.</param>
        /// <param name="visibilityTimeout">Optional. The time the messages should remain invisible.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of received messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxMessages"/> is not between 1 and 32.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<IEnumerable<QueueMessageItem>> ReceiveMessagesAsync(string queueName, int maxMessages = 1, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Peeks at messages without removing them from the queue or affecting their visibility.
        /// Peeked messages remain visible to all consumers and do not provide a pop receipt.
        /// Maximum 32 messages can be peeked at once.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="maxMessages">The maximum number of messages to peek (1-32). Defaults to 1.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of peeked messages (without pop receipt).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxMessages"/> is not between 1 and 32.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<IEnumerable<QueueMessageItem>> PeekMessagesAsync(string queueName, int maxMessages = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a message from a queue after processing.
        /// You must provide the message ID and pop receipt obtained from ReceiveMessagesAsync.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="messageId">The message ID.</param>
        /// <param name="popReceipt">The pop receipt from when the message was received.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a message's content and/or extends its visibility timeout.
        /// Returns an updated pop receipt for future operations.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="messageId">The message ID.</param>
        /// <param name="popReceipt">The current pop receipt from when the message was received.</param>
        /// <param name="message">The new message content.</param>
        /// <param name="visibilityTimeout">The new visibility timeout.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>An updated message item with the new pop receipt.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<QueueMessageItem> UpdateMessageAsync(string queueName, string messageId, string popReceipt, string message, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears all messages from a queue.
        /// Warning: This operation permanently deletes all messages in the queue and cannot be undone.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task ClearMessagesAsync(string queueName, CancellationToken cancellationToken = default);

        #endregion
    }
}
