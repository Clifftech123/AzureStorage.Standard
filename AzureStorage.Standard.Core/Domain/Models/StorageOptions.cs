

using System;

namespace AzureStorage.Standard.Core.Domain.Models
{
    /// <summary>
    /// Configuration options for Azure Storage services.
    /// Supports three authentication methods:
    /// 1. Connection String (takes precedence)
    /// 2. Account Name + Account Key
    /// 3. Account Name + SAS Token
    /// </summary>
    public class StorageOptions
    {
        #region Properties

        /// <summary>
        /// Azure Storage account name
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Azure Storage account key for shared key authentication
        /// </summary>
        public string AccountKey { get; set; }

        /// <summary>
        /// Connection string for Azure Storage (takes precedence over other authentication methods)
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Custom service URI for Azure Storage (optional)
        /// </summary>
        public Uri ServiceUri { get; set; }

        /// <summary>
        /// SAS (Shared Access Signature) token for authentication
        /// </summary>
        public string SasToken { get; set; }

		/// <summary>
		/// Retry policy options for automatic handling of transient failures.
		/// If not specified, default retry policy will be used (3 retries with exponential backoff).
		/// Set to RetryOptions.None to disable automatic retries.
		/// </summary>
		public RetryOptions RetryOptions { get; set; } = RetryOptions.Default;

        #endregion

        #region Public Methods

        /// <summary>
        /// Validates that the storage options are correctly configured with proper authentication
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
        public void Validate()
        {
            if (HasConnectionString())
            {
                return; // Connection string authentication is sufficient
            }

            ValidateAccountBasedAuthentication();
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a storage options instance from a connection string
        /// </summary>
        /// <param name="connectionString">The connection string for Azure Storage</param>
        /// <returns>A StorageOptions instance configured with the provided connection string</returns>
        /// <exception cref="ArgumentException">Thrown when connectionString is null or empty</exception>
        public static StorageOptions CreateFromConnectionString(string connectionString)
        {
            ValidateConnectionStringParameter(connectionString);

            return new StorageOptions
            {
                ConnectionString = connectionString
            };
        }

        /// <summary>
        /// Creates a storage options instance from account name and key
        /// </summary>
        /// <param name="accountName">The Azure Storage account name</param>
        /// <param name="accountKey">The Azure Storage account key</param>
        /// <returns>A StorageOptions instance configured with the provided account name and key</returns>
        /// <exception cref="ArgumentException">Thrown when accountName or accountKey is null or empty</exception>
        public static StorageOptions CreateFromAccountKey(string accountName, string accountKey)
        {
            ValidateAccountNameParameter(accountName);
            ValidateAccountKeyParameter(accountKey);

            return new StorageOptions
            {
                AccountName = accountName,
                AccountKey = accountKey
            };
        }

        /// <summary>
        /// Creates a storage options instance from account name and SAS token
        /// </summary>
        /// <param name="accountName">The Azure Storage account name</param>
        /// <param name="sasToken">The SAS (Shared Access Signature) token</param>
        /// <returns>A StorageOptions instance configured with the provided account name and SAS token</returns>
        /// <exception cref="ArgumentException">Thrown when accountName or sasToken is null or empty</exception>
        public static StorageOptions CreateFromSasToken(string accountName, string sasToken)
        {
            ValidateAccountNameParameter(accountName);
            ValidateSasTokenParameter(sasToken);

            return new StorageOptions
            {
                AccountName = accountName,
                SasToken = sasToken
            };
        }

        #endregion

        #region Parameter Validation

        /// <summary>
        /// Validates the connection string parameter
        /// </summary>
        private static void ValidateConnectionStringParameter(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(
                    "Connection string cannot be null or empty.",
                    nameof(connectionString));
            }
        }

        /// <summary>
        /// Validates the account name parameter
        /// </summary>
        private static void ValidateAccountNameParameter(string accountName)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new ArgumentException(
                    "Account name cannot be null or empty.",
                    nameof(accountName));
            }
        }

        /// <summary>
        /// Validates the account key parameter
        /// </summary>
        private static void ValidateAccountKeyParameter(string accountKey)
        {
            if (string.IsNullOrWhiteSpace(accountKey))
            {
                throw new ArgumentException(
                    "Account key cannot be null or empty.",
                    nameof(accountKey));
            }
        }

        /// <summary>
        /// Validates the SAS token parameter
        /// </summary>
        private static void ValidateSasTokenParameter(string sasToken)
        {
            if (string.IsNullOrWhiteSpace(sasToken))
            {
                throw new ArgumentException(
                    "SAS token cannot be null or empty.",
                    nameof(sasToken));
            }
        }

        #endregion

        #region Private Validation Methods

        /// <summary>
        /// Validates account-based authentication (Account Name + Key/SAS)
        /// </summary>
        private void ValidateAccountBasedAuthentication()
        {
            EnsureAccountNameIsProvided();
            EnsureAuthenticationCredentialsAreProvided();
        }

        /// <summary>
        /// Ensures that the account name is provided
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when AccountName is missing</exception>
        private void EnsureAccountNameIsProvided()
        {
            if (!HasAccountName())
            {
                throw new ArgumentException(
                    "AccountName is required when ConnectionString is not provided.",
                    nameof(AccountName));
            }
        }

        /// <summary>
        /// Ensures that at least one authentication credential (AccountKey or SasToken) is provided
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when both AccountKey and SasToken are missing</exception>
        private void EnsureAuthenticationCredentialsAreProvided()
        {
            if (!HasAccountKey() && !HasSasToken())
            {
                throw new ArgumentException(
                    "Either AccountKey or SasToken must be provided when using account-based authentication.",
                    nameof(AccountKey));
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Checks if a valid connection string is provided
        /// </summary>
        private bool HasConnectionString() =>
            !string.IsNullOrWhiteSpace(ConnectionString);

        /// <summary>
        /// Checks if a valid account name is provided
        /// </summary>
        private bool HasAccountName() =>
            !string.IsNullOrWhiteSpace(AccountName);

        /// <summary>
        /// Checks if a valid account key is provided
        /// </summary>
        private bool HasAccountKey() =>
            !string.IsNullOrWhiteSpace(AccountKey);

        /// <summary>
        /// Checks if a valid SAS token is provided
        /// </summary>
        private bool HasSasToken() =>
            !string.IsNullOrWhiteSpace(SasToken);

        #endregion
    }
}
