using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Fasterflect;

namespace SmartStore
{

    /// <summary>
    /// Base class for complex value objects, which do not have
    /// identifiers.
    /// </summary>
    /// <typeparam name="T">Type of the entity, which is the value obect.</typeparam>
    public abstract class ValueObject<T> : ComparableObject<ValueObject<T>>
        where T : ValueObject<T>
    {

        /// <summary>
        /// Registers all properties of the subclass as signature properties.
        /// </summary>
        protected void RegisterProperties()
        {
            RegisterProperties((pd) => true);
        }

        /// <summary>
        /// Registers any properties of the subclass matching 
        /// the given <param name="filter" /> as signature properties.
        /// </summary>
        protected void RegisterProperties(Func<PropertyInfo, bool> filter)
        {
            foreach (var pi in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (filter(pi))
                    RegisterSignatureProperty(pi);
            }
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var properties = GetSignatureProperties();
            int propsCount = properties.Count();

            int i = 1;

            foreach (var pi in properties)
            {
                object value = this.GetPropertyValue(pi.Name);
                if (value == null)
                    continue;

                sb.Append(pi.Name + ": " + value.ToString());
                if (i < propsCount)
                    sb.Append(", ");

                i++;
            }

            return sb.ToString();
        }

        public static bool operator ==(ValueObject<T> x, ValueObject<T> y)
        {
            if ((object)x == null)
                return (object)y == null;

            return x.Equals(y);
        }

        public static bool operator !=(ValueObject<T> x, ValueObject<T> y)
        {
            return !(x == y);
        }

    }

}
