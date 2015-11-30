using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Globalization;
using System.IO;
using Fasterflect;

namespace SmartStore.Utilities
{
    public static class TypeHelper
    {
        public delegate T Creator<T>();

        public static bool IsUnitializedValue(object value)
        {
            if (value == null)
            {
                return true;
            }
            else
            {
                object unitializedValue = CreateUnitializedValue(value.GetType());
                return value.Equals(unitializedValue);
            }
        }

        public static object CreateUnitializedValue(Type type)
        {
            Guard.ArgumentNotNull(type, "type");

            if (type.IsGenericTypeDefinition)
                throw new ArgumentException("Type {0} is a generic type definition and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, type), "type");

            if (type.IsClass || type.IsInterface || type == typeof(void))
                return null;
            else if (type.IsValueType)
                return Activator.CreateInstance(type);
            else
                throw new ArgumentException("Type {0} cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, type), "type");
        }

        public static Type GetElementType(Type type)
        {
            if (!type.IsPredefinedSimpleType())
            {
                if (type.HasElementType)
                {
                    return GetElementType(type.GetElementType());
                }
                if (type.IsPredefinedGenericType())
                {
                    return GetElementType(type.GetGenericArguments()[0]);
                }
                Type type2 = type.FindIEnumerable();
                if (type2 != null)
                {
                    Type type3 = type2.GetGenericArguments()[0];
                    return GetElementType(type3);
                }
            }
            return type;
        }

        public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type keyType, out Type valueType)
        {
            Guard.ArgumentNotNull(dictionaryType, "type");

            Type genericDictionaryType;
            if (dictionaryType.IsSubClass(typeof(IDictionary<,>), out genericDictionaryType))
            {
                if (genericDictionaryType.IsGenericTypeDefinition)
                    throw new Exception("Type {0} is not a dictionary.".FormatInvariant(dictionaryType));

                Type[] dictionaryGenericArguments = genericDictionaryType.GetGenericArguments();

                keyType = dictionaryGenericArguments[0];
                valueType = dictionaryGenericArguments[1];
                return;
            }
            else if (typeof(IDictionary).IsAssignableFrom(dictionaryType))
            {
                keyType = null;
                valueType = null;
                return;
            }
            else
            {
                throw new Exception("Type {0} is not a dictionary.".FormatInvariant(dictionaryType));
            }
        }

        public static Type GetDictionaryValueType(Type dictionaryType)
        {
            Type keyType;
            Type valueType;
            GetDictionaryKeyValueTypes(dictionaryType, out keyType, out valueType);

            return valueType;
        }

        public static Type GetDictionaryKeyType(Type dictionaryType)
        {
            Type keyType;
            Type valueType;
            GetDictionaryKeyValueTypes(dictionaryType, out keyType, out valueType);

            return keyType;
        }

        /// <summary>
        /// Tests whether the list's items are their unitialized value.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>Whether the list's items are their unitialized value</returns>
        public static bool ItemsUnitializedValue<T>(IList<T> list)
        {
            Guard.ArgumentNotNull(list, "list");

            Type elementType = GetElementType(list.GetType()); // GetListItemType(list.GetType());

            if (elementType.IsValueType)
            {
                object unitializedValue = CreateUnitializedValue(elementType);

                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].Equals(unitializedValue))
                        return false;
                }
            }
            else if (elementType.IsClass)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    object value = list[i];

                    if (value != null)
                        return false;
                }
            }
            else
            {
                throw new Exception("Type {0} is neither a ValueType or a Class.".FormatWith(CultureInfo.InvariantCulture, elementType));
            }

            return true;
        }

        /// <summary>
        /// Gets the member's underlying type.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The underlying type of the member.</returns>
        public static Type GetMemberUnderlyingType(MemberInfo member)
        {
            Guard.ArgumentNotNull(member, "member");

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }

        public static IList<string> GetPropertyNames(object target)
        {
            return target.GetType().GetProperties().Select(z => z.Name).ToList();
        }

        public static IEnumerable<MemberInfo> GetFieldsAndProperties<T>(BindingFlags bindingAttr)
        {
            return typeof(T).GetFieldsAndProperties(bindingAttr);
        }

        public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider) where T : Attribute
        {
            return GetAttribute<T>(attributeProvider, true);
        }

        public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
        {
            T[] attributes = GetAttributes<T>(attributeProvider, inherit);
            return attributes.FirstOrDefault<T>();
        }

        public static T[] GetAttributes<T>(ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
        {
            Guard.ArgumentNotNull(attributeProvider, "attributeProvider");

            return (T[])attributeProvider.GetCustomAttributes(typeof(T), inherit);
        }

        /// <summary>
        /// Gets the value of a property through reflection.
        /// </summary>
        /// <param name="from">The <see cref="object"/> to get the value from.</param>
        /// <param name="propertyName">The name of the property to extract the value for.</param>
        /// <returns>The value of the property.</returns>
        public static object GetPropertyValue(object from, string propertyName)
        {
            Guard.ArgumentNotNull(from, "value");
            var propertyInfo = from.GetType().GetProperty(propertyName,
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            return propertyInfo.GetValue(from, null);
        }

        public static bool TryAction<T>(Creator<T> creator, out T output)
        {
            Guard.ArgumentNotNull(creator, "creator");

            try
            {
                output = creator();
                return true;
            }
            catch
            {
                output = default(T);
                return false;
            }
        }

        public static bool TryGetDescription(object value, out string description)
        {
            return TryAction<string>(delegate { return GetDescription(value); }, out description);
        }

        public static string GetDescription(object o)
        {
            Guard.ArgumentNotNull(o, "o");

            ICustomAttributeProvider attributeProvider = o as ICustomAttributeProvider;

            // object passed in isn't an attribute provider
            // if value is an enum value, get value field member, otherwise get values type
            if (attributeProvider == null)
            {
                Type valueType = o.GetType();

                if (valueType.IsEnum)
                    attributeProvider = valueType.GetField(o.ToString());
                else
                    attributeProvider = valueType;
            }

            DescriptionAttribute descriptionAttribute = TypeHelper.GetAttribute<DescriptionAttribute>(attributeProvider);

            if (descriptionAttribute != null)
                return descriptionAttribute.Description;
            else
                throw new Exception("No DescriptionAttribute on '{0}'.".FormatWith(CultureInfo.InvariantCulture, o.GetType()));
        }

        public static IList<string> GetDescriptions(IList values)
        {
            Guard.ArgumentNotNull(values, "values");

            string[] descriptions = new string[values.Count];

            for (int i = 0; i < values.Count; i++)
            {
                descriptions[i] = GetDescription(values[i]);
            }

            return descriptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">The type from which to infer namespace name.</param>
        /// <param name="resourceName">The relative name of the resource, without (default) namespace name.</param>
        /// <returns></returns>
        public static string GetStringFromEmbeddedResource(Type type, string resourceName)
        {
            Assembly assembly = type.Assembly;
            resourceName = String.Concat(type.Namespace, ".", resourceName);
            return GetStringFromEmbeddedResource(assembly, resourceName);
        }

        public static string GetStringFromEmbeddedResource(Assembly assembly, string resourceFullName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceFullName))
            {
                try
                {
                    return stream.AsString();
                    //using (StreamReader reader = new StreamReader(stream))
                    //{
                    //    return reader.ReadToEnd();
                    //}
                }
                catch (Exception e)
                {
                    throw new Exception("Error retrieving from Resources. Tried '"
                                             + resourceFullName + "'\r\n" + e.ToString());
                }
            }
        }

        public static Type GetSequenceType(Type elementType)
        {
            return typeof(IEnumerable<>).MakeGenericType(elementType);
        }

        public static Type GetMemberType(MemberInfo mi)
        {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null)
                return fi.FieldType;
            PropertyInfo pi = mi as PropertyInfo;
            if (pi != null)
                return pi.PropertyType;
            EventInfo ei = mi as EventInfo;
            if (ei != null)
                return ei.EventHandlerType;
            return null;
        }

        public static void RegisterTypeConverter<T, TC>() where TC : TypeConverter
        {
            TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(TC)));
        }

        public static T CreateInstance<T>(params object[] parameters) where T : class
        {
            return (T)typeof(T).CreateInstance(parameters);
        }

        public static object CreateInstance(Type type, params object[] parameters)
        {
            return type.CreateInstance(parameters);
        }


    }
}