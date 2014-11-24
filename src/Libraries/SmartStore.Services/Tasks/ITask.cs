
namespace SmartStore.Services.Tasks
{
    /// <summary>
    /// Interface that should be implemented by each task
    /// </summary>
    public partial interface ITask
    {
        /// <summary>
        /// Execute task
        /// </summary>
		/// <param name="ctx">
		/// The execution context
		/// </param>
        void Execute(TaskExecutionContext ctx);
    }
}
