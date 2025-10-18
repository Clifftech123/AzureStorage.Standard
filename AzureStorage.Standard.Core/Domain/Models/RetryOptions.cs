
using System;

namespace AzureStorage.Standard.Core.Domain.Models
{
	/// <summary>
	/// Configuration options for automatic retry policies.
	/// Provides sensible defaults for transient failure handling.
	/// </summary>
	public class RetryOptions
	{
		/// <summary>
		/// Gets or sets the maximum number of retry attempts.
		/// Default is 3 retries.
		/// </summary>
		public int MaxRetryAttempts { get; set; } = 3;

		/// <summary>
		/// Gets or sets the initial delay between retries.
		/// Default is 1 second.
		/// </summary>
		public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Gets or sets the maximum delay between retries.
		/// Default is 30 seconds.
		/// </summary>
		public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

		/// <summary>
		/// Gets or sets whether retry is enabled.
		/// Default is true (automatic retries enabled).
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// Creates retry options with default settings (recommended for most scenarios).
		/// - 3 retry attempts
		/// - Exponential backoff starting at 1 second
		/// - Maximum 30 second delay
		/// </summary>
		public static RetryOptions Default => new RetryOptions();

		/// <summary>
		/// Creates retry options with no retries (for scenarios where immediate failure is preferred).
		/// </summary>
		public static RetryOptions None => new RetryOptions { Enabled = false };

		/// <summary>
		/// Creates aggressive retry options (for critical operations).
		/// - 5 retry attempts
		/// - Shorter delays
		/// </summary>
		public static RetryOptions Aggressive => new RetryOptions
		{
			MaxRetryAttempts = 5,
			InitialDelay = TimeSpan.FromMilliseconds(500),
			MaxDelay = TimeSpan.FromSeconds(10)
		};
	}
}
