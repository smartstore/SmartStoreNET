using System;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;

namespace SmartStore.Web.Framework.UI
{
    public class FileUploader : Component
    {
		public FileUploader()
			: this(EngineContext.Current.Resolve<Localizer>())
		{
		}

		public FileUploader(Localizer localizer)
        {
			HtmlAttributes.AppendCssClass("fileupload form-row align-items-center");
			HtmlAttributes.Add("data-accept", "gif|jpe?g|png");
			HtmlAttributes.Add("data-show-remove-after-upload", "false");
			IconCssClass = "fa fa-upload";
			ButtonStyle = ButtonStyle.Secondary;
			
			if (localizer != null)
			{
				CancelText = localizer("Common.Fileuploader.Cancel");
				RemoveText = localizer("Common.Remove");
				UploadText = localizer("Common.Fileuploader.Upload");
			}
		}

		public string UploadUrl
		{
			get { return HtmlAttributes["data-upload-url"] as string; }
			set { HtmlAttributes["data-upload-url"] = value; }
		}

		public string IconCssClass { get; set; }
		public ButtonStyle ButtonStyle { get; set; }
		public bool ButtonOutlineStyle { get; set; }
		public bool ShowRemoveButton { get; set; }

		public string CancelText { get; set; }
		public string RemoveText { get; set; }
		public string UploadText { get; set; }

		public string OnUploadingHandlerName { get; set; }
		public string OnUploadCompletedHandlerName { get; set; }
		public string OnErrorHandlerName { get; set; }
		public string OnFileRemoveHandlerName { get; set; }
		public string OnAbortedHandlerName { get; set; }
		public string OnCompletedHandlerName { get; set; }
	}
}
