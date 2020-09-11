using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using SmartStore.Utilities.ObjectPools;

namespace SmartStore
{
    public static class MiscExtensions
    {
        public static void Dump(this Exception exception)
        {
            try
            {
                exception.StackTrace.Dump();
                exception.Message.Dump();
            }
            catch { }
        }

        public static string ToAllMessages(this Exception exception, bool includeStackTrace = false)
        {
            var psb = PooledStringBuilder.Rent();
            var sb = (StringBuilder)psb;

            while (exception != null)
            {
                if (!sb.ToString().EmptyNull().Contains(exception.Message))
                {
                    if (includeStackTrace)
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine();
                            sb.AppendLine();
                        }
                        sb.AppendLine(exception.ToString());
                    }
                    else
                    {
                        sb.Grow(exception.Message, " * ");
                    }
                }

                exception = exception.InnerException;
            }

            return psb.ToStringAndReturn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToElapsedMinutes(this Stopwatch watch)
        {
            return "{0:0.0}".FormatWith(TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalMinutes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToElapsedSeconds(this Stopwatch watch)
        {
            return "{0:0.0}".FormatWith(TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrDefault<T>(this T? value) where T : struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }

        /// <summary>
        /// Converts bytes into a hex string.
        /// </summary>
        public static string ToHexString(this byte[] bytes, int length = 0)
        {
            if (bytes == null || bytes.Length <= 0)
            {
                return "";
            }

            var sb = new StringBuilder();

            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));

                if (length > 0 && sb.Length >= length)
                {
                    break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Append grow if string builder is empty. Append delimiter and grow otherwise.
        /// </summary>
        /// <param name="sb">Target string builder</param>
        /// <param name="grow">Value to append</param>
        /// <param name="delimiter">Delimiter to use</param>
        public static void Grow(this StringBuilder sb, string grow, string delimiter)
        {
            Guard.NotNull(delimiter, nameof(delimiter));

            if (!string.IsNullOrWhiteSpace(grow))
            {
                if (sb.Length <= 0)
                {
                    sb.Append(grow);
                }
                else
                {
                    sb.AppendFormat("{0}{1}", delimiter, grow);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SafeGet(this string[] arr, int index)
        {
            return arr != null && index < arr.Length ? arr[index] : "";
        }
    }
}
