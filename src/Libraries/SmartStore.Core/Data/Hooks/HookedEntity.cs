using System.Data.Entity.Infrastructure;

namespace SmartStore.Core.Data.Hooks
{
	public sealed class HookedEntity
	{
		public HookedEntity(DbEntityEntry entry)
		{
			Entry = entry;
			InitialState = (EntityState)entry.State;
		}

		/// <summary>
		/// Gets the hooked entity entry
		/// </summary>
		public DbEntityEntry Entry
		{
			get;
			private set;
		}

		public BaseEntity Entity
		{
			get { return Entry.Entity as BaseEntity; }
		}

		/// <summary>
		/// Gets the initial (presave) state of the hooked entity
		/// </summary>
		public EntityState InitialState
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets or sets the current state of the hooked entity
		/// </summary>
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

		/// <summary>
		/// Gets a value indicating whether the entity state has changed during hooking.
		/// </summary>
		public bool HasStateChanged
		{
			get
			{
				return InitialState != State;
			}
		}

		/// <summary>
		/// Gets a value indicating whether a property has been modified.
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
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
