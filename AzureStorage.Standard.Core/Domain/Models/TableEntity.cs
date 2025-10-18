

using System;

namespace AzureStorage.Standard.Core.Domain.Models {
    /// <summary>
    /// Base class for Azure Table entities
    /// </summary>
    public class TableEntity : ITableEntity {
        /// <summary>
        /// Partition key for the entity
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Row key for the entity
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// Timestamp when the entity was last modified
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// ETag for concurrency control
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Creates a new instance of TableEntity
        /// </summary>
        public TableEntity() {
        }

        /// <summary>
        /// Creates a new instance of TableEntity with partition and row keys
        /// </summary>
        public TableEntity(string partitionKey, string rowKey) {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
    }
}