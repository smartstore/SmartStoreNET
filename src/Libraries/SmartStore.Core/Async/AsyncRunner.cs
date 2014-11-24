using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;

namespace SmartStore.Core.Async
{

	public static class AsyncRunner
	{
		private static readonly TaskFactory _myTaskFactory;

		static AsyncRunner()
		{
			_myTaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
		}

		public static void RunSync(Func<Task> func)
		{
			_myTaskFactory.StartNew<Task>(func).Unwrap().GetAwaiter().GetResult();
		}

		public static TResult RunSync<TResult>(Func<Task<TResult>> func)
		{
			return _myTaskFactory.StartNew<Task<TResult>>(func).Unwrap<TResult>().GetAwaiter().GetResult();
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

	}

}
