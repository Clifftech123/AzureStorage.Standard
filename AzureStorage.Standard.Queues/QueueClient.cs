

namespace AzureStorage.Standard.Queues
{
	public class QueueClient : IQueueClient {

       
       private readonly 
        private readonly QueueServiceClient _queueServiceClient;
        private readonly StorageOptions _options;

        /// <summary>
        /// Creates a new instance of QueueClientWrapper
        /// </summary>
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

        public async Task SendMessageAsync(string queueName, byte[] message, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var base64Message = Convert.ToBase64String(message);
            await SendMessageAsync(queueName, base64Message, visibilityTimeout, timeToLive, cancellationToken);
        }

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
                    DequeueCount = msg.DequeueCount,
                    InsertedOn = msg.InsertedOn,
                    ExpiresOn = msg.ExpiresOn
                }).ToList();
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to peek messages from queue '{queueName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

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

        private static QueueMessageItem MapToQueueMessageItem(QueueMessage message)
        {
            return new QueueMessageItem
            {
                MessageId = message.MessageId,
                PopReceipt = message.PopReceipt,
                MessageText = message.Body.ToString(),
                MessageBytes = message.Body.ToBytes().ToArray(),
                DequeueCount = message.DequeueCount,
                InsertedOn = message.InsertedOn,
                ExpiresOn = message.ExpiresOn,
                NextVisibleOn = message.NextVisibleOn
            };
        }

        private static void ValidateQueueName(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName), "Queue name cannot be null or empty.");
        }

        #endregion

        public void Dispose()
        {
            // QueueServiceClient doesn't implement IDisposable, so nothing to dispose
        }
    }
}