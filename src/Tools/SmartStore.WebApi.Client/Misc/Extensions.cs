using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace SmartStoreNetWebApiClient
{
	public static class Extensions
	{
		private const char _delimiter = '¶';

		public static void Dump(this string value, bool appendMarks = false)
		{
			Debug.WriteLine(value);
			Debug.WriteLineIf(appendMarks, "------------------------------------------------");
		}

		public static void Dump(this Exception exc)
		{
			try
			{
				exc.StackTrace.Dump();
				exc.Message.Dump();
			}
			catch { }
		}

		public static DialogResult Box(this string message, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
		{
			return MessageBox.Show(message, Program.AppName, buttons, icon);
		}
		
		public static void InsertRolled(this ComboBox combo, string str, int max)
		{
			if (!string.IsNullOrEmpty(str))
			{
				int i;
				for (i = combo.Items.Count - 1; i >= 0; --i)
				{
					if (string.Compare(combo.Items[i].ToString(), str, true) == 0)
						combo.Items.RemoveAt(i);
				}

				combo.Items.Insert(0, str);

				for (i = combo.Items.Count; i > 64; --i)
					combo.Items.RemoveAt(i);

				combo.Text = str;
			}
		}
		
		public static void FromString(this ComboBox.ObjectCollection coll, string values)
		{
			if (!string.IsNullOrWhiteSpace(values))
			{
				string[] items = values.Split(new[] { _delimiter }, StringSplitOptions.RemoveEmptyEntries);
				coll.AddRange(items);
			}
		}
		
		public static string IntoString(this ComboBox.ObjectCollection coll)
		{
			if (coll.Count <= 0)
				return "";

			var sb = new StringBuilder();
			foreach (var item in coll)
			{
				if (sb.Length > 0)
					sb.Append(_delimiter);
				sb.Append(item);
			}
			return string.Join(_delimiter.ToString(), sb.ToString());
		}
		
		public static void RemoveCurrent(this ComboBox combo)
		{
			combo.Items.Remove(combo.Text);
			combo.ResetText();
		}

		public static int ToInt(this string value, int defaultValue = 0)
		{
			int result;
			if (int.TryParse(value, out result))
			{
				return result;
			}
			return defaultValue;
		}

		public static string EmptyNull(this string value)
		{
			return (value ?? string.Empty).Trim();
		}

		public static bool IsEmpty(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static bool HasValue(this string value)
		{
			return !string.IsNullOrWhiteSpace(value);
		}

		public static string FormatInvariant(this string format, params object[] objects)
		{
			return string.Format(CultureInfo.InvariantCulture, format, objects);
		}
	}
}
