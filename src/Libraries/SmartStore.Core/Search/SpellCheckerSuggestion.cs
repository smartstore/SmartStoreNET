using System;
using SmartStore.Utilities;

namespace SmartStore.Core.Search
{
	public class SpellCheckerSuggestion : IEquatable<SpellCheckerSuggestion>
	{
		public SpellCheckerSuggestion(string term, int? frequency)
		{
			Guard.NotEmpty(term, nameof(term));

			Term = term;
			Frequency = frequency;
		}

		public string Term { get; private set; }
		public int? Frequency { get; private set; }

		public override bool Equals(object obj)
		{
			return ((IEquatable<SpellCheckerSuggestion>)this).Equals(obj as SpellCheckerSuggestion);
		}

		bool IEquatable<SpellCheckerSuggestion>.Equals(SpellCheckerSuggestion other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return this.Term.Equals(other.Term, StringComparison.OrdinalIgnoreCase)
				&& this.Frequency == other.Frequency;
		}

		public override int GetHashCode()
		{
			var combiner = HashCodeCombiner
				.Start()
				.Add(this.Term)
				.Add(this.Frequency);

			return combiner.CombinedHash;
		}

		public override string ToString()
		{
			return "{0}, Freq: {1}".FormatInvariant(Term, Frequency);
		}
	}
}
