using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;

namespace SmartStore.Services.Messages.New
{
	public partial class MessageModelProvider
	{
		private readonly IEmailAccountService _emailAccountService;

		private readonly MediaSettings _mediaSettings;
		private readonly ContactDataSettings _contactDataSettings;
		private readonly MessageTemplatesSettings _templatesSettings;
		private readonly CatalogSettings _catalogSettings;
		private readonly TaxSettings _taxSettings;
		private readonly CompanyInformationSettings _companyInfoSettings;
		private readonly BankConnectionSettings _bankConnectionSettings;
		private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly SecuritySettings _securitySettings;

		public virtual void AddCompanyModel(dynamic model)
		{
			model.Company = new HybridExpando(_companyInfoSettings);
		}

		public virtual void AddBankModel(dynamic model)
		{
			model.Bank = new HybridExpando(_bankConnectionSettings);
		}

		public virtual void AddContactModel(dynamic model)
		{
			var contact = new HybridExpando(_contactDataSettings) as dynamic;
			model.Contact = contact;

			// TODO: (mc) Liquid > Use following aliases in Partials
			// Aliases
			contact.Phone = new
			{
				Company = _contactDataSettings.CompanyTelephoneNumber,
				Hotline = _contactDataSettings.HotlineTelephoneNumber,
				Mobile = _contactDataSettings.MobileTelephoneNumber,
				Fax = _contactDataSettings.CompanyFaxNumber
			};

			contact.Email = new
			{
				Company = _contactDataSettings.CompanyEmailAddress,
				Webmaster = _contactDataSettings.WebmasterEmailAddress,
				Support = _contactDataSettings.SupportEmailAddress,
				Contact = _contactDataSettings.ContactEmailAddress
			};
		}

		public virtual void AddStoreModel(dynamic model, Store store)
		{
			Guard.NotNull(store, nameof(store));

			var he = new HybridExpando(store) as dynamic;
			he.Email = _emailAccountService.GetDefaultEmailAccount()?.Email;
			// TODO: (mc) Liquid
			he.Logo = new
			{
				Src = "",
				Href = "",
				Width = 0,
				Height = 0
			};

			// TODO: (mc) Liquid > GetSupplierIdentification() as Partial

			model.Store = he;
		}
	}
}
