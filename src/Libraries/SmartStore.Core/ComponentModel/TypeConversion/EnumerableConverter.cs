using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace SmartStore.ComponentModel
{
	[SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
	public class EnumerableConverter<T> : TypeConverterBase
	{
		private readonly Func<IEnumerable<T>, object> _activator;
		private readonly ITypeConverter _elementTypeConverter;

		public EnumerableConverter(Type sequenceType)
			: base(typeof(object))
		{
			_elementTypeConverter = TypeConverterFactory.GetConverter<T>();
			if (_elementTypeConverter == null)
				throw new InvalidOperationException("No type converter exists for type " + typeof(T).FullName);

			_activator = CreateSequenceActivator(sequenceType);
        }

		[SuppressMessage("ReSharper", "RedundantLambdaSignatureParentheses")]
		private static Func<IEnumerable<T>, object> CreateSequenceActivator(Type sequenceType)
		{
			// Default is IEnumerable<T>
			Func<IEnumerable<T>, object> activator = null;

			var t = sequenceType;

			if (t == typeof(IEnumerable<T>))
			{
				activator = (x) => x;
			}
			else if (t == (typeof(IReadOnlyCollection<T>)) || t == (typeof(IReadOnlyList<T>)))
			{
				activator = (x) => x.AsReadOnly();
			}
			else if (t.IsAssignableFrom(typeof(List<T>)))
			{
				activator = (x) => x.ToList();
			}
			else if (t.IsAssignableFrom(typeof(HashSet<T>)))
			{
				activator = (x) => new HashSet<T>(x);
			}
			else if (t.IsAssignableFrom(typeof(Queue<T>)))
			{
				activator = (x) => new Queue<T>(x);
			}
			else if (t.IsAssignableFrom(typeof(Stack<T>)))
			{
				activator = (x) => new Stack<T>(x);
			}
			else if (t.IsAssignableFrom(typeof(LinkedList<T>)))
			{
				activator = (x) => new LinkedList<T>(x);
			}
			else if (t.IsAssignableFrom(typeof(ConcurrentBag<T>)))
			{
				activator = (x) => new ConcurrentBag<T>(x);
			}
			else if (t.IsAssignableFrom(typeof(ArraySegment<T>)))
			{
				activator = (x) => new ArraySegment<T>(x.ToArray());
			}

			if (activator == null)
			{
				throw new InvalidOperationException("'{0}' is not a valid type for enumerable conversion.".FormatInvariant(sequenceType.FullName));
			}

			return activator;
		}

		public override bool CanConvertFrom(Type type)
		{
			return type == typeof(string) || typeof(IConvertible).IsAssignableFrom(type);
		}

		public override bool CanConvertTo(Type type)
		{
			return type == typeof(string);
		}

		public override object ConvertFrom(CultureInfo culture, object value)
		{
			if (value == null)
			{
				return _activator(Enumerable.Empty<T>());
			}

			if (value is string)
			{
				var items = GetStringArray((string)value);

				var result = items
					.Select(x => _elementTypeConverter.ConvertFrom(culture, x))
					.Where(x => x != null)
					.Cast<T>();
				
				return _activator(result);
			}

			if (value is IConvertible)
			{
				var result2 = (new object[] { value })
					.Select(x => Convert.ChangeType(value, typeof(T)))
					.Cast<T>();

				return _activator(result2);
			}

			return base.ConvertFrom(culture, value);
		}

		public override object ConvertTo(CultureInfo culture, string format, object value, Type to)
		{
			if (to == typeof(string))
			{
				string result = string.Empty;

				if (value is IEnumerable<T> enumerable)
				{
					// we don't use string.Join() because it doesn't support invariant culture
					foreach (var token in enumerable)
					{
						var str = _elementTypeConverter.ConvertTo(culture, format, token, typeof(string));
                        result += str + ",";
					}

					result = result.TrimEnd(',');
				}

				return result;
			}

			return base.ConvertTo(culture, format, value, to);
		}

		protected virtual string[] GetStringArray(string input)
		{
			var result = input.SplitSafe(null);

			Array.ForEach(result, s => s.Trim());

			return result;
		}
	}
}
