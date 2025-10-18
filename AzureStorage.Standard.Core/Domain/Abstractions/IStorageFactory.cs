
using AzureSt.Storage.Standard.Queues;
using AzureStorage.Standard.Blobs;
using AzureStorage.Standard.Files;

namespace AzureStorage.Standard.Core.Domain.Abstractions
{
    public interface IStorageFactory
    {
            /// <summary>
        /// Creates a Blob Storage client
        /// </summary>
        IBlobClient CreateBlobClient();

        /// <summary>
        /// Creates a Queue Storage client
        /// </summary>
        IQueueClient CreateQueueClient();

        /// <summary>
        /// Creates a Table Storage client
        /// </summary>
        ITableClient CreateTableClient();

        /// <summary>
        /// Creates a File Share Storage client
        /// </summary>
        IFileShareClient CreateFileShareClient();
    }
}