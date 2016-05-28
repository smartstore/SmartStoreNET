using System;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange
{
	/// <summary>
	/// Declares data processing types supported by an export provider.
	/// Projection type controls whether to display corresponding projection fields while editing an export profile.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ExportFeaturesAttribute : Attribute
	{
		public ExportFeaturesAttribute(params ExportFeatures[] features)
		{
			Features = features;
		}

		public ExportFeatures[] Features { get; set; }
	}
}
