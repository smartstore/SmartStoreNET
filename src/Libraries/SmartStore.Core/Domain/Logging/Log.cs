using System;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Logging;

namespace SmartStore.Core.Domain.Logging
{
    /// <summary>
    /// Represents a log record
    /// </summary>
    public partial class Log : BaseEntity
    {
		/// <summary>
		/// Gets or sets the log level identifier
		/// </summary>
		[Index("IX_Log_Level", IsUnique = false)]
		public int LogLevelId { get; set; }

        /// <summary>
        /// Gets or sets the short message
        /// </summary>
        public string ShortMessage { get; set; }

        /// <summary>
        /// Gets or sets the full exception
        /// </summary>
        public string FullMessage { get; set; }

        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the page URL
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the referrer URL
        /// </summary>
        public string ReferrerUrl { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the logger name
		/// </summary>
		[Index("IX_Log_Logger", IsUnique = false)]
		public string Logger { get; set; }

		/// <summary>
		/// Gets or sets the HTTP method
		/// </summary>
		public string HttpMethod { get; set; }

		/// <summary>
		/// Gets or sets the user name
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Gets or sets the log level
		/// </summary>
		public LogLevel LogLevel
        {
            get
            {
                return (LogLevel)this.LogLevelId;
            }
            set
            {
                this.LogLevelId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }
    }
}
