using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Autofac;
using Autofac.Integration.Mvc;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
	public class AutofacLifetimeScopeProvider : ILifetimeScopeProvider
	{	
		private readonly ILifetimeScope _container;
		internal static readonly object HttpRequestTag = "AutofacWebRequest";

		public AutofacLifetimeScopeProvider(ILifetimeScope container)
		{
			Guard.ArgumentNotNull(() => container);

			this._container = container;
			AutofacRequestLifetimeHttpModule.SetLifetimeScopeProvider(this);

		}

		public ILifetimeScope ApplicationContainer
		{
			get { return _container; }
		}

		public void EndLifetimeScope()
		{
			try
			{
				ILifetimeScope lifetimeScope = LifetimeScope;
				if (lifetimeScope != null)
				{
					lifetimeScope.Dispose();
					HttpContext.Current.Items.Remove(typeof(ILifetimeScope));
				}
			}
			catch { }
		}

		public ILifetimeScope GetLifetimeScope(Action<ContainerBuilder> configurationAction)
		{
			//little hack here to get dependencies when HttpContext is not available
			if (HttpContext.Current != null)
			{
				return LifetimeScope ?? (LifetimeScope = GetLifetimeScopeCore(configurationAction));
			}
			else
			{
				return GetLifetimeScopeCore(configurationAction);
			}
		}

		protected virtual ILifetimeScope GetLifetimeScopeCore(Action<ContainerBuilder> configurationAction)
		{
			return (configurationAction == null)
				? _container.BeginLifetimeScope(HttpRequestTag)
				: _container.BeginLifetimeScope(HttpRequestTag, configurationAction);
		}

		private static ILifetimeScope LifetimeScope
		{
			get
			{
				return (ILifetimeScope)HttpContext.Current.Items[typeof(ILifetimeScope)];
			}
			set
			{
				HttpContext.Current.Items[typeof(ILifetimeScope)] = value;
			}
		}
 

	}
}
