
/* smartstore.jquery.utils.js
-------------------------------------------------------------- */
;
(function ($) {

    $.extend({

        fixIE7ZIndexBug: function () {
            var zIndexNumber = 4000;
            $('div').each(function () {
                $(this).css('zIndex', zIndexNumber);
                zIndexNumber -= 10;
            });
        },

        topZIndex: function (selector) {
            /*
            /// summary
            /// 	Returns the highest (top-most) zIndex in the document
            /// 	(minimum value returned: 0).
            /// param "selector"
            /// 	(optional, default = "body *") jQuery selector specifying
            /// 	the elements to use for calculating the highest zIndex.
            /// returns
            /// 	The minimum number returned is 0 (zero).
            */
            return Math.max(0, Math.max.apply(null, $.map($(selector || "body *"),
				function (v) {
				    return parseInt($(v).css("z-index")) || null;
				}
			)));
        }

    }); // $.extend

    $.fn.extend({

        topZIndex: function (opt) {
            /*
            /// summary:
            /// 	Increments the CSS z-index of each element in the matched set
            /// 	to a value larger than the highest current zIndex in the document.
            /// 	(i.e., brings all elements in the matched set to the top of the
            /// 	z-index order.)
            /// param "opt"
            /// 	(optional) Options, with the following possible values:
            /// 	increment: (Number, default = 1) increment value added to the
            /// 		highest z-index number to bring an element to the top.
            /// 	selector: (String, default = "body *") jQuery selector specifying
            /// 		the elements to use for calculating the highest zIndex.
            /// returns type="jQuery"
            */

            // Do nothing if matched set is empty
            if (this.length === 0) {
                return this;
            }

            opt = $.extend({ increment: 1, selector: "body *" }, opt);

            // Get the highest current z-index value
            var zmax = $.topZIndex(opt.selector), inc = opt.increment;

            // Increment the z-index of each element in the matched set to the next highest number
            return this.each(function () {
                $(this).css("z-index", zmax += inc);
            });
        },

        cushioning: function (withMargins) {
            var el = $(this[0]);
            // returns the differences between outer and inner
            // width, as well as outer and inner height
            withMargins = _.isBoolean(withMargins) ? withMargins : true;
            return {
                horizontal: el.outerWidth(withMargins) - el.width(),
                vertical: el.outerHeight(withMargins) - el.height()
            }
        },

        horizontalCushioning: function (withMargins) {
            var el = $(this[0]);
            // returns the difference between outer and inner width
            return el.outerWidth(_.isBoolean(withMargins) ? withMargins : true) - el.width();
        },

        verticalCushioning: function (withMargins) {
            var el = $(this[0]);
            // returns the difference between outer and inner height
            return el.outerHeight(_.isBoolean(withMargins) ? withMargins : true) - el.height();
        },

        outerHtml: function () {
            // returns the (outer)html of a new DOM element that contains
            // a clone of the first match
            return $(document.createElement("div"))
				.append($(this[0]).clone())
				.html();
        },

        max: function (callback) {
            var n;
            jQuery.each(this, function () {
                var m = callback.apply(this);
                n = n ? Math.max(n, m) : m;
            });
            return n;
        },

        min: function (callback) {
            var n;
            jQuery.each(this, function () {
                var m = callback.apply(this);
                n = n ? Math.min(n, m) : m;
            });
            return n;
        },

        sum: function (callback) {
            var n;
            jQuery.each(this, function () {
                m = callback.apply(this);
                n = n ? n + m : m;
            });
            return n;
        },

        ellipsis: function (enableUpdating) {
            // currently sets only the 'title' attribute
            // on element if ellipsized by browser.
            // Since FF7+ now supports ellipsis, no need
            // to update the text anymore.
            return this.each(function () {
                var el = $(this);

                if (el.css("overflow") == "hidden") {
                    var text = el.text();
                    var multiline = false; //el.hasClass('multiline');
                    var t = $(this.cloneNode(true))
	                        .hide()
	                        .css('position', 'absolute')
	                        .css('overflow', 'visible')
	                        .width(multiline ? el.width() : 'auto')
	                        .height(multiline ? 'auto' : el.height());

                    el.after(t);

                    function height() { return t.height() > el.height(); };
                    function width() { return t.width() > el.width(); };

                    var func = multiline ? height : width;

                    if (func()) {
                        el.attr('title', text);
                    }

                    t.remove();
                }
            });
        },

        evenIfHidden: function (callback) {
            return this.each(function () {
                var self = $(this);
                var styleBackups = [];

                var hiddenElements = self.parents().andSelf().filter(':hidden');

                if (!hiddenElements.length) {
                    callback(self);
                    return true; //continue the loop
                }

                hiddenElements.each(function () {
                    var style = $(this).attr('style');
                    style = typeof style == 'undefined' ? '' : style;
                    styleBackups.push(style);
                    $(this).attr('style', style + ' display: block !important;');
                });

                hiddenElements.eq(0).css('left', -10000);

                callback(self);

                hiddenElements.each(function () {
                    $(this).attr('style', styleBackups.shift());
                });
            });
        },

        /*
            Binds a simple JSON object (no collection) to a set of html elements
            defining the 'data-bind-to' attribute
        */
        bindData: function (data, options) {
            var defaults = {
                childrenOnly: false,
                includeSelf: false,
                showFalsy: false,
                fade: false
            };
            var opts = $.extend(defaults, options);

            return this.each(function () {
                var el = $(this);
                
                var elems = el.find(opts.childrenOnly ? '>[data-bind-to]' : '[data-bind-to]');
                if (opts.includeSelf)
                    elems = elems.andSelf();

                elems.each(function () {
                    var elem = $(this);
                    var val = data[elem.data("bind-to")];
                    if (val !== undefined) {

                        if (opts.fade) {
                            elem.fadeOut(400, function () {
                                elem.html(val);
                                elem.fadeIn(400);
                            });
                        }
                        else {
                            elem.html(val);
                        }
                        
                        if (!opts.showFalsy && !val) {
                            // it's falsy, so hide it
                            elem.hide();
                        }
                        else {
                            elem.show();
                        }
                    }
                });
            });
        },

        moreLess: function (opt) {
  
            opt = $.extend({ adjustheight: 260 }, opt);

            return this.each(function () {
                var el = $(this);

                var moreText = '<button class="btn btn-mini"><i class="icon icon-plus" style="font-size:10px"></i>&nbsp;&nbsp;' + Res['Products.Longdesc.More'] + '</button>';
                var lessText = '<button class="btn btn-mini"><i class="icon icon-minus" style="font-size:10px"></i>&nbsp;&nbsp;' + Res['Products.Longdesc.Less'] + '</button>';

                $(".more-less .more-block").css('height', opt.adjustheight).css('overflow', 'hidden');
                $(".more-less").append('<p class="continued">[&hellip;]</p><a href="#" class="adjust"></a>');
                $("a.adjust").html(moreText);

                $(".adjust").toggle(function () {
                    $(this).parents("div:first").find(".more-block").css('height', 'auto').css('overflow', 'visible');
                    $(this).parents("div:first").find("p.continued").css('display', 'none');
                    $(this).html(lessText);
                }, function () {
                    $(this).parents("div:first").find(".more-block").css('height', opt.adjustheight).css('overflow', 'hidden');
                    $(this).parents("div:first").find("p.continued").css('display', 'block');
                    $(this).html(moreText);
                });
            });
        }



    }); // $.fn.extend


})(jQuery);