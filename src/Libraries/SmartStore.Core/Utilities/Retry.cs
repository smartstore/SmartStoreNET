using System;
using System.Threading.Tasks;

namespace SmartStore.Utilities
{
	public static class Retry
	{
		public static void Execute(Action operation, int times, TimeSpan? wait = null, Func<int, Exception, bool> retry = null)
		{
			Guard.NotNull(operation, nameof(operation));

			Func<bool> wrapper = () =>
			{
				operation();
				return true;
			};

			Execute(wrapper, times, wait, retry);
		}

		public static T Execute<T>(Func<T> operation, int times, TimeSpan? wait, Func<int, Exception, bool> retry = null)
		{
			Guard.NotNull(operation, nameof(operation));

			if (times < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(times), times, "The maximum number of attempts must not be less than 1.");
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

					if (retry != null && !retry(attempt, ex))
					{
						throw;
					}

					if (attempt >= times)
					{
						throw;
					}
				}
			}
		}


		public static async Task ExecuteAsync(Func<Task> operation, int times, TimeSpan? wait = null, Func<int, Exception, bool> retry = null)
		{
			Guard.NotNull(operation, nameof(operation));

			Func<Task<bool>> wrapper = async () =>
			{
				await operation().ConfigureAwait(false);
				return true;
			};

			await ExecuteAsync(wrapper, times, wait, retry);
		}

		public static async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, int times, TimeSpan? wait, Func<int, Exception, bool> retry = null)
		{
			Guard.NotNull(operation, nameof(operation));

			if (times < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(times), times, "The maximum number of attempts must not be less than 1.");
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

					if (retry != null && !retry(attempt, ex))
					{
						throw;
					}

					if (attempt >= times)
					{
						throw;
					}
				}
			}
		}
	}
}
