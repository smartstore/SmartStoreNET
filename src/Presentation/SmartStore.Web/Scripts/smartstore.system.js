/* smartstore.system.js
-------------------------------------------------------------- */

(function ($) {

    function detectTouchscreen() {
        let result = false;
        if (window.PointerEvent && ('maxTouchPoints' in navigator)) {
            // if Pointer Events are supported, just check maxTouchPoints
            if (navigator.maxTouchPoints > 0) {
                result = true;
            }
        }
        else {
            // no Pointer Events...
            if (window.matchMedia && window.matchMedia("(any-pointer:coarse)").matches) {
                // check for any-pointer:coarse which mostly means touchscreen
                result = true;
            }
            else if (window.TouchEvent || ('ontouchstart' in window)) {
                // last resort - check for exposed touch events API / event handler
                result = true;
            }
        }

        return result;
    }

    Modernizr.touchevents = detectTouchscreen();

    if (Modernizr.touchevents) {
        window.document.documentElement.classList.remove("no-touchevents");
        window.document.documentElement.classList.add("touchevents");
    }

    var formatRe = /\{(\d+)\}/g;

    String.prototype.format = function () {
        let s = this, args = arguments;
        return s.replace(formatRe, function(m, i) {
            return args[i];
        });
    };

    // define noop funcs for window.console in order
    // to prevent scripting errors
    var c = window.console = window.console || {};
    var funcs = ['log', 'debug', 'info', 'warn', 'error', 'assert', 'dir', 'dirxml', 'group', 'groupEnd', 'time', 'timeEnd', 'count', 'trace', 'profile', 'profileEnd'],
        flen = funcs.length,
        noop = function () { };
    while (flen) {
        if (!c[funcs[--flen]]) {
            c[funcs[flen]] = noop;
        }
    }

    // define default secure-casts
    jQuery.extend(window, {

        toBool: function (val) {
            var defVal = typeof arguments[1] === "boolean" ? arguments[1] : false;
            var t = typeof val;
            if (t === "boolean") {
                return val;
            }
            else if (t === "string") {
                switch (val.toLowerCase()) {
                    case "1": case "true": case "yes": case "on": case "checked":
                        return true;
                    case "0": case "false": case "no": case "off":
                        return false;
                    default:
                        return defVal;
                }
            }
            else if (t === "number") {
                return Boolean(val);
            }
            else if (t === "null" || t === "undefined") {
                return defVal;
            }
            return defVal;
        },

        toStr: function (val) {
            var defVal = typeof arguments[1] === "string" ? arguments[1] : "";
            if (!val || val === "[NULL]") {
                return defVal;
            }
            return String(val) || defVal;
        },

        toInt: function (val) {
            var x = parseInt(val);
            if (isNaN(x)) {
                var defVal = 0;
                if (arguments.length > 1) {
                    var arg = arguments[1];
                    defVal = arg === null || typeof arg === "number" ? arg : 0;
                }
                return defVal;
            }

            return x;
        },

        toFloat: function (val) {
            var x = parseFloat(val);
            if (isNaN(x)) {
                var defVal = 0;
                if (arguments.length > 1) {
                    var arg = arguments[1];
                    defVal = arg === null || typeof arg === "number" ? arg : 0;
                }
                return defVal;
            }
            return x;
        },

        requestAnimationFrame: window.requestAnimationFrame ||
            window.webkitRequestAnimationFrame ||
            window.mozRequestAnimationFrame ||
            window.msRequestAnimationFrame ||
            window.oRequestAnimationFrame || function (callback) {
                setTimeout(callback, 10);
            },

        cancelAnimationFrame: window.cancelAnimationFrame ||
            window.webkitCancelAnimationFrame ||
            window.mozCancelAnimationFrame ||
            window.msCancelAnimationFrame ||
            window.oCancelAnimationFrame,

        requestIdleCallback: window.requestIdleCallback || function (cb) {
            var start = Date.now();
            return setTimeout(function () {
                cb({
                    didTimeout: false,
                    timeRemaining: function () {
                        return Math.max(0, 50 - (Date.now() - start));
                    },
                });
            }, 1);
        },

        cancelIdleCallback: window.cancelIdleCallback || function (id) { clearTimeout(id); }
    });

    // provide main app namespace
    window.SmartStore = {};
})(jQuery);
