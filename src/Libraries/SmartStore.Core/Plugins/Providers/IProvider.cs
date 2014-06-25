using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace SmartStore.Core.Plugins
{
	public interface IProvider
	{
	}

	public interface IConfigurableProvider : IProvider
	{
		void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);
	}

	public sealed class Provider<TProvider> where TProvider : IProvider
	{
		private readonly Lazy<TProvider, ProviderMetadata> _lazy;

		public Provider(Lazy<TProvider, ProviderMetadata> lazy) 
		{
			this._lazy = lazy;
		}

		public TProvider Value
		{
			get { return _lazy.Value; }
		}

		public ProviderMetadata Metadata
		{
			get { return _lazy.Metadata; }
		}

		public bool IsValueCreated
		{
			get { return _lazy.IsValueCreated; }
		}

		public override string ToString()
		{
			return _lazy.Metadata.SystemName;
		}
	}
}
