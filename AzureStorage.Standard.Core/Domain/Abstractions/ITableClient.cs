

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Standard.Core.Domain.Models;

namespace AzureStorage.Standard.Core.Domain.Abstractions
{
    /// <summary>
    /// Interface for Azure Table Storage operations
    /// </summary>
    public interface ITableClient : IDisposable
    {
        // Table Operations
        /// <summary>
        /// Lists all tables in the storage account
        /// </summary>
        Task<IEnumerable<string>> ListTablesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a table if it doesn't exist
        /// </summary>
        Task<bool> CreateTableIfNotExistsAsync(string tableName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a table
        /// </summary>
        Task<bool> DeleteTableAsync(string tableName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a table exists
        /// </summary>
        Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken = default);

        // Entity Operations
        /// <summary>
        /// Inserts an entity into a table
        /// </summary>
        Task InsertEntityAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default) where T : class, ITableEntity;

        /// <summary>
        /// Upserts an entity (insert or replace)
        /// </summary>
        Task UpsertEntityAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default) where T : class, ITableEntity;

        /// <summary>
        /// Gets an entity by partition key and row key
        /// </summary>
        Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();

        /// <summary>
        /// Updates an entity (merge)
        /// </summary>
        Task UpdateEntityAsync<T>(string tableName, T entity, string eTag = "*", CancellationToken cancellationToken = default) where T : class, ITableEntity;

        /// <summary>
        /// Deletes an entity
        /// </summary>
        Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey, string eTag = "*", CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries entities with a filter
        /// </summary>
        Task<IEnumerable<T>> QueryEntitiesAsync<T>(string tableName, string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();

        /// <summary>
        /// Queries entities by partition key
        /// </summary>
        Task<IEnumerable<T>> QueryByPartitionKeyAsync<T>(string tableName, string partitionKey, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();

        /// <summary>
        /// Gets all entities in a table
        /// </summary>
        Task<IEnumerable<T>> GetAllEntitiesAsync<T>(string tableName, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();

        /// <summary>
        /// Executes a batch transaction
        /// </summary>
        Task ExecuteBatchAsync(string tableName, IEnumerable<TableTransactionAction> actions, CancellationToken cancellationToken = default);
    }
}