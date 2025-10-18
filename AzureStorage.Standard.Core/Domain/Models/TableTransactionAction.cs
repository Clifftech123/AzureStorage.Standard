
namespace AzureStorage.Standard.Core.Domain.Models
{
    /// <summary>
    /// Represents an action in a table batch transaction
    /// </summary>
    public class TableTransactionAction
    {
        /// <summary>
        /// Type of action to perform
        /// </summary>
        public TableTransactionActionType ActionType { get; set; }

        /// <summary>
        /// Entity to perform the action on
        /// </summary>
        public ITableEntity Entity { get; set; }

        /// <summary>
        /// ETag for optimistic concurrency (optional, defaults to "*")
        /// </summary>
        public string ETag { get; set; } = "*";

        /// <summary>
        /// Creates a new insert action
        /// </summary>
        public static TableTransactionAction Insert(ITableEntity entity)
        {
            return new TableTransactionAction
            {
                ActionType = TableTransactionActionType.Insert,
                Entity = entity
            };
        }

        /// <summary>
        /// Creates a new upsert (insert or replace) action
        /// </summary>
        public static TableTransactionAction Upsert(ITableEntity entity)
        {
            return new TableTransactionAction
            {
                ActionType = TableTransactionActionType.Upsert,
                Entity = entity
            };
        }

        /// <summary>
        /// Creates a new update action
        /// </summary>
        public static TableTransactionAction Update(ITableEntity entity, string eTag = "*")
        {
            return new TableTransactionAction
            {
                ActionType = TableTransactionActionType.Update,
                Entity = entity,
                ETag = eTag
            };
        }

        /// <summary>
        /// Creates a new delete action
        /// </summary>
        public static TableTransactionAction Delete(ITableEntity entity, string eTag = "*")
        {
            return new TableTransactionAction
            {
                ActionType = TableTransactionActionType.Delete,
                Entity = entity,
                ETag = eTag
            };
        }
    }

    /// <summary>
    /// Types of table transaction actions
    /// </summary>
    public enum TableTransactionActionType
    {
        Insert,
        Update,
        Upsert,
        Delete
    }
}