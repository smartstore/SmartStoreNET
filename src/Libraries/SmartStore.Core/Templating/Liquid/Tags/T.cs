using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.Util;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;

namespace SmartStore.Templating.Liquid
{
	internal sealed class T : Tag
	{
		private static readonly Regex SimpleSyntax = R.B(@"^({0})", DotLiquid.Liquid.QuotedFragment);
		private static readonly Regex NamedSyntax = R.B(@"^({0})\s*\:\s*(.*)", DotLiquid.Liquid.QuotedFragment);

		private string _resName;
		private string[] _parameters;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match match = NamedSyntax.Match(markup);

			if (match.Success)
			{
				_resName = match.Groups[1].Value;
				_parameters = ParametersFromString(match.Groups[2].Value);
			}
			else
			{
				match = SimpleSyntax.Match(markup);
				if (match.Success)
				{
					_resName = match.Groups[1].Value;
					_parameters = new string[0];
				}
				else
				{
					throw new SyntaxException("Syntax Error in 'T' tag - Valid syntax: T '[ResourceName]'.");
				}	
			}			

			base.Initialize(tagName, markup, tokens);
		}

		public override void Render(Context context, TextWriter result)
		{
			var resName = (string)context[_resName] ?? _resName;
			var localizer = EngineContext.Current.Resolve<LocalizerEx>();
			string resValue = string.Empty;

			var parameters = new List<object>();
			foreach (var p in _parameters)
			{
				parameters.Add(context[p] ?? p);
			}

			if (context["Context.LanguageId"] is int lid)
			{
				resValue = localizer(resName, lid, parameters.ToArray());
			}
			else
			{
				resValue = localizer(resName, 0, parameters.ToArray());
			}
			
			result.Write(resValue);
		}

		private static string[] ParametersFromString(string markup)
		{
			return markup
				.Trim()
				.Split(',')
				.Select(x => x.Trim())
				.ToArray();
		}
	}
}
