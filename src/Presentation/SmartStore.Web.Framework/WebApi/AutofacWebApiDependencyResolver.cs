using System;
using System.Collections.Generic;
using System.Security;
using System.Web.Http.Dependencies;
using Autofac;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.WebApi
{

	[SecurityCritical]
	public class AutofacWebApiDependencyResolver : IDependencyResolver
	{
		private bool _disposed;
		readonly ILifetimeScope _container;
		readonly IDependencyScope _rootDependencyScope;

		internal static readonly string ApiRequestTag = "AutofacWebRequest";

		public AutofacWebApiDependencyResolver()
		{
			var container = EngineContext.Current.ContainerManager.Container;
			_container = container;
			_rootDependencyScope = new AutofacWebApiDependencyScope(container);
		}

		public ILifetimeScope Container
		{
			get { return _container; }
		}

		public object GetService(Type serviceType)
		{
			return _rootDependencyScope.GetService(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return _rootDependencyScope.GetServices(serviceType);
		}

		public IDependencyScope BeginScope()
		{
			ILifetimeScope lifetimeScope = _container.BeginLifetimeScope(ApiRequestTag);
			return new AutofacWebApiDependencyScope(lifetimeScope);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="AutofacWebApiDependencyResolver"/> class.
		/// </summary>
		[SecuritySafeCritical]
		~AutofacWebApiDependencyResolver()
		{
			Dispose(false);
		}

		[SecuritySafeCritical]
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_rootDependencyScope != null)
					{
						_rootDependencyScope.Dispose();
					}
				}
				_disposed = true;
			}
		}
	}

}