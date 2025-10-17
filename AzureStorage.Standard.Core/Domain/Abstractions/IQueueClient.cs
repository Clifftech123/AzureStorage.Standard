using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureSt.Storage.Standard.Queues
{
  

/// <summary>
    /// Interface for Azure Queue Storage operations
    /// </summary>
    public interface IQueueClient : IDisposable
    {
        // Queue Operations
        /// <summary>
        /// Lists all queues in the storage account
        /// </summary>
        Task<IEnumerable<string>> ListQueuesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a queue if it doesn't exist
        /// </summary>
        Task<bool> CreateQueueIfNotExistsAsync(string queueName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a queue
        /// </summary>
        Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a queue exists
        /// </summary>
        Task<bool> QueueExistsAsync(string queueName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the approximate number of messages in a queue
        /// </summary>
        Task<int> GetMessageCountAsync(string queueName, CancellationToken cancellationToken = default);

        // Message Operations
        /// <summary>
        /// Sends a message to a queue
        /// </summary>
        Task SendMessageAsync(string queueName, string message, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message with binary content to a queue
        /// </summary>
        Task SendMessageAsync(string queueName, byte[] message, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends multiple messages to a queue in batch
        /// </summary>
        Task SendMessagesAsync(string queueName, IEnumerable<string> messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives messages from a queue (up to 32)
        /// </summary>
        Task<IEnumerable<QueueMessageItem>> ReceiveMessagesAsync(string queueName, int maxMessages = 1, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Peeks at messages without removing them from the queue (up to 32)
        /// </summary>
        Task<IEnumerable<QueueMessageItem>> PeekMessagesAsync(string queueName, int maxMessages = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a message from a queue
        /// </summary>
        Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a message's content and visibility timeout
        /// </summary>
        Task<QueueMessageItem> UpdateMessageAsync(string queueName, string messageId, string popReceipt, string message, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears all messages from a queue
        /// </summary>
        Task ClearMessagesAsync(string queueName, CancellationToken cancellationToken = default);
    }

    }

