using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SmartStore.Core.Domain.Blogs
{
    public static class BlogExtensions
    {
	    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
	    public static string[] ParseTags(this BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            var parsedTags = new List<string>();
            if (!String.IsNullOrEmpty(blogPost.Tags))
            {
                var tags2 = blogPost.Tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var tag2 in tags2)
                {
					var tmp = tag2.Trim();
					if (!String.IsNullOrEmpty(tmp))
						parsedTags.Add(tmp);
                }
            }
            return parsedTags.ToArray();
        }
    }
}
