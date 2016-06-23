using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Caching
{
	public partial class DisplayedEntities : IDisplayedEntities
	{
		private readonly HashSet<BaseEntity> _entities = new HashSet<BaseEntity>();

		public void Add(BaseEntity entity)
		{
			if (entity != null)
			{
				_entities.Add(entity);
			}
		}

		public string GetCacheControlTagFor(BaseEntity entity)
		{
			Guard.ArgumentNotNull(() => entity);

			var typeName = entity.GetUnproxiedType().Name.ToLowerInvariant();
			string prefix = null;

			switch (typeName)
			{
				case "product":
					prefix = "p";
					break;
				case "category":
					prefix = "c";
					break;
				case "picture":
					prefix = "pic";
					break;
			}

			if (prefix != null)
			{
				return prefix + entity.Id.ToString();
			}

			return null;
		}

		public IEnumerable<string> GetCacheControlTags()
		{
			var entities = _entities.Where(x => x.Id > 0);

			foreach (var entity in entities)
			{
				var tag = GetCacheControlTagFor(entity);

				if (tag != null)
				{
					yield return tag;
				}
			}
		}
	}
}
