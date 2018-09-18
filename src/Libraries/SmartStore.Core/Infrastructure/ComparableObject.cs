using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SmartStore.Utilities;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using SmartStore.ComponentModel;

namespace SmartStore
{
    /// <summary>
    /// Provides a standard base class for facilitating sophisticated comparison of objects.
    /// </summary>
    [Serializable]
    public abstract class ComparableObject
    {
        private readonly HashSet<string> _extraSignatureProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private static readonly ConcurrentDictionary<Type, string[]> _signaturePropertyNames = new ConcurrentDictionary<Type, string[]>();

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
        [SuppressMessage("ReSharper", "BaseObjectGetHashCodeCallInGetHashCode")]
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public override int GetHashCode()
        {
            unchecked
            {
                var signatureProperties = GetSignatureProperties().ToArray();
                Type t = this.GetType();

				var combiner = HashCodeCombiner.Start();

				// It's possible for two objects to return the same hash code based on
				// identically valued properties, even if they're of two different types,
				// so we include the object's type in the hash calculation
				combiner.Add(t.GetHashCode());

                foreach (var prop in signatureProperties)
                {
                    var value = prop.GetValue(this);

                    if (value != null)
						combiner.Add(value.GetHashCode());
                }

                if (signatureProperties.Length > 0)
                    return combiner.CombinedHash;

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
                object thisValue = pi.GetValue(this);
                object thatValue = pi.GetValue(compareTo);

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
        public IEnumerable<FastProperty> GetSignatureProperties()
        {
			var type = GetType();
			var propertyNames = GetSignaturePropertyNamesCore();

			foreach (var name in propertyNames)
			{
				var fastProperty = FastProperty.GetProperty(type, name);
				if (fastProperty != null)
				{
					yield return fastProperty;
				}
			}
        }

        /// <summary>
        /// Enforces the template method pattern to have child objects determine which specific
        /// properties should and should not be included in the object signature comparison.
        /// </summary>
        protected virtual string[] GetSignaturePropertyNamesCore()
        {
            Type type = this.GetType();
			string[] names;

			if (!_signaturePropertyNames.TryGetValue(type, out names))
			{
				names = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(p => Attribute.IsDefined(p, typeof(ObjectSignatureAttribute), true))
					.Select(p => p.Name)
					.ToArray();

				_signaturePropertyNames.TryAdd(type, names);
			}

			if (_extraSignatureProperties.Count == 0)
			{
				return names;
			}

            return names.Union(_extraSignatureProperties).ToArray();
        }

        /// <summary>
        /// Adds an extra property to the type specific signature properties list.
        /// </summary>
        /// <param name="propertyName">Name of the property to add.</param>
        /// <remarks>Both lists are <c>unioned</c>, so
        /// that no duplicates can occur within the global descriptor collection.</remarks>
        protected void RegisterSignatureProperty(string propertyName)
        {
            Guard.NotEmpty(propertyName, nameof(propertyName));

			_extraSignatureProperties.Add(propertyName);
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
            Guard.NotNull(expression, nameof(expression));

            base.RegisterSignatureProperty(expression.ExtractPropertyInfo().Name);
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
