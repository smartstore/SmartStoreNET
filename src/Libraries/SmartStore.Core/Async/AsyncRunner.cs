using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;

namespace SmartStore.Core.Async
{

	public static class AsyncRunner
	{

		/// <summary>
		/// Execute's an async Task<T> method which has a void return value synchronously
		/// </summary>
		/// <param name="func">Task<T> method to execute</param>
		public static void RunSync(Func<Task> func)
		{
			var oldContext = SynchronizationContext.Current;
			var synch = new ExclusiveSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(synch);
			synch.Post(async _ =>
			{
				try
				{
					await func();
				}
				catch (Exception e)
				{
					synch.InnerException = e;
					throw;
				}
				finally
				{
					synch.EndMessageLoop();
				}
			}, null);
			synch.BeginMessageLoop();

			SynchronizationContext.SetSynchronizationContext(oldContext);
		}

		/// <summary>
		/// Execute's an async Task<T> method which has a T return type synchronously
		/// </summary>
		/// <typeparam name="T">Return Type</typeparam>
		/// <param name="func">Task<T> method to execute</param>
		/// <returns></returns>
		public static TResult RunSync<TResult>(Func<Task<TResult>> func)
		{
			var oldContext = SynchronizationContext.Current;
			var synch = new ExclusiveSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(synch);
			TResult ret = default(TResult);
			synch.Post(async _ =>
			{
				try
				{
					ret = await func();
				}
				catch (Exception e)
				{
					synch.InnerException = e;
					throw;
				}
				finally
				{
					synch.EndMessageLoop();
				}
			}, null);
			synch.BeginMessageLoop();
			SynchronizationContext.SetSynchronizationContext(oldContext);
			return ret;
		}

		public static Task Run(Action<ILifetimeScope> action)
		{
			return Run(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public static Task Run(Action<ILifetimeScope> action, CancellationToken cancellationToken)
		{
			return Run(action, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public static Task Run(Action<ILifetimeScope> action, TaskCreationOptions options)
		{
			return Run(action, CancellationToken.None, options, TaskScheduler.Default);
		}

		public static Task Run(Action<ILifetimeScope> action, CancellationToken cancellationToken, TaskCreationOptions options)
		{
			return Run(action, cancellationToken, options, TaskScheduler.Default);
		}

		public static Task Run(Action<ILifetimeScope> action, TaskScheduler scheduler)
		{
			return Run(action, CancellationToken.None, TaskCreationOptions.None, scheduler);
		}
		
		public static Task Run(Action<ILifetimeScope> action, CancellationToken cancellationToken, TaskCreationOptions options, TaskScheduler scheduler)
		{
			Guard.ArgumentNotNull(() => action);
			Guard.ArgumentNotNull(() => scheduler);

			var t = Task.Factory.StartNew(() => {
				using (var container = EngineContext.Current.ContainerManager.Container.BeginLifetimeScope(AutofacLifetimeScopeProvider.HttpRequestTag))
				{
					action(container);
				}
			}, cancellationToken, options, scheduler);

			return t;
		}

		public static Task Run(Action<ILifetimeScope, object> action, object state, CancellationToken cancellationToken, TaskCreationOptions options, TaskScheduler scheduler)
		{
			Guard.ArgumentNotNull(() => state);
			Guard.ArgumentNotNull(() => action);
			Guard.ArgumentNotNull(() => scheduler);

			var t = Task.Factory.StartNew((o) =>
			{
				using (var container = EngineContext.Current.ContainerManager.Container.BeginLifetimeScope(AutofacLifetimeScopeProvider.HttpRequestTag))
				{
					action(container, o);
				}
			}, state, cancellationToken, options, scheduler);

			return t;
		}

		private class ExclusiveSynchronizationContext : SynchronizationContext
		{
			private bool done;
			public Exception InnerException { get; set; }
			readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
			readonly Queue<Tuple<SendOrPostCallback, object>> items = new Queue<Tuple<SendOrPostCallback, object>>();

			public override void Send(SendOrPostCallback d, object state)
			{
				throw new NotSupportedException("We cannot send to the same thread");
			}

			public override void Post(SendOrPostCallback d, object state)
			{
				lock (items)
				{
					items.Enqueue(Tuple.Create(d, state));
				}
				workItemsWaiting.Set();
			}

			public void EndMessageLoop()
			{
				Post(_ => done = true, null);
			}

			public void BeginMessageLoop()
			{
				while (!done)
				{
					Tuple<SendOrPostCallback, object> task = null;
					lock (items)
					{
						if (items.Count > 0)
						{
							task = items.Dequeue();
						}
					}
					if (task != null)
					{
						task.Item1(task.Item2);
						if (InnerException != null) // the method threw an exeption
						{
							throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
						}
					}
					else
					{
						workItemsWaiting.WaitOne();
					}
				}
			}

			public override SynchronizationContext CreateCopy()
			{
				return this;
			}
		}

	}

}
