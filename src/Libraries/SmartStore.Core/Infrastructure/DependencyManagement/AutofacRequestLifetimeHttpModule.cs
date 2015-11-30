using System;
using System.Web;
using Autofac.Integration.Mvc;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// An <see cref="IHttpModule"/> and <see cref="ILifetimeScopeProvider"/> implementation 
    /// that creates a nested lifetime scope for each HTTP request.
    /// </summary>
    public class AutofacRequestLifetimeHttpModule : IHttpModule
	{

		public void Init(HttpApplication context)
		{
			Guard.ArgumentNotNull(() => context);

			context.EndRequest += OnEndRequest;
		}

		public static void OnEndRequest(object sender, EventArgs e)
		{
			if (LifetimeScopeProvider != null)
			{
				LifetimeScopeProvider.EndLifetimeScope();
			}
		}

		public static void SetLifetimeScopeProvider(ILifetimeScopeProvider lifetimeScopeProvider)
		{
			if (lifetimeScopeProvider == null)
			{
				throw new ArgumentNullException("lifetimeScopeProvider");
			}
			LifetimeScopeProvider = lifetimeScopeProvider;
		}


		internal static ILifetimeScopeProvider LifetimeScopeProvider
		{
			get;
			private set;
		}

		public void Dispose()
		{
		}

	}
}
