using System.Runtime.Serialization;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Blogs
{
	/// <summary>
	/// Represents a blog comment
	/// </summary>
	[DataContract]
	public partial class BlogComment : CustomerContent
    {
		/// <summary>
		/// Gets or sets the comment text
		/// </summary>
		[DataMember]
		public string CommentText { get; set; }

		/// <summary>
		/// Gets or sets the blog post identifier
		/// </summary>
		[DataMember]
		public int BlogPostId { get; set; }

		/// <summary>
		/// Gets or sets the blog post
		/// </summary>
		[DataMember]
		public virtual BlogPost BlogPost { get; set; }
    }
}