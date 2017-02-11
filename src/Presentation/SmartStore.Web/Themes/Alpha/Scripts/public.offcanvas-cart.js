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
        ShopBar.toggleCart(data.type);
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
            $(parentSelector + " .qty-input .form-control").each(function () {

                var qtyControl = $(this);

                qtyControl.TouchSpin({
                    min: qtyControl.data("min-qty"),   
                    max: qtyControl.data("max-qty"),
                    step: qtyControl.data("min-step"),
                    buttondown_class: 'btn btn-secondary',
                    buttonup_class: 'btn btn-secondary',
                    buttondown_txt: '<i class="fa fa-minus"></i>',
                    buttonup_txt: '<i class="fa fa-plus"></i>',
                }).change(function (e) {

                    var currentValue = this.value;
                    
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: qtyControl.data("update-url"),
                        data: { "sciItemId": qtyControl.data("sci-id"), "newQuantity": currentValue },
                        success: function (data) {
                            if(data.success == true) {
                                var type = qtyControl.data("type");
                                ShopBar.loadSummary(type, true, function (data) { });
                                $("#offcanvas-cart .offcanvas-cart-summary .sub-total").html(data.SubTotal);
                            }
                            else {
                                $(data.message).each(function (index, value) {
                                    displayNotification(value, "error", false);
                                });
                            }
                        },
                        complete: function (jqXHR, textStatus) { }
                    });
                });
            });
        },

        toggleCart: function (tab) {
        	buttons[tab].find(".shopbar-button").trigger('click');
        },

        loadSummary: function (type, fade, fn /* successCallBack */) {
            var tool = _.isString(type) ? buttons[type] : type;
            if (!tool) return;

            var button = tool.find(".shopbar-button");
            if (button.data("summary-href")) {

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: button.data("summary-href"),
                    success: function (data) {

                        tools[type].bindData(data, { fade: fade });

                        button.bindData(data, { fade: fade });

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
                    cnt.find('.offcanvas-cart-summary').remove();
                    cnt.find('.offcanvas-cart-buttons').remove();
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