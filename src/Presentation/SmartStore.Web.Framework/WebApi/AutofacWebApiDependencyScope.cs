using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web.Http.Dependencies;
using Autofac;

namespace SmartStore.Web.Framework.WebApi
{

	[SecurityCritical]
	internal class AutofacWebApiDependencyScope : IDependencyScope
	{
		private bool _disposed;

		readonly ILifetimeScope _lifetimeScope;

		/// <summary>
		/// Initializes a new instance of the <see cref="AutofacWebApiDependencyScope"/> class.
		/// </summary>
		/// <param name="lifetimeScope">The lifetime scope to resolve services from.</param>
		public AutofacWebApiDependencyScope(ILifetimeScope lifetimeScope)
		{
			Guard.NotNull(lifetimeScope, nameof(lifetimeScope));
			_lifetimeScope = lifetimeScope;
		}

		/// <summary>
		/// Gets the lifetime scope for the current dependency scope.
		/// </summary>
		public ILifetimeScope LifetimeScope
		{
			get { return _lifetimeScope; }
		}

		[SecurityCritical]
		public object GetService(Type serviceType)
		{
			return _lifetimeScope.ResolveOptional(serviceType);
		}

		[SecurityCritical]
		public IEnumerable<object> GetServices(Type serviceType)
		{
			if (!_lifetimeScope.IsRegistered(serviceType))
				return Enumerable.Empty<object>();

			var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);
			var instance = _lifetimeScope.Resolve(enumerableServiceType);
			return (IEnumerable<object>)instance;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="AutofacWebApiDependencyScope"/> class.
		/// </summary>
		[SecuritySafeCritical]
		~AutofacWebApiDependencyScope()
		{
			Dispose(false);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
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
					if (_lifetimeScope != null)
					{
						_lifetimeScope.Dispose();
					}
				}
				_disposed = true;
			}
		}

	}

}
