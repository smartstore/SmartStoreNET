
(function (webApi, $, undefined) {

	webApi.init = function () {

		// grid button clicked
		$('#apiuser-grid').on('click', '.api-grid-button', function () {
			var container = $('#apiuser-grid-container');
			var url = container.attr('data-url') + '/' + $(this).attr('name') + '?customerId=' + $(this).parent().attr('data-id');

			container.doAjax({
				url: url,
				callbackSuccess: function (resp) {

					var grid = $('#apiuser-grid').data('tGrid');
					grid.currentPage = 1;
					grid.ajaxRequest();

				}
			});
			return false;
		});
	};


}(window.webApi = window.webApi || {}, jQuery));