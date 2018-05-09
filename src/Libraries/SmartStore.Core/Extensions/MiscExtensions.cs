﻿using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Text;
using System.Web.Routing;
using SmartStore.Core;

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

		public static string ToAllMessages(this Exception exception)
		{
			var sb = new StringBuilder();
			
			while (exception != null)
			{
				if (!sb.ToString().EmptyNull().Contains(exception.Message))
				{
					sb.Grow(exception.Message, " * ");
				}
				exception = exception.InnerException;
			}
			return sb.ToString();
		}

		public static string ToElapsedMinutes(this Stopwatch watch) 
        {
			return "{0:0.0}".FormatWith(TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalMinutes);
		}

		public static string ToElapsedSeconds(this Stopwatch watch) 
        {
			return "{0:0.0}".FormatWith(TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds);
		}

        public static bool IsNullOrDefault<T>(this T? value) where T : struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }

		/// <summary>Converts bytes into a hex string.</summary>
		public static string ToHexString(this byte[] bytes, int length = 0)
		{
			if (bytes == null || bytes.Length <= 0)
				return "";

			var sb = new StringBuilder();

			foreach (byte b in bytes)
			{
				sb.Append(b.ToString("x2"));

				if (length > 0 && sb.Length >= length)
					break;
			}
			return sb.ToString();
		}

		public static T GetMergedDataValue<T>(this IMergedData mergedData, string key, T defaultValue)
		{
			if (mergedData.MergedDataValues != null && !mergedData.MergedDataIgnore)
			{
				object value;

				if (mergedData.MergedDataValues.TryGetValue(key, out value))
					return (T)value;
			}

			return defaultValue;
		}

		/// <summary>
		/// Append grow if string builder is empty. Append delimiter and grow otherwise.
		/// </summary>
		/// <param name="sb">Target string builder</param>
		/// <param name="grow">Value to append</param>
		/// <param name="delimiter">Delimiter to use</param>
		public static void Grow(this StringBuilder sb, string grow, string delimiter)
		{
			Guard.NotNull(delimiter, "delimiter");

			if (!string.IsNullOrWhiteSpace(grow))
			{
				if (sb.Length <= 0)
					sb.Append(grow);
				else
					sb.AppendFormat("{0}{1}", delimiter, grow);
			}
		}

		public static string SafeGet(this string[] arr, int index)
		{
			return (arr != null && index < arr.Length ? arr[index] : "");
		}
    }
}
