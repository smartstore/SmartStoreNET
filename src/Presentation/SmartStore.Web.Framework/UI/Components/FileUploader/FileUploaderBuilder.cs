using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Media;

namespace SmartStore.Web.Framework.UI
{
    public class FileUploaderBuilder<TModel> : ComponentBuilder<FileUploader, FileUploaderBuilder<TModel>, TModel>
    {
        public FileUploaderBuilder(FileUploader component, HtmlHelper<TModel> htmlHelper)
            : base(component, htmlHelper)
        {
            WithRenderer(new ViewBasedComponentRenderer<FileUploader>("FileUploader"));
            TypeFilter("*");
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

        public FileUploaderBuilder<TModel> ShowBrowseMedia(bool value)
        {
            base.Component.ShowBrowseMedia = value;
            return this;
        }

        public FileUploaderBuilder<TModel> HasTemplatePreview(bool value)
        {
            base.Component.HasTemplatePreview = value;
            return this;
        }

        public FileUploaderBuilder<TModel> DownloadEnabled(bool value)
        {
            base.Component.DownloadEnabled = value;
            return this;
        }

        public FileUploaderBuilder<TModel> ClickableElement(string value)
        {
            base.Component.ClickableElement = value;
            return this;
        }

        public FileUploaderBuilder<TModel> Multifile(bool value)
        {
            base.Component.Multifile = value;
            return this;
        }

        public FileUploaderBuilder<TModel> TypeFilter(string value)
        {
            var mediaTypeResolver = EngineContext.Current.Resolve<IMediaTypeResolver>();
            var extensions = mediaTypeResolver.ParseTypeFilter(value);

            base.Component.HtmlAttributes["data-accept"] = "." + String.Join(",.", extensions);
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

        /// <summary>
        /// Sets the maximum file size.
        /// Use "0" for an unlimited file size and "null" for the default size (MediaSettings.MaxUploadFileSize).
        /// </summary>
        /// <param name="value">Maximum file size in KB.</param>
        public FileUploaderBuilder<TModel> MaxFileSize(long? value)
        {
            base.Component.MaxFileSize = value;
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
