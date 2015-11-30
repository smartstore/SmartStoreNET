using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace SmartStore.Utilities
{

    /// <summary>
    /// This class is used to use wildcards and number ranges while
    /// searching in text. * is used of any chars, ? for one char and
    /// a number range is used with the - char. (12-232)
    /// </summary>
    public class Wildcard : Regex
    {
        #region Fields

        /// <summary>
        /// This flag determines whether the parser is forward
        /// direction or not.
        /// </summary>
        private static bool m_isForward;

		private readonly string _pattern;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Wildcard"/> class.
        /// </summary>
        /// <param name="pattern">The wildcard pattern.</param>
        public Wildcard(string pattern) 
			: this(pattern, RegexOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wildcard"/> class.
        /// </summary>
        /// <param name="pattern">The wildcard pattern.</param>
        /// <param name="options">The regular expression options.</param>
        public Wildcard(string pattern, RegexOptions options) 
			: this(WildcardToRegex(pattern), options, Timeout.InfiniteTimeSpan)
        {
			
        }

		internal Wildcard(string parsedPattern, RegexOptions options, TimeSpan matchTimeout)
			: base(parsedPattern, options, matchTimeout)
		{
			_pattern = parsedPattern;
		}

        #endregion

		public string Pattern 
		{
			get
			{
				return _pattern;
			}
		}

        #region Private Implementation
        /// <summary>
        /// Searches all number range terms and converts them
        /// to a regular expression term.
        /// </summary>
        /// <param name="pattern">The wildcard pattern.</param>
        /// <returns>A converted regular expression term.</returns>
        private static string WildcardToRegex(string pattern)
        {
            m_isForward = true;
            //escape and beginning
            pattern = "^" + Regex.Escape(pattern);
            //replace * with .*
            pattern = pattern.Replace("\\*", ".*");
            //$ is for end position and replace ? with a .
            pattern = pattern.Replace("\\?", ".") + "$";

            //convert the number ranges into regular expression
            Regex re = new Regex("[0-9]+-[0-9]+");
            MatchCollection collection = re.Matches(pattern);
            foreach (Match match in collection)
            {
                string[] split = match.Value.Split(new char[] { '-' });
                int leadingZeroesCount = split[0].TakeWhile(x => x == '0').Count();
                int min = Int32.Parse(split[0]);
                int max = Int32.Parse(split[1]);

                pattern = pattern.Replace(match.Value, ConvertNumberRange(min, max, leadingZeroesCount));
            }

            return pattern;
        }

        /// <summary>
        /// Converts the number range into regular expression term.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The regular expression pattern for the number range term.</returns>
        private static string ConvertNumberRange(int min, int max, int leadingZeroesCount)
        {
            if (max < min)
            {
                throw new InvalidOperationException("The minimum value could not be greater than the maximum value.");
            }

            string prefix = new String('0', leadingZeroesCount); // for leading zeroes
            string pattern = string.Empty;

			if (min > -1 && max - min < 10)
			{
				// special treatment: the below function has issues with too small ranges
				for (var i = min; i <= max; i++)
				{
					pattern += prefix + i.ToString() + (i < max ? "|" : "");
				}
			}
			else
			{

				int currentValue = min;
				int tempMax = 0;
				int radix = 1;

				while (m_isForward || radix != 0)
				{
					tempMax = GetNextMaximum(currentValue, max, radix);

					if (tempMax >= currentValue)
					{
						pattern += prefix + ParseRange(currentValue, tempMax, radix);
						if (!(!m_isForward && radix == 1))
						{
							pattern += "|";
						}
					}
					radix += (m_isForward ? 1 : -1);
					currentValue = tempMax + 1;
				}
			}

            //add a negative look behind and a negative look ahead in order
            //to avoid that 122-321 is found in 2308.
            return @"((?<!\d)(" + pattern + @")(?!\d))";
        }
		
        /// <summary>
        /// Gets the next maximum value in condition to the current value.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="maximum">The absolute maximum.</param>
        /// <param name="radix">The radix.</param>
        /// <returns>The next number which is greater than the current value,
        /// but less or equal than the absolute maximum value.
        /// </returns>
        private static int GetNextMaximum(int currentValue, int maximum, int radix)
        {
			//backward
            if (!m_isForward)
            {
                if (radix != 1)
                {
                    return ((maximum / (int)Math.Pow(10, radix - 1))) * (int)Math.Pow(10, radix - 1) - 1;
                }
                //end is reached
                return maximum;
            }
            //forward
            int tempMax = ((currentValue / (int)Math.Pow(10, radix)) + 1) * (int)Math.Pow(10, radix) - 1;
            if (tempMax > maximum)
            {
                m_isForward = false;
                radix--;
                tempMax = ((maximum / (int)Math.Pow(10, radix))) * (int)Math.Pow(10, radix) - 1;
            }
			
            return tempMax;
        }

        /// <summary>
        /// Parses the range and converts it into a regular expression term.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="radix">The radix form where up the value is changed.</param>
        /// <returns>The pattern which descripes the specified range.</returns>
        private static string ParseRange(int min, int max, int radix)
        {
            string pattern = string.Empty;
            int length = max.ToString().Length;

            pattern += min.ToString().Substring(0, length - radix);
            int minDigit = ExtractDigit(min, length - radix);
            int maxDigit = ExtractDigit(max, length - radix);

            pattern += GetRangePattern(minDigit, maxDigit, 1);
            pattern += GetRangePattern(0, 9, radix - 1);

            return pattern;
        }

        /// <summary>
        /// Gets the range pattern for the specified figures.
        /// </summary>
        /// <param name="beginDigit">The begin digit.</param>
        /// <param name="endDigit">The end digit.</param>
        /// <param name="count">The count of the pattern.</param>
        /// <returns>The pattern for the atomic number range.</returns>
        private static string GetRangePattern(int beginDigit, int endDigit, int count)
        {
            if (count == 0)
            {
				return string.Empty;
            }

            if (beginDigit == endDigit)
            {
				return beginDigit.ToString();
            }

            string pattern = string.Format("[{0}-{1}]", beginDigit, endDigit);
            if (count > 1)
            {
                pattern += string.Format("{{{0}}}", count);
            }

            return pattern;
        }

        /// <summary>
        /// Extracts the digit form the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="digit">The digit.</param>
        /// <returns>The figure at the specified digit.</returns>
        private static int ExtractDigit(int value, int digit)
        {
            return Int32.Parse(value.ToString()[digit].ToString());
        }
        #endregion
    }

}
