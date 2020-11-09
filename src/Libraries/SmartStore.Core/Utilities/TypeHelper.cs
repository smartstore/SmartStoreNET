using System;
using System.Linq.Expressions;

namespace SmartStore.Utilities
{
    public static class TypeHelper
    {
        public delegate T Creator<T>();

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

        /// <summary>
        /// Exctracts and returns the name of a property accessor lambda
        /// </summary>
        /// <typeparam name="T">The containing type</typeparam>
        /// <param name="propertyAccessor">The accessor lambda</param>
        /// <param name="includeTypeName">When <c>true</c>, returns the result as '[TyoeName].[PropertyName]'.</param>
        /// <returns>The property name</returns>
        public static string NameOf<T>(Expression<Func<T, object>> propertyAccessor, bool includeTypeName = false)
        {
            Guard.NotNull(propertyAccessor, nameof(propertyAccessor));

            var name = propertyAccessor.ExtractPropertyInfo().Name;

            if (includeTypeName)
            {
                return typeof(T).Name + "." + name;
            }

            return name;
        }
    }
}