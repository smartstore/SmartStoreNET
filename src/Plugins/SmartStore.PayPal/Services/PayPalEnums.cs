
namespace SmartStore.PayPal.Services
{
	public enum PayPalPaymentInstructionItem
	{
		Reference = 0,
		BankRoutingNumber,
		Bank,
		Bic,
		Iban,
		AccountHolder,
		AccountNumber,
		Amount,
		PaymentDueDate,
		Details
	}

	public enum PayPalMessage
	{
		Message = 0,
		Event,
		EventId,
		State,
		Amount,
		PaymentId
	}
}