using System;
using System.Globalization;
using System.Text;

namespace SmartStore.Services.Pdf
{
    public class PdfTocOptions : PdfPageOptions
    {
        public PdfTocOptions()
            : base()
        {
        }

        /// <summary>
        /// TOC creation enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The header text of the toc (default Table of Contents)
        /// </summary>
        public string TocHeaderText { get; set; }

        /// <summary>
        /// Do not use dotted lines in the toc
        /// </summary>
        public bool DisableDottedLines { get; set; }

        /// <summary>
        /// Do not link from toc to sections
        /// </summary>
        public bool DisableTocLinks { get; set; }

        /// <summary>
        /// For each level of headings in the toc indent by this length (default 1em)
        /// </summary>
        public string TocLevelIndendation { get; set; }

        /// <summary>
        /// For each level of headings in the toc the font is scaled by this factor (default 0.8)
        /// </summary>
        public float? TocTextSizeShrink { get; set; }


        public override void Process(string flag, StringBuilder builder)
        {
            builder.Append(" toc");

            if (TocHeaderText.HasValue())
            {
                builder.AppendFormat(" --toc-header-text \"{0}\"", TocHeaderText.Replace("\"", "\\\""));
            }

            if (DisableDottedLines)
            {
                builder.Append(" --disable-dotted-lines");
            }

            if (DisableTocLinks)
            {
                builder.Append(" --disable-toc-links");
            }

            if (TocLevelIndendation.HasValue())
            {
                builder.AppendFormat(" --toc-level-indentation {0}", TocLevelIndendation);
            }

            if (TocTextSizeShrink.HasValue)
            {
                builder.AppendFormat(" --toc-text-size-shrink {0}", TocTextSizeShrink.Value);
            }

            base.Process(flag, builder);
        }

    }
}
