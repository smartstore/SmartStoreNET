using System;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework
{
	/// <summary>
	/// Wrapper for the most common string extension helpers used in views.
	/// Just here to avoid runtime exceptions in views after refactoring GetLocalized() helper.
	/// </summary>
	public static class LocalizedValueExtensions
	{
		public static bool HasValue(this LocalizedValue<string> value)
		{
			return value.Value.HasValue();
		}

		public static bool IsEmpty(this LocalizedValue<string> value)
		{
			return value.Value.IsEmpty();
		}

		public static string Truncate(this LocalizedValue<string> value, int maxLength, string suffix = "")
		{
			return value.Value.Truncate(maxLength, suffix);
		}
	}
}
