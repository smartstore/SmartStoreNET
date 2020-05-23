using SmartStore.Core.Domain.Catalog;
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

		public FileUploaderBuilder<TModel> Path(string value)
		{
			base.Component.Path = value;
			return this;
		}

		public FileUploaderBuilder<TModel> UploadUrl(string value)
		{
			base.Component.UploadUrl = value;
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
			base.Component.ShowRemoveButtonAfterUpload = value;
			return this;
		}

		public FileUploaderBuilder<TModel> Compact(bool value)
		{
			base.Component.Compact = value;
			return this;
		}

		public FileUploaderBuilder<TModel> Multifile(bool value)
		{
			base.Component.Multifile = value;
			return this;
		}

		public FileUploaderBuilder<TModel> TypeFilter(string value)
		{
			base.Component.TypeFilter = value;
			return this;
		}

		public FileUploaderBuilder<TModel> PreviewContainerId(string value)
		{
			base.Component.PreviewContainerId = value;
			return this;
		}

		public FileUploaderBuilder<TModel> MainFileId(int? value)
		{
			base.Component.MainFileId = value;
			return this;
		}

		public FileUploaderBuilder<TModel> UploadedFiles(IEnumerable<IMediaFile> value)
		{
			base.Component.UploadedFiles = value;
			return this;
		}

		public FileUploaderBuilder<TModel> EntityType(string value)
		{
			base.Component.EntityType = value;
			return this;
		}

		public FileUploaderBuilder<TModel> EntityId(int value)
		{
			base.Component.EntityId = value;
			return this;
		}

		public FileUploaderBuilder<TModel> DeleteUrl(string value)
		{
			base.Component.DeleteUrl = value;
			return this;
		}

		public FileUploaderBuilder<TModel> SortUrl(string value)
		{
			base.Component.SortUrl = value;
			return this;
		}

		public FileUploaderBuilder<TModel> EntityAssignmentUrl(string value)
		{
			base.Component.EntityAssignmentUrl = value;
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

		public FileUploaderBuilder<TModel> OnMediaSelectedHandlerName(string handlerName)
		{
			base.Component.OnMediaSelectedHandlerName = handlerName;
			return this;
		}
	}
}
