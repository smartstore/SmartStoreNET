using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Caching
{
	public interface IDisplayControl
	{
		void Announce(BaseEntity entity);

		bool IsDisplayed(BaseEntity entity);

		void MarkRequestAsUncacheable();

		bool IsUncacheableRequest { get; }

		IEnumerable<string> GetCacheControlTagsFor(BaseEntity entity);

		IEnumerable<string> GetAllCacheControlTags();

		IDisposable BeginIdleScope();
	}

	public static class IDisplayControlExtensions
	{
		public static void AnnounceRange(this IDisplayControl displayedEntities, IEnumerable<BaseEntity> entities)
		{
			if (entities != null)
			{
				entities.Each(x => displayedEntities.Announce(x));
			}
		}
	}

}
