using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using SmartStore.Utilities;

namespace SmartStore.ComponentModel
{
	/// <summary>
	/// A very simple object mapper utility which tries to map properties of the same name between two objects.
	/// If matched properties has different types, the mapper tries to convert them.
	/// If conversion fails, the property is skipped (no exception is thrown).
	/// MiniMapper cannot handle sequence and predefined types.
	/// </summary>
	public static class MiniMapper
	{
		public static TTo Map<TFrom, TTo>(TFrom from, CultureInfo culture = null)
			where TFrom : class
			where TTo : class
		{
			var to = Map(from, typeof(TTo), culture);
			return (TTo)to;
		}

		//public static void Map<TFrom, TTo>(TFrom from, TTo to, CultureInfo culture = null) 
		//	where TFrom : class 
		//	where TTo : class
		//{
		//	Map((object)from, (object)to, culture);
		//}

		public static object Map<TFrom>(TFrom from, Type toType, CultureInfo culture = null)
			where TFrom : class
		{
			Guard.NotNull(toType, nameof(toType));
			Guard.HasDefaultConstructor(toType);

			var target = Activator.CreateInstance(toType);

			Map(from, target, culture);
			return target;
		}

		public static void Map<TFrom, TTo>(TFrom from, TTo to, CultureInfo culture = null)
			where TFrom : class
			where TTo : class
		{
			Guard.NotNull(from, nameof(from));
			Guard.NotNull(to, nameof(to));

			if (object.ReferenceEquals(from, to))
			{
				// Cannot map the same instance
				return;
			}

			var fromType = from.GetType();
			var toType = to.GetType();

			ValidateType(fromType);
			ValidateType(toType);

			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}

			var toProps = GetFastPropertiesFor(toType).ToArray();

			foreach (var toProp in toProps)
			{
				var fromProp = FastProperty.GetProperty(fromType, toProp.Name, PropertyCachingStrategy.Uncached);
				if (fromProp == null)
				{
					continue;
				}

				object value = null;
				try
				{
					// Get the value from source instance and try to convert it to target props type
					value = fromProp.GetValue(from).Convert(toProp.Property.PropertyType, culture);
					
					// Set it
					toProp.SetValue(to, value);
				}
				catch { }
			}
		}

		private static IEnumerable<FastProperty> GetFastPropertiesFor(Type type)
		{
			return FastProperty.GetCandidateProperties(type)
				.Select(pi => FastProperty.GetProperty(pi, PropertyCachingStrategy.Uncached))
				.Where(pi => pi.IsPublicSettable);
		}

		private static void ValidateType(Type type)
		{
			if (type.IsPredefinedType())
			{
				throw new InvalidOperationException("Mapping from or to predefined types is not possible. Type was: {0}".FormatInvariant(type.FullName));
			}

			if (type.IsSequenceType())
			{
				throw new InvalidOperationException("Mapping from or to sequence types is not possible. Type was: {0}".FormatInvariant(type.FullName));
			}
		}
	}
}
