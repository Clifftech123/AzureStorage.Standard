using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureSt.Storage.Standard.Queues;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Queues
{
	public class QueueClient : IQueueClient {
		public Task ClearMessagesAsync(string queueName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<bool> CreateQueueIfNotExistsAsync(string queueName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		public Task<int> GetMessageCountAsync(string queueName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<IEnumerable<string>> ListQueuesAsync(CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<IEnumerable<QueueMessageItem>> PeekMessagesAsync(string queueName, int maxMessages = 1, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<bool> QueueExistsAsync(string queueName, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<IEnumerable<QueueMessageItem>> ReceiveMessagesAsync(string queueName, int maxMessages = 1, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task SendMessageAsync(string queueName, string message, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task SendMessageAsync(string queueName, byte[] message, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task SendMessagesAsync(string queueName, IEnumerable<string> messages, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<QueueMessageItem> UpdateMessageAsync(string queueName, string messageId, string popReceipt, string message, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}
	}
}