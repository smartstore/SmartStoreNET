
(function (smNetClient, $, undefined) {

	smNetClient.init = function () {
		$('#GetData').click(GetData);
		$('#AddOrderNote').click(AddOrderNote);
	};

	function GetData() {
		$('#Outputs').empty();

		smNetConsumer.startRequest({
			resource: GetResource(),
			content: null,
			beforeSend: BeforeSend,
			fail: Fail,
			done: Done
		});
	}

	function AddOrderNote() {
		$('#Outputs').empty();

		smNetConsumer.startRequest({
			resource: smNetConsumer.settings.odataService + '/Orders?$top=1&$select=Id',
			content: null,
			beforeSend: BeforeSend,
			fail: Fail,
			done: function (data, textStatus, jqXHR) {
				Done(data, textStatus, jqXHR);

				if (data && data.value && data.value.length > 0) {
					smNetConsumer.startRequest({
						method: 'POST',
						resource: smNetConsumer.settings.odataService + '/OrderNotes',
						content: { OrderId: data.value[0].Id, Note: 'Hello € wörld!', DisplayToCustomer: false, CreatedOnUtc: (new Date()).toISOString() },
						beforeSend: BeforeSend,
						done: Done,
						fail: Fail
					});
				}
				else {
					Output('No order found!');
				}
			}
		});
	}

	function GetResource() {
		return $('input[name=service]').val() + $('input[name=resource]').val();
	}

	function BeforeSend(jqXHR, settings) {
		Output('<b>Request</b> ajax settings: ' + JsonStringify(settings), true);
		Output('<img class="spinner" src="spinner.gif" style="width:128px; height:15px;" />');
	}

	function Done(data, textStatus, jqXHR) {
		$('img.spinner').parent().remove();
		Output('<b>Response</b> ok:\r\n' + jqXHR.getAllResponseHeaders() + '\r\n' + JsonStringify(data));
	}

	function Fail(jqXHR, textStatus, errorThrown) {
		$('img.spinner').parent().remove();
		Output('Response failed (' + (errorThrown || '?') + '):\r\n' + jqXHR.getAllResponseHeaders() + '\r\n' + jqXHR.responseText);
	}

	function Output(str, clear) {
		if (str) {
			$('#Outputs').append((clear ? '<pre class="clear">' : '<pre>') + str + '</pre>');
			$('html, body').animate({ scrollTop: $(document).height() }, 'slow');
		}
	}

	function JsonStringify(obj) {
		return obj ? JSON.stringify(obj, undefined, 2) : null;
	}

}(window.smNetClient = window.smNetClient || {}, jQuery));