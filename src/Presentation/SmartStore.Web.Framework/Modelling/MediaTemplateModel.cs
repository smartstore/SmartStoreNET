using System.Collections.Generic;
using SmartStore.Services.Media;

namespace SmartStore.Web.Framework.Modelling
{
    public partial class MediaTemplateModel : EntityModelBase
    {
        public MediaTemplateModel(MediaFileInfo file, bool renderViewer)
        {
            Guard.NotNull(file, nameof(file));

            Id = file.Id;
            File = file;
            RenderViewer = renderViewer;
        }

        public MediaFileInfo File { get; private set; }
        public bool RenderViewer { get; private set; }

        public int ThumbSize { get; set; } = 256;
        public IDictionary<string, object> HtmlAttributes { get; set; }

        public string Title { get; set; }
        public string Alt { get; set; }
    }
}
