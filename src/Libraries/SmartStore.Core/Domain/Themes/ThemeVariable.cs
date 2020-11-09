using System;

// codehint: sm-add (whole file)

namespace SmartStore.Core.Domain.Themes
{
    public class ThemeVariable : BaseEntity
    {
        /// <summary>
        /// Gets or sets the theme the variable belongs to
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the theme attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the theme attribute value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }

        protected override bool Equals(BaseEntity other)
        {
            var equals = base.Equals(other);
            if (!equals)
            {
                var o2 = other as ThemeVariable;
                if (o2 != null)
                {
                    equals = this.Theme.Equals(o2.Theme, StringComparison.OrdinalIgnoreCase) && this.Name == o2.Name;
                }
            }
            return equals;
        }
    }

}
