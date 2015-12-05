using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.ComponentModel
{
	[SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
	public class BooleanConverter : TypeConverterBase
	{
		private readonly HashSet<string> _trueValues;
		private readonly HashSet<string> _falseValues;

		public BooleanConverter(string[] trueValues, string[] falseValues)
			: base(typeof(bool))
		{
			_trueValues = new HashSet<string>(trueValues, StringComparer.OrdinalIgnoreCase);
			_falseValues = new HashSet<string>(falseValues, StringComparer.OrdinalIgnoreCase);
		}

		public ICollection<string> TrueValues
		{
			get { return _trueValues; }
		}

		public ICollection<string> FalseValues
		{
			get { return _falseValues; }
		}

		public override object ConvertFrom(CultureInfo culture, object value)
		{
			if (value is short)
			{
				if ((short)value == 0)
				{
					return false;
				}
				if ((short)value == 1)
				{
					return true;
				}
			}

			if (value is string)
			{
				var str = (string)value;

				bool b;
				if (bool.TryParse(str, out b))
				{
					return b;
				}

				short sh;
				if (short.TryParse(str, out sh))
				{
					if (sh == 0)
					{
						return false;
					}
					if (sh == 1)
					{
						return true;
					}
				}

				str = (str.NullEmpty() ?? string.Empty).Trim();
				if (_trueValues.Contains(str))
				{
					return true;
				}

				if (_falseValues.Contains(str))
				{
					return false;
				}
			}

			return base.ConvertFrom(culture, value);
		}
	}
}
