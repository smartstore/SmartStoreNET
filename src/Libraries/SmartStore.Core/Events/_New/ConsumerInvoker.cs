using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Autofac;
using SmartStore.ComponentModel;
using SmartStore.Core.Async;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;

namespace SmartStore.Core.Events
{
	public interface IConsumerInvoker
	{
		void Invoke<TMessage>(ConsumerDescriptor descriptor, IConsumer consumer, ConsumeContext<TMessage> envelope) where TMessage : class;
	}

	public class ConsumerInvoker : IConsumerInvoker
	{
		public ConsumerInvoker(IComponentContext container)
		{
			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public void Invoke<TMessage>(ConsumerDescriptor descriptor, IConsumer consumer, ConsumeContext<TMessage> envelope) where TMessage : class
		{
			var d = descriptor;
			var p = descriptor.WithEnvelope ? (object)envelope : envelope.Message;
			var fastInvoker = FastInvoker.GetInvoker(d.Method);

			var items = HttpContext.Current.GetItem("ConsumerInvoker", () => new List<ConsumerDescriptor>());
			items.Add(d);

			if (!d.FireForget && !d.IsAsync)
			{
				// The all synch case
				try
				{
					InvokeCore();
				}
				catch (Exception ex)
				{
					HandleException(ex, d);
				}
			}
			else if (!d.FireForget && d.IsAsync)
			{
				// The awaitable Task case
				BeginInvoke((Task)InvokeCore(), EndInvoke, d);
			}
			else if (d.FireForget && !d.IsAsync)
			{
				// A synch method should be executed async (without awaiting)
				AsyncRunner.Run((c, ct, state) => InvokeCore(c, ct), d)
					.ContinueWith(t => EndInvoke(t), /*TaskContinuationOptions.OnlyOnFaulted*/ TaskContinuationOptions.None)
					.ConfigureAwait(false);
			}
			else if (d.FireForget && d.IsAsync)
			{
				// An async (Task) method should be executed without awaiting
				AsyncRunner.Run((c, ct) => (Task)InvokeCore(c, ct), d)
					.ContinueWith(t => EndInvoke(t), TaskContinuationOptions.OnlyOnFaulted)
					.ContinueWith(t =>
					{
						var xxx = t.AsyncState;
					})
					.ConfigureAwait(false);
			}

			object InvokeCore(IComponentContext c = null, CancellationToken cancelToken = default(CancellationToken))
			{
				if (d.Parameters.Length == 0)
				{
					// Only one method param: the message!
					return fastInvoker.Invoke(consumer, p);
				}

				c = c ?? EngineContext.Current.ContainerManager.Scope();

				var parameters = new object[d.Parameters.Length + 1];
				parameters[0] = p;

				int i = 0;
				foreach (var obj in ResolveParameters(c, d, cancelToken).ToArray())
				{
					i++;
					parameters[i] = obj;
				}

				return fastInvoker.Invoke(consumer, parameters);
			}
		}

		/// <summary>
		/// Wraps a <see cref="Task"/> into the Begin method of an APM pattern.
		/// </summary>
		/// <param name="task">The task to wrap.</param>
		/// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
		/// <param name="descriptor">The state passed into the Begin method of the APM pattern.</param>
		/// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
		protected IAsyncResult BeginInvoke(Task task, AsyncCallback callback, ConsumerDescriptor descriptor)
		{
			var tcs = new TaskCompletionSource<object>(descriptor, TaskCreationOptions.RunContinuationsAsynchronously);

			// "_ =" to discard 'async/await' compiler warning
			_ = AwaitCompletionAsync(task, callback, tcs, descriptor);

			return tcs.Task;
		}

		private async Task AwaitCompletionAsync(
			Task task, 
			AsyncCallback callback, 
			TaskCompletionSource<object> tcs, 
			ConsumerDescriptor descriptor)
		{
			try
			{
				await task;
				tcs.TrySetResult(null);
			}
			catch (OperationCanceledException ex)
			{
				tcs.TrySetCanceled(ex.CancellationToken);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
			finally
			{
				callback?.Invoke(tcs.Task);
			}
		}

		protected virtual void EndInvoke(IAsyncResult asyncResult)
		{
			var task = (Task)asyncResult;

			if (task.IsFaulted && task.Exception != null)
			{
				HandleException(task.Exception, (ConsumerDescriptor)asyncResult.AsyncState);
			}
			if (HttpContext.Current != null)
			{
				var d = HttpContext.Current.Items["ConsumerInvoker"];
			}
		}

		private void HandleException(Exception ex, ConsumerDescriptor descriptor)
		{
			if (ex is AggregateException ae)
			{
				ae.InnerExceptions.Each(x => Logger.Error(x));
			}
			else
			{
				Logger.Error(ex);
			}

			if (!descriptor.FireForget)
			{
				throw ex;
			}
		}

		protected internal virtual IEnumerable<object> ResolveParameters(
			IComponentContext container, 
			ConsumerDescriptor descriptor, 
			CancellationToken cancelToken)
		{
			foreach (var p in descriptor.Parameters)
			{
				if (p.ParameterType == typeof(CancellationToken))
				{
					yield return cancelToken;
				}
				else
				{
					yield return container.Resolve(p.ParameterType);
				}
			}
		}

		//private void NoContext(Action action)
		//{
		//	var synchContext = SynchronizationContext.Current;
		//	SynchronizationContext.SetSynchronizationContext(null);

		//	try
		//	{
		//		action();
		//	}
		//	finally
		//	{
		//		SynchronizationContext.SetSynchronizationContext(synchContext);
		//	}
		//}

		//private Task NoContext(Func<Task> func)
		//{
		//	var synchContext = SynchronizationContext.Current;
		//	SynchronizationContext.SetSynchronizationContext(null);

		//	try
		//	{
		//		return func();
		//	}
		//	finally
		//	{
		//		SynchronizationContext.SetSynchronizationContext(synchContext);
		//	}
		//}
	}
}
