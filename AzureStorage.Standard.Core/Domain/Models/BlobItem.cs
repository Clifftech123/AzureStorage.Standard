
using System;
using System.Collections.Generic;

namespace AzureStorage.Standard.Core.Domain.Models
{
    /// <summary>
    /// Represents an Azure blob item with its properties
    /// </summary>
    public class BlobItem {
        /// <summary>
        /// Gets or sets the name of the blob
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL of the blob
        /// </summary>
        public string Url { get; set; }


        /// <summary>
        /// Container name
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// Gets or sets the size of the blob in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the content type of the blob
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// MD5 hash of the blob content
        /// </summary>
        public string ContentMD5 { get; set; }

        /// <summary>
        /// When the blob was created
        /// </summary>
        public DateTimeOffset? CreatedOn { get; set; }

        /// <summary>
        /// When the blob was last modified
        /// </summary>
        public DateTimeOffset? LastModified { get; set; }

        /// <summary>
        /// ETag of the blob
        /// </summary>
        public string ETag { get; set; }


        /// <summary>
        /// Custom metadata associated with the blob
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase
        );
          
          
        /// <summary>
        /// Indicates if this is a directory/prefix (for hierarchical namespace)
        /// </summary>
        public bool IsDirectory { get; set; }

        
     
        /// <summary>
        /// Blob type (Block, Page, Append)
        /// </summary>
        public string BlobType { get; set; }

    }
}