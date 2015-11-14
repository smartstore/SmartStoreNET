using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SmartStore.Core;

namespace SmartStore
{
	public static class BaseEntityExtensions
	{
		public static TEntity ShallowCopy<TEntity>(this TEntity source) where TEntity : BaseEntity, new()
		{
			var newObject = new TEntity();

			var sourceProperties = typeof(TEntity)
				.GetProperties()
				.Where(x => x.CanRead && x.CanWrite && 0 == x.GetCustomAttributes(typeof(NotMappedAttribute), true).Length);

			foreach (var property in sourceProperties)
			{
				property.SetValue(newObject, property.GetValue(source, null), null);
			}

			return newObject;
		}
	}
}
