using System;
using System.Reflection;
using SmartStore.ComponentModel;
using SmartStore.Core;

namespace SmartStore.Templating.Liquid
{
	internal class ObjectDrop : SafeDropBase
	{
		private readonly object _data;
		private readonly Type _type;

		public ObjectDrop(object data)
		{
			Guard.NotNull(data, nameof(data));

			_data = data;

			if (data is BaseEntity be)
			{
				_type = be.GetUnproxiedType();
			}
			else
			{
				_type = data.GetType();
			}
		}

		public override bool ContainsKey(object key)
		{
			return true;
		}

		protected override object InvokeMember(string name)
		{
			var prop = FastProperty.GetProperty(_type, name);
			if (prop != null)
			{
				return prop.GetValue(_data);
			}

			var method = _type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
			if (method != null && method.GetParameters().Length == 0)
			{
				return method.Invoke(_data, null);
			}

			return null;
		}

		public override object GetWrappedObject()
		{
			return _data;
		}
	}
}
