

using System;

namespace AzureStorage.Standard.Core.Domain.Models
{
     /// <summary>
    /// Represents an Azure Queue message
    /// </summary>
    public class QueueMessageItem
    {
        /// <summary>
        /// Unique identifier for the message
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Pop receipt used to delete or update the message
        /// </summary>
        public string PopReceipt { get; set; }

        /// <summary>
        /// The message content as string
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// The message content as bytes
        /// </summary>
        public byte[] MessageBytes { get; set; }

        /// <summary>
        /// Number of times this message has been dequeued
        /// </summary>
        public int DequeueCount { get; set; }

        /// <summary>
        /// When the message was first added to the queue
        /// </summary>
        public DateTimeOffset? InsertedOn { get; set; }

        /// <summary>
        /// When the message expires
        /// </summary>
        public DateTimeOffset? ExpiresOn { get; set; }

        /// <summary>
        /// When the message will become visible again (after being received with a visibility timeout)
        /// </summary>
        public DateTimeOffset? NextVisibleOn { get; set; }
    }
}