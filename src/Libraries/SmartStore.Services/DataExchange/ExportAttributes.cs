using System;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange
{
	/// <summary>
	/// Declares projection types supported by an export provider.
	/// Controls whether to display corresponding projection fields while editing an export profile.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ExportProjectionSupportAttribute : Attribute
	{
		public ExportProjectionSupportAttribute(params ExportProjectionSupport[] types)
		{
			Types = types;
		}

		public ExportProjectionSupport[] Types { get; set; }
	}
}
