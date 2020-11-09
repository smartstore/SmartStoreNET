;
(function ($) {
    var regexHex = /^0x[a-f0-9]+$/i,
        regexInfinity = /^[+\-]?infinity$/i,
        regexParseFloat = /^[+\-]?\d*\.?\d*(e[+\-]?\d+)?$/,
        regexTrim = /^\s+|\s+$/g;

    var patterns = {
        numeric: {
            negative: ["(n)", "-n", "- n", "n-", "n -"]
        },
        currency: {
            positive: ["$n", "n$", "$ n", "n $"],
            negative: ["($n)", "-$n", "$-n", "$n-", "(n$)", "-n$", "n-$", "n$-", "-n $", "-$ n", "n $-", "$ n-", "$ -n", "n- $", "($ n)", "(n $)"]
        },
        percent: {
            positive: ["n %", "n%", "%n", "% n"],
            negative: ["-n %", "-n%", "-%n", "%-n", "%n-", "n-%", "n%-", "-% n", "n %-", "% n-", "% -n", "n- %"]
        }
    };

    var defaultCulture = {
        name: "en-US",
        englishName: "English (United States)",
        nativeName: "English",
        isRTL: false,
        language: "en",
        numberFormat: {
            // number groups separator
            ",": ",",
            // decimal separator
            ".": ".",
            // [negativePattern]
            // Note, numberFormat.pattern has no "positivePattern" unlike percent and currency,
            // but is still defined as an array for consistency with them.
            pattern: [1],
            // NumberDecimalDigits
            decimals: 2,
            // NumberGroupSizes
            groupSizes: [3],
            // symbol used for positive numbers (PositiveSign)
            "+": "+",
            // symbol used for negative numbers (NegativeSign)
            "-": "-",
            // symbol used for NaN (Not-A-Number) (NaNSymbol)
            "NaN": "NaN",
            // symbol used for Negative Infinity
            negativeInfinity: "-Infinity",
            // symbol used for Positive Infinity
            positiveInfinity: "Infinity",
            percent: {
                pattern: [0, 0],
                decimals: 2,
                groupSizes: [3],
                ",": ",",
                ".": ".",
                symbol: "%"
            },
            currency: {
                pattern: [0, 0],
                decimals: 2,
                groupSizes: [3],
                ",": ",",
                ".": ".",
                symbol: "$"
            }
        },
        dateTimeFormat: {
            calendarName: "Gregorian_USEnglish",
            "/": "/", // separator of parts of a date (e.g. "/" in 11/05/1955)
            ":": ":", // separator of parts of a time (e.g. ":" in 05:44 PM)
            firstDay: 0, // the first day of the week (0 = Sunday, 1 = Monday, etc)
            days: {
                names: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
                namesAbbr: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"],
                namesShort: ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"]
            },
            months: {
                // full month names (13 months for lunar calendards -- 13th month should be "" if not lunar)
                names: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December", ""],
                namesAbbr: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", ""]
            },
            // AM and PM designators in one of these forms:
            // The usual view, and the upper and lower case versions
            //   [ standard, lowercase, uppercase ]
            // The culture does not use AM or PM (likely all standard date formats use 24 hour time)
            //   null
            AM: ["AM", "am", "AM"], // null if empty
            PM: ["PM", "pm", "PM"], // null if empty
            twoDigitYearMax: 2029,
            // set of predefined date and time patterns used by the culture
            // these represent the format someone in this culture would expect
            // to see given the portions of the date that are shown.
            patterns: {
                // short date pattern
                d: "M/d/yyyy",
                // long date pattern
                D: "dddd, MMMM dd, yyyy",
                // short time pattern
                t: "h:mm tt",
                // long time pattern
                T: "h:mm:ss tt",
                // general date time pattern (short time)
                g: "M/d/yyyy h:mm tt",
                // general date time pattern (long time)
                G: "M/d/yyyy h:mm:ss tt",
                // long date, short time pattern
                f: "dddd, MMMM dd, yyyy h:mm tt",
                // long date, long time pattern
                F: "dddd, MMMM dd, yyyy h:mm:ss tt",
                // month/day pattern
                M: "MMMM dd",
                // month/year pattern
                Y: "yyyy MMMM",
                // Universal sortable pattern
                u: "yyyy\u0027-\u0027MM\u0027-\u0027dd\u0027T\u0027HH\u0027:\u0027mm\u0027:\u0027ss"
            }
        }
    };

    var g = {
        patterns: patterns,
        culture: defaultCulture
    };

    function truncate(value) {
        if (isNaN(value)) {
            return NaN;
        }
        return Math[value < 0 ? "ceil" : "floor"](value);
    }

    function zeroPad(str, count, left) {
        var l;
        for (l = str.length; l < count; l += 1) {
            str = (left ? ("0" + str) : (str + "0"));
        }
        return str;
    };

    function parseNegativePattern(value, nf, negativePattern) {
        var neg = nf["-"],
            pos = nf["+"],
            ret;
        switch (negativePattern) {
            case "n -":
                neg = " " + neg;
                pos = " " + pos;
            /* falls through */
            case "n-":
                if (_.str.endsWith(value, neg)) {
                    ret = ["-", value.substr(0, value.length - neg.length)];
                }
                else if (_.str.endsWith(value, pos)) {
                    ret = ["+", value.substr(0, value.length - pos.length)];
                }
                break;
            case "- n":
                neg += " ";
                pos += " ";
            /* falls through */
            case "-n":
                if (_.str.startsWith(value, neg)) {
                    ret = ["-", value.substr(neg.length)];
                }
                else if (_.str.startsWith(value, pos)) {
                    ret = ["+", value.substr(pos.length)];
                }
                break;
            case "(n)":
                if (_.str.startsWith(value, "(") && _.str.endsWith(value, ")")) {
                    ret = ["-", value.substr(1, value.length - 2)];
                }
                break;
        }
        return ret || ["", value];
    };

    //// Not implemented. Should use moment.js for
    //// datetime processing.
    //g.parseDate = function (value) {
    //	return value;
    //}

    g.parseInt = function (value) {
        return truncate(g.parseFloat(value));
    }

    g.parseFloat = function (value) {
        var radix = 10;

        var culture = g.culture;
        var ret = NaN,
            nf = culture.numberFormat;

        if (value.indexOf(culture.numberFormat.currency.symbol) > -1) {
            // remove currency symbol
            value = value.replace(culture.numberFormat.currency.symbol, "");
            // replace decimal seperator
            value = value.replace(culture.numberFormat.currency["."], culture.numberFormat["."]);
        }

        //Remove percentage character from number string before parsing
        if (value.indexOf(culture.numberFormat.percent.symbol) > -1) {
            value = value.replace(culture.numberFormat.percent.symbol, "");
        }

        // remove spaces: leading, trailing and between - and number. Used for negative currency pt-BR
        value = value.replace(/ /g, "");

        // allow infinity or hexidecimal
        if (regexInfinity.test(value)) {
            ret = parseFloat(value);
        }
        else if (!radix && regexHex.test(value)) {
            ret = parseInt(value, 16);
        }
        else {

            // determine sign and number
            var signInfo = parseNegativePattern(value, nf, patterns.numeric.negative[nf.pattern[0]]),
                sign = signInfo[0],
                num = signInfo[1];

            // #44 - try parsing as "(n)"
            if (sign === "" && nf.pattern[0] !== "(n)") {
                signInfo = parseNegativePattern(value, nf, "(n)");
                sign = signInfo[0];
                num = signInfo[1];
            }

            // try parsing as "-n"
            if (sign === "" && nf.pattern[0] !== "-n") {
                signInfo = parseNegativePattern(value, nf, "-n");
                sign = signInfo[0];
                num = signInfo[1];
            }

            sign = sign || "+";

            // determine exponent and number
            var exponent,
                intAndFraction,
                exponentPos = num.indexOf("e");
            if (exponentPos < 0) exponentPos = num.indexOf("E");
            if (exponentPos < 0) {
                intAndFraction = num;
                exponent = null;
            }
            else {
                intAndFraction = num.substr(0, exponentPos);
                exponent = num.substr(exponentPos + 1);
            }
            // determine decimal position
            var integer,
                fraction,
                decSep = nf["."],
                decimalPos = intAndFraction.indexOf(decSep);
            if (decimalPos < 0) {
                integer = intAndFraction;
                fraction = null;
            }
            else {
                integer = intAndFraction.substr(0, decimalPos);
                fraction = intAndFraction.substr(decimalPos + decSep.length);
            }
            // handle groups (e.g. 1,000,000)
            var groupSep = nf[","];
            integer = integer.split(groupSep).join("");
            var altGroupSep = groupSep.replace(/\u00A0/g, " ");
            if (groupSep !== altGroupSep) {
                integer = integer.split(altGroupSep).join("");
            }
            // build a natively parsable number string
            var p = sign + integer;
            if (fraction !== null) {
                p += "." + fraction;
            }
            if (exponent !== null) {
                // exponent itself may have a number patternd
                var expSignInfo = parseNegativePattern(exponent, nf, "-n");
                p += "e" + (expSignInfo[0] || "+") + expSignInfo[1];
            }
            if (regexParseFloat.test(p)) {
                ret = parseFloat(p);
            }
        }

        return ret;
    }

    g.convertDatePatternToMomentFormat = function (pattern) {
        // Converts .NET date format string to moment.js format
        var result = '',
            token = '';

        function convertToken(t) {
            switch (t) {
                case 'd': return 'D';
                case 'dd': return 'DD';
                case 'ddd': return 'dd';
                case 'yy': return 'YY';
                case 'yyy': case 'yyyy': case 'yyyyy': return 'YYYY';
                case 'zz': return 'ZZ';
                case 'zzz': return 'Z';
                case 'tt': return 'A';
                case 'ff': case 'FF': return 'SS';
                case 'fff': case 'FFF': return 'SSS';
                case 'ffff': case 'FFFF': return 'SSSS';
                case 'fffff': case 'FFFFF': return 'SSSSS';
                case 'ffffff': case 'FFFFFF': return 'SSSSSS';
                case 'fffffff': case 'FFFFFFF': return 'SSSSSSS';
                default:
                    return t;
            }
        }

        for (var i = 0; i < pattern.length; i++) {
            switch (pattern[i]) {
                case 'd': case 'y': case 'z': case 't': case 'f':
                    token += pattern[i];
                    continue;
                default:
                    if (token.length > 0) {
                        result += convertToken(token);
                        token = '';
                    }

                    result += pattern[i];
            }
        }

        if (token.length > 0) {
            result += convertToken(token);
        }

        return result;
    }

    // formatNumber
    var formatNumber;
    (function () {
        var expandNumber;

        expandNumber = function (number, precision, formatInfo) {
            var groupSizes = formatInfo.groupSizes,
                curSize = groupSizes[0],
                curGroupIndex = 1,
                factor = Math.pow(10, precision),
                rounded = Math.round(number * factor) / factor;

            if (!isFinite(rounded)) {
                rounded = number;
            }
            number = rounded;

            var numberString = number + "",
                right = "",
                split = numberString.split(/e/i),
                exponent = split.length > 1 ? parseInt(split[1], 10) : 0;
            numberString = split[0];
            split = numberString.split(".");
            numberString = split[0];
            right = split.length > 1 ? split[1] : "";

            if (exponent > 0) {
                right = zeroPad(right, exponent, false);
                numberString += right.slice(0, exponent);
                right = right.substr(exponent);
            }
            else if (exponent < 0) {
                exponent = -exponent;
                numberString = zeroPad(numberString, exponent + 1, true);
                right = numberString.slice(-exponent, numberString.length) + right;
                numberString = numberString.slice(0, -exponent);
            }

            if (precision > 0) {
                right = formatInfo["."] +
                    ((right.length > precision) ? right.slice(0, precision) : zeroPad(right, precision));
            }
            else {
                right = "";
            }

            var stringIndex = numberString.length - 1,
                sep = formatInfo[","],
                ret = "";

            while (stringIndex >= 0) {
                if (curSize === 0 || curSize > stringIndex) {
                    return numberString.slice(0, stringIndex + 1) + (ret.length ? (sep + ret + right) : right);
                }
                ret = numberString.slice(stringIndex - curSize + 1, stringIndex + 1) + (ret.length ? (sep + ret) : "");

                stringIndex -= curSize;

                if (curGroupIndex < groupSizes.length) {
                    curSize = groupSizes[curGroupIndex];
                    curGroupIndex++;
                }
            }

            return numberString.slice(0, stringIndex + 1) + sep + ret + right;
        };

        formatNumber = function (value, format) {
            var culture = g.culture;

            if (!isFinite(value)) {
                if (value === Infinity) {
                    return culture.numberFormat.positiveInfinity;
                }
                if (value === -Infinity) {
                    return culture.numberFormat.negativeInfinity;
                }
                return culture.numberFormat.NaN;
            }
            if (!format || format === "i") {
                return culture.name.length ? value.toLocaleString() : value.toString();
            }
            format = format || "D";

            var nf = culture.numberFormat,
                number = Math.abs(value),
                precision = -1,
                pattern;
            if (format.length > 1) precision = parseInt(format.slice(1), 10);

            var current = format.charAt(0).toUpperCase(),
                formatInfo,
                patterns = g.patterns.numeric;

            switch (current) {
                case "D":
                    pattern = "n";
                    number = truncate(number);
                    if (precision !== -1) {
                        number = zeroPad("" + number, precision, true);
                    }
                    if (value < 0) number = "-" + number;
                    break;
                case "N":
                    formatInfo = nf;
                /* falls through */
                case "C":
                    formatInfo = formatInfo || nf.currency;
                    patterns = g.patterns.currency;
                /* falls through */
                case "P":
                    formatInfo = formatInfo || nf.percent;
                    patterns = g.patterns.percent;
                    pattern = value < 0 ? patterns.negative[formatInfo.pattern[0]] : (patterns.positive[formatInfo.pattern[1]] || "n");
                    if (precision === -1) precision = formatInfo.decimals;
                    number = expandNumber(number * (current === "P" ? 100 : 1), precision, formatInfo);
                    break;
                default:
                    throw "Bad number format specifier: " + current;
            }

            var patternParts = /n|\$|-|%/g,
                ret = "";
            for (; ;) {
                var index = patternParts.lastIndex,
                    ar = patternParts.exec(pattern);

                ret += pattern.slice(index, ar ? ar.index : pattern.length);

                if (!ar) {
                    break;
                }

                switch (ar[0]) {
                    case "n":
                        ret += number;
                        break;
                    case "$":
                        ret += nf.currency.symbol;
                        break;
                    case "-":
                        // don't make 0 negative
                        if (/[1-9]/.test(number)) {
                            ret += nf["-"];
                        }
                        break;
                    case "%":
                        ret += nf.percent.symbol;
                        break;
                }
            }

            return ret;
        };

        g.formatNumber = formatNumber;
    }());

    SmartStore.globalization = g;
})(jQuery);