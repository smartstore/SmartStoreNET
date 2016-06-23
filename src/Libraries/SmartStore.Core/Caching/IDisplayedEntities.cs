using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Caching
{
	public interface IDisplayedEntities
	{
		void Add(BaseEntity entity);

		string GetCacheControlTagFor(BaseEntity entity);

		IEnumerable<string> GetCacheControlTags();
	}

	public static class IDisplayedEntitiesExtensions
	{
		public static void AddRange(this IDisplayedEntities displayedEntities, IEnumerable<BaseEntity> entities)
		{
			if (entities != null)
			{
				entities.Each(x => displayedEntities.Add(x));
			}
		}
	}

}
