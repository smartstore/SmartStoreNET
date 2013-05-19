using System;
using System.Collections.Generic;
using System.Text;

namespace SmartStore
{
    
    public static class ListExtensions
    {

        public static string ToSeparatedString<T>(this IList<T> value)
        {
            return ToSeparatedString(value, ",");
        }

        public static string ToSeparatedString<T>(this IList<T> value, string separator)
        {
            if (value.Count == 0)
            {
                return String.Empty;
            }
            if (value.Count == 1)
            {
                if (value[0] != null)
                {
                    return value[0].ToString();
                }
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            bool flag = true;
            bool flag2 = false;
            foreach (object obj2 in value)
            {
                if (!flag)
                {
                    builder.Append(separator);
                }
                if (obj2 != null)
                {
                    builder.Append(obj2.ToString().TrimEnd(new char[0]));
                    flag2 = true;
                }
                flag = false;
            }
            if (!flag2)
            {
                return string.Empty;
            }
            return builder.ToString();
        }

        /// <summary>
        /// Makes a slice of the specified list in between the start and end indexes.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="start">The start index.</param>
        /// <param name="end">The end index.</param>
        /// <returns>A slice of the list.</returns>
        public static IList<T> Slice<T>(this IList<T> list, int? start, int? end)
        {
            return list.Slice(start, end, null);
        }

        /// <summary>
        /// Makes a slice of the specified list in between the start and end indexes,
        /// getting every so many items based upon the step.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="start">The start index.</param>
        /// <param name="end">The end index.</param>
        /// <param name="step">The step.</param>
        /// <returns>A slice of the list.</returns>
        public static IList<T> Slice<T>(this IList<T> list, int? start, int? end, int? step)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (step == 0)
                throw Error.Argument("step", "Step cannot be zero.");

            List<T> slicedList = new List<T>();

            // nothing to slice
            if (list.Count == 0)
                return slicedList;

            // set defaults for null arguments
            int s = step ?? 1;
            int startIndex = start ?? 0;
            int endIndex = end ?? list.Count;

            // start from the end of the list if start is negative
            startIndex = (startIndex < 0) ? list.Count + startIndex : startIndex;

            // end from the start of the list if end is negative
            endIndex = (endIndex < 0) ? list.Count + endIndex : endIndex;

            // ensure indexes keep within collection bounds
            startIndex = Math.Max(startIndex, 0);
            endIndex = Math.Min(endIndex, list.Count - 1);

            // loop between start and end indexes, incrementing by the step
            for (int i = startIndex; i < endIndex; i += s)
            {
                slicedList.Add(list[i]);
            }

            return slicedList;
        }

    }

}
