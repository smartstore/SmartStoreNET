
using System;
using System.Threading.Tasks;

namespace SmartStore.Services.Tasks
{
    /// <summary>
    /// Interface that should be implemented by background tasks
    /// </summary>
    public partial interface ITask
    {
        /// <summary>
        /// Executes a task synchronously
        /// </summary>
		/// <param name="ctx">
		/// The execution context
		/// </param>
        void Execute(TaskExecutionContext ctx);
    }

    /// <summary>
    /// Interface that should be implemented by background tasks
    /// </summary>
    public interface IAsyncTask : ITask
    {
        /// <summary>
        /// Executes a task asynchronously
        /// </summary>
        /// <param name="ctx">
        /// The execution context
        /// </param>
        Task ExecuteAsync(TaskExecutionContext ctx);
    }

    public abstract class AsyncTask : IAsyncTask
    {
        public void Execute(TaskExecutionContext ctx)
        {
            throw new NotSupportedException();
        }

        public abstract Task ExecuteAsync(TaskExecutionContext ctx);
    }
}
