using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Caching
{
	public partial class DisplayControl : IDisplayControl
	{
		private readonly HashSet<BaseEntity> _entities = new HashSet<BaseEntity>();

		private bool? _isUncacheableRequest;

		public void Announce(BaseEntity entity)
		{
			if (entity != null)
			{
				_entities.Add(entity);
			}
		}

		public bool IsDisplayed(BaseEntity entity)
		{
			if (entity == null)
				return false;

			return _entities.Contains(entity);
		}

		public void MarkRequestAsUncacheable()
		{
			// First wins: subsequent calls should not be able to cancel this
			_isUncacheableRequest = true;
		}

		public bool IsUncacheableRequest
		{
			get
			{
				return _isUncacheableRequest.GetValueOrDefault() == true;
			}
		}

		public string GetCacheControlTagFor(BaseEntity entity)
		{
			Guard.NotNull(entity, nameof(entity));

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
