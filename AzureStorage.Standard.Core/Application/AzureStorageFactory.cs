using System;
using AzureSt.Storage.Standard.Queues; // IQueueClient interface
using AzureStorage.Standard.Blobs; // IBlobClient interface
using AzureStorage.Standard.Core.Domain.Abstractions;
using AzureStorage.Standard.Core.Domain.Models;
using AzureStorage.Standard.Files; // IFileShareClient interface

namespace AzureStorage.Standard.Core.Application
{
    /// <summary>
    /// Factory for creating Azure Storage service clients (Blob, Queue, Table, and File Share)
    /// </summary>
    public class AzureStorageFactory : IStorageFactory
    {
        #region Fields

        private readonly StorageOptions _storageOptions;
        private readonly Func<StorageOptions, IBlobClient> _blobClientFactory;
        private readonly Func<StorageOptions, IQueueClient> _queueClientFactory;
        private readonly Func<StorageOptions, ITableClient> _tableClientFactory;
        private readonly Func<StorageOptions, IFileShareClient> _fileShareClientFactory;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageFactory"/> class.
        /// Consumers should provide factories for concrete client implementations from their respective packages
        /// to avoid circular dependencies from Core -> (Blobs/Queues/Tables/Files).
        /// </summary>
        /// <param name="options">Storage configuration options</param>
        /// <param name="blobClientFactory">Factory to create an <see cref="IBlobClient"/></param>
        /// <param name="queueClientFactory">Factory to create an <see cref="IQueueClient"/></param>
        /// <param name="tableClientFactory">Factory to create an <see cref="ITableClient"/></param>
        /// <param name="fileShareClientFactory">Factory to create an <see cref="IFileShareClient"/></param>
        /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
        /// <exception cref="ArgumentException">Thrown when options validation fails</exception>
        public AzureStorageFactory(
            StorageOptions options,
            Func<StorageOptions, IBlobClient> blobClientFactory = null,
            Func<StorageOptions, IQueueClient> queueClientFactory = null,
            Func<StorageOptions, ITableClient> tableClientFactory = null,
            Func<StorageOptions, IFileShareClient> fileShareClientFactory = null)
        {
            ValidateOptions(options);
            _storageOptions = options;

            _blobClientFactory = blobClientFactory;
            _queueClientFactory = queueClientFactory;
            _tableClientFactory = tableClientFactory;
            _fileShareClientFactory = fileShareClientFactory;
        }

        #endregion

        #region Public Factory Methods

        /// <summary>
        /// Creates a new Blob Storage client
        /// </summary>
        /// <returns>An instance of <see cref="IBlobClient"/> for blob operations</returns>
        public IBlobClient CreateBlobClient()
        {
            if (_blobClientFactory == null)
            {
                throw new NotSupportedException(
                    "No blob client factory provided. Pass a factory delegate to AzureStorageFactory that returns an IBlobClient from your Blobs package.");
            }

            return _blobClientFactory(_storageOptions);
        }

        /// <summary>
        /// Creates a new Queue Storage client
        /// </summary>
        /// <returns>An instance of <see cref="IQueueClient"/> for queue operations</returns>
        public IQueueClient CreateQueueClient()
        {
            if (_queueClientFactory == null)
            {
                throw new NotSupportedException(
                    "No queue client factory provided. Pass a factory delegate to AzureStorageFactory that returns an IQueueClient from your Queues package.");
            }

            return _queueClientFactory(_storageOptions);
        }

        /// <summary>
        /// Creates a new Table Storage client
        /// </summary>
        /// <returns>An instance of <see cref="ITableClient"/> for table operations</returns>
        public ITableClient CreateTableClient()
        {
            if (_tableClientFactory == null)
            {
                throw new NotSupportedException(
                    "No table client factory provided. Pass a factory delegate to AzureStorageFactory that returns an ITableClient from your Tables package.");
            }

            return _tableClientFactory(_storageOptions);
        }

        /// <summary>
        /// Creates a new File Share Storage client
        /// </summary>
        /// <returns>An instance of <see cref="IFileShareClient"/> for file share operations</returns>
        public IFileShareClient CreateFileShareClient()
        {
            if (_fileShareClientFactory == null)
            {
                throw new NotSupportedException(
                    "No file share client factory provided. Pass a factory delegate to AzureStorageFactory that returns an IFileShareClient from your Files package.");
            }

            return _fileShareClientFactory(_storageOptions);
        }

        #endregion

        #region Private Validation Methods

        /// <summary>
        /// Validates the storage options
        /// </summary>
        /// <param name="options">Options to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
        /// <exception cref="ArgumentException">Thrown when options validation fails</exception>
        private static void ValidateOptions(StorageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options),
                    "Storage options cannot be null.");
            }

            options.Validate();
        }

        #endregion
    }
}