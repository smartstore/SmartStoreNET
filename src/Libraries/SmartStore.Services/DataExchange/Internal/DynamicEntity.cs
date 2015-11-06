using SmartStore.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Internal
{
	internal class DynamicEntity : HybridExpando
	{
		public DynamicEntity(DynamicEntity dynamicEntity)
			: this(dynamicEntity.WrappedObject)
		{
			base.Properties.Merge(dynamicEntity, true);
		}

		public DynamicEntity(object entity)
			: base(entity)
		{
			// TODO: Umbenennen!!!
			base.Properties["_Entity"] = entity;
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
			// Virtual navigation properties of entities should not be set as is.
			// A previously created ExpandoEntity instance is set instead.
			// But assigning the value to the wrapped entity instance would result
			// in a type mismatch error, as both types obviously don't match.
			// Therefore we save reference values in the local values dictionary.

			var pi = base.GetPropertyInfo(name);

			if (pi != null && (pi.PropertyType.IsPredefinedType() || !pi.GetMethod.IsVirtual))
			{
				// Property exists, is NOT complex, or IS complex but NOT virtual
				try
				{
					Fasterflect.PropertyInfoExtensions.Set(pi, WrappedObject, value);
					return true;
				}
				catch { }
			}

			// Property does not exist, or is virtual complex
			Properties[name] = value;
			return true;
		}
	}
}
