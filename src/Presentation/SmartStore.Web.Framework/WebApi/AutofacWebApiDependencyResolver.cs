using System;
using System.Collections.Generic;
using System.Security;
using System.Web.Http.Dependencies;
using Autofac;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;

namespace SmartStore.Web.Framework.WebApi
{

    [SecurityCritical]
    public class AutofacWebApiDependencyResolver : IDependencyResolver
    {
        private bool _disposed;
        readonly ILifetimeScope _container;
        readonly IDependencyScope _rootDependencyScope;
        readonly ILifetimeScopeAccessor _accessor;

        internal static readonly string ApiRequestTag = "AutofacWebRequest";

        public AutofacWebApiDependencyResolver()
        {
            _container = EngineContext.Current.ContainerManager.Container;
            _accessor = _container.Resolve<ILifetimeScopeAccessor>();
            _rootDependencyScope = new AutofacWebApiDependencyScope(_container);
        }

        public ILifetimeScope Container => _container;

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
            //ILifetimeScope lifetimeScope = _container.BeginLifetimeScope(ApiRequestTag);
            ILifetimeScope lifetimeScope = _accessor.GetLifetimeScope(null);
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