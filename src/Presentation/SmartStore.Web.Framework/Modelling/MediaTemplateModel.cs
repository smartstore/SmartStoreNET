using SmartStore.Services.Media;

namespace SmartStore.Web.Framework.Modelling
{
    public class MediaTemplateModel : EntityModelBase
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

        public string Url { get; set; }
        public string ExtraCssClasses { get; set; }
    }
}
