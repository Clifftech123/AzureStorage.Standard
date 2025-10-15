

namespace AzureStorage.Standard.Core.Domain.Models {
    /// <summary>
    /// Permissions for Blob SAS tokens
    /// </summary>
    [Flags]
    public enum BlobSasPermissions
    {
        /// <summary>
        /// No permissions
        /// </summary>
        None = 0,

        /// <summary>
        /// Read permission
        /// </summary>
        Read = 1,

        /// <summary>
        /// Add/Append permission
        /// </summary>
        Add = 2,

        /// <summary>
        /// Create permission
        /// </summary>
        Create = 4,

        /// <summary>
        /// Write permission
        /// </summary>
        Write = 8,

        /// <summary>
        /// Delete permission
        /// </summary>
        Delete = 16,

        /// <summary>
        /// List permission
        /// </summary>
        List = 32,

        /// <summary>
        /// All permissions
        /// </summary>
        All = Read | Add | Create | Write | Delete | List
    }
}