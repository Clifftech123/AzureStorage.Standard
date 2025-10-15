
namespace AzureStorage.Standard.Core.Domain.Models
{
 
    /// <summary>
    /// Interface for Azure Table entities
    /// </summary>
    public interface ITableEntity
    {
        /// <summary>
        /// Partition key for the entity
        /// </summary>
        string PartitionKey { get; set; }

        /// <summary>
        /// Row key for the entity
        /// </summary>
        string RowKey { get; set; }

        /// <summary>
        /// Timestamp when the entity was last modified
        /// </summary>
        DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// ETag for concurrency control
        /// </summary>
        string ETag { get; set; }
    }
}