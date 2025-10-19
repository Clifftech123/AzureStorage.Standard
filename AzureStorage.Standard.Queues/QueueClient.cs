using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using AzureSt.Storage.Standard.Queues;
using AzureStorage.Standard.Core;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Queues
{
    /// <summary>
    /// Azure Queue Storage client implementation that wraps the Azure.Storage.Queues SDK.
    /// Provides simplified access to Azure Queue Storage operations including queue management and message handling.
    /// <para>
    /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/queues/storage-queues-introduction">Azure Queue Storage Overview</see>
    /// </para>
    /// <para>
    /// SDK Reference: <see href="https://learn.microsoft.com/en-us/dotnet/api/azure.storage.queues">Azure.Storage.Queues Namespace</see>
    /// </para>
    /// </summary>
    public class QueueClient : IQueueClient
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly StorageOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueClient"/> class.
        /// <para>
        /// Supports multiple authentication methods:
        /// - Connection string (recommended for development)
        /// - Service URI (for managed identity scenarios)
        /// - Account name and key (for explicit credential scenarios)
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues">Use Azure Queue Storage with .NET</see>
        /// </para>
        /// </summary>
        /// <param name="options">Configuration options for connecting to Azure Queue Storage.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when required options are invalid or missing.</exception>
        public QueueClient(StorageOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.Validate();

            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                _queueServiceClient = new QueueServiceClient(options.ConnectionString);
            }
            else if (options.ServiceUri != null)
            {
                _queueServiceClient = new QueueServiceClient(options.ServiceUri);
            }
            else
            {
                var accountUri = new Uri($"https://{options.AccountName}.queue.core.windows.net");
                var credential = new Azure.Storage.StorageSharedKeyCredential(options.AccountName, options.AccountKey);
                _queueServiceClient = new QueueServiceClient(accountUri, credential);
            }
        }

        #region Queue Operations

        /// <summary>
        /// Lists all queues in the storage account.
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/queues/storage-queues-introduction">Azure Queue Storage Overview</see>
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of queue names.</returns>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<IEnumerable<string>> ListQueuesAsync(CancellationToken cancellationToken = default)
        {
            var queues = new List<string>();

            try
            {
                await foreach (var queue in _queueServiceClient.GetQueuesAsync(cancellationToken: cancellationToken))
                {
                    queues.Add(queue.Name);
                }

                return queues;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException("Failed to list queues.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Creates a queue if it does not already exist.
        /// <para>
        /// Queue names must be lowercase, 3-63 characters, and contain only letters, numbers, and hyphens.
        /// Queue names cannot start or end with a hyphen, and cannot have consecutive hyphens.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/naming-queues-and-metadata">Naming Queues and Metadata</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue to create.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the queue was created; false if it already existed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> CreateQueueIfNotExistsAsync(string queueName, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                var response = await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                return response != null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to create queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Deletes a queue if it exists.
        /// <para>
        /// Warning: Deleting a queue permanently removes all messages within it.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues">Use Azure Queue Storage with .NET</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the queue was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                var response = await queueClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Checks if a queue exists in the storage account.
        /// </summary>
        /// <param name="queueName">The name of the queue to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the queue exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> QueueExistsAsync(string queueName, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                return await queueClient.ExistsAsync(cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to check if queue '{queueName}' exists.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Gets the approximate number of messages in a queue.
        /// <para>
        /// Note: The count is approximate and may not be exact due to the distributed nature of Azure Storage.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/queues/storage-queues-introduction">Azure Queue Storage Overview</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>The approximate number of messages in the queue.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<int> GetMessageCountAsync(string queueName, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                var properties = await queueClient.GetPropertiesAsync(cancellationToken);
                return properties.Value.ApproximateMessagesCount;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to get message count for queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        #endregion

        #region Message Operations

        /// <summary>
        /// Sends a text message to a queue.
        /// <para>
        /// Messages can be up to 64 KB in size. Use visibilityTimeout to delay message processing.
        /// Messages are stored for 7 days by default, or specify a custom timeToLive.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/put-message">Put Message</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="message">The message text to send.</param>
        /// <param name="visibilityTimeout">Optional. The time the message should be invisible after being retrieved. Maximum 7 days.</param>
        /// <param name="timeToLive">Optional. The time-to-live for the message. Maximum 7 days. If not specified, defaults to 7 days.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> or <paramref name="message"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task SendMessageAsync(string queueName, string message, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                await queueClient.SendMessageAsync(message, visibilityTimeout, timeToLive, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to send message to queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Sends a binary message to a queue.
        /// <para>
        /// The byte array is automatically converted to Base64 encoding for transmission.
        /// Messages can be up to 64 KB in size.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/put-message">Put Message</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="message">The binary message content to send.</param>
        /// <param name="visibilityTimeout">Optional. The time the message should be invisible after being retrieved. Maximum 7 days.</param>
        /// <param name="timeToLive">Optional. The time-to-live for the message. Maximum 7 days.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task SendMessageAsync(string queueName, byte[] message, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var base64Message = Convert.ToBase64String(message);
            await SendMessageAsync(queueName, base64Message, visibilityTimeout, timeToLive, cancellationToken);
        }

        /// <summary>
        /// Sends multiple messages to a queue in sequence.
        /// <para>
        /// Note: Messages are sent one at a time. For high-throughput scenarios, consider using
        /// parallel processing or Azure Service Bus for batch operations.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues">Use Azure Queue Storage with .NET</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="messages">The collection of messages to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> or <paramref name="messages"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task SendMessagesAsync(string queueName, IEnumerable<string> messages, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            var queueClient = _queueServiceClient.GetQueueClient(queueName);

            foreach (var message in messages)
            {
                await queueClient.SendMessageAsync(message, cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Receives and removes messages from a queue, making them invisible to other consumers.
        /// <para>
        /// Retrieved messages are hidden from other consumers for the visibility timeout period.
        /// You must delete the message explicitly using <see cref="DeleteMessageAsync"/> after processing.
        /// Maximum 32 messages can be retrieved at once.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/get-messages">Get Messages</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="maxMessages">The maximum number of messages to retrieve (1-32). Defaults to 1.</param>
        /// <param name="visibilityTimeout">Optional. The time the messages should remain invisible. Defaults to 30 seconds. Maximum 7 days.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of received messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxMessages"/> is not between 1 and 32.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<IEnumerable<QueueMessageItem>> ReceiveMessagesAsync(string queueName, int maxMessages = 1, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            if (maxMessages < 1 || maxMessages > 32)
                throw new ArgumentOutOfRangeException(nameof(maxMessages), "Must be between 1 and 32.");

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                var response = await queueClient.ReceiveMessagesAsync(maxMessages, visibilityTimeout, cancellationToken);

                return response.Value.Select(MapToQueueMessageItem).ToList();
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to receive messages from queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Peeks at messages without removing them from the queue or affecting their visibility.
        /// <para>
        /// Peeked messages remain visible to all consumers and do not provide a pop receipt.
        /// Use this to preview messages without committing to processing them.
        /// Maximum 32 messages can be peeked at once.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/peek-messages">Peek Messages</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="maxMessages">The maximum number of messages to peek (1-32). Defaults to 1.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of peeked messages (without pop receipt).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxMessages"/> is not between 1 and 32.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<IEnumerable<QueueMessageItem>> PeekMessagesAsync(string queueName, int maxMessages = 1, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            if (maxMessages < 1 || maxMessages > 32)
                throw new ArgumentOutOfRangeException(nameof(maxMessages), "Must be between 1 and 32.");

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                var response = await queueClient.PeekMessagesAsync(maxMessages, cancellationToken);

                return response.Value.Select(msg => new QueueMessageItem
                {
                    MessageId = msg.MessageId,
                    MessageText = msg.Body.ToString(),
                    DequeueCount = (int)msg.DequeueCount,
                    InsertedOn = msg.InsertedOn,
                    ExpiresOn = msg.ExpiresOn
                }).ToList();
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to peek messages from queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Deletes a message from a queue after processing.
        /// <para>
        /// You must provide the message ID and pop receipt obtained from <see cref="ReceiveMessagesAsync"/>.
        /// Messages must be deleted within the visibility timeout period, or they become visible again.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/delete-message2">Delete Message</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="messageId">The message ID.</param>
        /// <param name="popReceipt">The pop receipt from when the message was received.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            if (string.IsNullOrEmpty(messageId))
                throw new ArgumentNullException(nameof(messageId));

            if (string.IsNullOrEmpty(popReceipt))
                throw new ArgumentNullException(nameof(popReceipt));

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                await queueClient.DeleteMessageAsync(messageId, popReceipt, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete message from queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Updates a message's content and/or extends its visibility timeout.
        /// <para>
        /// Use this to extend processing time for a message or update its content.
        /// You must provide the message ID and current pop receipt.
        /// Returns an updated pop receipt for future operations.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/update-message">Update Message</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="messageId">The message ID.</param>
        /// <param name="popReceipt">The current pop receipt from when the message was received.</param>
        /// <param name="message">The new message content.</param>
        /// <param name="visibilityTimeout">The new visibility timeout (how long the message remains invisible).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>An updated message item with the new pop receipt.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<QueueMessageItem> UpdateMessageAsync(string queueName, string messageId, string popReceipt, string message, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            if (string.IsNullOrEmpty(messageId))
                throw new ArgumentNullException(nameof(messageId));

            if (string.IsNullOrEmpty(popReceipt))
                throw new ArgumentNullException(nameof(popReceipt));

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                var response = await queueClient.UpdateMessageAsync(messageId, popReceipt, message, visibilityTimeout, cancellationToken);

                return new QueueMessageItem
                {
                    MessageId = messageId,
                    PopReceipt = response.Value.PopReceipt,
                    MessageText = message,
                    NextVisibleOn = response.Value.NextVisibleOn
                };
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to update message in queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Clears all messages from a queue.
        /// <para>
        /// Warning: This operation permanently deletes all messages in the queue and cannot be undone.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/clear-messages">Clear Messages</see>
        /// </para>
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task ClearMessagesAsync(string queueName, CancellationToken cancellationToken = default)
        {
            ValidateQueueName(queueName);

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                await queueClient.ClearMessagesAsync(cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to clear messages from queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Maps an Azure SDK QueueMessage to a custom QueueMessageItem.
        /// Extracts all message properties including ID, pop receipt, body, and metadata.
        /// </summary>
        private static QueueMessageItem MapToQueueMessageItem(QueueMessage message)
        {
            return new QueueMessageItem
            {
                MessageId = message.MessageId,
                PopReceipt = message.PopReceipt,
                MessageText = message.Body.ToString(),
                MessageBytes = message.Body.ToMemory().ToArray(),
                DequeueCount = (int)message.DequeueCount,
                InsertedOn = message.InsertedOn,
                ExpiresOn = message.ExpiresOn,
                NextVisibleOn = message.NextVisibleOn
            };
        }

        /// <summary>
        /// Validates that a queue name is not null or empty.
        /// </summary>
        private static void ValidateQueueName(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName), "Queue name cannot be null or empty.");
        }

        #endregion

        /// <summary>
        /// Disposes the QueueClient instance.
        /// <para>
        /// Note: QueueServiceClient does not implement IDisposable, so this method has no implementation.
        /// It exists to satisfy the IQueueClient interface contract.
        /// </para>
        /// </summary>
        public void Dispose()
        {
            // QueueServiceClient doesn't implement IDisposable, so nothing to dispose
        }
    }
}