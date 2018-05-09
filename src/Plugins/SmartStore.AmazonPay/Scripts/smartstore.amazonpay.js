
(function (amazonpay, $, undefined) {

	amazonpay.init = function () {

		// show/hide transaction type warning
		$('#TransactionType').change(function () {
			$('#TransactionTypeWarning').toggle($(this).val() === '2');
		}).trigger('change');

		// show/hide status fetching warning
		$('#DataFetching').change(function () {
			var val = $(this).val(),
				configTable = $('#AmazonPayConfigTable');

			$('#DataFetchingWarning').toggle(val === '1');

			configTable.find('.data-fetching').hide();
			if (val === '1')
				configTable.find('.data-fetching-ipn').show();
			else if (val === '2')
				configTable.find('.data-fetching-polling').show();

		}).trigger('change');

		// show/hide inform customer add errors option
		$('#InformCustomerAboutErrors').change(function () {
			$('#InformCustomerAddErrorsContainer').toggle($(this).is(':checked'));
		}).trigger('change');

	};

}(window.amazonpay = window.amazonpay || {}, jQuery));