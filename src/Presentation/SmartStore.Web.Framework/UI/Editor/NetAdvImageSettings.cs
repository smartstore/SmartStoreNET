//Contributor: http://aspnetadvimage.codeplex.com/
namespace SmartStore.Web.Framework.UI.Editor
{
    public partial class NetAdvImageSettings
    {
        public static string TreeStateSessionKey = "NetAdvImageTreeState";

        public static string UploadPath
        {
            get
            {
                return "~/Media/Uploaded/";
            }
        }

        public static string[] AllowedFileTypes
        {
            get
            {
                return new string[] { ".gif", ".jpg", ".jpeg", ".png", ".bmp" };
            }
        }
    }
}