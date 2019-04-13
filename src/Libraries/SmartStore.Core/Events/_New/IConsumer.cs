using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using SmartStore.Core.Data;
using SmartStore.Core.Email;

namespace SmartStore.Core.Events
{
	public interface IConsumer
	{
	}

	public class TestConsumer : IConsumer
	{
		//[FireForget]
		//public void Handle(AppStartedEvent e)
		//{
		//	Thread.Sleep(300);
		//}

		//public void Consume(ConsumeContext<AppInitScheduledTasksEvent> e)
		//{
		//	//throw new NotImplementedException();
		//}

		////[FireForget]
		//public async Task HandleEvent(AppRegisterGlobalFiltersEvent e, CancellationToken ct, IDbContext dbContext, IEmailSender sender)
		//{
		//	var ctx = HttpContext.Current;
		//	//await Task.Delay(1000);
		//	var products = await dbContext.Set<Domain.Catalog.Product>().ToListAsync();
		//	//await Task.Delay(500);
		//	var categories = await dbContext.Set<Domain.Catalog.Category>().ToListAsync();
		//	var media = await dbContext.Set<Domain.Media.Picture>().ToListAsync();

		//	products = await dbContext.Set<Domain.Catalog.Product>().ToListAsync();
		//	categories = await dbContext.Set<Domain.Catalog.Category>().ToListAsync();
		//	media = await dbContext.Set<Domain.Media.Picture>().ToListAsync();

		//	products = await dbContext.Set<Domain.Catalog.Product>().ToListAsync();
		//	categories = await dbContext.Set<Domain.Catalog.Category>().ToListAsync();
		//	media = await dbContext.Set<Domain.Media.Picture>().ToListAsync();

		//	System.Diagnostics.Debug.WriteLine("HandleEvent fertig!");
		//	ctx = HttpContext.Current;
		//	//throw new NotSupportedException();
		//}

		[FireForget]
		public void HandleEvent(AppStartedEvent e, CancellationToken ct, IDbContext dbContext, IEmailSender sender)
		{
			var ctx = HttpContext.Current;
			//await Task.Delay(1000);
			var products = dbContext.Set<Domain.Catalog.Product>().ToList();
			//await Task.Delay(500);
			var categories = dbContext.Set<Domain.Catalog.Category>().ToList();
			var media = dbContext.Set<Domain.Media.Picture>().ToList();

			products = dbContext.Set<Domain.Catalog.Product>().ToList();
			categories = dbContext.Set<Domain.Catalog.Category>().ToList();
			media = dbContext.Set<Domain.Media.Picture>().ToList();

			products = dbContext.Set<Domain.Catalog.Product>().ToList();
			categories = dbContext.Set<Domain.Catalog.Category>().ToList();
			media = dbContext.Set<Domain.Media.Picture>().ToList();

			System.Diagnostics.Debug.WriteLine("HandleEvent fertig!");
			ctx = HttpContext.Current;

			//throw new SystemException();
		}
	}
}
