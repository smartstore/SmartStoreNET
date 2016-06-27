using System;
using System.Linq;
using Newtonsoft.Json;

namespace SmartStore.Services.DataExchange.Csv
{
	/// <summary>
	/// Specifies the action to take when a parsing error has occured.
	/// </summary>
	public enum ParseErrorAction
	{
		/// <summary>
		/// Raises the <see cref="M:CsvReader.ParseError"/> event.
		/// </summary>
		RaiseEvent = 0,

		/// <summary>
		/// Tries to advance to next line.
		/// </summary>
		AdvanceToNextLine = 1,

		/// <summary>
		/// Throws an exception.
		/// </summary>
		ThrowException = 2,
	}

	/// <summary>
	/// Specifies the action to take when a field is missing.
	/// </summary>
	public enum MissingFieldAction
	{
		/// <summary>
		/// Treat as a parsing error.
		/// </summary>
		ParseError = 0,

		/// <summary>
		/// Replaces by an empty value.
		/// </summary>
		ReplaceByEmpty = 1,

		/// <summary>
		/// Replaces by a null value (<see langword="null"/>).
		/// </summary>
		ReplaceByNull = 2,
	}

	public class CsvConfiguration
	{
		private char _delimiter;
		private char _escape;
		private char _quote;
		private string _quoteString;
		private char[] _quotableChars;

		public CsvConfiguration()
		{
			_escape = '"';
			_delimiter = ';';
			_quote = '"';
			_quoteString = new String(new char[] { _escape, _quote });

			Comment = '#';
			HasHeaders = true;
			SkipEmptyLines = true;
			SupportsMultiline = true;
			DefaultHeaderName = "Column";

			BuildQuotableChars();
        }

		private void BuildQuotableChars()
		{
			_quotableChars = new char[] { '\r', '\n', _delimiter, _quote };
        }

		internal char[] QuotableChars
		{
			get { return _quotableChars; }
		}

		/// <summary>
		/// Gets an Excel friendly configuration where the result can be directly edited by Excel
		/// </summary>
		public static CsvConfiguration ExcelFriendlyConfiguration
		{
			get
			{
				return new CsvConfiguration
				{
					Delimiter = ';',
					Quote = '"',
					Escape = '"',
					SupportsMultiline = false
				};
			}
		}

		/// <summary>
		/// Gets an array with preset characters
		/// </summary>
		public static char[] PresetCharacters
		{
			get { return new char[] { '\n', '\r', '\0' }; }
		}

		/// <summary>
		/// Gets the comment character indicating that a line is commented out (default: #).
		/// </summary>
		/// <value>The comment character indicating that a line is commented out.</value>
		public char Comment
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the escape character letting insert quotation characters inside a quoted field (default: ").
		/// </summary>
		/// <value>The escape character letting insert quotation characters inside a quoted field.</value>
		public char Escape
		{
			get { return _escape; }
			set
			{
				if (value == _escape)
					return;

				if (PresetCharacters.Contains(value))
				{
					throw new SmartException("'{0}' is not a valid escape char.".FormatInvariant(value));
				}
				if (value == _delimiter)
				{
					throw new SmartException("Escape and delimiter chars cannot be equal in CSV files.");
				}

				_escape = value;
				_quoteString = new String(new char[] { _escape, _quote });
			}
		}

		/// <summary>
		/// Gets the delimiter character separating each field (default: ;).
		/// </summary>
		/// <value>The delimiter character separating each field.</value>
		public char Delimiter
		{
			get { return _delimiter; }
			set
			{
				if (value == _delimiter)
					return;

				if (PresetCharacters.Contains(value))
				{
					throw new SmartException("'{0}' is not a valid delimiter char.".FormatInvariant(value));
				}
				if (value == _quote)
				{
					throw new SmartException("Quote and delimiter chars cannot be equal in CSV files.");
				}

				_delimiter = value;
				BuildQuotableChars();

			}
		}

		/// <summary>
		/// Gets the quotation character wrapping every field (default: ").
		/// </summary>
		/// <value>The quotation character wrapping every field.</value>
		public char Quote
		{
			get { return _quote; }
			set
			{
				if (value == _quote)
					return;

				if (PresetCharacters.Contains(value))
				{
					throw new SmartException("'{0}' is not a valid quote char.".FormatInvariant(value));
				}
				if (value == _delimiter)
				{
					throw new SmartException("Quote and delimiter chars cannot be equal in CSV files.");
				}

				_quote = value;
				_quoteString = new String(new char[] { _escape, _quote });
				BuildQuotableChars();
			}
		}

		/// <summary>
		/// Gets the concatenation of escape and quote char
		/// </summary>
		[JsonIgnore]
		public string QuoteString
		{
			get
			{
				return _quoteString;
			}
		}

		public bool QuoteAllFields
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates if field names are located on the first non commented line (default: true).
		/// </summary>
		/// <value><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</value>
		public bool HasHeaders
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates if spaces at the start and end of a field are trimmed (default: false).
		/// </summary>
		/// <value><see langword="true"/> if spaces at the start and end of a field are trimmed, otherwise, <see langword="false"/>.</value>
		public bool TrimValues
		{
			get;
			set;
		}

		/// <summary>
		/// Contains the value which denotes a DbNull-value.
		/// </summary>
		public string NullValue
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the default action to take when a parsing error has occured.
		/// </summary>
		/// <value>The default action to take when a parsing error has occured.</value>
		public ParseErrorAction DefaultParseErrorAction
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the action to take when a field is missing.
		/// </summary>
		/// <value>The action to take when a field is missing.</value>
		public MissingFieldAction MissingFieldAction
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating if the reader supports multiline fields (default: true).
		/// </summary>
		/// <value>A value indicating if the reader supports multiline field.</value>
		public bool SupportsMultiline
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating if the reader will skip empty lines (default: true).
		/// </summary>
		/// <value>A value indicating if the reader will skip empty lines.</value>
		public bool SkipEmptyLines
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the default header name when it is an empty string or only whitespaces (default: Column).
		/// The header index will be appended to the specified name.
		/// </summary>
		/// <value>The default header name when it is an empty string or only whitespaces.</value>
		public string DefaultHeaderName
		{
			get;
			set;
		}
	}
}
