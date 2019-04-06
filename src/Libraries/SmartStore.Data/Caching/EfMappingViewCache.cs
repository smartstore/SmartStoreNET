using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Data.Entity.Core.Metadata.Edm;

namespace SmartStore.Data.Caching
{
	public class EfMappingViewCache : DbMappingViewCache
	{
		private readonly string _hash;
		private readonly Dictionary<string, DbMappingView> _views;

		public EfMappingViewCache(object json)
		{
			// TODO
		}

		public EfMappingViewCache(string hash, Dictionary<EntitySetBase, DbMappingView> views)
		{
			Guard.NotEmpty(hash, nameof(hash));
			Guard.NotNull(views, nameof(views));

			_hash = hash;
			_views = views.ToDictionary(kvp => GetExtentFullName(kvp.Key), kvp => kvp.Value);
		}

		public override string MappingHashValue
		{
			get { return _hash; }
		}

		public override DbMappingView GetView(EntitySetBase extent)
		{
			_views.TryGetValue(GetExtentFullName(extent), out var mappingView);
			return mappingView;
		}

		private string GetExtentFullName(EntitySetBase entitySet)
		{
			return entitySet.EntityContainer.Name + "." + entitySet.Name;
		}
	}
}
