(function (window, undefined) {
    // jQuery 1.9 has removed the `$.browser` property, telerik relies on
    // it, so we patch it here if it's missing.
    // This has been copied from jQuery migrate 1.1.1.
    if (!jQuery.browser) {
        var uaMatch = function (ua) {
            ua = ua.toLowerCase();

            var match = /(chrome)[ \/]([\w.]+)/.exec(ua) ||
                /(webkit)[ \/]([\w.]+)/.exec(ua) ||
                /(opera)(?:.*version|)[ \/]([\w.]+)/.exec(ua) ||
                /(msie) ([\w.]+)/.exec(ua) ||
                ua.indexOf("compatible") < 0 && /(mozilla)(?:.*? rv:([\w.]+)|)/.exec(ua) ||
                [];

            return {
                browser: match[1] || "",
                version: match[2] || "0"
            };
        };

        matched = uaMatch(navigator.userAgent);
        browser = {};

        if (matched.browser) {
            browser[matched.browser] = true;
            browser.version = matched.version;
        }

        // Chrome is Webkit, but Webkit is also Safari.
        if (browser.chrome) {
            browser.webkit = true;
        } else if (browser.webkit) {
            browser.safari = true;
        }

        jQuery.browser = browser;
    }

    if (!jQuery.fn.live) {
        jQuery.fn.live = jQuery.fn.on;
    }

    if (!jQuery.fn.andSelf) {
        jQuery.fn.andSelf = jQuery.fn.addBack;
    }
})(window);