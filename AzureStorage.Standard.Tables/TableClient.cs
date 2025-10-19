using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using AzureStorage.Standard.Core;
using AzureStorage.Standard.Core.Domain.Abstractions;
using AzureStorage.Standard.Core.Domain.Models;
using AzureTableEntity = Azure.Data.Tables.TableEntity;
using CustomITableEntity = AzureStorage.Standard.Core.Domain.Models.ITableEntity;

namespace AzureStorage.Standard.Tables
{
    /// <summary>
    /// Azure Table Storage client implementation that wraps the Azure.Data.Tables SDK.
    /// Provides simplified access to Azure Table Storage operations including table management and entity CRUD operations.
    /// <para>
    /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-overview">Azure Table Storage Overview</see>
    /// </para>
    /// <para>
    /// SDK Reference: <see href="https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables">Azure.Data.Tables Namespace</see>
    /// </para>
    /// </summary>
    public class TableClient : ITableClient
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly StorageOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableClient"/> class.
        /// <para>
        /// Supports multiple authentication methods:
        /// - Connection string (recommended for development)
        /// - Service URI (for managed identity scenarios)
        /// - Account name and key (for explicit credential scenarios)
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-how-to-use-dotnet">Use Azure Table Storage with .NET</see>
        /// </para>
        /// </summary>
        /// <param name="options">Configuration options for connecting to Azure Table Storage.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when required options are invalid or missing.</exception>
        public TableClient(StorageOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.Validate();

            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                _tableServiceClient = new TableServiceClient(options.ConnectionString);
            }
            else if (options.ServiceUri != null)
            {
                _tableServiceClient = new TableServiceClient(options.ServiceUri);
            }
            else
            {
                var accountUri = new Uri($"https://{options.AccountName}.table.core.windows.net");
                var credential = new TableSharedKeyCredential(options.AccountName, options.AccountKey);
                _tableServiceClient = new TableServiceClient(accountUri, credential);
            }
        }

        #region Table Operations

        /// <summary>
        /// Lists all tables in the storage account.
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-design">Azure Table Storage Design</see>
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of table names.</returns>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<IEnumerable<string>> ListTablesAsync(CancellationToken cancellationToken = default)
        {
            var tables = new List<string>();

            try
            {
                await foreach (var table in _tableServiceClient.QueryAsync(cancellationToken: cancellationToken))
                {
                    tables.Add(table.Name);
                }

                return tables;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException("Failed to list tables.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Creates a table if it does not already exist.
        /// <para>
        /// Table names must be alphanumeric, cannot start with a number, and must be between 3-63 characters.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model">Understanding the Table Service Data Model</see>
        /// </para>
        /// </summary>
        /// <param name="tableName">The name of the table to create.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the table was created; false if it already existed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> CreateTableIfNotExistsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            ValidateTableName(tableName);

            try
            {
                var response = await _tableServiceClient.CreateTableIfNotExistsAsync(tableName, cancellationToken);
                return response != null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to create table '{tableName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Deletes a table if it exists.
        /// <para>
        /// Warning: Deleting a table permanently removes all entities within it.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-how-to-use-dotnet">Use Azure Table Storage with .NET</see>
        /// </para>
        /// </summary>
        /// <param name="tableName">The name of the table to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the table was deleted; false if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> DeleteTableAsync(string tableName, CancellationToken cancellationToken = default)
        {
            ValidateTableName(tableName);

            try
            {
                await _tableServiceClient.DeleteTableAsync(tableName, cancellationToken);
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete table '{tableName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Checks if a table exists in the storage account.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>True if the table exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            ValidateTableName(tableName);

            try
            {
                await foreach (var table in _tableServiceClient.QueryAsync(filter: $"TableName eq '{tableName}'", cancellationToken: cancellationToken))
                {
                    if (table.Name == tableName)
                        return true;
                }

                return false;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to check if table '{tableName}' exists.", ex.ErrorCode, ex.Status, ex);
            }
        }

        #endregion

        #region Entity Operations

        /// <summary>
        /// Inserts a new entity into a table.
        /// <para>
        /// This operation will fail if an entity with the same PartitionKey and RowKey already exists.
        /// Use <see cref="UpsertEntityAsync{T}"/> to insert or replace an entity.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/insert-entity">Insert Entity</see>
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="CustomITableEntity"/>.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> or <paramref name="entity"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails (e.g., entity already exists).</exception>
        public async Task InsertEntityAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default) where T : class, CustomITableEntity
        {
            ValidateTableName(tableName);
            ValidateEntity(entity);

            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var tableEntity = ConvertToTableEntity(entity);
                await tableClient.AddEntityAsync(tableEntity, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to insert entity into table '{tableName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Inserts or replaces an entity in a table.
        /// <para>
        /// If an entity with the same PartitionKey and RowKey exists, it will be replaced.
        /// Otherwise, a new entity is inserted. This uses Replace mode which replaces all properties.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/insert-or-replace-entity">Insert Or Replace Entity</see>
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="CustomITableEntity"/>.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="entity">The entity to upsert.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> or <paramref name="entity"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task UpsertEntityAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default) where T : class, CustomITableEntity
        {
            ValidateTableName(tableName);
            ValidateEntity(entity);

            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var tableEntity = ConvertToTableEntity(entity);
                await tableClient.UpsertEntityAsync(tableEntity, TableUpdateMode.Replace, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to upsert entity into table '{tableName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Retrieves a single entity from a table by its partition key and row key.
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/query-entities">Query Entities</see>
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="CustomITableEntity"/> and have a parameterless constructor.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="partitionKey">The partition key of the entity.</param>
        /// <param name="rowKey">The row key of the entity.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey, CancellationToken cancellationToken = default) where T : class, CustomITableEntity, new()
        {
            ValidateTableName(tableName);
            ValidateKey(partitionKey, nameof(partitionKey));
            ValidateKey(rowKey, nameof(rowKey));

            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var response = await tableClient.GetEntityAsync<AzureTableEntity>(partitionKey, rowKey, cancellationToken: cancellationToken);

                return ConvertFromTableEntity<T>(response.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to get entity from table '{tableName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Updates an existing entity in a table using merge mode.
        /// <para>
        /// Merge mode updates only the properties provided, leaving other properties unchanged.
        /// Specify an ETag for optimistic concurrency control, or use "*" to ignore concurrency checks.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/merge-entity">Merge Entity</see>
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="CustomITableEntity"/>.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="eTag">The ETag for optimistic concurrency. Use "*" to ignore ETag checks.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> or <paramref name="entity"/> is null.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails (e.g., entity not found or ETag mismatch).</exception>
        public async Task UpdateEntityAsync<T>(string tableName, T entity, string eTag = "*", CancellationToken cancellationToken = default) where T : class, CustomITableEntity
        {
            ValidateTableName(tableName);
            ValidateEntity(entity);

            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var tableEntity = ConvertToTableEntity(entity);
                await tableClient.UpdateEntityAsync(tableEntity, new ETag(eTag), TableUpdateMode.Merge, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to update entity in table '{tableName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Deletes an entity from a table.
        /// <para>
        /// Specify an ETag for optimistic concurrency control, or use "*" to ignore concurrency checks.
        /// If the entity does not exist, the operation completes successfully without error.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/delete-entity1">Delete Entity</see>
        /// </para>
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="partitionKey">The partition key of the entity to delete.</param>
        /// <param name="rowKey">The row key of the entity to delete.</param>
        /// <param name="eTag">The ETag for optimistic concurrency. Use "*" to ignore ETag checks.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey, string eTag = "*", CancellationToken cancellationToken = default)
        {
            ValidateTableName(tableName);
            ValidateKey(partitionKey, nameof(partitionKey));
            ValidateKey(rowKey, nameof(rowKey));

            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.DeleteEntityAsync(partitionKey, rowKey, new ETag(eTag), cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Entity doesn't exist, ignore
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to delete entity from table '{tableName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Queries entities from a table using an optional filter.
        /// <para>
        /// Filters use OData query syntax. For example: "PartitionKey eq 'Sales' and Timestamp gt datetime'2023-01-01T00:00:00Z'".
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/querying-tables-and-entities">Querying Tables and Entities</see>
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="CustomITableEntity"/> and have a parameterless constructor.</typeparam>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="filter">Optional OData filter expression to apply to the query.</param>
        /// <param name="maxPerPage">Optional maximum number of entities to return per page.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of entities matching the filter criteria.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<IEnumerable<T>> QueryEntitiesAsync<T>(string tableName, string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default) where T : class, CustomITableEntity, new()
        {
            ValidateTableName(tableName);

            var results = new List<T>();

            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var queryResults = tableClient.QueryAsync<AzureTableEntity>(filter, maxPerPage, cancellationToken: cancellationToken);

                await foreach (var entity in queryResults)
                {
                    results.Add(ConvertFromTableEntity<T>(entity));
                }

                return results;
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to query entities from table '{tableName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        /// <summary>
        /// Queries all entities with a specific partition key.
        /// <para>
        /// This is a convenience method that filters by PartitionKey. Querying by partition key
        /// is efficient because entities are stored together by partition.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="CustomITableEntity"/> and have a parameterless constructor.</typeparam>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="partitionKey">The partition key to filter by.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of entities with the specified partition key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="partitionKey"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<IEnumerable<T>> QueryByPartitionKeyAsync<T>(string tableName, string partitionKey, CancellationToken cancellationToken = default) where T : class, CustomITableEntity, new()
        {
            ValidateKey(partitionKey, nameof(partitionKey));

            var filter = $"PartitionKey eq '{partitionKey}'";
            return await QueryEntitiesAsync<T>(tableName, filter, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves all entities from a table.
        /// <para>
        /// Warning: This method retrieves all entities in the table. For large tables, consider using
        /// <see cref="QueryEntitiesAsync{T}"/> with pagination or filtering instead.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of entity, which must implement <see cref="CustomITableEntity"/> and have a parameterless constructor.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A collection of all entities in the table.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails.</exception>
        public async Task<IEnumerable<T>> GetAllEntitiesAsync<T>(string tableName, CancellationToken cancellationToken = default) where T : class, CustomITableEntity, new()
        {
            return await QueryEntitiesAsync<T>(tableName, null, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Executes multiple operations as a batch transaction.
        /// <para>
        /// All operations in a batch must operate on entities with the same partition key.
        /// The batch is atomic - either all operations succeed or all fail.
        /// Maximum of 100 operations per batch.
        /// </para>
        /// <para>
        /// Learn more: <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/performing-entity-group-transactions">Performing Entity Group Transactions</see>
        /// </para>
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="actions">The collection of batch actions to execute.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="actions"/> is null or empty.</exception>
        /// <exception cref="AzureStorageException">Thrown when the operation fails (e.g., mixed partition keys, too many operations).</exception>
        public async Task ExecuteBatchAsync(string tableName, IEnumerable<AzureStorage.Standard.Core.Domain.Models.TableTransactionAction> actions, CancellationToken cancellationToken = default)
        {
            ValidateTableName(tableName);

            if (actions == null || !actions.Any())
                throw new ArgumentException("Batch actions cannot be null or empty.", nameof(actions));

            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var batchActions = new List<Azure.Data.Tables.TableTransactionAction>();

                foreach (var action in actions)
                {
                    var tableEntity = ConvertToTableEntity(action.Entity);

                    switch (action.ActionType)
                    {
                        case AzureStorage.Standard.Core.Domain.Models.TableTransactionActionType.Insert:
                            batchActions.Add(new Azure.Data.Tables.TableTransactionAction(Azure.Data.Tables.TableTransactionActionType.Add, tableEntity));
                            break;
                        case AzureStorage.Standard.Core.Domain.Models.TableTransactionActionType.Update:
                            batchActions.Add(new Azure.Data.Tables.TableTransactionAction(Azure.Data.Tables.TableTransactionActionType.UpdateMerge, tableEntity, new ETag(action.ETag)));
                            break;
                        case AzureStorage.Standard.Core.Domain.Models.TableTransactionActionType.Upsert:
                            batchActions.Add(new Azure.Data.Tables.TableTransactionAction(Azure.Data.Tables.TableTransactionActionType.UpsertReplace, tableEntity));
                            break;
                        case AzureStorage.Standard.Core.Domain.Models.TableTransactionActionType.Delete:
                            batchActions.Add(new Azure.Data.Tables.TableTransactionAction(Azure.Data.Tables.TableTransactionActionType.Delete, tableEntity, new ETag(action.ETag)));
                            break;
                    }
                }

                await tableClient.SubmitTransactionAsync(batchActions, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                throw new AzureStorageException($"Failed to execute batch transaction on table '{tableName}'.", ex.ErrorCode, ex.Status, ex);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Converts a custom ITableEntity to Azure's TableEntity for SDK operations.
        /// Uses reflection to copy properties, excluding system properties (PartitionKey, RowKey, Timestamp, ETag).
        /// </summary>
        private static Azure.Data.Tables.TableEntity ConvertToTableEntity(AzureStorage.Standard.Core.Domain.Models.ITableEntity entity)
        {
            var tableEntity = new Azure.Data.Tables.TableEntity(entity.PartitionKey, entity.RowKey);

            // Copy properties using reflection
            var properties = entity.GetType().GetProperties()
                .Where(p => p.Name != nameof(AzureStorage.Standard.Core.Domain.Models.ITableEntity.PartitionKey) &&
                           p.Name != nameof(AzureStorage.Standard.Core.Domain.Models.ITableEntity.RowKey) &&
                           p.Name != nameof(AzureStorage.Standard.Core.Domain.Models.ITableEntity.Timestamp) &&
                           p.Name != nameof(AzureStorage.Standard.Core.Domain.Models.ITableEntity.ETag) &&
                           p.CanRead);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    tableEntity[prop.Name] = value;
                }
            }

            if (!string.IsNullOrEmpty(entity.ETag))
            {
                tableEntity.ETag = new ETag(entity.ETag);
            }

            return tableEntity;
        }

        /// <summary>
        /// Converts Azure's TableEntity to a custom ITableEntity type.
        /// Uses reflection to map properties and handles type conversions including enums.
        /// </summary>
        private static T ConvertFromTableEntity<T>(Azure.Data.Tables.TableEntity tableEntity) where T : class, AzureStorage.Standard.Core.Domain.Models.ITableEntity, new()
        {
            var entity = new T
            {
                PartitionKey = tableEntity.PartitionKey,
                RowKey = tableEntity.RowKey,
                Timestamp = tableEntity.Timestamp,
                ETag = tableEntity.ETag.ToString()
            };

            // Copy properties using reflection
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != nameof(AzureStorage.Standard.Core.Domain.Models.ITableEntity.PartitionKey) &&
                           p.Name != nameof(AzureStorage.Standard.Core.Domain.Models.ITableEntity.RowKey) &&
                           p.Name != nameof(AzureStorage.Standard.Core.Domain.Models.ITableEntity.Timestamp) &&
                           p.Name != nameof(AzureStorage.Standard.Core.Domain.Models.ITableEntity.ETag) &&
                           p.CanWrite);

            foreach (var prop in properties)
            {
                if (tableEntity.TryGetValue(prop.Name, out var value))
                {
                    try
                    {
                        // Handle type conversion
                        if (value != null && prop.PropertyType != value.GetType())
                        {
                            if (prop.PropertyType.IsEnum)
                            {
                                value = Enum.Parse(prop.PropertyType, value.ToString());
                            }
                            else
                            {
                                value = Convert.ChangeType(value, prop.PropertyType);
                            }
                        }

                        prop.SetValue(entity, value);
                    }
                    catch
                    {
                        // Ignore conversion errors for incompatible properties
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Validates that a table name is not null or empty.
        /// </summary>
        private static void ValidateTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName), "Table name cannot be null or empty.");
        }

        /// <summary>
        /// Validates that an entity and its keys are not null or empty.
        /// </summary>
        private static void ValidateEntity(AzureStorage.Standard.Core.Domain.Models.ITableEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ValidateKey(entity.PartitionKey, nameof(entity.PartitionKey));
            ValidateKey(entity.RowKey, nameof(entity.RowKey));
        }

        /// <summary>
        /// Validates that a key (PartitionKey or RowKey) is not null or empty.
        /// </summary>
        private static void ValidateKey(string key, string paramName)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null or empty.");
        }

        #endregion

        /// <summary>
        /// Disposes the TableClient instance.
        /// <para>
        /// Note: TableServiceClient does not implement IDisposable, so this method has no implementation.
        /// It exists to satisfy the ITableClient interface contract.
        /// </para>
        /// </summary>
        public void Dispose()
        {
            // TableServiceClient doesn't implement IDisposable, so nothing to dispose
        }
    }
}