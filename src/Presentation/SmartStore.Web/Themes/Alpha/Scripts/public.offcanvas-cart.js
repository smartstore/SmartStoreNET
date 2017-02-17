;

$(function () {
    var shopBar = $(".shopbar");

    shopBar.find(".shopbar-button").on("click", function () {
    	var el = $(this);
        var tool = el.parent();

        // Open corresponding tab
        $('.nav-tabs a' + tool.data("target")).tab('show');
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
    $('#offcanvas-cart').on('change', '.qty-input .form-control', function (e) {
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
    		}
    	});
    });
}); 

var ShopBar = (function($) {

    var offcanvasCart = $("#offcanvas-cart");

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
        
        if (badge.hasClass("hidden-xs-up")) {
            badge.removeClass("hidden-xs-up");
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
        		cnt.throbber({ white: true, small: true, message: '', show: false });
        	}
        	else {
        		throbber.show();
        	}
		},

        hideThrobber: function() {
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

                        if (_.isFunction(fn)) {
                            fn.call(this, data);
                        }
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
                    cnt.find('.offcanvas-cart-payment-buttonbar').remove();
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