using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace SmartStore.Core
{
    
    /// <summary>
    /// Base class for entities
    /// </summary>
    [DataContract]
    public abstract partial class BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
		[DataMember, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity);
        }

        private static bool IsTransient(BaseEntity obj)
        {
            return obj != null && Equals(obj.Id, default(int));
        }

        public Type GetUnproxiedType()
        {
			var t = GetType();
			if (t.AssemblyQualifiedName.StartsWith("System.Data.Entity."))
			{
				// it's a proxied type
				t = t.BaseType;
			}
			return t;
        }

        public virtual bool Equals(BaseEntity other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTransient(this) &&
                !IsTransient(other) &&
                Equals(Id, other.Id))
            {
                var otherType = other.GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.IsAssignableFrom(otherType) ||
                        otherType.IsAssignableFrom(thisType);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (Equals(Id, default(int)))
                return base.GetHashCode();
            return Id.GetHashCode();
        }

        public static bool operator ==(BaseEntity x, BaseEntity y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(BaseEntity x, BaseEntity y)
        {
            return !(x == y);
        }
    }
}
