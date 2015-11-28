using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using Fasterflect;

namespace SmartStore.Utilities
{

    [Serializable]
    public class ConvertProblem
    {
        public object Item { get; set; }
        public PropertyInfo Property { get; set; }
        public object AttemptedValue { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return
                @"Item type:     {0}
                Property:        {1}
                Property Type:   {2}
                Attempted Value: {3}
                Exception:
                {4}."
                    .FormatCurrent(
                    ((Item != null) ? Item.GetType().FullName : "(null)"),
                    Property.Name,
                    Property.PropertyType,
                    AttemptedValue,
                    Exception);
        }
    }

    [Serializable]
    public class DictionaryConvertException : Exception
    {
        public DictionaryConvertException(ICollection<ConvertProblem> problems)
            : this(CreateMessage(problems), problems)
        {
        }

        public DictionaryConvertException(string message, ICollection<ConvertProblem> problems)
            : base(message)
        {
            Problems = problems;
        }

        public DictionaryConvertException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ICollection<ConvertProblem> Problems { get; private set; }

        private static string CreateMessage(ICollection<ConvertProblem> problems)
        {
            var counter = 0;
            var builder = new StringBuilder();
            builder.Append("Could not convert all input values into their expected types:");
            builder.Append(Environment.NewLine);
            foreach (var prob in problems)
            {
                builder.AppendFormat("-----Problem[{0}]---------------------", counter++);
                builder.Append(Environment.NewLine);
                builder.Append(prob);
                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
        }
    }

    public static class DictionaryConverter
    {

        public static bool CanCreateType(Type itemType)
        {
            return itemType.IsClass && itemType.GetConstructor(Type.EmptyTypes) != null;
        }

        public static T CreateAndPopulate<T>(IDictionary<string, object> source, out ICollection<ConvertProblem> problems)
            where T : class, new()
        {
            return (T)CreateAndPopulate(typeof(T), source, out problems);
        }

        public static object CreateAndPopulate(Type targetType, IDictionary<string, object> source, out ICollection<ConvertProblem> problems)
        {
            Guard.ArgumentNotNull(() => targetType);

            var target = targetType.CreateInstance(); //Activator.CreateInstance(targetType);

            Populate(source, target, out problems);

            return target;
        }

        public static object SafeCreateAndPopulate(Type targetType, IDictionary<string, object> source)
        {
            ICollection<ConvertProblem> problems;
            var item = CreateAndPopulate(targetType, source, out problems);

            if (problems.Count > 0)
                throw new DictionaryConvertException(problems);

            return item;
        }

        public static T SafeCreateAndPopulate<T>(IDictionary<string, object> source)
            where T : class, new()
        {
            return (T)SafeCreateAndPopulate(typeof(T), source);
        }

        public static void Populate(IDictionary<string, object> source, object target, params object[] populated)
        {
            ICollection<ConvertProblem> problems;

            Populate(source, target, out problems, populated);

            if (problems.Count > 0)
                throw new DictionaryConvertException(problems);
        }

        public static void Populate(IDictionary<string, object> source, object target, out ICollection<ConvertProblem> problems, params object[] populated)
        {
            Guard.ArgumentNotNull(() => source);
            Guard.ArgumentNotNull(() => target);

            problems = new List<ConvertProblem>();

            if (populated.Any(x => x == target))
                return;

            Type t = target.GetType();

            if (source != null)
            {
                // TODO: Metadaten aus einem TypeCache ziehen zwecks Performance!
                foreach (var pi in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {

                    object value;

                    if (!pi.PropertyType.IsPredefinedSimpleType() && source.TryGetValue(pi.Name, out value) && value is IDictionary<string, object>)
                    {
                        var nestedValue = target.GetPropertyValue(pi.Name);
                        ICollection<ConvertProblem> nestedProblems;

                        populated = populated.Concat(new object[] { target }).ToArray();
                        Populate((IDictionary<string, object>)value, nestedValue, out nestedProblems, populated);

                        if (nestedProblems != null && nestedProblems.Any())
                        {
                            problems.AddRange(nestedProblems);
                        }
                        WriteToProperty(target, pi, nestedValue, problems);
                    }
                    else if (pi.PropertyType.IsArray && !source.ContainsKey(pi.Name))
                    {
                        WriteToProperty(target, pi, RetrieveArrayValues(pi, source, problems), problems);
                    }
                    else
                    {
                        if (source.TryGetValue(pi.Name, out value))
                        {
                            WriteToProperty(target, pi, value, problems);
                        }
                    }
                }
            }
        }

        // REVIEW: Dieser Code ist redundant mit DefaultModelBinder u.Ä. Entweder ablösen oder eliminieren (vielleicht ist es ja in diesem Kontext notwendig!??!)
        private static object RetrieveArrayValues(PropertyInfo arrayProp, IDictionary<string, object> source, ICollection<ConvertProblem> problems)
        {
            Type elemType = arrayProp.PropertyType.GetElementType();
            bool anyValuesFound = true;
            int idx = 0;
            //var elements = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType));
            var elements = (IList)typeof(List<>).MakeGenericType(elemType).CreateInstance();

            // TODO: Metadaten aus einem TypeCache ziehen zwecks Performance!
            var properties = elemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            while (anyValuesFound)
            {
                object curElement = null;
                anyValuesFound = false; // false until proven otherwise

                foreach (var pi in properties)
                {
                    //var key = string.Format("_{0}{1}_{2}", idx, arrayProp.Name, pd.Name);
                    var key = string.Format("{0}[{1}].{2}", arrayProp.Name, idx, pi.Name);
                    object value;

                    if (source.TryGetValue(key, out value))
                    {
                        anyValuesFound = true;

                        if (curElement == null)
                        {
                            curElement = elemType.CreateInstance();
                            elements.Add(curElement);
                        }

                        SetPropFromValue(value, curElement, pi, problems);
                    }
                }

                idx++;
            }

            var elementArray = Array.CreateInstance(elemType, elements.Count);
            elements.CopyTo(elementArray, 0);

            return elementArray;
        }


        private static void SetPropFromValue(object value, object item, PropertyInfo pi, ICollection<ConvertProblem> problems)
        {
            WriteToProperty(item, pi, value, problems);
        }

        private static void WriteToProperty(object item, PropertyInfo pi, object value, ICollection<ConvertProblem> problems)
        {
            if (!pi.CanWrite)
                return;

            try
            {
                if (value != null && !Equals(value, ""))
                {
                    Type destType = pi.PropertyType;

                    if (destType == typeof(bool) && Equals(value, pi.Name))
                    {
                        value = true;
                    }

                    if (pi.PropertyType.IsAssignableFrom(value.GetType()))
                    {
                        //pi.SetValue(item, value);
                        item.SetPropertyValue(pi.Name, value);
                        return;
                    }

                    if (pi.PropertyType.IsNullable())
                    {
                        destType = pi.PropertyType.GetGenericArguments()[0];
                    }

                    //pi.SetValue(item, value.Convert(destType));
                    item.SetPropertyValue(pi.Name, value.Convert(destType));
                }
            }
            catch (Exception ex)
            {
                problems.Add(new ConvertProblem { Item = item, Property = pi, AttemptedValue = value, Exception = ex });
            }
        }

    }

}
