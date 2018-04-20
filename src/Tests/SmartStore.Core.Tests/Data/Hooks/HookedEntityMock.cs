using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;

namespace SmartStore.Core.Tests.Data.Hooks
{
	internal class HookedEntityMock : IHookedEntity
	{
		private BaseEntity _entity;
		private EntityState _state;

		public HookedEntityMock(BaseEntity entity, EntityState state)
		{
			_entity = entity;
			_state = state;
			InitialState = state;
		}

		public DbEntityEntry Entry
		{
			// Is unmockable
			get { return null; }
		}

		public BaseEntity Entity
		{
			get { return _entity; }
		}

		public Type EntityType
		{
			get { return _entity.GetUnproxiedType(); }
		}

		public EntityState InitialState
		{
			get;
			set;
		}

		public EntityState State
		{
			get { return _state; }
			set { _state = value; }
		}

		public bool HasStateChanged
		{
			get
			{
				return InitialState != _state;
			}
		}

        public bool IsSoftDeleted
        {
            get
            {
                return false;
            }
        }

        public bool IsPropertyModified(string propertyName)
		{
			return false;
		}
	}
}
