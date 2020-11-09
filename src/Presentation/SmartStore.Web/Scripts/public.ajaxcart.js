/*
** Ajax cart implementation
*/
;
var AjaxCart = (function ($, window, document, undefined) {

    $(function () {
        // GLOBAL event handler
        $("body").on("click", ".ajax-cart-link", function (e) {
            //e.stopPropagation();
            return AjaxCart.executeRequest(this);
        });
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
            data: undefined // wird weiter unten
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
            else if (cmd.action === "addfromwishlist" || cmd.action === "addfromcart") {
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
