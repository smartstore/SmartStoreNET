;

$(function () {

    var shopBar = $(".shopbar");

    shopBar.find(".shopbar-button").on("click", function ()
    {
        var el = $(this);
        var tool = el.parent();

        // open corresponding tab
        $('.nav-tabs a' + tool.data("target")).tab('show');
    });

    // register for tab change event 
    $('#offcanvas-cart a[data-toggle="tab"]').on('shown.bs.tab', function (e)
    {
        var tool = $(e.target);
        var cnt = $("#offcanvas-cart .tab-content " + tool.attr("href"));

        if (!tool.hasClass("loaded") && !tool.hasClass("loading")) {

            cnt.throbber({ white: true, small: true, message: '' });
            ShopBar.loadHtml(tool, function () {
                cnt.data("throbber").hide();
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
        // show transfer effect
        var tool = buttons[data.type];

        if (data.src) {
            // "guess" the closest transferrable element
            var transferSrc = $(data.src).closest(".item-box, [data-transfer-src]");
            if (!transferSrc.length) {
                // ... couldn't find any? then take the src itself (could be a bit small though)
                transferSrc = data.src.parent() || data.src;
            }

            transferSrc.stop(true, true).effect("transfer", { to: tool.find(".shopbar-button-icon"), easing: "easeOutQuad", className: "transfer" }, 800, function () { });
        }
    });

    EventBroker.subscribe("ajaxcart.item.added", function (msg, data) {

        var tool = buttons[data.type];
        var badge = $("span.label", tool);
        if (badge.hasClass("hidden-xs-up")) {
            badge.removeClass("hidden-xs-up");
        }

        ShopBar.loadSummary(data.type, true /*fade*/, function (resultData) { });

        notify(data.response);
    });

    EventBroker.subscribe("ajaxcart.item.removed", function (msg, data) {

        var tool = tools[data.type];
        var tabId = tool.attr("href");
        var cnt = $(".tab-content " + tabId, offcanvasCart);
        cnt.throbber({ white: true, small: true, message: '' });

        ShopBar.loadHtml(tool, function () {
            cnt.data("throbber").hide();
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

        initQtyControls: function(parentSelector) {
            
            $(parentSelector + " .qty-input").each(function () {

                var qtyControl = $(this);

                qtyControl.TouchSpin({
                    min: qtyControl.data("min-qty"),   
                    max: qtyControl.data("max-qty"),
                    step: qtyControl.data("min-step"),
                    buttondown_class: "btn btn-sm btn-secondary",
                    buttonup_class: "btn btn-sm btn-secondary"
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
            if (!tool) return;

            var cnt = $(".tab-content " + tool.attr("href"), offcanvasCart);

            tool.removeClass("loaded").addClass("loading");

            $.ajax({
                cache: false,
                type: "POST",
                url: tool.data("url"),
                success: function (data) {
                    cnt.find('.offcanvas-cart-body').remove();
                    cnt.find('.summary').remove();
                    cnt.find('.buttons').remove();
                    cnt.append(data);
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