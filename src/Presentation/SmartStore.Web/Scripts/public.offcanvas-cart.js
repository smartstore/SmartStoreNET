;

/*
** Ajax cart implementation
*/
var AjaxCart = (function ($, window, document, undefined) {

	$(function () {
		// GLOBAL event handler
		$("body").on("click", ".ajax-cart-link", function (e) {
			//e.stopPropagation();
			return AjaxCart.executeRequest(this);
		});

		// Load summaries after page init
		ShopBar.loadSummaries(false /* animate */);
	});

	function createMessageObj(el) {
		return {
			success: { title: el.data("msg-success-title"), text: el.data("msg-success-text") },
			error: { title: el.data("msg-error-title"), text: el.data("msg-error-text") }
		}
	}

	/*
        attr("href") > href
    */
	function createCommand(el) {
		if (!_.isElement(el)) {
			return null;
		}

		el = $(el);

		var cmd = {
			src: el,
			type: el.data("type") || "cart", // or "wishlist" or "compare",
			action: el.data("action") || "add", // or "remove" or "addfromwishlist" or "addfromcart"
			href: el.data("href") || el.attr("href"),
			data: undefined // handled further below
		};

		if (el.data("form-selector")) {
			str = $(el.data("form-selector")).serialize();

			// HACK (MC)!
			// we changed the ModelType of the _AddToCart
			// from ...ProductModel.AddToCart to .ProductModel.
			// Therefore input names are not in the form anymore as the ShoppingCartController 
			// expects them. Hacking here ist much easier than refactoring the controller method.
			// But change this in future of couse.
			arr = str.split(".");
			if (arr.length == 3 && arr[1] == "AddToCart") {
				str = arr[0] + "." + arr[2];
			}

			cmd.data = str;
		}

		return cmd;
	}

	function verifyCommand(cmd) {
		return !!(cmd.href); // TODO: implement (MC)
	}

	var busy = false;

	return {

		executeRequest: function (cmd) {
			if (busy)
				return false;

			if (!$.isPlainObject(cmd)) {
				cmd = createCommand(cmd);
			}
			if (!cmd || !verifyCommand(cmd)) return;

			busy = true;

			if (cmd.action === "add") {
				EventBroker.publish("ajaxcart.item.adding", cmd);
			}
			else if (cmd.action === "addfromwishlist" || cmd.action === "addfromwishcart") {
				EventBroker.publish("ajaxcart.item.adding", cmd);
			}
			else if (cmd.action === "remove") {
				EventBroker.publish("ajaxcart.item.removing", cmd);
			}

			$.ajax({
				cache: false,
				url: cmd.href,
				data: cmd.data,
				type: 'POST',

				success: function (response) {
					if (response.redirect) {
						// when the controller sets the "redirect"
						// property (either to cart, product page etc.), 
						// it's mandatory to do so and useless to do ajax stuff.
						location.href = response.redirect;
						return false;
					}

					// success is optional and therefore true by default
					isSuccess = response.success === undefined ? true : response.success;

					var msg = cmd.action === "add" || cmd.action === "addfromwishlist" || cmd.action === "addfromcart" ? "ajaxcart.item.added" : "ajaxcart.item.removed";
					EventBroker.publish(
                        isSuccess
                            ? msg
                            : "ajaxcart.error",
                        $.extend(cmd, { response: response })
                    );

					if (isSuccess && (cmd.action === "addfromwishlist" || cmd.action === "addfromcart")) {
						// special case when item was copied/moved from wishlist
						if (response.wasMoved) {
							// if an item was MOVED from Wishlist to cart,
							// we must also set the wishlist dropdown dirty
							var clonedCmd = $.extend({}, cmd, { type: "wishlist" });
							EventBroker.publish(
                                "ajaxcart.item.removed",
                                clonedCmd
                            );
						}
					}
				},

				error: function (jqXHR, textStatus, errorThrown) {
					EventBroker.publish(
                        "ajaxcart.error",
                        $.extend(cmd, { response: { success: false, message: errorThrown } })
                    );
				},

				complete: function () {
					// never say never ;-)
					busy = false;
					EventBroker.publish("ajaxcart.complete", cmd);
				}
			});

			// for stopping event propagation
			return false;
		}
	}

})(jQuery, this, document);

$(function () {
    var shopBar = $(".shopbar");
    var shouldOpen = !$("body").hasClass("no-offcanvas-cart");

    shopBar.find(".shopbar-button").on("click", function (e) {
        var isMenu = $(e.target).closest(".shopbar-button").data("target") == "#offcanvas-menu";

        if (!shouldOpen && !isMenu) {
            // navigate to link (href target)
            e.stopPropagation();
            return;
        }
        else
        {
            var el = $(this);
            var tool = el.parent();

            // Open corresponding tab
            $(tool.data("target")).tab('show');
        }
    });

    // Register for tab change event 
    $('#offcanvas-cart a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        var tool = $(e.target);

        if (!tool.hasClass("loaded") && !tool.hasClass("loading")) {
        	ShopBar.showThrobber();
            ShopBar.loadHtml(tool, function () {
            	ShopBar.hideThrobber();
            });
        }
    });

	// React to touchspin change
	var updatingCart = false;
	var debouncedSpin = _.debounce(function (e) {
		if (updatingCart)
			return;

		updatingCart = true;
		var el = $(this);

		$.ajax({
			cache: false,
			type: "POST",
			url: el.data("update-url"),
			data: { "sciItemId": el.data("sci-id"), "newQuantity": el.val() },
			success: function (data) {
				if (data.success == true) {
					var type = el.data("type");
					ShopBar.loadSummary(type, true);
					el.closest('.tab-pane').find('.sub-total').html(data.SubTotal);
				}
				else {
					$(data.message).each(function (index, value) {
						displayNotification(value, "error", false);
					});
				}
			},
			complete: function () {
				updatingCart = false;
			}
		});
	}, 350, false);

	$('#offcanvas-cart').on('change', '.qty-input .form-control', debouncedSpin);
}); 

var ShopBar = (function($) {

	var offcanvasCart = $("#offcanvas-cart");
	var shopBarTools = $(".shopbar-tools");

    var tools = {
        "cart": $(".nav-tabs #cart-tab", offcanvasCart),
        "wishlist": $(".nav-tabs #wishlist-tab", offcanvasCart),
        "compare": $(".nav-tabs #compare-tab", offcanvasCart)
    };

    var buttons = {
        "cart": $("#shopbar-cart"),
        "wishlist": $("#shopbar-wishlist"),
        "compare": $("#shopbar-compare")
    };

    function notify(resp) {
        if (resp && resp.message) {
            displayNotification(resp.message, !!(resp.success) ? "success" : "error");
        }
    }

    EventBroker.subscribe("ajaxcart.item.adding", function (msg, data) {
    	var tool = tools[data.type];
    	ShopBar.showThrobber();
    });

    EventBroker.subscribe("ajaxcart.item.added", function (msg, data) {
        var tool = tools[data.type];
        var button = buttons[data.type];
        var badge = $("span.label", button);
        
        if (badge.hasClass("d-none")) {
        	badge.removeClass("d-none");
        }

        ShopBar.loadHtml(tool, function () {
        	ShopBar.hideThrobber();
        });

        ShopBar.loadSummary(data.type, true /*fade*/, function (resultData) { });

        var action = data.action;

        if (action == "addfromwishlist" || action == "addfromcart") 
        {
            $('.nav-tabs ' + (action == "addfromcart" ? "#wishlist-tab" : "#cart-tab")).tab('show');
        }
        else {
            ShopBar.toggleCart(data.type);
        }
    });

    EventBroker.subscribe("ajaxcart.item.removing", function (msg, data) {
    	var tool = tools[data.type];
    	ShopBar.showThrobber();
    });

    EventBroker.subscribe("ajaxcart.item.removed", function (msg, data) {
    	var tool = tools[data.type];
        
        ShopBar.loadHtml(tool, function () {
        	ShopBar.hideThrobber();
        });

        ShopBar.loadSummary(data.type, true /*fade*/, function (resultData) { });
    });

    EventBroker.subscribe("ajaxcart.error", function (msg, data) {
        notify(data.response);
    });

    EventBroker.subscribe("ajaxcart.complete", function (msg, data) {
        // [...]
    });

    return {

        init: function(opts) {
            // [...]
        },

        showThrobber: function () {
			var cnt = $(".tab-content", offcanvasCart);
        	var throbber = cnt.data('throbber');
        	if (!throbber) {
        		throbber = cnt.throbber({ white: true, small: true, message: '', show: false, speed: 0 }).data('throbber');
        	}

        	throbber.show();
		},

        hideThrobber: function () {      	
        	var cnt = $(".tab-content", offcanvasCart);
        	_.delay(function () { cnt.data("throbber").hide(); }, 100);
		},

        initQtyControls: function(parentSelector) {
            $(parentSelector + " .qty-input .form-control").each(function (e) {
                var el = $(this);
                el.TouchSpin({
                	min: el.data("min-qty"),
                	max: el.data("max-qty"),
                	step: el.data("min-step"),
                    buttondown_class: 'btn btn-secondary',
                    buttonup_class: 'btn btn-secondary',
                    buttondown_txt: '<i class="fa fa-minus"></i>',
                    buttonup_txt: '<i class="fa fa-plus"></i>',
                });
            });
        },

        toggleCart: function (tab) {
        	buttons[tab].find(".shopbar-button").trigger('click');
        },

        loadSummaries: function (animate, fn /* successCallBack */) {
        	if (shopBarTools.data("summary-href")) {
        		$.ajax({
        			cache: false,
        			type: "POST",
        			url: shopBarTools.data("summary-href"),
        			success: function (data) {
        				shopBarTools.bindData(data, { animate: animate });
        				offcanvasCart.bindData(data, { animate: animate });

        				if (_.isFunction(fn))
        					fn.call(this, data);
        			},
        			complete: function (jqXHR, textStatus) { }
        		});
        	}
        },

        loadSummary: function (type, animate, fn /* successCallBack */) {
            var tool = _.isString(type) ? buttons[type] : type;
            if (!tool) return;

            var button = tool.find(".shopbar-button");
            if (button.data("summary-href")) {
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: button.data("summary-href"),
                    success: function (data) {
                    	tools[type].bindData(data, { animate: animate });
                    	button.bindData(data, { animate: animate });

                    	if (_.isFunction(fn))
                    		fn.call(this, data);
                    },
                    complete: function (jqXHR, textStatus) { }
                });
            }
        },

        loadHtml: function (type, fn /* completeCallback */) {
            var tool = _.isString(type) ? tools[type] : type;
            if (!tool || tool.data("url") == undefined) return;

            var cnt = $(".tab-content " + tool.attr("href"), offcanvasCart);
            tool.removeClass("loaded").addClass("loading");

            $.ajax({
                cache: false,
                type: "POST",
                url: tool.data("url"),
                success: function (data) {
                    cnt.find('.offcanvas-cart-body').remove();
                    cnt.find('.offcanvas-cart-footer').remove();
                    cnt.find('.offcanvas-cart-external-checkout').remove();
                    cnt.prepend(data);
                },
                complete: function (jqXHR, textStatus) {
                    tool.removeClass("loading").addClass("loaded");
                    ShopBar.initQtyControls(tool.attr("href"));

                    if (_.isFunction(fn)) {
                        fn.apply(this);
                    }
                }
            });
        }
    }

})(jQuery)