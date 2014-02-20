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

	}

}
