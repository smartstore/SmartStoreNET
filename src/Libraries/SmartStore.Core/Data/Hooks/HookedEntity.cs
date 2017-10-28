using System;
using System.Data.Entity.Infrastructure;

namespace SmartStore.Core.Data.Hooks
{
	public interface IHookedEntity
	{
		/// <summary>
		/// Gets the hooked entity entry
		/// </summary>
		DbEntityEntry Entry { get; }

		BaseEntity Entity { get; }

		Type EntityType { get; }

		/// <summary>
		/// Gets or sets the initial (presave) state of the hooked entity.
		/// The setter is for internal use only, don't invoke!
		/// </summary>
		EntityState InitialState { get; set; }

		/// <summary>
		/// Gets or sets the current state of the hooked entity
		/// </summary>
		EntityState State { get; set; }

		/// <summary>
		/// Gets a value indicating whether the entity state has changed during hooking.
		/// </summary>
		bool HasStateChanged { get; }

		/// <summary>
		/// Gets a value indicating whether a property has been modified.
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		bool IsPropertyModified(string propertyName);
	}

	public class HookedEntity : IHookedEntity
	{
		private Type _entityType;

		public HookedEntity(DbEntityEntry entry)
		{
			Entry = entry;
			InitialState = (EntityState)entry.State;
		}

		public DbEntityEntry Entry
		{
			get;
			private set;
		}

		public BaseEntity Entity
		{
			get { return Entry.Entity as BaseEntity; }
		}

		public Type EntityType
		{
			get
			{
				return _entityType ?? (_entityType = this.Entity?.GetUnproxiedType());
			}
		}

		public EntityState InitialState
		{
			get;
			set;
		}

		public EntityState State
		{
			get
			{
				return (EntityState)Entry.State;
			}
			set
			{
				Entry.State = (System.Data.Entity.EntityState)((int)value);
			}
		}

		public bool HasStateChanged
		{
			get
			{
				return InitialState != State;
			}
		}

		public bool IsPropertyModified(string propertyName)
		{
			Guard.NotEmpty(propertyName, nameof(propertyName));

			if (State == EntityState.Modified)
			{
				var prop = Entry.Property(propertyName);
				if (prop == null)
				{
					throw new SmartException($"An entity property '{propertyName}' does not exist.");
				}

				return prop.CurrentValue != null && !prop.CurrentValue.Equals(prop.OriginalValue);
			}

			return false;
		}
	}
}
