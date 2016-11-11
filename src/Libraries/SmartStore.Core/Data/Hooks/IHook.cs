
//namespace SmartStore.Core.Data.Hooks
//{
//    public interface IHook
//    {
//        void HookObject(object entity, HookEntityMetadata metadata);

//		/// <summary>
//		/// Indicates whether the hook instance can be processed for the given <see cref="EntityState"/>
//		/// </summary>
//		/// <param name="state">The state of the entity</param>
//		/// <returns><c>true</c> when the hook should be processed, <c>false</c> otherwise</returns>
//		bool CanProcess(EntityState state);

//		/// <summary>
//		/// Called after all entities in the current unit of work has been hooked
//		/// </summary>
//		void OnCompleted();
//	}

//	/// <summary>
//	/// A hook that is executed after an action.
//	/// </summary>
//	public interface IPostActionHook : IHook
//	{
//	}

//	/// <summary>
//	/// A hook that is executed before an action.
//	/// </summary>
//	public interface IPreActionHook : IHook
//	{
//		/// <summary>
//		/// Gets a value indicating whether the hook is only used after successful validation.
//		/// </summary>
//		/// <value>
//		///   <c>true</c> if requires validation; otherwise, <c>false</c>.
//		/// </value>
//		bool RequiresValidation { get; }
//	}
//}
