
using System;
using System.Net;
using System.Threading.Tasks;
using Azure;
using AzureStorage.Standard.Core.Domain.Models;
using Polly;
using Polly.Retry;

namespace AzureStorage.Standard.Files.Internal
{
	/// <summary>
	/// Internal helper class for managing retry policies.
	/// This is transparent to library users - retries happen automatically.
	/// </summary>
	internal static class RetryPolicyHelper
	{
		private static readonly Random _random = new Random();
		/// <summary>
		/// Creates a retry policy based on the provided options.
		/// Handles common Azure transient errors automatically.
		/// </summary>
		internal static IAsyncPolicy CreateRetryPolicy(RetryOptions options)
		{
			if (options == null || !options.Enabled)
			{
				// Return a no-op policy if retries are disabled
				return Policy.NoOpAsync();
			}

			return Policy
				.Handle<RequestFailedException>(IsTransientError)
				.WaitAndRetryAsync(
					retryCount: options.MaxRetryAttempts,
					sleepDurationProvider: retryAttempt => CalculateDelay(retryAttempt, options),
					onRetry: (exception, timeSpan, retryCount, context) =>
					{
						// Log retry attempt (optional - can be extended with ILogger)
						Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
					});
		}

		/// <summary>
		/// Determines if an Azure exception is transient and should be retried.
		/// </summary>
		private static bool IsTransientError(RequestFailedException exception)
		{
			// Retry on these HTTP status codes (transient errors)
			return exception.Status switch
			{
				(int)HttpStatusCode.RequestTimeout => true,          // 408
				(int)HttpStatusCode.InternalServerError => true,     // 500
				(int)HttpStatusCode.BadGateway => true,               // 502
				(int)HttpStatusCode.ServiceUnavailable => true,       // 503
				(int)HttpStatusCode.GatewayTimeout => true,           // 504
				429 => true,                                          // Too Many Requests (throttling)
				_ => IsServerBusy(exception) || IsNetworkError(exception)
			};
		}

		/// <summary>
		/// Checks if the error is a "ServerBusy" error.
		/// </summary>
		private static bool IsServerBusy(RequestFailedException exception)
		{
			return exception.ErrorCode == "ServerBusy" ||
				   exception.ErrorCode == "OperationTimedOut";
		}

		/// <summary>
		/// Checks if the error is a network-related error.
		/// </summary>
		private static bool IsNetworkError(RequestFailedException exception)
		{
			return exception.InnerException is System.Net.Http.HttpRequestException ||
				   exception.InnerException is System.Net.Sockets.SocketException ||
				   exception.InnerException is System.IO.IOException;
		}

		/// <summary>
		/// Calculates exponential backoff delay with jitter.
		/// </summary>
		private static TimeSpan CalculateDelay(int retryAttempt, RetryOptions options)
		{
			// Exponential backoff: initialDelay * 2^(retryAttempt - 1)
			var exponentialDelay = options.InitialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt - 1);

			// Add jitter (random 0-20% variation) to prevent thundering herd
			var jitter = _random.NextDouble() * 0.2 * exponentialDelay;
			var totalDelay = exponentialDelay + jitter;

			// Cap at max delay
			var delay = TimeSpan.FromMilliseconds(Math.Min(totalDelay, options.MaxDelay.TotalMilliseconds));

			return delay;
		}

		/// <summary>
		/// Executes an async operation with retry policy.
		/// This is the main entry point for automatic retries.
		/// </summary>
		internal static async Task<T> ExecuteWithRetryAsync<T>(
			RetryOptions options,
			Func<Task<T>> operation)
		{
			var policy = CreateRetryPolicy(options);
			return await policy.ExecuteAsync(operation);
		}

		/// <summary>
		/// Executes an async operation (without return value) with retry policy.
		/// </summary>
		internal static async Task ExecuteWithRetryAsync(
			RetryOptions options,
			Func<Task> operation)
		{
			var policy = CreateRetryPolicy(options);
			await policy.ExecuteAsync(operation);
		}
	}
}
