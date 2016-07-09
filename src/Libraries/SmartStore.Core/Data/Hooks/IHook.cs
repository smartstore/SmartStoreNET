
namespace SmartStore.Core.Data.Hooks
{
    public interface IHook
    {
        void HookObject(object entity, HookEntityMetadata metadata);

		/// <summary>
		/// Gets the entity state to listen for.
		/// </summary>
		EntityState HookStates { get; }

		/// <summary>
		/// Indicates whether the hook instance can be processed for the given <see cref="EntityState"/>
		/// </summary>
		/// <param name="state">The state of the entity</param>
		/// <returns><c>true</c> when the hook should be processed, <c>false</c> otherwise</returns>
		bool CanProcess(EntityState state);
	}
}
