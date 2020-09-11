using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SmartStore.Utilities.ObjectPools;

namespace SmartStore.Utilities
{
    /// <summary>
    /// This class is used to use wildcards and number ranges while
    /// searching in text. * is used of any chars, ? for one char and
    /// a number range is used with the - char. (12-232)
    /// </summary>
    public class Wildcard : Regex
    {
        /// <summary>
        /// This flag determines whether the parser is forward
        /// direction or not.
        /// </summary>
        private static bool m_isForward;

        private readonly string _pattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="Wildcard"/> class.
        /// </summary>
        /// <param name="pattern">The wildcard pattern.</param>
        /// <param name="parseNumberRanges">
        /// Specifies whether number ranges (e.g. 1234-5678) should
        /// be converted to a regular expression pattern.
        /// </param>
        public Wildcard(string pattern, bool parseNumberRanges = false)
            : this(pattern, RegexOptions.None, parseNumberRanges)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wildcard"/> class.
        /// </summary>
        /// <param name="pattern">The wildcard pattern.</param>
		/// <param name="parseNumberRanges">
		/// Specifies whether number ranges (e.g. 1234-5678) should
		/// be converted to a regular expression pattern.
		/// </param>
        /// <param name="options">The regular expression options.</param>
        public Wildcard(string pattern, RegexOptions options, bool parseNumberRanges = false)
            : this(WildcardToRegex(pattern, parseNumberRanges), options, Timeout.InfiniteTimeSpan)
        {

        }

        internal Wildcard(string parsedPattern, RegexOptions options, TimeSpan matchTimeout)
            : base(parsedPattern, options, matchTimeout)
        {
            _pattern = parsedPattern;
        }

        public string Pattern => _pattern;

        /// <summary>
        /// Searches all number range terms and converts them
        /// to a regular expression term.
        /// </summary>
        /// <param name="pattern">The wildcard pattern.</param>
        /// <returns>A converted regular expression term.</returns>
        private static string WildcardToRegex(string pattern, bool parseNumberRanges)
        {
            m_isForward = true;

            // Replace ? with . and * with .*
            // Prepend ^, append $
            // Escape all chars except []^ 
            pattern = ToGlobPattern(pattern);

            // convert the number ranges into regular expression
            if (parseNumberRanges)
            {
                var re = new Regex("[0-9]+-[0-9]+");
                MatchCollection collection = re.Matches(pattern);
                foreach (Match match in collection)
                {
                    string[] split = match.Value.Split(new char[] { '-' });
                    int leadingZeroesCount = split[0].TakeWhile(x => x == '0').Count();
                    int min = Int32.Parse(split[0]);
                    int max = Int32.Parse(split[1]);

                    pattern = pattern.Replace(match.Value, ConvertNumberRange(min, max, leadingZeroesCount));
                }
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
                int radix = 1;

                while (m_isForward || radix != 0)
                {
                    int tempMax = GetNextMaximum(currentValue, max, radix);

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

        #region Escaping

        /* --------------------------------------------------------------
		Stuff here partly copied over from .NET's internal RegexParser 
		class and modified for performance reasons: we don't want to escape 
		'[^]' chars, but Regex.Escape() does. Besides, wen need 
		'*' and '?' as wildcard chars.
		-------------------------------------------------------------- */

        const byte W = 6;    // wildcard char
        const byte Q = 5;    // quantifier
        const byte S = 4;    // ordinary stopper
        const byte Z = 3;    // ScanBlank stopper
        const byte X = 2;    // whitespace
        const byte E = 1;    // should be escaped

        /*
         * For categorizing ASCII characters.
        */
        private static readonly byte[] _category = new byte[]
        {
            // 0 1 2 3 4 5 6 7 8 9 A B C D E F 0 1 2 3 4 5 6 7 8 9 A B C D E F
               0,0,0,0,0,0,0,0,0,X,X,0,X,X,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            //   ! " # $ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ?
               X,0,0,Z,S,0,0,0,S,S,W,Q,0,0,S,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,W,
            // @ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z [ \ ] ^ _
               0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,S,0,0,0,
            // ' a b c d e f g h i j k l m n o p q r s t u v w x y z { | } ~
               0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,Q,S,0,0,0
        };

        private static bool IsMetachar(char ch)
        {
            return (ch <= '|' && _category[ch] >= E);
        }

        private static bool IsGlob(char ch)
        {
            return (ch <= '|' && _category[ch] >= W);
        }

        private static string ToGlobPattern(string input)
        {
            Guard.NotNull(input, nameof(input));

            for (int i = 0; i < input.Length; i++)
            {
                if (IsMetachar(input[i]))
                {
                    var psb = PooledStringBuilder.Rent("^");
                    var sb = (StringBuilder)psb;

                    char ch = input[i];
                    int lastpos;

                    sb.Append(input, 0, i);
                    do
                    {
                        if (IsGlob(ch))
                        {
                            sb.Append('.'); // '?' > '.'
                            if (ch == '*') sb.Append('*'); // '*' > '.*'
                        }
                        else
                        {
                            sb.Append('\\');
                            switch (ch)
                            {
                                case '\n':
                                    ch = 'n';
                                    break;
                                case '\r':
                                    ch = 'r';
                                    break;
                                case '\t':
                                    ch = 't';
                                    break;
                                case '\f':
                                    ch = 'f';
                                    break;
                            }
                            sb.Append(ch);
                        }

                        i++;
                        lastpos = i;

                        while (i < input.Length)
                        {
                            ch = input[i];
                            if (IsMetachar(ch))
                                break;

                            i++;
                        }

                        sb.Append(input, lastpos, i - lastpos);
                    } while (i < input.Length);

                    sb.Append('$');
                    return psb.ToStringAndReturn();
                }
            }

            return '^' + input + '$';
        }

        #endregion
    }

}
