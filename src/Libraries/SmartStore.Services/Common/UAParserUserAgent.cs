using System.Text.RegularExpressions;
using System.Web;
using uap = UAParser;

namespace SmartStore.Services.Common
{
	
	public class UAParserUserAgent : IUserAgent
	{
		private readonly static uap.Parser s_uap;
		private static readonly Regex s_pdfConverterPattern = new Regex(@"wkhtmltopdf", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		private readonly HttpContextBase _httpContext;

		private UserAgentInfo _userAgent;
		private DeviceInfo _device;
		private OSInfo _os;

		static UAParserUserAgent()
		{
			s_uap = uap.Parser.GetDefault();
		}

		public UAParserUserAgent(HttpContextBase httpContext)
		{
			this._httpContext = httpContext;
		}
		
		public UserAgentInfo UserAgent
		{
			get 
			{
				if (_userAgent == null)
				{
					var tmp = s_uap.ParseUserAgent(GetUaString());
					_userAgent = new UserAgentInfo(tmp.Family, tmp.Major, tmp.Minor, tmp.Patch);
				}
				return _userAgent;
			}
		}

		public DeviceInfo Device
		{
			get 
			{
				if (_device == null)
				{
					var uaString = GetUaString();
					var tmp = s_uap.ParseDevice(uaString);
					var isPdfConverter = false;
					if (tmp.Family.IsCaseInsensitiveEqual("Other"))
					{
						isPdfConverter = s_pdfConverterPattern.IsMatch(uaString);
					}
					_device = new DeviceInfo(tmp.Family, tmp.IsSpider, isPdfConverter);
				}
				return _device;
			}
		}

		public OSInfo OS
		{
			get 
			{
				if (_os == null)
				{
					var tmp = s_uap.ParseOS(GetUaString());
					_os = new OSInfo(tmp.Family, tmp.Major, tmp.Minor, tmp.Patch, tmp.PatchMinor);
				}
				return _os;
			}
		}

		private string GetUaString()
		{
			if (_httpContext.Request != null)
			{
				return _httpContext.Request.UserAgent.EmptyNull();
			}

			return "";
		}
	}

}
