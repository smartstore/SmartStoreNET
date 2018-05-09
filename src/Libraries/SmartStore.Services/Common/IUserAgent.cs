﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Common
{	
	public interface IUserAgent
	{
		string RawValue { get; set; }
		
		UserAgentInfo UserAgent { get; }
		DeviceInfo Device { get; }
		OSInfo OS { get; }

		bool IsBot { get; }
		bool IsMobileDevice { get; }
		bool IsTablet { get; }
		bool IsPdfConverter { get; }
	}

	public sealed class DeviceInfo
	{
		public DeviceInfo(string family, bool isBot)
		{
			this.Family = family;
			this.IsBot = isBot;
		}
		public override string ToString()
		{
			return this.Family;
		}
		public string Family { get; private set; }
		public bool IsBot { get; private set; }
	}

	public sealed class OSInfo
	{
		public OSInfo(string family, string major, string minor, string patch, string patchMinor)
		{
			this.Family = family;
			this.Major = major;
			this.Minor = minor;
			this.Patch = patch;
			this.PatchMinor = patchMinor;
		}
		public override string ToString()
		{
			var str = VersionString.Format(Major, Minor, Patch, PatchMinor);
			return (this.Family + (!string.IsNullOrEmpty(str) ? (" " + str) : null));
		}
		public string Family { get; private set; }
		public string Major { get; private set; }
		public string Minor { get; private set; }
		public string Patch { get; private set; }
		public string PatchMinor { get; private set; }

		private static string FormatVersionString(params string[] parts)
		{
			return string.Join(".", (from v in parts
									 where !string.IsNullOrEmpty(v)
									 select v).ToArray<string>());
		}
	}

	public sealed class UserAgentInfo
	{
		private static readonly HashSet<string> s_Bots = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			"BingPreview"
		};

		private bool? _isBot;

		public UserAgentInfo(string family, string major, string minor, string patch)
		{
			this.Family = family;
			this.Major = major;
			this.Minor = minor;
			this.Patch = patch;
		}
		public override string ToString()
		{
			var str = VersionString.Format(Major, Minor, Patch);
			return (this.Family + (!string.IsNullOrEmpty(str) ? (" " + str) : null));
		}
		public string Family { get; private set; }
		public string Major { get; private set; }
		public string Minor { get; private set; }
		public string Patch { get; private set; }
		public bool IsBot
		{
			get
			{
				if (!_isBot.HasValue)
				{
					_isBot = s_Bots.Contains(Family);
				}
				return _isBot.Value;
			}
		}
	}

	internal static class VersionString
	{
		public static string Format(params string[] parts)
		{
			return string.Join(".", (from v in parts 
									 where !string.IsNullOrEmpty(v) 
									 select v).ToArray<string>());
		}
	}


}
