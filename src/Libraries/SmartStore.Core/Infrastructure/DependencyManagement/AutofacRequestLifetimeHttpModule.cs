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
            Guard.NotNull(context, nameof(context));

            context.EndRequest += OnEndRequest;
        }

        public static void OnEndRequest(object sender, EventArgs e)
        {
            if (LifetimeScopeProvider != null)
            {
                LifetimeScopeProvider.EndLifetimeScope();
            }

            // Dispose all other disposable object in HttpContext.Items
            PurgeContextItems(sender as HttpApplication);
        }

        private static void PurgeContextItems(HttpApplication app)
        {
            var items = app?.Context?.Items;

            if (items != null)
            {
                int size = items.Count;
                if (size > 0)
                {
                    var keys = new object[size];
                    items.Keys.CopyTo(keys, 0);

                    for (int i = 0; i < size; i++)
                    {
                        var obj = items[keys[i]] as IDisposable;
                        if (obj != null)
                        {
                            try
                            {
                                obj.Dispose();
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        public static void SetLifetimeScopeProvider(ILifetimeScopeProvider lifetimeScopeProvider)
        {
            LifetimeScopeProvider = lifetimeScopeProvider ?? throw new ArgumentNullException("lifetimeScopeProvider");
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
