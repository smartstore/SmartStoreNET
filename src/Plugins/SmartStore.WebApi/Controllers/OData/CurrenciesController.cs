using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class CurrenciesController : WebApiEntityController<Currency, ICurrencyService>
	{
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Read)]
		public IQueryable<Currency> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Read)]
		public SingleResult<Currency> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Create)]
		public IHttpActionResult Post(Currency entity)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			entity = FulfillPropertiesOn(entity);
			Service.InsertCurrency(entity);

			return Created(entity);
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Update)]
		public async Task<IHttpActionResult> Put(int key, Currency entity)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			if (key != entity.Id)
			{
				return BadRequest();
			}

			entity = FulfillPropertiesOn(entity);

			try
			{
				Service.UpdateCurrency(entity);
			}
			catch (DbUpdateConcurrencyException)
			{
				if (await Repository.GetByIdAsync(key) == null)
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return Updated(entity);
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Update)]
		public async Task<IHttpActionResult> Patch(int key, Delta<Currency> model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var entity = await Repository.GetByIdAsync(key);
			if (entity == null)
			{
				return NotFound();
			}

			model?.Patch(entity);
			entity = FulfillPropertiesOn(entity);

			try
			{
				Service.UpdateCurrency(entity);
			}
			catch (DbUpdateConcurrencyException)
			{
				if (await Repository.GetByIdAsync(key) == null)
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return Updated(entity);
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Delete)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var entity = await Repository.GetByIdAsync(key);
			if (entity == null)
			{
				return NotFound();
			}

			Service.DeleteCurrency(entity);

			return StatusCode(HttpStatusCode.NoContent);
		}
	}
}
