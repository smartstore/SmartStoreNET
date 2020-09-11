using System.Web;
using SmartStore.Core.Localization;

namespace SmartStore.Services.Localization
{
    public class LocalizedUrlHelper
    {
        public LocalizedUrlHelper(HttpRequestBase httpRequest, bool rawUrl = false)
            : this(httpRequest.ApplicationPath, rawUrl ? httpRequest.RawUrl : httpRequest.AppRelativeCurrentExecutionFilePath, rawUrl)
        {
            Guard.NotNull(httpRequest, nameof(httpRequest));
        }

        public LocalizedUrlHelper(string applicationPath, string relativePath, bool rawUrl = false)
        {
            Guard.NotNull(applicationPath, nameof(applicationPath));
            Guard.NotNull(relativePath, nameof(relativePath));

            this.ApplicationPath = applicationPath;

            if (rawUrl)
            {
                this.RelativePath = RemoveApplicationPathFromRawUrl(relativePath).TrimStart('~', '/');
            }
            else
            {
                this.RelativePath = relativePath.TrimStart('~', '/');
            }
        }

        public string ApplicationPath { get; private set; }

        public string RelativePath { get; private set; }

        public bool IsLocalizedUrl()
        {
            return IsLocalizedUrl(out _);
        }

        public bool IsLocalizedUrl(out string seoCode)
        {
            seoCode = null;

            string firstPart = this.RelativePath;

            if (firstPart.IsEmpty())
            {
                return false;
            }

            int firstSlash = firstPart.IndexOf('/');

            if (firstSlash > 0)
            {
                firstPart = firstPart.Substring(0, firstSlash);
            }

            if (LocalizationHelper.IsValidCultureCode(firstPart))
            {
                seoCode = firstPart;
                return true;
            }

            return false;
        }

        public string StripSeoCode()
        {
            if (IsLocalizedUrl(out var seoCode))
            {
                this.RelativePath = this.RelativePath.Substring(seoCode.Length).TrimStart('/');
                //if (this.RelativePath.IsEmpty())
                //{
                //    this.RelativePath = "/";
                //}
            }

            return this.RelativePath;
        }

        public string PrependSeoCode(string seoCode, bool safe = false)
        {
            Guard.NotEmpty(seoCode, nameof(seoCode));

            if (safe)
            {
                string currentSeoCode;
                if (IsLocalizedUrl(out currentSeoCode))
                {
                    if (seoCode == currentSeoCode)
                    {
                        return this.RelativePath;
                    }
                    else
                    {
                        StripSeoCode();
                    }
                }
            }

            this.RelativePath = "{0}/{1}".FormatCurrent(seoCode, this.RelativePath).TrimEnd('/');
            return this.RelativePath;
        }

        public string GetAbsolutePath()
        {
            string path = this.ApplicationPath.EnsureEndsWith("/");
            path += this.RelativePath;

            if (path.Length > 1 && path[0] != '/')
            {
                path = "/" + path;
            }

            return path;
        }

        private string RemoveApplicationPathFromRawUrl(string rawUrl)
        {
            if (rawUrl.Length == ApplicationPath.Length)
            {
                return "/";
            }

            var result = rawUrl.Substring(ApplicationPath.Length);
            // raw url always starts with '/'
            if (!result.StartsWith("/"))
            {
                result = "/" + result;
            }

            return result;
        }
    }
}
