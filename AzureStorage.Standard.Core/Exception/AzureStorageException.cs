using System;

namespace AzureStorage.Standard.Core
{
    /// <summary>
    /// Custom exception for Azure Storage operations
    /// </summary>
    public class AzureStorageException : Exception
    {
        /// <summary>
        /// HTTP status code from the Azure Storage service
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Azure Storage error code
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Creates a new instance of AzureStorageException
        /// </summary>
        public AzureStorageException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of AzureStorageException with inner exception
        /// </summary>
        public AzureStorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of AzureStorageException with error details
        /// </summary>
        public AzureStorageException(string message, string errorCode, int? statusCode = null)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a new instance of AzureStorageException with full details
        /// </summary>
        public AzureStorageException(string message, string errorCode, int? statusCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}
