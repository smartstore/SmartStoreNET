/* underscore.mixins.js
-------------------------------------------------------------- */

; (function (root, $) {

    var toString = Object.prototype.toString,
        hasOwn = Object.prototype.hasOwnProperty,
        nativeFormat = String.prototype.format;

    var emailRegex = /^[a-z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+\/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$/;
    var encodeJsRegex = /[\\\"\'\x00-\x1f\x7f-\uffff]/g;

    var encodeJsMap = {
        "\b": '\\b',
        "\t": '\\t',
        "\n": '\\n',
        "\f": '\\f',
        "\r": '\\r',
        '"': "\\\"",
        "'": "\\\"",
        "\\": '\\\\',
        '\x0b': '\\u000b' //ie doesn't handle \v
    };

    // ------------------------- URL regexp ---------------------------------

    var urlRegex = (function () {
        var alpha = 'a-z',
            alnum = alpha + '\\d',
            hex = 'a-f\\d',
            unreserved = '-_.!~*\'()' + alnum,
            reserved = ';/?:@&=+$,\\[\\]',
            escaped = '%[' + hex + ']{2}',
            uric = '(?:[' + unreserved + reserved + ']|' + escaped + ')',
            userinfo = '(?:[' + unreserved + ';:&=+$,]|' + escaped + ')*',
            domlabel = '(?:[' + alnum + '](?:[-' + alnum + ']*[' + alnum + '])?)',
            toplabel = '(?:[' + alpha + '](?:[-' + alnum + ']*[' + alnum + '])?)',
            ipv4addr = '\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}',
            hex4 = '[' + hex + ']{1,4}',
            lastpart = '(?:' + hex4 + '|' + ipv4addr + ')',
            hexseq1 = '(?:' + hex4 + ':)*' + hex4,
            hexseq2 = '(?:' + hex4 + ':)*' + lastpart,
            ipv6addr = '(?:' + hexseq2 + '|(?:' + hexseq1 + ')?::(?:' + hexseq2 + ')?)',
            ipv6ref = '\\[' + ipv6addr + '\\]',
            hostname = '(?:' + domlabel + '\\.)*' + toplabel + '\\.?',
            host = '(?:' + hostname + '|' + ipv4addr + '|' + ipv6ref + ')',
            pchar = '(?:[' + unreserved + ':@&=+$,]|' + escaped + ')',
            param = pchar + '*',
            segment = pchar + '*(?:;' + param + ')*',
            path_segments = segment + '(?:/' + segment + ')*',
            path = '/' + path_segments,
            query = uric + '*',
            fragment = query,
            port = '\\:\\d+',
            authority = '(?:' + userinfo + '@)?' + host + '(?:' + port + ')?';

        function makeSchemes(schemes) {
            return '(?:' + schemes.join('|') + ')://';
        }

        var defaultSchemes = '(?:' + makeSchemes(['http', 'https']) + '|//)';

        return function (schemes) {
            var scheme = schemes && schemes.length ? makeSchemes(schemes) : defaultSchemes,
                regexStr = '^' + scheme + authority + '(?:' + path + ')?' + '(?:\\?' + query + ')?' + '(?:#' + fragment + ')?$';
            return new RegExp(regexStr, 'i');
        };
    })();

    var defaultUrlRegex = urlRegex();

    var m = {

        provide: function (namespace) {
            // split the input on each dot to provide subnamespaces
            namespace = namespace.split('.');
            // ensure root namespace
            var ns = namespace[0];
            ns = ns.replace("$", "jQuery"); // IE8 HACK!!!
            if (!root[ns]) root[ns] = {};
            // create all the subnamespaces, if not present
            for (var i = 1, length = namespace.length; i < length; i++) {
                ns += "." + namespace[i];
                eval("if(!window." + ns + ") window." + ns + "={};");
            }
        },

        escape: function (str) {
            return str.replace(/\'/g, "");
        },

        format: function (str) {
            var args;
            if (_.isArray(arguments[1]))
                args = arguments[1];
            else
                args = _.toArray(arguments).slice(1);
            return nativeFormat.apply(str, args);
        },

        encodeJson: function (str) {
            return str.replace(encodeJsRegex, function (a) {
                var c = encodeJsMap[a];
                return typeof c === 'string' ? c : '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4);
            });
        },

        isURL: function () {
            var schemes = slice(arguments),
                str = schemes.shift(),
                regex = schemes.length ? urlRegex(schemes) : defaultUrlRegex;

            return regex.test(str);
        },

        isEmail: function (str) {
            return emailRegex.test(str);
        },

        isTrue: function (obj) {
            return (_.isBoolean(obj) && true === obj);
        },

        isFalse: function (obj) {
            return (_.isBoolean(obj) && false === obj);
        },

        createGuid: function (withParens) {
            // creates a guid {xxxxxxxx-xxxx-xxxx-xxxxxxxxxxxx}
            var blocks = [8, 4, 4, 12],
                sequence = [],
                chars = "",
                ret = withParens ? "\{{0}\}" : "{0}";
            for (var block in blocks) {
                chars = "";
                for (var i = 0, length = blocks[block]; i < length; i++) {
                    chars += Math.floor(Math.random() * 16).toString(0x10);
                }
                sequence.push(chars);
            }

            return ret.format(sequence.join("-"));
        },

        setAttr: function () {
            // simplifies the creation of an html attribute
            var args = arguments, r = "";

            if (args.length == 2) {
                if (!_.isBlank(args[0])) {
                    var val = args[1];
                    if (!_.isEmpty(val) && (_.isNumber(val) || !_.isBlank(val) || _.isBoolean(val))) {
                        r = " " + args[0] + "='" + val + "'";
                    }
                }
            }

            return r;
        },

        isEmpty: function (obj) {
            if (_.isArray(obj) || _.isString(obj)) return obj.length === 0;
            if ($.isPlainObject(obj)) {
                for (var key in obj) return false;
                return true;
            }
            else {
                return (obj == void 0 || obj == null);
            }
        },

        now: function () {
            return (new Date()).getTime();
        },

        call: function (func) {
            if (typeof func === 'function')
                return func.apply(this, Array.prototype.slice.call(arguments, 1));
            return null;
        },

        /* Formatting */

        formatFileSize: function (bytes) {
            if (typeof bytes !== 'number') {
                return '';
            }

            var val, unit;

            if (bytes >= 1000000000) {
                val = (bytes / 1000000000);
                unit = "GB";
            }
            else if (bytes >= 1000000) {
                val = (bytes / 1000000);
                unit = "MB";
            }
            else {
                val = (bytes / 1000).toFixed(0);
                unit = "KB";
            }

            return (unit === 'KB' ? val : SmartStore.globalization.formatNumber(val, 'N')) + ' ' + unit;
        },

        formatBitrate: function (bits) {
            if (typeof bits !== 'number') {
                return '';
            }

            var val, unit, format;

            if (bits >= 1000000000) {
                val = (bits / 1000000000);
                unit = "Gbit/s";
                format = true;
            }
            else if (bits >= 1000000) {
                val = (bits / 1000000);
                unit = "Mbit/s";
                format = true;
            }
            else if (bits >= 1000) {
                val = (bits / 1000);
                unit = "Kbit/s";
            }
            else {
                val = bits;
                unit = "bit/s";
            }

            return (format ? val : SmartStore.globalization.formatNumber(val, 'N')) + ' ' + unit;
        },

        formatTime: function (seconds) {
            var date = new Date(seconds * 1000),
                days = parseInt(seconds / 86400, 10);
            days = days ? days + 'd ' : '';
            return days +
                ('0' + date.getUTCHours()).slice(-2) + ':' +
                ('0' + date.getUTCMinutes()).slice(-2) + ':' +
                ('0' + date.getUTCSeconds()).slice(-2);
        },

        formatPercentage: function (floatValue) {
            return (floatValue * 100).toFixed(2) + ' %';
        },


        /* Cookies */

        getCookie: function (name) {
            try {
                if (document.cookie && document.cookie != '') {
                    var cookies = document.cookie.split(';');
                    for (var i = 0; i < cookies.length; i++) {
                        var cookie = _.str.trim(cookies[i]);
                        if (cookie.substring(0, name.length + 1) == (name + '=')) {
                            return decodeURIComponent(cookie.substring(name.length + 1));
                        }
                    }
                }
            }
            catch (err) {
                console.error(err);
            }
            return null;
        },

        setCookie: function (name, value, path, expires, domain, secure) {
            try {
                if (_.isUndefined(expires) || _.isNull(expires)) {
                    expires = 365;
                }
                if (_.isUndefined(path) || _.isNull(path)) {
                    path = '/';
                }
                if (value === null) {
                    value = '';
                    expires = -1;
                }

                if (typeof expires == 'number' || expires.toUTCString) {
                    var date;
                    if (typeof expires == 'number') {
                        date = new Date();
                        date.setTime(date.getTime() + (expires * 24 * 60 * 60 * 1000));
                    }
                    else {
                        date = expires;
                    }
                    expires = '; expires=' + date.toUTCString();
                }

                path = '; path=' + path;
                domain = (_.isUndefined(domain) || _.isNull(domain)) ? '' : '; domain=' + domain;
                secure = (_.isUndefined(secure) || _.isNull(secure)) ? '' : '; secure';

                //console.log([name, '=', encodeURIComponent(value), expires, path, domain, secure].join(''));
                document.cookie = [name, '=', encodeURIComponent(value), expires, path, domain, secure].join('');
            }
            catch (err) {
                console.error(err);
            }
        }
    };

    // underscore.string mixins
    var s = {
        // overwrite original isBlank, this is better
	    /*isBlank: function (str) {
	        return str == false;
	    },*/
        grow: function (str, val, delimiter) {
            if (_.isEmpty(val))
                return str;
            if (!_.isEmpty(str))
                str += delimiter;
            return str + val;
        }
    }

    // integrate m with underscore
    root['_'].mixin(m);

    // integrate s with underscore.string
    if (!root._.str) {
        root._.str = root._.string = {};
    }
    _.extend(_.str, s)

})(window, jQuery);