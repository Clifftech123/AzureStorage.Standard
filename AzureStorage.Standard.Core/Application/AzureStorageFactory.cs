

using System;
using AzureStorage.Standard.Core.Domain.Abstractions;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Core.Application
{
    /// <summary>
    /// Factory for creating Azure Storage service clients (Blob, Queue, Table, and File Share)
    /// </summary>
    public class AzureStorageFactory : IStorageFactory
    {
        #region Fields

        private readonly StorageOptions _storageOptions;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageFactory"/> class
        /// </summary>
        /// <param name="options">Storage configuration options</param>
        /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
        /// <exception cref="ArgumentException">Thrown when options validation fails</exception>
        public AzureStorageFactory(StorageOptions options)
        {
            ValidateOptions(options);
            _storageOptions = options;
        }

        #endregion

        #region Public Factory Methods

        /// <summary>
        /// Creates a new Blob Storage client
        /// </summary>
        /// <returns>An instance of <see cref="IBlobClient"/> for blob operations</returns>
        public IBlobClient CreateBlobClient()
        {
            return new AzureBlobClient(_storageOptions);
        }

        /// <summary>
        /// Creates a new Queue Storage client
        /// </summary>
        /// <returns>An instance of <see cref="IQueueClient"/> for queue operations</returns>
        public IQueueClient CreateQueueClient()
        {
            return new AzureQueueClient(_storageOptions);
        }

        /// <summary>
        /// Creates a new Table Storage client
        /// </summary>
        /// <returns>An instance of <see cref="ITableClient"/> for table operations</returns>
        public ITableClient CreateTableClient()
        {
            return new AzureTableClient(_storageOptions);
        }

        /// <summary>
        /// Creates a new File Share Storage client
        /// </summary>
        /// <returns>An instance of <see cref="IFileShareClient"/> for file share operations</returns>
        public IFileShareClient CreateFileShareClient()
        {
            return new AzureFileShareClient(_storageOptions);
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