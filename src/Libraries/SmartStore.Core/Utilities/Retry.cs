using System;
using System.Threading.Tasks;

namespace SmartStore.Utilities
{
	/// <summary>
	/// Provides functionality to automatically try the given piece of logic some number of times before re-throwing the exception. 
	/// This is useful for any piece of code which may experience transient failures. Be cautious of passing code with two distinct 
	/// actions given that if the second or subsequent piece of logic fails, the first will also be retried upon each retry. 
	/// </summary>
	public static class Retry
	{
		/// <summary>
		/// Executes an action with retry logic.
		/// </summary>
		/// <param name="operation">The action to be executed.</param>
		/// <param name="attempts">The maximum number of attempts.</param>
		/// <param name="wait">Timespan to wait between attempts of the operation</param>
		/// <param name="onFailed">The callback executed when an attempt is failed.</param>
		public static void Run(Action operation, int attempts, TimeSpan? wait = null, Action<int, Exception> onFailed = null)
		{
			Guard.NotNull(operation, nameof(operation));

			Run(Operation, attempts, wait, onFailed);

			bool Operation() 
			{
				operation();
				return true;
			}
		}

		/// <summary>
		/// Executes a function with retry logic.
		/// </summary>
		/// <param name="operation">The function to be executed.</param>
		/// <param name="attempts">The maximum number of attempts.</param>
		/// <param name="wait">Timespan to wait between attempts of the operation</param>
		/// <param name="onFailed">The callback executed when an attempt is failed.</param>
		public static T Run<T>(Func<T> operation, int attempts, TimeSpan? wait, Action<int, Exception> onFailed = null)
		{
			Guard.NotNull(operation, nameof(operation));

			if (attempts < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(attempts), attempts, "The maximum number of attempts must not be less than 1.");
			}

			var attempt = 0;

			while (true)
			{
				if (attempt > 0 && wait != null)
				{
					Task.Delay(wait.Value).Wait();
				}

				try
				{
					// Call the function passed in by the caller. 
					return operation();
				}
				catch (Exception ex)
				{
					attempt++;

					onFailed?.Invoke(attempt, ex);

					if (attempt >= attempts)
					{
						throw;
					}
				}
			}
		}


		/// <summary>
		/// Executes an asynchronous action with retry logic.
		/// </summary>
		/// <param name="operation">The asynchronous action to be executed.</param>
		/// <param name="attempts">The maximum number of attempts.</param>
		/// <param name="wait">Timespan to wait between attempts of the operation</param>
		/// <param name="onFailed">The callback executed when an attempt is failed.</param>
		public static async Task RunAsync(Func<Task> operation, int attempts, TimeSpan? wait = null, Action<int, Exception> onFailed = null)
		{
			Guard.NotNull(operation, nameof(operation));

			Func<Task<bool>> wrapper = async () =>
			{
				await operation().ConfigureAwait(false);
				return true;
			};

			await RunAsync(wrapper, attempts, wait, onFailed);
		}

		/// <summary>
		/// Executes an asynchronous function with retry logic.
		/// </summary>
		/// <param name="operation">The asynchronous function to be executed.</param>
		/// <param name="attempts">The maximum number of attempts.</param>
		/// <param name="wait">Timespan to wait between attempts of the operation</param>
		/// <param name="onFailed">The callback executed when an attempt is failed.</param>
		public static async Task<T> RunAsync<T>(Func<Task<T>> operation, int attempts, TimeSpan? wait, Action<int, Exception> onFailed = null)
		{
			Guard.NotNull(operation, nameof(operation));

			if (attempts < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(attempts), attempts, "The maximum number of attempts must not be less than 1.");
			}

			var attempt = 0;

			while (true)
			{
				if (attempt > 0 && wait != null)
				{
					await Task.Delay(wait.Value);
				}

				try
				{
					// Call the function passed in by the caller. 
					return await operation().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					attempt++;

					onFailed?.Invoke(attempt, ex);

					if (attempt >= attempts)
					{
						throw;
					}
				}
			}
		}
	}
}
