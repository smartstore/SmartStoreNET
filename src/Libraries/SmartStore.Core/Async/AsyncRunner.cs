using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;

namespace SmartStore.Core.Async
{
	
	public interface IAsyncRunner
	{
		Task Run(Action<ILifetimeScope> action, CancellationToken cancellationToken, TaskCreationOptions options, TaskScheduler scheduler);
	}

	public static class IAsyncRunnerExtensions
	{
		public static Task Run(this IAsyncRunner runner, Action<ILifetimeScope> action)
		{
			return runner.Run(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public static Task Run(this IAsyncRunner runner, Action<ILifetimeScope> action, CancellationToken cancellationToken)
		{
			return runner.Run(action, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public static Task Run(this IAsyncRunner runner, Action<ILifetimeScope> action, TaskCreationOptions options)
		{
			return runner.Run(action, CancellationToken.None, options, TaskScheduler.Default);
		}

		public static Task Run(this IAsyncRunner runner, Action<ILifetimeScope> action, CancellationToken cancellationToken, TaskCreationOptions options)
		{
			return runner.Run(action, cancellationToken, options, TaskScheduler.Default);
		}

		public static Task Run(this IAsyncRunner runner, Action<ILifetimeScope> action, TaskScheduler scheduler)
		{
			return runner.Run(action, CancellationToken.None, TaskCreationOptions.None, scheduler);
		}
	}

	public class AsyncRunner : IAsyncRunner
	{
		public Task Run(Action<ILifetimeScope> action, CancellationToken cancellationToken, TaskCreationOptions options, TaskScheduler scheduler)
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
	}

}
