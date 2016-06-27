using System.Collections.Generic;
using SmartStore.ComponentModel;

namespace SmartStore.Services.DataExchange.Export.Internal
{
	internal class DynamicEntity : HybridExpando
	{
		public DynamicEntity(DynamicEntity dynamicEntity)
			: this(dynamicEntity.WrappedObject)
		{
			MergeRange(dynamicEntity);
		}

		public DynamicEntity(object entity)
			: base(entity)
		{
			base.Properties["Entity"] = entity;
		}

		public void Merge(string name, object value)
		{
			Properties[name] = value;
		}

		public void MergeRange(IDictionary<string, object> other)
		{
			foreach (var kvp in other)
			{
				Properties[kvp.Key] = kvp.Value;
			}
		}

		protected override bool TrySetMemberCore(string name, object value)
		{
			Properties[name] = value;
			return true;
		}
	}
}
