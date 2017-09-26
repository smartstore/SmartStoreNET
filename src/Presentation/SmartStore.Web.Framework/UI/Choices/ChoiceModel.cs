using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.UI.Choices
{
	public abstract class ChoiceModel : EntityModelBase
	{
		protected ChoiceModel()
		{
			this.AllowedFileExtensions = new List<string>();
			this.Values = new List<ChoiceItemModel>();
		}

		public AttributeControlType AttributeControlType { get; set; }

		public string Alias { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public string TextPrompt { get; set; }

		public bool IsRequired { get; set; }

		public bool IsDisabled { get; set; }

		/// <summary>
		/// Allowed file extensions for customer uploaded files
		/// </summary>
		public IList<string> AllowedFileExtensions { get; set; }

		/// <summary>
		/// Selected value for textboxes
		/// </summary>
		public string TextValue { get; set; }
		/// <summary>
		/// Selected day value for datepicker
		/// </summary>
		public int? SelectedDay { get; set; }
		/// <summary>
		/// Selected month value for datepicker
		/// </summary>
		public int? SelectedMonth { get; set; }
		/// <summary>
		/// Selected year value for datepicker
		/// </summary>
		public int? SelectedYear { get; set; }
		/// <summary>
		/// Begin year for datepicker
		/// </summary>
		public int? BeginYear { get; set; }
		/// <summary>
		/// End year for datepicker
		/// </summary>
		public int? EndYear { get; set; }

		public string UploadedFileGuid { get; set; }
		public string UploadedFileName { get; set; }

		public virtual IList<ChoiceItemModel> Values { get; set; }

		public abstract string BuildControlId();

		public virtual string GetLabel()
		{
			return TextPrompt.NullEmpty() ?? Name;
		}

		public virtual string GetDescription()
		{
			var containsImg = Description.IsEmpty() ? false : Description.Contains("<img");

			var desc = Description.RemoveHtml();
			if (containsImg || (desc.HasValue() && !desc.Trim().IsCaseInsensitiveEqual(GetLabel())))
			{
				return Description;
			}

			return null;
		}

		public virtual string GetFileUploadUrl(UrlHelper url)
		{
			return null;
		}
	}
}
