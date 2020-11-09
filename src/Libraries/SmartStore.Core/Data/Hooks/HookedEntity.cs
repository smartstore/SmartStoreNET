using System;
using System.Data.Entity.Infrastructure;
using EfState = System.Data.Entity.EntityState;

namespace SmartStore.Core.Data.Hooks
{
    public interface IHookedEntity
    {
        /// <summary>
        /// Gets the impl type of the data context that triggered the hook.
        /// </summary>
        Type ContextType { get; }

        /// <summary>
        /// Gets the hooked entity entry
        /// </summary>
        DbEntityEntry Entry { get; }

        /// <summary>
        /// Gets the hooked entity instance
        /// </summary>
        BaseEntity Entity { get; }

        /// <summary>
        /// Gets the unproxied type of the hooked entity instance. 
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether the entity is in soft deleted state.
        /// This is the case when the entity is an instance of <see cref="ISoftDeletable"/>
        /// and the value of its <c>Deleted</c> property is true AND has changed since tracking.
        /// But when the entity is not in modified state the snapshot comparison is omitted.
        /// </summary>
        bool IsSoftDeleted { get; }
    }

    public class HookedEntity : IHookedEntity
    {
        private Type _entityType;

        public HookedEntity(IDbContext context, DbEntityEntry entry)
            : this(context.GetType(), entry)
        {
        }

        internal HookedEntity(Type contextType, DbEntityEntry entry)
        {
            ContextType = contextType;
            Entry = entry;
            InitialState = (EntityState)entry.State;
        }

        public Type ContextType
        {
            get;
        }

        public DbEntityEntry Entry
        {
            get;
        }

        public BaseEntity Entity => Entry.Entity as BaseEntity;

        public Type EntityType => _entityType ?? (_entityType = this.Entity?.GetUnproxiedType());

        public EntityState InitialState
        {
            get;
            set;
        }

        public EntityState State
        {
            get => (EntityState)Entry.State;
            set => Entry.State = (EfState)((int)value);
        }

        public bool HasStateChanged => InitialState != State;

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

        public bool IsSoftDeleted
        {
            get
            {
                var entity = Entry.Entity as ISoftDeletable;
                if (entity != null)
                {
                    return Entry.State == EfState.Modified
                        ? entity.Deleted && IsPropertyModified("Deleted")
                        : entity.Deleted;
                }

                return false;
            }
        }
    }
}
