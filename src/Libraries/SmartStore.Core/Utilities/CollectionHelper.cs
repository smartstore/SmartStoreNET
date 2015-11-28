using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Collections.Specialized;
using System.ComponentModel;

using Fasterflect;

namespace SmartStore.Utilities
{
    public static class CollectionHelper
    {

        /// <summary>
        /// Determines whether the collection is null, empty or its contents are uninitialized values.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>
        /// 	<c>true</c> if the collection is null or empty or its contents are uninitialized values; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmptyOrDefault<T>(IList<T> value)
        {
            if (value.IsNullOrEmpty())
                return true;

            return TypeHelper.ItemsUnitializedValue<T>(value);
        }


        public static List<List<T>> Flatten<T>(params IList<T>[] lists)
        {
            List<List<T>> flattened = new List<List<T>>();
            Dictionary<int, T> currentList = new Dictionary<int, T>();

            Recurse<T>(new List<IList<T>>(lists), 0, currentList, flattened);

            return flattened;
        }

        private static void Recurse<T>(IList<IList<T>> global, int current, Dictionary<int, T> currentSet, List<List<T>> flattenedResult)
        {
            IList<T> currentArray = global[current];

            for (int i = 0; i < currentArray.Count; i++)
            {
                currentSet[current] = currentArray[i];

                if (current == global.Count - 1)
                {
                    List<T> items = new List<T>();

                    for (int k = 0; k < currentSet.Count; k++)
                    {
                        items.Add(currentSet[k]);
                    }

                    flattenedResult.Add(items);
                }
                else
                {
                    Recurse(global, current + 1, currentSet, flattenedResult);
                }
            }
        }

        public static IList<T> Minus<T>(IList<T> list, IList<T> minus)
        {
            Guard.ArgumentNotNull(list, "list");

            List<T> result = new List<T>(list.Count);
            foreach (T t in list)
            {
                if (minus == null || !minus.Contains(t))
                    result.Add(t);
            }

            return result;
        }

        public static T[] CreateArray<T>(IEnumerable<T> enumerable)
        {
            Guard.ArgumentNotNull(enumerable, "enumerable");

            if (enumerable is T[])
                return (T[])enumerable;

            return enumerable.ToArray();
        }


        public static IList CreateAndPopulateList(Type listType, Action<IList> populateList)
        {
            Guard.ArgumentNotNull(listType, "listType");
            Guard.ArgumentNotNull(populateList, "populateList");

            IList list;
            Type collectionType;
            bool isReadOnlyOrFixedSize = false;

            if (listType.IsArray)
            {
                // have to use an arraylist when creating array
                // there is no way to know the size until it is finised
                list = new ArrayList();
                isReadOnlyOrFixedSize = true;
            }
            else if (listType.IsSubClass(typeof(ReadOnlyCollection<>), out collectionType))
            {
                Type readOnlyCollectionContentsType = collectionType.GetGenericArguments()[0];
                Type genericEnumerable = typeof(IEnumerable<>).MakeGenericType(readOnlyCollectionContentsType);
                bool suitableConstructor = false;

                foreach (ConstructorInfo constructor in listType.GetConstructors())
                {
                    IList<ParameterInfo> parameters = constructor.GetParameters();

                    if (parameters.Count == 1)
                    {
                        if (genericEnumerable.IsAssignableFrom(parameters[0].ParameterType))
                        {
                            suitableConstructor = true;
                            break;
                        }
                    }
                }

                if (!suitableConstructor)
                    throw new Exception("Read-only type {0} does not have a public constructor that takes a type that implements {1}.".FormatWith(CultureInfo.InvariantCulture, listType, genericEnumerable));

                // can't add or modify a readonly list
                // use List<T> and convert once populated
                list = (IList)readOnlyCollectionContentsType.CreateGenericList();
                isReadOnlyOrFixedSize = true;
            }
            else if (typeof(IList).IsAssignableFrom(listType))
            {
                if (listType.IsConstructable())
                    list = (IList)Activator.CreateInstance(listType);
                else if (listType == typeof(IList))
                    list = new List<object>();
                else
                    list = null;
            }
            else if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                list = TypeHelper.GetElementType(listType).CreateGenericList(); //TypeHelper.GetListItemType(listType).CreateGenericList();
            }
            else
            {
                list = null;
            }

            if (list == null)
                throw new Exception("Cannot create and populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, listType));

            populateList(list);

            // create readonly and fixed sized collections using the temporary list
            if (isReadOnlyOrFixedSize)
            {
                if (listType.IsArray)
                    list = ((ArrayList)list).ToArray(TypeHelper.GetElementType(listType));
                else if (listType.IsSubClass(typeof(ReadOnlyCollection<>)))
                    list = (IList)Activator.CreateInstance(listType, list);
            }

            return list;
        }

		public static string ToUnsortedHtmlList(ICollection<string> data)
        {
            return ToUnsortedHtmlList(data, null, null);
        }

		public static string ToUnsortedHtmlList(ICollection<string> data, string ulClassName)
        {
            return ToUnsortedHtmlList(data, ulClassName, null);
        }

        public static string ToUnsortedHtmlList(ICollection<string> data, string ulClassName, string liClassName)
        {
            if (data.IsNullOrEmpty())
                return String.Empty;

            StringBuilder ul = new StringBuilder();
            if (!String.IsNullOrEmpty(ulClassName))
            {
                ul.Append(string.Format("<ul class=\"{0}\">\n", ulClassName));
            }
            else
            {
                ul.Append("<ul>\n");
            }

            foreach (var m in data)
            {
                if (!String.IsNullOrEmpty(liClassName))
                {
                    ul.Append(string.Format("<li class=\"{0}\">", liClassName));
                }
                else
                {
                    ul.Append("<li>");
                }
                ul.Append(m).AppendLine("</li>");
            }

            return ul.AppendLine("</ul>").ToString();
            ;

        }

        public static IDictionary<string, object> ObjectToDictionary(object obj)
        {
            Guard.ArgumentNotNull(() => obj);

            Type t = obj.GetType();

            return t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                   .ToDictionary(k => k.Name.Replace("_", "-"),
                                 v => obj.GetPropertyValue(v.Name),
                                 StringComparer.OrdinalIgnoreCase);
        }

        public static NameValueCollection ObjectToNameValueCollection(object obj)
        {
            var result = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                object value = descriptor.GetValue(obj);
                result.Add(descriptor.Name, value.ToString());
            }

            return result;
        }

        /// <summary>
        /// Converts the <see cref="Enum" /> type to an <see cref="IList" /> 
        /// compatible object.
        /// </summary>
        /// <param name="type">The <see cref="Enum"/> type.</param>
        /// <returns>An <see cref="IList"/> containing the enumerated
        /// type value and description.</returns>
        public static IDictionary<object, string> EnumToDictionary(Type type)
        {
            Guard.ArgumentIsEnumType(type, "type");

            IDictionary<object, string> dict = new Dictionary<object, string>();
            Array enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                object key = Convert.ChangeType(value, Enum.GetUnderlyingType(type));
                dict[key] = value.GetDescription();
            }

            return dict;
        }

    }
}