using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Core.Domain.Abstractions
{
    /// <summary>
    /// Interface for Azure Table Storage operations.
    /// Provides comprehensive methods for managing tables and entities.
    /// This is a simplified wrapper around the Azure.Data.Tables SDK.
    /// </summary>
    public interface ITableClient : IDisposable
    {
        #region Table Operations

        /// <summary>
        /// Lists all tables in the storage account.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of table names.</returns>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<IEnumerable<string>> ListTablesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a table if it does not already exist.
        /// Table names must be alphanumeric, cannot start with a number, and must be between 3-63 characters.
        /// </summary>
        /// <param name="tableName">The name of the table to create.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the table was created; false if it already existed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> CreateTableIfNotExistsAsync(string tableName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a table if it exists.
        /// Warning: Deleting a table permanently removes all entities within it.
        /// </summary>
        /// <param name="tableName">The name of the table to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the table was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> DeleteTableAsync(string tableName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a table exists in the storage account.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the table exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken = default);

        #endregion

        #region Entity Operations

        /// <summary>
        /// Inserts a new entity into a table.
        /// This operation will fail if an entity with the same PartitionKey and RowKey already exists.
        /// Use <see cref="UpsertEntityAsync{T}"/> to insert or replace an entity.
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="ITableEntity"/>.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> or <paramref name="entity"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails (e.g., entity already exists).</exception>
        Task InsertEntityAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default) where T : class, ITableEntity;

        /// <summary>
        /// Inserts or replaces an entity in a table.
        /// If an entity with the same PartitionKey and RowKey exists, it will be replaced.
        /// Otherwise, a new entity is inserted.
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="ITableEntity"/>.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="entity">The entity to upsert.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> or <paramref name="entity"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task UpsertEntityAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default) where T : class, ITableEntity;

        /// <summary>
        /// Retrieves a single entity from a table by its partition key and row key.
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="ITableEntity"/> and have a parameterless constructor.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="partitionKey">The partition key of the entity.</param>
        /// <param name="rowKey">The row key of the entity.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();

        /// <summary>
        /// Updates an existing entity in a table using merge mode.
        /// Merge mode updates only the properties provided, leaving other properties unchanged.
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="ITableEntity"/>.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="eTag">The ETag for optimistic concurrency. Use "*" to ignore ETag checks.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> or <paramref name="entity"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails (e.g., entity not found or ETag mismatch).</exception>
        Task UpdateEntityAsync<T>(string tableName, T entity, string eTag = "*", CancellationToken cancellationToken = default) where T : class, ITableEntity;

        /// <summary>
        /// Deletes an entity from a table.
        /// If the entity does not exist, the operation completes successfully without error.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="partitionKey">The partition key of the entity to delete.</param>
        /// <param name="rowKey">The row key of the entity to delete.</param>
        /// <param name="eTag">The ETag for optimistic concurrency. Use "*" to ignore ETag checks.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey, string eTag = "*", CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries entities from a table using an optional OData filter.
        /// Filters use OData query syntax. For example: "PartitionKey eq 'Sales' and Timestamp gt datetime'2023-01-01T00:00:00Z'".
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="ITableEntity"/> and have a parameterless constructor.</typeparam>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="filter">Optional OData filter expression to apply to the query.</param>
        /// <param name="maxPerPage">Optional maximum number of entities to return per page.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of entities matching the filter criteria.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<IEnumerable<T>> QueryEntitiesAsync<T>(string tableName, string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();

        /// <summary>
        /// Queries all entities with a specific partition key.
        /// Querying by partition key is efficient because entities are stored together by partition.
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="ITableEntity"/> and have a parameterless constructor.</typeparam>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="partitionKey">The partition key to filter by.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of entities with the specified partition key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="partitionKey"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<IEnumerable<T>> QueryByPartitionKeyAsync<T>(string tableName, string partitionKey, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();

        /// <summary>
        /// Retrieves all entities from a table.
        /// Warning: This method retrieves all entities in the table. For large tables, consider using
        /// <see cref="QueryEntitiesAsync{T}"/> with pagination or filtering instead.
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="ITableEntity"/> and have a parameterless constructor.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of all entities in the table.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        Task<IEnumerable<T>> GetAllEntitiesAsync<T>(string tableName, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();

        /// <summary>
        /// Executes multiple operations as a batch transaction.
        /// All operations in a batch must operate on entities with the same partition key.
        /// The batch is atomic - either all operations succeed or all fail. Maximum of 100 operations per batch.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="actions">The collection of batch actions to execute.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="actions"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails (e.g., mixed partition keys, too many operations).</exception>
        Task ExecuteBatchAsync(string tableName, IEnumerable<TableTransactionAction> actions, CancellationToken cancellationToken = default);

        #endregion
    }
}