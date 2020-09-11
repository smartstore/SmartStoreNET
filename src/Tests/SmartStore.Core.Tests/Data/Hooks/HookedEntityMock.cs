using System;
using System.Data.Entity.Infrastructure;
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

        public Type ContextType => typeof(IDbContext);

        public DbEntityEntry Entry => null;

        public BaseEntity Entity => _entity;

        public Type EntityType => _entity.GetUnproxiedType();

        public EntityState InitialState
        {
            get;
            set;
        }

        public EntityState State
        {
            get => _state;
            set => _state = value;
        }

        public bool HasStateChanged => InitialState != _state;

        public bool IsSoftDeleted => false;

        public bool IsPropertyModified(string propertyName)
        {
            return false;
        }
    }
}
