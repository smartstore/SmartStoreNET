using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SmartStore
{
    public class Guard
    {
        const string AgainstMessage = "Assertion evaluation failed with 'false'.";
        const string ImplementsMessage = "Type '{0}' must implement type '{1}'.";
        const string InheritsFromMessage = "Type '{0}' must inherit from type '{1}'.";
        const string IsTypeOfMessage = "Type '{0}' must be of type '{1}'.";
        const string IsEqualMessage = "Compared objects must be equal.";
        const string IsPositiveMessage = "Argument '{0}' must be a positive value. Value: '{1}'.";
        const string IsTrueMessage = "True expected for '{0}' but the condition was False.";
        const string NotNegativeMessage = "Argument '{0}' cannot be a negative value. Value: '{1}'.";
        const string NotEmptyStringMessage = "String parameter '{0}' cannot be null or all whitespace.";
        const string NotEmptyColMessage = "Collection cannot be null and must contain at least one item.";
        const string NotEmptyGuidMessage = "Argument '{0}' cannot be an empty guid.";
        const string InRangeMessage = "The argument '{0}' must be between '{1}' and '{2}'.";
        const string NotOutOfLengthMessage = "Argument '{0}' cannot be more than {1} characters long.";
        const string NotZeroMessage = "Argument '{0}' must be greater or less than zero. Value: '{1}'.";
        const string IsEnumTypeMessage = "Type '{0}' must be a valid Enum type.";
        const string IsEnumTypeMessage2 = "The value of the argument '{0}' provided for the enumeration '{1}' is invalid.";
        const string IsSubclassOfMessage = "Type '{0}' must be a subclass of type '{1}'.";
        const string HasDefaultConstructorMessage = "The type '{0}' must have a default parameterless constructor.";

        private Guard()
        {
        }

        #region 3.0

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull(object arg, string argName)
        {
            if (arg == null)
                throw new ArgumentNullException(argName);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty(string arg, string argName)
        {
            if (string.IsNullOrWhiteSpace(arg))
                throw Error.Argument(argName, NotEmptyStringMessage, argName);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty<T>(ICollection<T> arg, string argName)
        {
            if (arg == null || !arg.Any())
                throw Error.Argument(argName, NotEmptyColMessage);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty(Guid arg, string argName)
        {
            if (arg == Guid.Empty)
                throw Error.Argument(argName, NotEmptyGuidMessage, argName);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InRange<T>(T arg, T min, T max, string argName) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(min) < 0 || arg.CompareTo(max) > 0)
                throw Error.ArgumentOutOfRange(argName, InRangeMessage, argName, min, max);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotOutOfLength(string arg, int maxLength, string argName)
        {
            if (arg.Trim().Length > maxLength)
            {
                throw Error.Argument(argName, NotOutOfLengthMessage, argName, maxLength);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNegative<T>(T arg, string argName, string message = NotNegativeMessage) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default(T)) < 0)
                throw Error.ArgumentOutOfRange(argName, message.FormatInvariant(argName, arg));
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotZero<T>(T arg, string argName) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default(T)) == 0)
                throw Error.ArgumentOutOfRange(argName, NotZeroMessage, argName, arg);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Against<TException>(bool assertion, string message = AgainstMessage) where TException : Exception
        {
            if (assertion)
                throw (TException)Activator.CreateInstance(typeof(TException), message);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Against<TException>(Func<bool> assertion, string message = AgainstMessage) where TException : Exception
        {
            //Execute the lambda and if it evaluates to true then throw the exception.
            if (assertion())
                throw (TException)Activator.CreateInstance(typeof(TException), message);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsPositive<T>(T arg, string argName, string message = IsPositiveMessage) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default(T)) < 1)
                throw Error.ArgumentOutOfRange(argName, message.FormatInvariant(argName));
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsTrue(bool arg, string argName, string message = IsTrueMessage)
        {
            if (!arg)
                throw Error.Argument(argName, message.FormatInvariant(argName));
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsEnumType(Type arg, string argName)
        {
            NotNull(arg, argName);

            if (!arg.IsEnum)
                throw Error.Argument(argName, IsEnumTypeMessage, arg.FullName);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsEnumType(Type enumType, object arg, string argName)
        {
            NotNull(arg, argName);

            if (!Enum.IsDefined(enumType, arg))
            {
                throw Error.ArgumentOutOfRange(argName, IsEnumTypeMessage2, argName, enumType.FullName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotDisposed(DisposableObject arg, string argName)
        {
            NotNull(arg, argName);

            if (arg.IsDisposed)
                throw Error.ObjectDisposed(argName);
        }

        [DebuggerStepThrough]
        public static void PagingArgsValid(int indexArg, int sizeArg, string indexArgName, string sizeArgName)
        {
            NotNegative<int>(indexArg, indexArgName, "PageIndex cannot be below 0");

            if (indexArg > 0)
            {
                // if pageIndex is specified (> 0), PageSize CANNOT be 0 
                IsPositive<int>(sizeArg, sizeArgName, "PageSize cannot be below 1 if a PageIndex greater 0 was provided.");
            }
            else
            {
                // pageIndex 0 actually means: take all!
                NotNegative(sizeArg, sizeArgName);
            }
        }

        #endregion

        [DebuggerStepThrough]
        [Obsolete("Use NotNull() with nameof operator instead")]
        public static void ArgumentNotNull(object arg, string argName)
        {
            if (arg == null)
                throw new ArgumentNullException(argName);
        }

        [DebuggerStepThrough]
        [Obsolete("Use NotNull() with nameof operator instead")]
        public static void ArgumentNotNull<T>(Func<T> arg)
        {
            if (arg() == null)
                throw new ArgumentNullException(GetParamName(arg));
        }

        [DebuggerStepThrough]
        [Obsolete("Use NotEmpty() with nameof operator instead")]
        public static void ArgumentNotEmpty(Func<string> arg)
        {
            if (arg().IsEmpty())
            {
                var argName = GetParamName(arg);
                throw Error.Argument(argName, "String parameter '{0}' cannot be null or all whitespace.", argName);
            }
        }

        [DebuggerStepThrough]
        [Obsolete("Use NotEmpty() with nameof operator instead")]
        public static void ArgumentNotEmpty(string arg, string argName)
        {
            if (arg.IsEmpty())
                throw Error.Argument(argName, "String parameter '{0}' cannot be null or all whitespace.", argName);
        }

        [DebuggerStepThrough]
        public static void InheritsFrom<TBase>(Type type)
        {
            InheritsFrom<TBase>(type, InheritsFromMessage.FormatInvariant(type.FullName, typeof(TBase).FullName));
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InheritsFrom<TBase>(Type type, string message)
        {
            if (type.BaseType != typeof(TBase))
                throw new InvalidOperationException(message);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Implements<TInterface>(Type type, string message = ImplementsMessage)
        {
            if (!typeof(TInterface).IsAssignableFrom(type))
                throw new InvalidOperationException(message.FormatInvariant(type.FullName, typeof(TInterface).FullName));
        }

        [DebuggerStepThrough]
        public static void IsSubclassOf<TBase>(Type type)
        {
            var baseType = typeof(TBase);
            if (!baseType.IsSubClass(type))
            {
                throw new InvalidOperationException(IsSubclassOfMessage.FormatInvariant(type.FullName, baseType.FullName));
            }
        }

        [DebuggerStepThrough]
        public static void IsTypeOf<TType>(object instance)
        {
            IsTypeOf<TType>(instance, IsTypeOfMessage.FormatInvariant(instance.GetType().Name, typeof(TType).FullName));
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsTypeOf<TType>(object instance, string message)
        {
            if (!(instance is TType))
                throw new InvalidOperationException(message);
        }

        [DebuggerStepThrough]
        public static void IsEqual<TException>(object compare, object instance, string message = IsEqualMessage) where TException : Exception
        {
            if (!compare.Equals(instance))
                throw (TException)Activator.CreateInstance(typeof(TException), message);
        }

        [DebuggerStepThrough]
        public static void HasDefaultConstructor<T>()
        {
            HasDefaultConstructor(typeof(T));
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HasDefaultConstructor(Type t)
        {
            if (!t.HasDefaultConstructor())
                throw Error.InvalidOperation(HasDefaultConstructorMessage, t.FullName);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetParamName<T>(Func<T> expression)
        {
            return expression.Method.Name;
        }

    }

}
