using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using SmartStore.Utilities;
using uap = UAParser;

namespace SmartStore.Services.Common
{
	public class UAParserUserAgent : IUserAgent
	{
		private readonly static uap.Parser s_uap;
		private static readonly Regex s_pdfConverterPattern = new Regex(@"wkhtmltopdf", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		# region Mobile UAs, OS & Devices

		private static readonly HashSet<string> s_MobileOS = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			"Android", 
			"iOS", 
			"Windows Mobile", 
			"Windows Phone", 
			"Windows CE",
			"Symbian OS", 
			"BlackBerry OS", 
			"BlackBerry Tablet OS", 
			"Firefox OS", 
			"Brew MP", 
			"webOS",
			"Bada",
			"Kindle",
			"Maemo"
		};

		private static readonly HashSet<string> s_MobileBrowsers = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			"Googlebot-Mobile",
			"Baiduspider-mobile",
			"Android", 
			"Firefox Mobile", 
			"Opera Mobile", 
			"Opera Mini", 
			"Mobile Safari",
			"Amazon Silk", 
			"webOS Browser", 
			"MicroB", 
			"Ovi Browser", 
			"NetFront", 
			"NetFront NX",
			"Chrome Mobile", 
			"Chrome Mobile iOS", 
			"UC Browser", 
			"Tizen Browser", 
			"Baidu Explorer", 
			"QQ Browser Mini",
			"QQ Browser Mobile", 
			"IE Mobile", 
			"Polaris", 
			"ONE Browser", 
			"iBrowser Mini", 
			"Nokia Services (WAP) Browser",
			"Nokia Browser", 
			"Nokia OSS Browser", 
			"BlackBerry WebKit", 
			"BlackBerry", "Palm", 
			"Palm Blazer",
			"Palm Pre", 
			"Teleca Browser", 
			"SEMC-Browser", 
			"PlayStation Portable", 
			"Nokia", 
			"Maemo Browser",
			"Obigo", 
			"Bolt", 
			"Iris", 
			"UP.Browser", 
			"Minimo", 
			"Bunjaloo",
			"Jasmine", 
			"Dolfin", 
			"Polaris",
			"Skyfire"
		};

		private static readonly HashSet<string> s_MobileDevices = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			"BlackBerry", 
			"MI PAD", 
			"iPhone", 
			"iPad", 
			"iPod",
			"Kindle", 
			"Kindle Fire", 
			"Nokia", 
			"Lumia", 
			"Palm", 
			"DoCoMo",
			"HP TouchPad",
			"Xoom",
			"Motorola",
			"Generic Feature Phone",
			"Generic Smartphone"
		};

		#endregion

		private readonly HttpContextBase _httpContext;

		private string _rawValue;
		private UserAgentInfo _userAgent;
		private DeviceInfo _device;
		private OSInfo _os;

		private bool? _isBot;
		private bool? _isMobileDevice;
		private bool? _isTablet;
		private bool? _isPdfConverter;

		static UAParserUserAgent()
		{
			//s_uap = uap.Parser.GetDefault();
			s_uap = uap.Parser.FromYamlFile(CommonHelper.MapPath("~/App_Data/ua-parser.regexes.yaml"));
		}

		public UAParserUserAgent(HttpContextBase httpContext)
		{
			this._httpContext = httpContext;
		}

		public string RawValue
		{
			get 
			{
				if (_rawValue == null)
				{
					if (_httpContext.Request != null)
					{
						_rawValue = _httpContext.Request.UserAgent.EmptyNull();
					}
					else
					{
						_rawValue = "";
					}
				}

				return _rawValue;
			}
			// for (unit) test purpose
			set 
			{
				_rawValue = value;
				_userAgent = null;
				_device = null;
				_os = null;
				_isBot = null;
				_isMobileDevice = null;
				_isTablet = null;
				_isPdfConverter = null;
			}
		}

		public virtual UserAgentInfo UserAgent
		{
			get 
			{
				if (_userAgent == null)
				{
					var tmp = s_uap.ParseUserAgent(this.RawValue);
					_userAgent = new UserAgentInfo(tmp.Family, tmp.Major, tmp.Minor, tmp.Patch);
				}
				return _userAgent;
			}
		}

		public virtual DeviceInfo Device
		{
			get 
			{
				if (_device == null)
				{
					var tmp = s_uap.ParseDevice(this.RawValue);
					_device = new DeviceInfo(tmp.Family, tmp.IsSpider());
				}
				return _device;
			}
		}

		public virtual OSInfo OS
		{
			get 
			{
				if (_os == null)
				{
					var tmp = s_uap.ParseOS(this.RawValue);
					_os = new OSInfo(tmp.Family, tmp.Major, tmp.Minor, tmp.Patch, tmp.PatchMinor);
				}
				return _os;
			}
		}

		public virtual bool IsBot
		{
			get
			{
				if (!_isBot.HasValue)
				{
					// empty useragent > bad bot!
					_isBot = this.RawValue.IsEmpty() || _httpContext.Request.Browser.Crawler || this.Device.IsBot || this.UserAgent.IsBot;
				}
				return _isBot.Value;
			}
		}

		public virtual bool IsMobileDevice
		{
			get
			{
				if (!_isMobileDevice.HasValue)
				{
					_isMobileDevice =
						s_MobileOS.Contains(this.OS.Family) ||
						s_MobileBrowsers.Contains(this.UserAgent.Family) ||
						s_MobileDevices.Contains(this.Device.Family);
				}
				return _isMobileDevice.Value;
			}
		}

		public virtual bool IsTablet
		{
			get
			{
				if (!_isTablet.HasValue)
				{
					_isTablet =
						Regex.IsMatch(this.Device.Family, "iPad|Kindle Fire|Nexus 10|Xoom|Transformer|MI PAD|IdeaTab", RegexOptions.CultureInvariant) ||
						this.OS.Family == "BlackBerry Tablet OS";
				}
				return _isTablet.Value;
			}
		}

		public virtual bool IsPdfConverter
		{
			get
			{
				if (!_isPdfConverter.HasValue)
				{
					_isPdfConverter = s_pdfConverterPattern.IsMatch(this.RawValue);
				}
				return _isPdfConverter.Value;
			}
		}
	}

	internal static class DeviceExtensions
	{
		internal static bool IsSpider(this UAParser.Device device)
		{
			return device.Family.Equals("Spider", StringComparison.InvariantCultureIgnoreCase);
		}
	}

}
