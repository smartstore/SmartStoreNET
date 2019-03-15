namespace SmartStore.Web.Framework.UI.Blocks
{
    public interface IBindableBlock : IBlock
    {
        string BindEntityName { get; set; }
        int? BindEntityId { get; set; }
    }
}
