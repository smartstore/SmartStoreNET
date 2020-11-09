namespace SmartStore.Web.Framework.UI
{
    public interface IMenuResolver
    {
        int Order { get; }

        bool Exists(string menuName);
        IMenu Resolve(string menuName);
    }
}
