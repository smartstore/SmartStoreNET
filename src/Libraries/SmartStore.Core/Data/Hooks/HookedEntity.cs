using System;
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
	}
}
