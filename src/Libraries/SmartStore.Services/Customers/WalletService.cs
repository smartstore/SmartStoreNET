using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Customers
{
	public partial class WalletService : IWalletService
	{
		private readonly IRepository<WalletHistory> _walletHistoryRepository;

		public WalletService(
			IRepository<WalletHistory> walletHistoryRepository)
		{
			_walletHistoryRepository = walletHistoryRepository;
		}


	}
}
