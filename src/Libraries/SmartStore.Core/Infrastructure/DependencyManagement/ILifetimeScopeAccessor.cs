using System;
using System.Collections.Generic;
using Autofac;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
	public interface ILifetimeScopeAccessor
	{   
		/// <summary>
		/// Gets the global, application-wide container.
		/// </summary>
		ILifetimeScope ApplicationContainer { get; }

		/// <summary>
		/// Ends the current lifetime scope.
		/// </summary>
		void EndLifetimeScope();

		/// <summary>
		///		Gets a nested lifetime scope that services can be resolved from
		/// </summary>
		/// <param name="configurationAction">A configuration action that will execute during lifetime scope creation.</param>
		/// <returns>
		///		A new or existing nested lifetime scope.
		///	</returns>
		ILifetimeScope GetLifetimeScope(Action<ContainerBuilder> configurationAction);

		/// <summary>
		///		Either creates a new lifetime scope when <c>HttpContext.Current</c> is <c>null</c>,
		///		OR returns the current http context scoped lifetime.
		/// </summary>
		/// <returns>
		///		A disposable object which does nothing when internal lifetime scope is bound to the http context,
		///		OR ends the lifetime scope otherwise.
		/// </returns>
		/// <remarks>
		///		This method is intended for usage in background threads or tasks. There may be situations where HttpContext is present,
		///		especially when a task was started with <c>TaskScheduler.FromCurrentSynchronizationContext()</c>. In this case it may not be
		///		desirable to create a new scope, but use the existing, http context bound scope instead.
		/// </remarks>
		IDisposable BeginContextAwareScope();
	}
}
