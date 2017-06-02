using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SmartStore.Core.Html;

namespace SmartStore.Services.Media
{
	public class FixUploadPathsHtmlFilter : IHtmlFilter
	{
		// Matches absolute or relative paths
		private static readonly Regex s_PathPattern = new Regex(@"(?<=(?:href|src)=(?:'|""))(?<url>[\w \-+.:,;/?&=%~#$@()\[\]{}]+)(?:'|"")", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private readonly IMediaFileSystem _mediaFileSystem;

		public FixUploadPathsHtmlFilter(IMediaFileSystem mediaFileSystem)
		{
			_mediaFileSystem = mediaFileSystem;
		}

		public string Flavor
		{
			get { return "html"; }
		}

		public int Ordinal
		{
			get { return 0; }
		}

		public string Process(string input, IDictionary<string, object> parameters)
		{
			MatchEvaluator evaluator = (match) =>
			{
				var url = match.Groups["url"].Value;
				// TODO: do something with 'url' [...]

				return url;
			};

			var result = s_PathPattern.Replace(input, evaluator);

			return result;
		}
	}
}
