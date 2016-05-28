using System;
using System.Linq;
using System.Reflection;

namespace SmartStore
{

    public static class EnumExtensions
    {

        /// <summary>
        /// Checks if the specified enum flag is set on a flagged enumeration type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool IsSet<T>(this T value, T flags) where T : struct
        {
            Type type = typeof(T);

            // only works with enums   
            if (!type.IsEnum)
                throw Error.Argument("T", "The type parameter T must be an enum type.");

            // handle each underlying type   
            Type numberType = Enum.GetUnderlyingType(type);

            if (numberType.Equals(typeof(int)))
            {
                return BoxUnbox<int>(value, flags, (a, b) => (a & b) == b);
            }
            else if (numberType.Equals(typeof(sbyte)))
            {
                return BoxUnbox<sbyte>(value, flags, (a, b) => (a & b) == b);
            }
            else if (numberType.Equals(typeof(byte)))
            {
                return BoxUnbox<byte>(value, flags, (a, b) => (a & b) == b);
            }
            else if (numberType.Equals(typeof(short)))
            {
                return BoxUnbox<short>(value, flags, (a, b) => (a & b) == b);
            }
            else if (numberType.Equals(typeof(ushort)))
            {
                return BoxUnbox<ushort>(value, flags, (a, b) => (a & b) == b);
            }
            else if (numberType.Equals(typeof(uint)))
            {
                return BoxUnbox<uint>(value, flags, (a, b) => (a & b) == b);
            }
            else if (numberType.Equals(typeof(long)))
            {
                return BoxUnbox<long>(value, flags, (a, b) => (a & b) == b);
            }
            else if (numberType.Equals(typeof(ulong)))
            {
                return BoxUnbox<ulong>(value, flags, (a, b) => (a & b) == b);
            }
            else if (numberType.Equals(typeof(char)))
            {
                return BoxUnbox<char>(value, flags, (a, b) => (a & b) == b);
            }
            else
            {
                throw new ArgumentException("Unknown enum underlying type " +
                    numberType.Name + ".");
            }
        }

		/// <summary>   
		/// Helper function for handling the value types. Boxes the params to   
		/// object so that the cast can be called on them.   
		/// </summary>   
		private static bool BoxUnbox<T>(object value, object flags, Func<T, T, bool> op)
        {
            return op((T)value, (T)flags);
        }

    }

}
