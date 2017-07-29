﻿using System;
using System.Collections.Generic;
using SmartStore.Services.Authentication.External;

namespace SmartStore.AmazonPay.Services
{
	[Serializable]
	public class AmazonAuthenticationParameters : OpenAuthenticationParameters
	{
		private IList<UserClaims> _claims;

		public override string ProviderSystemName
		{
			get { return AmazonPayPlugin.SystemName; }
		}

		public override IList<UserClaims> UserClaims
		{
			get { return _claims; }
		}

		public void AddClaim(UserClaims claim)
		{
			if (_claims == null)
			{
				_claims = new List<UserClaims>();
			}

			_claims.Add(claim);
		}
	}
}