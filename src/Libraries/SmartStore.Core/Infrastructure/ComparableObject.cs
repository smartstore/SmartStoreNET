using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using Fasterflect;

namespace SmartStore
{

    /// <summary>
    /// Provides a standard base class for facilitating sophisticated comparison of objects.
    /// </summary>
    [Serializable]
    public abstract class ComparableObject
    {
        /// <summary>
        /// To help ensure hashcode uniqueness, a carefully selected random number multiplier
        /// is used within the calculation.  Goodrich and Tamassia's Data Structures and
        /// Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
        /// of collissions.  See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
        /// for more information.
        /// </summary>
        protected const int HashMultiplier = 31;

        private readonly List<PropertyInfo> _extraSignatureProperties = new List<PropertyInfo>();

        /// <summary>
        /// This static member caches the domain signature properties to avoid looking them up for
        /// each instance of the same type.
        ///
        /// A description of the ThreadStatic attribute may be found at
        /// http://www.dotnetjunkies.com/WebLog/chris.taylor/archive/2005/08/18/132026.aspx
        /// </summary>
        [ThreadStatic]
        private static IDictionary<Type, IEnumerable<PropertyInfo>> s_signatureProperties;

        public override bool Equals(object obj)
        {
            ComparableObject compareTo = obj as ComparableObject;

            if (ReferenceEquals(this, compareTo))
                return true;

            return compareTo != null && GetType().Equals(compareTo.GetTypeUnproxied()) &&
                HasSameSignatureAs(compareTo);
        }

        /// <summary>
        /// Used to provide the hashcode identifier of an object using the signature
        /// properties of the object; Since it is recommended that GetHashCode change infrequently,
        /// if at all, in an object's lifetime; it's important that properties are carefully
        /// selected which truly represent the signature of an object.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var signatureProperties = GetSignatureProperties();
                Type t = this.GetType();

                // It's possible for two objects to return the same hash code based on
                // identically valued properties, even if they're of two different types,
                // so we include the object's type in the hash calculation
                int hashCode = t.GetHashCode();

                foreach (var pi in signatureProperties)
                {
                    object value = this.GetPropertyValue(pi.Name); // pi.GetValue(this);

                    if (value != null)
                        hashCode = (hashCode * HashMultiplier) ^ value.GetHashCode();
                }

                if (signatureProperties.Any())
                    return hashCode;

                // If no properties were flagged as being part of the signature of the object,
                // then simply return the hashcode of the base object as the hashcode.
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// Returns the real underlying type of proxied objects.
        /// </summary>
        protected virtual Type GetTypeUnproxied()
        {
            return this.GetType();
        }

        /// <summary>
        /// You may override this method to provide your own comparison routine.
        /// </summary>
        protected virtual bool HasSameSignatureAs(ComparableObject compareTo)
        {
            if (compareTo == null)
                return false;

            var signatureProperties = GetSignatureProperties();

            foreach (var pi in signatureProperties)
            {
                object thisValue = this.GetPropertyValue(pi.Name);
                object thatValue = compareTo.GetPropertyValue(pi.Name);

                if (thisValue == null && thatValue == null)
                    continue;

                if ((thisValue == null ^ thatValue == null) ||
                    (!thisValue.Equals(thatValue)))
                {
                    return false;
                }
            }

            // If we've gotten this far and signature properties were found, then we can
            // assume that everything matched; otherwise, if there were no signature
            // properties, then simply return the default bahavior of Equals
            return signatureProperties.Any() || base.Equals(compareTo);
        }

        /// <summary>
        /// </summary>
        public IEnumerable<PropertyInfo> GetSignatureProperties()
        {
            IEnumerable<PropertyInfo> properties;

            // Init the signaturePropertiesDictionary here due to reasons described at
            // http://blogs.msdn.com/jfoscoding/archive/2006/07/18/670497.aspx
            if (s_signatureProperties == null)
                s_signatureProperties = new Dictionary<Type, IEnumerable<PropertyInfo>>();

			var t = GetType();

            if (s_signatureProperties.TryGetValue(t, out properties))
                return properties;

            return (s_signatureProperties[t] = GetSignaturePropertiesCore());
        }

        /// <summary>
        /// Enforces the template method pattern to have child objects determine which specific
        /// properties should and should not be included in the object signature comparison.
        /// </summary>
        protected virtual IEnumerable<PropertyInfo> GetSignaturePropertiesCore()
        {
            Type t = this.GetType();
            //var properties = TypeDescriptor.GetProvider(t).GetTypeDescriptor(t)
            //                               .GetPropertiesWith<ObjectSignatureAttribute>();

            //if (_extraSignatureProperties.Count > 0)
            //{
            //    properties = properties.Union(_extraSignatureProperties);
            //}

            //return new PropertyDescriptorCollection(properties.ToArray(), true);

            var properties = t.GetProperties()
                            .Where(p => Attribute.IsDefined(p, typeof(ObjectSignatureAttribute), true));

            return properties.Union(_extraSignatureProperties).ToList();
        }

        /// <summary>
        /// Adds an extra property to the type specific signature properties list.
        /// </summary>
        /// <param name="propertyInfo">The property to add.</param>
        /// <remarks>Both lists are <c>unioned</c>, so
        /// that no duplicates can occur within the global descriptor collection.</remarks>
        protected void RegisterSignatureProperty(PropertyInfo propertyInfo)
        {
            Guard.ArgumentNotNull(() => propertyInfo);
            _extraSignatureProperties.Add(propertyInfo);
        }

        /// <summary>
        /// Adds an extra property to the type specific signature properties list.
        /// </summary>
        /// <param name="propertyName">Name of the property to add.</param>
        /// <remarks>Both lists are <c>unioned</c>, so
        /// that no duplicates can occur within the global descriptor collection.</remarks>
        protected void RegisterSignatureProperty(string propertyName)
        {
            Guard.ArgumentNotEmpty(() => propertyName);

            Type t = GetType();

            var pi = t.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (pi == null)
                throw Error.Argument("propertyName", "Could not find property '{0}' on type '{1}'.", propertyName, t);

            RegisterSignatureProperty(pi);
        }

    }


    /// <summary>
    /// Generic version of <see cref="ComparableObject" />.
    /// </summary>
	[Serializable]
    public abstract class ComparableObject<T> : ComparableObject, IEquatable<T>
    {

        /// <summary>
        /// Adds an extra property to the type specific signature properties list.
        /// </summary>
        /// <param name="expression">The lambda expression for the property to add.</param>
        /// <remarks>Both lists are <c>unioned</c>, so
        /// that no duplicates can occur within the global descriptor collection.</remarks>
        protected void RegisterSignatureProperty(Expression<Func<T, object>> expression)
        {
            Guard.ArgumentNotNull(() => expression);

            base.RegisterSignatureProperty(expression.ExtractPropertyInfo());
        }

        public virtual bool Equals(T other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

    }

}
