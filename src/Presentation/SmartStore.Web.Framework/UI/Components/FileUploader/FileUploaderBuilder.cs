using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
    public class FileUploaderBuilder<TModel> : ComponentBuilder<FileUploader, FileUploaderBuilder<TModel>, TModel>
    {
        public FileUploaderBuilder(FileUploader component, HtmlHelper<TModel> htmlHelper)
            : base(component, htmlHelper)
        {
			WithRenderer(new ViewBasedComponentRenderer<FileUploader>("FileUploader"));
		}

		public FileUploaderBuilder<TModel> UploadUrl(string value)
		{
			base.Component.UploadUrl = value;
			return this;
		}

		public FileUploaderBuilder<TModel> IconCssClass(string value)
		{
			base.Component.IconCssClass = value;
			return this;
		}

		public FileUploaderBuilder<TModel> ButtonStyle(ButtonStyle value)
		{
			base.Component.ButtonStyle = value;
			return this;
		}

		public FileUploaderBuilder<TModel> ButtonOutlineStyle(bool value)
		{
			base.Component.ButtonOutlineStyle = value;
			return this;
		}

		public FileUploaderBuilder<TModel> ShowRemoveButton(bool value)
		{
			base.Component.ShowRemoveButton = value;
			return this;
		}

		public FileUploaderBuilder<TModel> ShowRemoveButtonAfterUpload(bool value)
		{
			base.Component.HtmlAttributes["data-show-remove-after-upload"] = value.ToString().ToLower();
			return this;
		}

		public FileUploaderBuilder<TModel> AcceptedFileTypes(string value)
		{
			if (value.IsEmpty())
			{
				if (base.Component.HtmlAttributes.ContainsKey("data-accept"))
					base.Component.HtmlAttributes.Remove("data-accept");
			}
			else
			{
				base.Component.HtmlAttributes["data-accept"] = value;
			}
			
			return this;
		}

		public FileUploaderBuilder<TModel> CancelText(string value)
		{
			base.Component.CancelText = value;
			return this;
		}

		public FileUploaderBuilder<TModel> RemoveText(string value)
		{
			base.Component.RemoveText = value;
			return this;
		}

		public FileUploaderBuilder<TModel> UploadText(string value)
		{
			base.Component.UploadText = value;
			return this;
		}

		public FileUploaderBuilder<TModel> OnUploadingHandlerName(string handlerName)
		{
			base.Component.OnUploadingHandlerName = handlerName;
			return this;
		}

		public FileUploaderBuilder<TModel> OnUploadCompletedHandlerName(string handlerName)
		{
			base.Component.OnUploadCompletedHandlerName = handlerName;
			return this;
		}

		public FileUploaderBuilder<TModel> OnErrorHandlerName(string handlerName)
		{
			base.Component.OnErrorHandlerName = handlerName;
			return this;
		}

		public FileUploaderBuilder<TModel> OnFileRemoveHandlerName(string handlerName)
		{
			base.Component.OnFileRemoveHandlerName = handlerName;
			return this;
		}

		public FileUploaderBuilder<TModel> OnAbortedHandlerName(string handlerName)
		{
			base.Component.OnAbortedHandlerName = handlerName;
			return this;
		}

		public FileUploaderBuilder<TModel> OnCompletedHandlerName(string handlerName)
		{
			base.Component.OnCompletedHandlerName = handlerName;
			return this;
		}
	}
}
