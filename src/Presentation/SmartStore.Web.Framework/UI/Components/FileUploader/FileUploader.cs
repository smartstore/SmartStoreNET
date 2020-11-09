using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Services.Media;

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
            HtmlAttributes.AppendCssClass("fu-fileupload");
            HtmlAttributes.Add("data-accept", "gif,jpeg,jpg,png");
            HtmlAttributes.Add("data-show-remove-after-upload", "false");

            if (localizer != null)
            {
                UploadText = localizer("Common.Fileuploader.Upload");
            }
        }

        public string Path { get; set; } = SystemAlbumProvider.Files;

        public string UploadUrl
        {
            get => HtmlAttributes["data-upload-url"] as string;
            set => HtmlAttributes["data-upload-url"] = value;
        }

        public string UploadText { get; set; }
        public bool ShowRemoveButton { get; set; }
        public bool ShowRemoveButtonAfterUpload { get; set; }
        public bool ShowBrowseMedia { get; set; }
        public bool HasTemplatePreview { get; set; }
        public string ClickableElement { get; set; }
        public bool DownloadEnabled { get; set; }

        public bool Multifile { get; set; }
        public string TypeFilter { get; set; }
        public string PreviewContainerId { get; set; }
        public int? MainFileId { get; set; }
        public long? MaxFileSize { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string DeleteUrl { get; set; }
        public string SortUrl { get; set; }
        public string EntityAssignmentUrl { get; set; }
        public IEnumerable<IMediaFile> UploadedFiles { get; set; }
        public string OnUploadingHandlerName { get; set; }
        public string OnUploadCompletedHandlerName { get; set; }
        public string OnErrorHandlerName { get; set; }
        public string OnFileRemoveHandlerName { get; set; }
        public string OnAbortedHandlerName { get; set; }
        public string OnCompletedHandlerName { get; set; }
        public string OnMediaSelectedHandlerName { get; set; }
    }
}
