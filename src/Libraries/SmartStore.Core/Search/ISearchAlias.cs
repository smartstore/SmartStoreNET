namespace SmartStore.Core.Search
{
    /// <summary>
    /// Represents an entity which supports SEO friendly search alias
    /// </summary>
    public interface ISearchAlias
    {
        int Id { get; set; }

        string Alias { get; set; }
    }
}
