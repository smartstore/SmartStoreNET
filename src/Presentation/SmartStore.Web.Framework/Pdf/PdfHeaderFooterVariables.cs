namespace SmartStore.Web.Framework.Pdf
{
    public class PdfHeaderFooterVariables
    {
        /// <summary>
        /// The number of the page currently being printed
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The number of the first page to be printed
        /// </summary>
        public int FromPage { get; set; }

        /// <summary>
        /// The number of the last page to be printed
        /// </summary>
        public int ToPage { get; set; }

        /// <summary>
        /// The URL of the page being printed
        /// </summary>
        public string WebPage { get; set; }

        /// <summary>
        /// The name of the current section
        /// </summary>
        public string Section { get; set; }

        /// <summary>
        /// The name of the current subsection
        /// </summary>
        public string SubSection { get; set; }

        /// <summary>
        /// The title of the of the current page object
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The title of the output document
        /// </summary>
        public string DocTitle { get; set; }

        /// <summary>
        /// The number of the page in the current site being converted
        /// </summary>
        public int SitePage { get; set; }

        /// <summary>
        /// The number of pages in the current site being converted
        /// </summary>
        public int SitePages { get; set; }
    }
}
