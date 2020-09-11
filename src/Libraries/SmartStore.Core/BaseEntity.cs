using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SmartStore.Core
{
    /// <summary>
    /// Base class for entities
    /// </summary>
    [DataContract]
    public abstract partial class BaseEntity : IEquatable<BaseEntity>
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string GetEntityName()
        {
            return GetUnproxiedType().Name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetUnproxiedType()
        {
            #region Old
            //var t = GetType();
            //if (t.AssemblyQualifiedName.StartsWith("System.Data.Entity."))
            //{
            //	// it's a proxied type
            //	t = t.BaseType;
            //}

            //return t;
            #endregion

            return ObjectContext.GetObjectType(GetType());
        }

        /// <summary>
        /// Transient objects are not associated with an item already in storage. For instance,
        /// a Product entity is transient if its Id is 0.
        /// </summary>
        public virtual bool IsTransientRecord()
        {
            return Id == 0;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BaseEntity);
        }

        bool IEquatable<BaseEntity>.Equals(BaseEntity other)
        {
            return this.Equals(other);
        }

        protected virtual bool Equals(BaseEntity other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (HasSameNonDefaultIds(other))
            {
                var otherType = other.GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.Equals(otherType);
            }

            return false;
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            if (IsTransientRecord())
            {
                return base.GetHashCode();
            }
            else
            {
                unchecked
                {
                    // It's possible for two objects to return the same hash code based on
                    // identically valued properties, even if they're of two different types,
                    // so we include the object's type in the hash calculation
                    var hashCode = GetUnproxiedType().GetHashCode();
                    return (hashCode * 31) ^ Id.GetHashCode();
                }
            }
        }

        public static bool operator ==(BaseEntity x, BaseEntity y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(BaseEntity x, BaseEntity y)
        {
            return !(x == y);
        }

        private bool HasSameNonDefaultIds(BaseEntity other)
        {
            return !this.IsTransientRecord() && !other.IsTransientRecord() && this.Id == other.Id;
        }
    }
}
