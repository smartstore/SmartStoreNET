using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Localization;
using SmartStore.Core.Search;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Search
{
    public abstract class SearchResultModelBase : ModelBase
    {
        protected SearchResultModelBase()
        {
            HitGroups = new List<HitGroup>();
        }

        public abstract IList<HitGroup> HitGroups { get; protected set; }

        /// <summary>
        /// Adds spell checker suggestions to this model.
        /// </summary>
        /// <param name="suggestions">Spell checker suggestions.</param>
        /// <param name="T">Localizer.</param>
        /// <param name="hitUrl">Function to get the hit URL for a suggestion.</param>
        public void AddSpellCheckerSuggestions(string[] suggestions, Localizer T, Func<string, string> hitUrl)
        {
            if (suggestions.Length == 0)
            {
                return;
            }

            var hitGroup = new HitGroup(this)
            {
                Name = "SpellChecker",
                DisplayName = T("Search.DidYouMean"),
                Ordinal = -100
            };

            hitGroup.Hits.AddRange(suggestions.Select(x => new HitItem
            {
                Label = x,
                Url = hitUrl(x),
                NoHighlight = true
            }));

            HitGroups.Add(hitGroup);
        }

        /// <summary>
        /// Highlights chosen terms in a text, extracting the most relevant sections.
        /// </summary>
        /// <param name="input">Text to highlight terms in.</param>
        /// <param name="fieldName">Name of the field to highlight.</param>
        /// <param name="query">Search query.</param>
        /// <param name="engine">Search engine. Use <c>null</c> to fallback to simple string highlighting method.</param>
        /// <param name="preMatch">Pre matching HTML.</param>
        /// <param name="postMatch">Post matching HTML.</param>
        /// <returns>Highlighted text fragments.</returns>
        public string Highlight(
            string input,
            string fieldName,
            ISearchQuery query,
            ISearchEngine engine = null,
            string preMatch = "<strong>",
            string postMatch = "</strong>")
        {
            Guard.NotEmpty(fieldName, nameof(fieldName));

            if (query?.Term == null || input.IsEmpty())
            {
                return input;
            }

            string hilite = null;

            if (engine != null)
            {
                try
                {
                    hilite = engine.Highlight(input, fieldName, preMatch, postMatch);
                }
                catch { }
            }

            if (hilite.HasValue())
            {
                return hilite;
            }

            return input.HighlightKeywords(query.Term, preMatch, postMatch);
        }

        #region Nested classes

        public class HitGroup : IOrdered
        {
            public HitGroup(SearchResultModelBase parent)
            {
                Guard.NotNull(parent, nameof(parent));

                Parent = parent;
                Hits = new List<HitItem>();
            }

            public string Name { get; set; }
            public string DisplayName { get; set; }
            public int Ordinal { get; set; }
            public IList<HitItem> Hits { get; private set; }
            public SearchResultModelBase Parent { get; private set; }
        }

        public class HitItem
        {
            public string Label { get; set; }
            public string Tag { get; set; }
            public string Url { get; set; }
            public bool NoHighlight { get; set; }
        }

        #endregion
    }
}