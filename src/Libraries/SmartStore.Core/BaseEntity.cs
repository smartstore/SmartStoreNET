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
		private int? _cachedHashcode;
		
		/// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
		[DataMember]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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

		/// <summary>
		/// Transient objects are not associated with an item already in storage.  For instance,
		/// a Product entity is transient if its Id is 0.
		/// </summary>
		public virtual bool IsTransient
		{
			get { return Id == 0; }
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as BaseEntity);
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
                var thisType = this.GetUnproxiedType();
                return thisType.Equals(otherType);
            }

            return false;
        }

        public override int GetHashCode()
        {
			// once hashcode is generated, never change it!
			if (_cachedHashcode.HasValue)
				return _cachedHashcode.Value;

			if (this.IsTransient)
			{
				_cachedHashcode = base.GetHashCode();
			}
			else
			{
				unchecked
				{
					// It's possible for two objects to return the same hash code based on
					// identically valued properties, even if they're of two different types,
					// so we include the object's type in the hash calculation
					int hashCode = GetUnproxiedType().GetHashCode();
					_cachedHashcode = (hashCode * 31) ^ Id.GetHashCode();
				}
			}

			return _cachedHashcode.Value;
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
			return !this.IsTransient && !other.IsTransient && this.Id == other.Id;
		}
    }
}
