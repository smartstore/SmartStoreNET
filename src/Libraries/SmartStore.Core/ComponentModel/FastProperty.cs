// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SmartStore.ComponentModel
{
	public enum PropertyCachingStrategy
	{
		/// <summary>
		/// Don't cache FastProperty instances
		/// </summary>
		Uncached = 0,
		/// <summary>
		/// Always cache FastProperty instances
		/// </summary>
		Cached = 1,
		/// <summary>
		/// Always cache FastProperty instances. PLUS cache all other properties of the declaring type.
		/// </summary>
		EagerCached = 2
	}

	public class FastProperty
	{
		// Delegate type for a by-ref property getter
		private delegate TValue ByRefFunc<TDeclaringType, TValue>(ref TDeclaringType arg);

		private static readonly MethodInfo CallPropertyGetterOpenGenericMethod = typeof(FastProperty).GetTypeInfo().GetDeclaredMethod("CallPropertyGetter");
		private static readonly MethodInfo CallPropertyGetterByReferenceOpenGenericMethod = typeof(FastProperty).GetTypeInfo().GetDeclaredMethod("CallPropertyGetterByReference");
		private static readonly MethodInfo CallNullSafePropertyGetterOpenGenericMethod = typeof(FastProperty).GetTypeInfo().GetDeclaredMethod("CallNullSafePropertyGetter");
		private static readonly MethodInfo CallNullSafePropertyGetterByReferenceOpenGenericMethod = typeof(FastProperty).GetTypeInfo().GetDeclaredMethod("CallNullSafePropertyGetterByReference");
		private static readonly MethodInfo CallPropertySetterOpenGenericMethod = typeof(FastProperty).GetTypeInfo().GetDeclaredMethod("CallPropertySetter");

		private static readonly ConcurrentDictionary<PropertyKey, FastProperty> _singlePropertiesCache = new ConcurrentDictionary<PropertyKey, FastProperty>();

		// Using an array rather than IEnumerable, as target will be called on the hot path numerous times.
		private static readonly ConcurrentDictionary<Type, IDictionary<string, FastProperty>> _propertiesCache = new ConcurrentDictionary<Type, IDictionary<string, FastProperty>>();
		private static readonly ConcurrentDictionary<Type, IDictionary<string, FastProperty>> _visiblePropertiesCache = new ConcurrentDictionary<Type, IDictionary<string, FastProperty>>();

		private Action<object, object> _valueSetter;
		private bool? _isPublicSettable;
		private bool? _isSequenceType;

		/// <summary>
		/// Initializes a <see cref="FastProperty"/>.
		/// This constructor does not cache the helper. For caching, use <see cref="GetProperties(object, PropertyCachingStrategy)"/>.
		/// </summary>
		[SuppressMessage("ReSharper", "VirtualMemberCallInContructor")]
		public FastProperty(PropertyInfo property)
		{
			Guard.NotNull(property, nameof(property));

			Property = property;
			Name = property.Name;
			ValueGetter = MakeFastPropertyGetter(property);
		}

		/// <summary>
		/// Gets the backing <see cref="PropertyInfo"/>.
		/// </summary>
		public PropertyInfo Property { get; private set; }

		/// <summary>
		/// Gets (or sets in derived types) the property name.
		/// </summary>
		public virtual string Name { get; protected set; }

		/// <summary>
		/// Gets the property value getter.
		/// </summary>
		public Func<object, object> ValueGetter { get; private set; }

		public bool IsPublicSettable
		{
			get
			{
				if (!_isPublicSettable.HasValue)
				{
					_isPublicSettable = Property.CanWrite && Property.GetSetMethod(false) != null;
				}
				return _isPublicSettable.Value;
			}
		}

		public bool IsSequenceType
		{
			get
			{
				if (!_isSequenceType.HasValue)
				{
					_isSequenceType = Property.PropertyType != typeof(string) && Property.PropertyType.IsSubClass(typeof(IEnumerable<>));
				}
				return _isSequenceType.Value;
			}
		}

		/// <summary>
		/// Gets the property value setter.
		/// </summary>
		public Action<object, object> ValueSetter
		{
			get
			{
				if (_valueSetter == null)
				{
					// We'll allow safe races here.
					_valueSetter = MakeFastPropertySetter(Property);
				}

				return _valueSetter;
			}
		}

		/// <summary>
		/// Returns the property value for the specified <paramref name="instance"/>.
		/// </summary>
		/// <param name="instance">The object whose property value will be returned.</param>
		/// <returns>The property value.</returns>
		public object GetValue(object instance)
		{
			return ValueGetter(instance);
		}

		/// <summary>
		/// Sets the property value for the specified <paramref name="instance" />.
		/// </summary>
		/// <param name="instance">The object whose property value will be set.</param>
		/// <param name="value">The property value.</param>
		public void SetValue(object instance, object value)
		{
			ValueSetter(instance, value);
		}

		/// <summary>
		/// Creates and caches fast property helpers that expose getters for every public get property on the
		/// underlying type.
		/// </summary>
		/// <param name="instance">the instance to extract property accessors for.</param>
		/// <returns>A cached array of all public property getters from the underlying type of target instance.
		/// </returns>
		public static IReadOnlyDictionary<string, FastProperty> GetProperties(object instance)
		{
			return GetProperties(instance.GetType());
		}

		/// <summary>
		/// Creates and caches fast property helpers that expose getters for every public get property on the
		/// specified type.
		/// </summary>
		/// <param name="type">The type to extract property accessors for.</param>
		/// <returns>A cached array of all public property getters from the type of target instance.
		/// </returns>
		public static IReadOnlyDictionary<string, FastProperty> GetProperties(Type type, PropertyCachingStrategy cachingStrategy = PropertyCachingStrategy.Cached)
		{
			var propertiesCache = cachingStrategy > PropertyCachingStrategy.Uncached ? _propertiesCache : CreateVolatileCache();

			return (IReadOnlyDictionary<string, FastProperty>)GetProperties(type, CreateInstance, propertiesCache);
		}

		/// <summary>
		/// <para>
		/// Creates and caches fast property helpers that expose getters for every non-hidden get property
		/// on the specified type.
		/// </para>
		/// <para>
		/// <see cref="GetVisibleProperties(object, PropertyCachingStrategy)"/> excludes properties defined on base types that have been
		/// hidden by definitions using the <c>new</c> keyword.
		/// </para>
		/// </summary>
		/// <param name="instance">The instance to extract property accessors for.</param>
		/// <returns>
		/// A cached array of all public property getters from the instance's type.
		/// </returns>
		public static IReadOnlyDictionary<string, FastProperty> GetVisibleProperties(object instance, PropertyCachingStrategy cachingStrategy = PropertyCachingStrategy.Cached)
		{
			var propertiesCache = cachingStrategy > PropertyCachingStrategy.Uncached ? _propertiesCache : CreateVolatileCache();
			var visiblePropertiesCache = cachingStrategy > PropertyCachingStrategy.Uncached ? _visiblePropertiesCache : CreateVolatileCache();

			return (IReadOnlyDictionary<string, FastProperty>)GetVisibleProperties(instance.GetType(), CreateInstance, propertiesCache, visiblePropertiesCache);
		}

		/// <summary>
		/// <para>
		/// Creates and caches fast property helpers that expose getters for every non-hidden get property
		/// on the specified type.
		/// </para>
		/// <para>
		/// <see cref="GetVisibleProperties(Type, PropertyCachingStrategy)"/> excludes properties defined on base types that have been
		/// hidden by definitions using the <c>new</c> keyword.
		/// </para>
		/// </summary>
		/// <param name="type">The type to extract property accessors for.</param>
		/// <returns>
		/// A cached array of all public property getters from the type.
		/// </returns>
		public static IReadOnlyDictionary<string, FastProperty> GetVisibleProperties(Type type, PropertyCachingStrategy cachingStrategy = PropertyCachingStrategy.Cached)
		{
			var propertiesCache = cachingStrategy > PropertyCachingStrategy.Uncached ? _propertiesCache : CreateVolatileCache();
			var visiblePropertiesCache = cachingStrategy > PropertyCachingStrategy.Uncached ? _visiblePropertiesCache : CreateVolatileCache();

			return (IReadOnlyDictionary<string, FastProperty>)GetVisibleProperties(type, CreateInstance, propertiesCache, visiblePropertiesCache);
		}

		public static FastProperty GetProperty<T>(
			Expression<Func<T, object>> property,
			PropertyCachingStrategy cachingStrategy = PropertyCachingStrategy.Cached)
		{
			return GetProperty(property.ExtractPropertyInfo(), cachingStrategy);
		}

		public static FastProperty GetProperty(
			Type type,
			string propertyName,
			PropertyCachingStrategy cachingStrategy = PropertyCachingStrategy.Cached)
		{
			Guard.NotNull(type, nameof(type));
			Guard.NotEmpty(propertyName, nameof(propertyName));

			if (TryGetCachedProperty(type, propertyName, cachingStrategy == PropertyCachingStrategy.EagerCached, out var fastProperty))
			{
				return fastProperty;
			}

			var key = new PropertyKey(type, propertyName);
			if (!_singlePropertiesCache.TryGetValue(key, out fastProperty))
			{
				var pi = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (pi != null)
				{
					fastProperty = CreateInstance(pi);
					if (cachingStrategy > PropertyCachingStrategy.Uncached)
					{
						_singlePropertiesCache.TryAdd(key, fastProperty);
					}
				}
			}

			return fastProperty;
		}

		public static FastProperty GetProperty(
			PropertyInfo propertyInfo,
			PropertyCachingStrategy cachingStrategy = PropertyCachingStrategy.Cached)
		{
			Guard.NotNull(propertyInfo, nameof(propertyInfo));

			if (TryGetCachedProperty(propertyInfo.ReflectedType, propertyInfo.Name, cachingStrategy == PropertyCachingStrategy.EagerCached, out var fastProperty))
			{
				return fastProperty;
			}

			var key = new PropertyKey(propertyInfo.ReflectedType, propertyInfo.Name);
			if (!_singlePropertiesCache.TryGetValue(key, out fastProperty))
			{
				fastProperty = CreateInstance(propertyInfo);
				if (cachingStrategy > PropertyCachingStrategy.Uncached)
				{
					_singlePropertiesCache.TryAdd(key, fastProperty);
				}
			}

			return fastProperty;
		}

		private static bool TryGetCachedProperty(
			Type type,
			string propertyName,
			bool eagerCached,
			out FastProperty fastProperty)
		{
			fastProperty = null;
			IDictionary<string, FastProperty> allProperties;

			if (eagerCached)
			{
				allProperties = (IDictionary<string, FastProperty>)GetProperties(type);
				allProperties.TryGetValue(propertyName, out fastProperty);
			}

			if (fastProperty == null && _propertiesCache.TryGetValue(type, out allProperties))
			{
				allProperties.TryGetValue(propertyName, out fastProperty);
			}

			return fastProperty != null;
		}

		/// <summary>
		/// Creates a single fast property getter. The result is not cached.
		/// </summary>
		/// <param name="propertyInfo">propertyInfo to extract the getter for.</param>
		/// <returns>a fast getter.</returns>
		/// <remarks>
		/// This method is more memory efficient than a dynamically compiled lambda, and about the
		/// same speed.
		/// </remarks>
		public static Func<object, object> MakeFastPropertyGetter(PropertyInfo propertyInfo)
		{
			Debug.Assert(propertyInfo != null);

			return MakeFastPropertyGetter(
				propertyInfo,
				CallPropertyGetterOpenGenericMethod,
				CallPropertyGetterByReferenceOpenGenericMethod);
		}

		/// <summary>
		/// Creates a single fast property getter which is safe for a null input object. The result is not cached.
		/// </summary>
		/// <param name="propertyInfo">propertyInfo to extract the getter for.</param>
		/// <returns>a fast getter.</returns>
		/// <remarks>
		/// This method is more memory efficient than a dynamically compiled lambda, and about the
		/// same speed.
		/// </remarks>
		public static Func<object, object> MakeNullSafeFastPropertyGetter(PropertyInfo propertyInfo)
		{
			Debug.Assert(propertyInfo != null);

			return MakeFastPropertyGetter(
				propertyInfo,
				CallNullSafePropertyGetterOpenGenericMethod,
				CallNullSafePropertyGetterByReferenceOpenGenericMethod);
		}

		private static Func<object, object> MakeFastPropertyGetter(
			PropertyInfo propertyInfo,
			MethodInfo propertyGetterWrapperMethod,
			MethodInfo propertyGetterByRefWrapperMethod)
		{
			Debug.Assert(propertyInfo != null);

			// Must be a generic method with a Func<,> parameter
			Debug.Assert(propertyGetterWrapperMethod != null);
			Debug.Assert(propertyGetterWrapperMethod.IsGenericMethodDefinition);
			Debug.Assert(propertyGetterWrapperMethod.GetParameters().Length == 2);

			// Must be a generic method with a ByRefFunc<,> parameter
			Debug.Assert(propertyGetterByRefWrapperMethod != null);
			Debug.Assert(propertyGetterByRefWrapperMethod.IsGenericMethodDefinition);
			Debug.Assert(propertyGetterByRefWrapperMethod.GetParameters().Length == 2);

			var getMethod = propertyInfo.GetMethod;
			Debug.Assert(getMethod != null);
			Debug.Assert(!getMethod.IsStatic);
			Debug.Assert(getMethod.GetParameters().Length == 0);

			// Instance methods in the CLR can be turned into static methods where the first parameter
			// is open over "target". This parameter is always passed by reference, so we have a code
			// path for value types and a code path for reference types.
			if (getMethod.DeclaringType.GetTypeInfo().IsValueType)
			{
				// Create a delegate (ref TDeclaringType) -> TValue
				return MakeFastPropertyGetter(
					typeof(ByRefFunc<,>),
					getMethod,
					propertyGetterByRefWrapperMethod);
			}
			else
			{
				// Create a delegate TDeclaringType -> TValue
				return MakeFastPropertyGetter(
					typeof(Func<,>),
					getMethod,
					propertyGetterWrapperMethod);
			}
		}

		private static Func<object, object> MakeFastPropertyGetter(
			Type openGenericDelegateType,
			MethodInfo propertyGetMethod,
			MethodInfo openGenericWrapperMethod)
		{
			var typeInput = propertyGetMethod.DeclaringType;
			var typeOutput = propertyGetMethod.ReturnType;

			var delegateType = openGenericDelegateType.MakeGenericType(typeInput, typeOutput);
			var propertyGetterDelegate = propertyGetMethod.CreateDelegate(delegateType);

			var wrapperDelegateMethod = openGenericWrapperMethod.MakeGenericMethod(typeInput, typeOutput);
			var accessorDelegate = wrapperDelegateMethod.CreateDelegate(
				typeof(Func<object, object>),
				propertyGetterDelegate);

			return (Func<object, object>)accessorDelegate;
		}

		/// <summary>
		/// Creates a single fast property setter for reference types. The result is not cached.
		/// </summary>
		/// <param name="propertyInfo">propertyInfo to extract the setter for.</param>
		/// <returns>a fast getter.</returns>
		/// <remarks>
		/// This method is more memory efficient than a dynamically compiled lambda, and about the
		/// same speed. This only works for reference types.
		/// </remarks>
		public static Action<object, object> MakeFastPropertySetter(PropertyInfo propertyInfo)
		{
			Debug.Assert(propertyInfo != null);
			Debug.Assert(!propertyInfo.DeclaringType.GetTypeInfo().IsValueType);

			var setMethod = propertyInfo.SetMethod;
			Debug.Assert(setMethod != null);
			Debug.Assert(!setMethod.IsStatic);
			Debug.Assert(setMethod.ReturnType == typeof(void));
			var parameters = setMethod.GetParameters();
			Debug.Assert(parameters.Length == 1);

			// Instance methods in the CLR can be turned into static methods where the first parameter
			// is open over "target". This parameter is always passed by reference, so we have a code
			// path for value types and a code path for reference types.
			var typeInput = setMethod.DeclaringType;
			var parameterType = parameters[0].ParameterType;

			// Create a delegate TDeclaringType -> { TDeclaringType.Property = TValue; }
			var propertySetterAsAction =
				setMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(typeInput, parameterType));
			var callPropertySetterClosedGenericMethod =
				CallPropertySetterOpenGenericMethod.MakeGenericMethod(typeInput, parameterType);
			var callPropertySetterDelegate =
				callPropertySetterClosedGenericMethod.CreateDelegate(
					typeof(Action<object, object>), propertySetterAsAction);

			return (Action<object, object>)callPropertySetterDelegate;
		}

		///  <summary>
		///  Given an object, adds each instance property with a public get method as a key and its
		///  associated value to a dictionary.
		/// 
		///  If the object is already an <see>
		///          <cref>IDictionary{string, object}</cref>
		///      </see>
		///      instance, then a copy
		///  is returned.
		///  </summary>
		///  <param name="keySelector">Key selector</param>
		///  <param name="deep">When true, converts all nested objects to dictionaries also</param>
		///  <remarks>
		///  The implementation of FastProperty will cache the property accessors per-type. This is
		///  faster when the the same type is used multiple times with ObjectToDictionary.
		///  </remarks>
		public static IDictionary<string, object> ObjectToDictionary(object value, Func<string, string> keySelector = null, bool deep = false)
		{
			if (value is IDictionary<string, object> dictionary)
			{
				return new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
			}

			dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			if (value != null)
			{
				keySelector = keySelector ?? new Func<string, string>(key => key);

				foreach (var prop in GetProperties(value).Values)
				{
					var propValue = prop.GetValue(value);
					if (deep && propValue != null && prop.Property.PropertyType.IsPlainObjectType())
					{
						propValue = ObjectToDictionary(propValue, deep: true);
					}

					dictionary[keySelector(prop.Name)] = propValue;
				}
			}

			return dictionary;
		}

		private static FastProperty CreateInstance(PropertyInfo property)
		{
			return new FastProperty(property);
		}

		// Called via reflection
		private static object CallPropertyGetter<TDeclaringType, TValue>(
			Func<TDeclaringType, TValue> getter,
			object target)
		{
			return getter((TDeclaringType)target);
		}

		// Called via reflection
		private static object CallPropertyGetterByReference<TDeclaringType, TValue>(
			ByRefFunc<TDeclaringType, TValue> getter,
			object target)
		{
			var unboxed = (TDeclaringType)target;
			return getter(ref unboxed);
		}

		// Called via reflection
		private static object CallNullSafePropertyGetter<TDeclaringType, TValue>(
			Func<TDeclaringType, TValue> getter,
			object target)
		{
			if (target == null)
			{
				return null;
			}

			return getter((TDeclaringType)target);
		}

		// Called via reflection
		private static object CallNullSafePropertyGetterByReference<TDeclaringType, TValue>(
			ByRefFunc<TDeclaringType, TValue> getter,
			object target)
		{
			if (target == null)
			{
				return null;
			}

			var unboxed = (TDeclaringType)target;
			return getter(ref unboxed);
		}

		private static void CallPropertySetter<TDeclaringType, TValue>(
			Action<TDeclaringType, TValue> setter,
			object target,
			object value)
		{
			setter((TDeclaringType)target, (TValue)value);
		}

		protected static IDictionary<string, FastProperty> GetVisibleProperties(
			Type type,
			Func<PropertyInfo, FastProperty> createPropertyHelper,
			ConcurrentDictionary<Type, IDictionary<string, FastProperty>> allPropertiesCache,
			ConcurrentDictionary<Type, IDictionary<string, FastProperty>> visiblePropertiesCache)
		{
			if (visiblePropertiesCache.TryGetValue(type, out var result))
			{
				return result;
			}

			// The simple and common case, this is normal POCO object - no need to allocate.
			var allPropertiesDefinedOnType = true;
			var allProperties = GetProperties(type, createPropertyHelper, allPropertiesCache);
			foreach (var propertyHelper in allProperties.Values)
			{
				if (propertyHelper.Property.DeclaringType != type)
				{
					allPropertiesDefinedOnType = false;
					break;
				}
			}

			if (allPropertiesDefinedOnType)
			{
				result = allProperties;
				visiblePropertiesCache.TryAdd(type, result);
				return result;
			}

			// There's some inherited properties here, so we need to check for hiding via 'new'.
			var filteredProperties = new List<FastProperty>(allProperties.Count);
			foreach (var propertyHelper in allProperties.Values)
			{
				var declaringType = propertyHelper.Property.DeclaringType;
				if (declaringType == type)
				{
					filteredProperties.Add(propertyHelper);
					continue;
				}

				// If this property was declared on a base type then look for the definition closest to the
				// the type to see if we should include it.
				var ignoreProperty = false;

				// Walk up the hierarchy until we find the type that actally declares this
				// PropertyInfo.
				var currentTypeInfo = type.GetTypeInfo();
				var declaringTypeInfo = declaringType.GetTypeInfo();
				while (currentTypeInfo != null && currentTypeInfo != declaringTypeInfo)
				{
					// We've found a 'more proximal' public definition
					var declaredProperty = currentTypeInfo.GetDeclaredProperty(propertyHelper.Name);
					if (declaredProperty != null)
					{
						ignoreProperty = true;
						break;
					}

					if (currentTypeInfo.BaseType != null)
					{
						currentTypeInfo = currentTypeInfo.BaseType.GetTypeInfo();
					}

				}

				if (!ignoreProperty)
				{
					filteredProperties.Add(propertyHelper);
				}
			}

			result = filteredProperties.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
			visiblePropertiesCache.TryAdd(type, result);
			return result;
		}

		protected static IDictionary<string, FastProperty> GetProperties(
			Type type,
			Func<PropertyInfo, FastProperty> createPropertyHelper,
			ConcurrentDictionary<Type, IDictionary<string, FastProperty>> cache)
		{
			// Unwrap nullable types. This means Nullable<T>.Value and Nullable<T>.HasValue will not be
			// part of the sequence of properties returned by this method.
			type = Nullable.GetUnderlyingType(type) ?? type;

			return cache.GetOrAdd(type, Get);

			IDictionary<string, FastProperty> Get(Type t)
			{
				var candidates = GetCandidateProperties(t);
				var fastProperties = candidates.Select(p => createPropertyHelper(p)).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
				return fastProperties;
			}
		}

		internal static IEnumerable<PropertyInfo> GetCandidateProperties(Type type)
		{
			// We avoid loading indexed properties using the Where statement.
			var properties = type.GetRuntimeProperties().Where(IsCandidateProperty);

			var typeInfo = type.GetTypeInfo();
			if (typeInfo.IsInterface)
			{
				// Reflection does not return information about inherited properties on the interface itself.
				properties = properties.Concat(typeInfo.ImplementedInterfaces.SelectMany(
					interfaceType => interfaceType.GetRuntimeProperties().Where(IsCandidateProperty)));
			}

			return properties;
		}

		// Indexed properties are not useful (or valid) for grabbing properties off an object.
		private static bool IsCandidateProperty(PropertyInfo property)
		{
			return property.GetIndexParameters().Length == 0 &&
				property.GetMethod != null &&
				property.GetMethod.IsPublic &&
				!property.GetMethod.IsStatic;
		}

		private static ConcurrentDictionary<Type, IDictionary<string, FastProperty>> CreateVolatileCache()
		{
			return new ConcurrentDictionary<Type, IDictionary<string, FastProperty>>();
		}

		class PropertyKey : Tuple<Type, string>
		{
			public PropertyKey(Type type, string propertyName)
				: base(type, propertyName)
			{
			}
			public Type Type { get { return base.Item1; } }
			public string PropertyName { get { return base.Item2; } }
		}
	}
}