using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using SmartStore.Core;
using SmartStore.Utilities;

namespace SmartStore.Services.Pdf
{
    public class PdfHtmlContent : IPdfContent
    {
        private static readonly Uri _engineBaseUri;

        private string _html;
        private string _originalHtml;
        private bool _processed;
        private string _tempFilePath;
        private PdfContentKind _kind = PdfContentKind.Html;

        static PdfHtmlContent()
        {
            var baseUrl = CommonHelper.GetAppSetting<string>("sm:PdfEngineBaseUrl").TrimSafe().NullEmpty();
            if (baseUrl != null)
            {
                Uri uri;
                if (Uri.TryCreate(baseUrl, UriKind.Absolute, out uri))
                {
                    _engineBaseUri = uri;
                }
            }
        }

        public PdfHtmlContent(string html)
        {
            Guard.NotEmpty(html, nameof(html));

            _originalHtml = html;
            _html = html;
        }

        public PdfContentKind Kind => _kind;

        public string Process(string flag)
        {
            if (!_processed)
            {
                if (_engineBaseUri != null)
                {
                    _html = WebHelper.MakeAllUrlsAbsolute(_html, _engineBaseUri.Scheme, _engineBaseUri.Authority);
                }
                else if (HttpContext.Current?.Request != null)
                {
                    _html = WebHelper.MakeAllUrlsAbsolute(_html, new HttpRequestWrapper(HttpContext.Current.Request));
                }

                if (!flag.IsCaseInsensitiveEqual("page"))
                {
                    CreateTempFile();
                }

                _processed = true;
            }

            return _tempFilePath ?? _html;
        }

        private void CreateTempFile()
        {
            // TODO: (mc) This is a very weak mechanism to determine if html is partial. Find a better way!
            bool isPartial = !_html.Trim().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
            if (isPartial)
            {
                _html = WrapPartialHtml(_html);
            }

            string tempPath = Path.GetTempPath();
            _tempFilePath = Path.Combine(tempPath, "pdfgen-" + Path.GetRandomFileName() + ".html");
            File.WriteAllBytes(_tempFilePath, Encoding.UTF8.GetBytes(_html));

            _kind = PdfContentKind.Url;
        }

        private string WrapPartialHtml(string html)
        {
            return string.Format("<!DOCTYPE html><html><head>\r\n<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />\r\n<script>\r\nfunction subst() {{\r\n    var vars={{}};\r\n    var x=document.location.search.substring(1).split('&');\r\n\r\n    for(var i in x) {{var z=x[i].split('=',2);vars[z[0]] = unescape(z[1]);}}\r\n    var x=['frompage','topage','page','webpage','section','subsection','subsubsection'];\r\n    for(var i in x) {{\r\n      var y = document.getElementsByClassName(x[i]);\r\n      for(var j=0; j<y.length; ++j) y[j].textContent = vars[x[i]];\r\n    }}\r\n}}\r\n</script></head><body style=\"border:0; margin: 0;\" onload=\"subst()\">{0}</body></html>\r\n", html);
        }

        public void WriteArguments(string flag, StringBuilder builder)
        {
            // noop
        }

        public void Teardown()
        {
            if (_tempFilePath != null && File.Exists(_tempFilePath))
            {
                try
                {
                    File.Delete(_tempFilePath);
                }
                catch { }
            }

            _kind = PdfContentKind.Html;
            _html = _originalHtml;
            _tempFilePath = null;
            _processed = false;
        }
    }
}
