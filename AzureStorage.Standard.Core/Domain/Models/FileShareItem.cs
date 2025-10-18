

using System;
using System.Collections.Generic;

namespace AzureStorage.Standard.Core.Domain.Models
{/// <summary>
    /// Represents an Azure File Share item (file or directory)
    /// </summary>
    public class FileShareItem
    {
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full path to the item
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Share name
        /// </summary>
        public string ShareName { get; set; }

        /// <summary>
        /// Indicates if this is a directory
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Size of the file in bytes (null for directories)
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// Content type of the file
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// MD5 hash of the file content
        /// </summary>
        public string ContentMD5 { get; set; }

        /// <summary>
        /// When the item was created
        /// </summary>
        public DateTimeOffset? CreatedOn { get; set; }

        /// <summary>
        /// When the item was last modified
        /// </summary>
        public DateTimeOffset? LastModified { get; set; }

        /// <summary>
        /// ETag of the item
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Custom metadata associated with the item
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}