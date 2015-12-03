using SmartStore.Web.Framework;
using System;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.WebApi.Models
{
	public class WebApiUserModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.Username")]
		public string Username { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.Email")]
		public string Email { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.AdminComment")]
		public string AdminComment { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.PublicKey")]
		public string PublicKey { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.SecretKey")]
		public string SecretKey { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.ApiEnabled")]
		public bool Enabled { get; set; }

        [SmartResourceDisplayName("Plugins.Api.WebApi.LastRequest")]
		public DateTime? LastRequest { get; set; }

		public string ButtonDisplayEnable
		{
			get
			{
				return (PublicKey.HasValue() && !Enabled) ? "inline-block" : "none";
			}
		}
		public string ButtonDisplayDisable
		{
			get
			{
				return Enabled ? "inline-block" : "none";
			}
		}
		public string ButtonDisplayRemoveKeys
		{
			get
			{
				return PublicKey.HasValue() ? "inline-block" : "none";
			}
		}
		public string ButtonDisplayCreateKeys
		{
			get
			{
				return !PublicKey.HasValue() ? "inline-block" : "none";
			}
		}
	}
}
