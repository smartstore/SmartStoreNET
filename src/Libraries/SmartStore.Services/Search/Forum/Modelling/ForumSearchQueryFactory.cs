using System.Web;

namespace SmartStore.Services.Search.Modelling
{
    /*
		TOKENS:
		===============================
		q	-	Search term
		i	-	Page index
		s	-	Page size
		o	-	Order by
		v	-	View Mode
	*/

    public class ForumSearchQueryFactory : IForumSearchQueryFactory
    {
        protected readonly HttpContextBase _httpContext;

        public ForumSearchQueryFactory(
            HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        public ForumSearchQuery Current { get; private set; }

        public ForumSearchQuery CreateFromQuery()
        {
            var ctx = _httpContext;

            if (ctx.Request == null)
                return null;

            throw new System.NotImplementedException();
        }
    }
}
