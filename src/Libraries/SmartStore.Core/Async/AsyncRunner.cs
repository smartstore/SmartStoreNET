using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Autofac;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Core.Async
{
	public static class AsyncRunner
	{
		private static readonly BackgroundWorkHost _host = new BackgroundWorkHost();

		/// <summary>
		/// Gets the global cancellation token which signals the application shutdown
		/// </summary>
		public static CancellationToken AppShutdownCancellationToken
		{
			get { return _host.ShutdownCancellationTokenSource.Token; }
		}

		/// <summary>
		/// Executes an async Task method which has a void return value synchronously
		/// </summary>
		/// <param name="func">Task method to execute</param>
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
		/// Executes an async Task method which has a TResult return type synchronously
		/// </summary>
		/// <typeparam name="TResult">Return Type</typeparam>
		/// <param name="func">Task method to execute</param>
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

		public static Task Run(Action<ILifetimeScope, CancellationToken> action)
		{
			return Run(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public static Task Run(Action<ILifetimeScope, CancellationToken> action, CancellationToken cancellationToken)
		{
			return Run(action, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public static Task Run(Action<ILifetimeScope, CancellationToken> action, TaskCreationOptions options)
		{
			return Run(action, CancellationToken.None, options, TaskScheduler.Default);
		}

		public static Task Run(Action<ILifetimeScope, CancellationToken> action, CancellationToken cancellationToken, TaskCreationOptions options)
		{
			return Run(action, cancellationToken, options, TaskScheduler.Default);
		}

		public static Task Run(Action<ILifetimeScope, CancellationToken> action, TaskScheduler scheduler)
		{
			return Run(action, CancellationToken.None, TaskCreationOptions.None, scheduler);
		}

		public static Task Run(Action<ILifetimeScope, CancellationToken> action, CancellationToken cancellationToken, TaskCreationOptions options, TaskScheduler scheduler)
		{
			Guard.NotNull(action, nameof(action));
			Guard.NotNull(scheduler, nameof(scheduler));

			var ct = _host.CreateCompositeCancellationTokenSource(cancellationToken).Token;
			options |= TaskCreationOptions.LongRunning; // enforce an exclusive thread (not from pool)

			var t = Task.Factory.StartNew(() => {
				var accessor = EngineContext.Current.ContainerManager.ScopeAccessor;
				using (accessor.BeginContextAwareScope())
				{
					action(accessor.GetLifetimeScope(null), ct);
				}
			}, ct, options, scheduler);

			_host.Register(t, ct);

			return t;
		}

		public static Task Run(Action<ILifetimeScope, CancellationToken, object> action, object state, CancellationToken cancellationToken, TaskCreationOptions options, TaskScheduler scheduler)
		{
			Guard.NotNull(state, nameof(state));
			Guard.NotNull(action, nameof(action));
			Guard.NotNull(scheduler, nameof(scheduler));

			var ct = _host.CreateCompositeCancellationTokenSource(cancellationToken).Token;
			options |= TaskCreationOptions.LongRunning; // enforce an exclusive thread (not from pool)

			var t = Task.Factory.StartNew((o) =>
			{
				var accessor = EngineContext.Current.ContainerManager.ScopeAccessor;
				using (accessor.BeginContextAwareScope())
				{
					action(accessor.GetLifetimeScope(null), ct, o);
				}
			}, state, ct, options, scheduler);

			_host.Register(t, ct);

			return t;
		}



		public static Task<TResult> Run<TResult>(Func<ILifetimeScope, CancellationToken, TResult> function)
		{
			return Run(function, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public static Task<TResult> Run<TResult>(Func<ILifetimeScope, CancellationToken, TResult> function, CancellationToken cancellationToken)
		{
			return Run(function, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public static Task<TResult> Run<TResult>(Func<ILifetimeScope, CancellationToken, TResult> function, TaskCreationOptions options)
		{
			return Run(function, CancellationToken.None, options, TaskScheduler.Default);
		}

		public static Task<TResult> Run<TResult>(Func<ILifetimeScope, CancellationToken, TResult> function, CancellationToken cancellationToken, TaskCreationOptions options)
		{
			return Run(function, cancellationToken, options, TaskScheduler.Default);
		}

		public static Task<TResult> Run<TResult>(Func<ILifetimeScope, CancellationToken, TResult> function, TaskScheduler scheduler)
		{
			return Run(function, CancellationToken.None, TaskCreationOptions.None, scheduler);
		}

		public static Task<TResult> Run<TResult>(Func<ILifetimeScope, CancellationToken, TResult> function, CancellationToken cancellationToken, TaskCreationOptions options, TaskScheduler scheduler)
		{
			Guard.NotNull(function, nameof(function));
			Guard.NotNull(scheduler, nameof(scheduler));

			var ct = _host.CreateCompositeCancellationTokenSource(cancellationToken).Token;
			options |= TaskCreationOptions.LongRunning; // enforce an exclusive thread (not from pool)

			var t = Task.Factory.StartNew(() =>
			{
				var accessor = EngineContext.Current.ContainerManager.ScopeAccessor;
				using (accessor.BeginContextAwareScope())
				{
					return function(accessor.GetLifetimeScope(null), ct);
				}
			}, ct, options, scheduler);

			_host.Register(t, ct);

			return t;
		}

		public static Task<TResult> Run<TResult>(Func<ILifetimeScope, CancellationToken, object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions options, TaskScheduler scheduler)
		{
			Guard.NotNull(state, nameof(state));
			Guard.NotNull(function, nameof(function));
			Guard.NotNull(scheduler, nameof(scheduler));

			var ct = _host.CreateCompositeCancellationTokenSource(cancellationToken).Token;
			options |= TaskCreationOptions.LongRunning; // enforce an exclusive thread (not from pool)

			var t = Task.Factory.StartNew((o) =>
			{
				var accessor = EngineContext.Current.ContainerManager.ScopeAccessor;
				using (accessor.BeginContextAwareScope())
				{
					return function(accessor.GetLifetimeScope(null), ct, o);
				}
			}, state, ct, options, scheduler);

			_host.Register(t, ct);

			return t;
		}


		private class ExclusiveSynchronizationContext : SynchronizationContext
		{
			private bool _done;
			readonly AutoResetEvent _workItemsWaiting = new AutoResetEvent(false);
			readonly Queue<Tuple<SendOrPostCallback, object>> _items = new Queue<Tuple<SendOrPostCallback, object>>();

			public Exception InnerException { get; set; }

			public override void Send(SendOrPostCallback d, object state)
			{
				throw new NotSupportedException("We cannot send to the same thread");
			}

			public override void Post(SendOrPostCallback d, object state)
			{
				lock (_items)
				{
					_items.Enqueue(Tuple.Create(d, state));
				}
				_workItemsWaiting.Set();
			}

			public void EndMessageLoop()
			{
				Post(_ => _done = true, null);
			}

			public void BeginMessageLoop()
			{
				while (!_done)
				{
					Tuple<SendOrPostCallback, object> task = null;
					lock (_items)
					{
						if (_items.Count > 0)
						{
							task = _items.Dequeue();
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
						_workItemsWaiting.WaitOne();
					}
				}
			}

			public override SynchronizationContext CreateCopy()
			{
				return this;
			}
		}

	}

	internal class BackgroundWorkHost : IRegisteredObject
	{
		private readonly CancellationTokenSource _shutdownCancellationTokenSource = new CancellationTokenSource();
		private int _numRunningWorkItems;
		
		public BackgroundWorkHost()
		{
			HostingEnvironment.RegisterObject(this);
		}

		public CancellationTokenSource ShutdownCancellationTokenSource
		{
			get { return _shutdownCancellationTokenSource; }
		}

		public void Stop(bool immediate)
		{
			int num;
			lock (this)
			{
				_shutdownCancellationTokenSource.Cancel();
				num = _numRunningWorkItems;
			}
			if (num == 0)
			{
				FinalShutdown();
			}
		}

		public CancellationTokenSource CreateCompositeCancellationTokenSource(CancellationToken userCancellationToken)
		{
			if (userCancellationToken == CancellationToken.None)
			{
				return _shutdownCancellationTokenSource;
			}
			return CancellationTokenSource.CreateLinkedTokenSource(_shutdownCancellationTokenSource.Token, userCancellationToken);
		}

		public void Register(Task work, CancellationToken cancellationToken)
		{
			if (!cancellationToken.IsCancellationRequested)
			{
				lock (this)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						return;
					}
					_numRunningWorkItems++;
				}
				
				work.ContinueWith(
					WorkItemComplete,
					CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
			}
		}

		private void WorkItemComplete(Task work)
		{
			int num;
			bool isCancellationRequested;
			lock (this)
			{
				num = --_numRunningWorkItems;
				isCancellationRequested = _shutdownCancellationTokenSource.IsCancellationRequested; 
			}
			if (num == 0 && isCancellationRequested)
			{
				FinalShutdown();
			}
		}

		private void FinalShutdown()
		{
			HostingEnvironment.UnregisterObject(this);
		}

	}
}
