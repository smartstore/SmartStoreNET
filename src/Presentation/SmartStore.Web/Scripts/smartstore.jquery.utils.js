
/* smartstore.jquery.utils.js
-------------------------------------------------------------- */
;
(function ($) {

    var $w = $(window);

    $.extend({

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

        isChildOverflowing: function (child) {
            var p = jQuery(this).get(0);
            var el = jQuery(child).get(0);
            return (el.offsetTop < p.offsetTop || el.offsetLeft < p.offsetLeft) ||
                (el.offsetTop + el.offsetHeight > p.offsetTop + p.offsetHeight || el.offsetLeft + el.offsetWidth > p.offsetLeft + p.offsetWidth);
        },

        evenIfHidden: function (callback) {
            return this.each(function () {
                var self = $(this);
                var styleBackups = [];

                var hiddenElements = self.parents().addBack().filter(':hidden');

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
                animate: false
            };
            var opts = $.extend(defaults, options);

            return this.each(function () {
                var el = $(this);

                var elems = el.find(opts.childrenOnly ? '>[data-bind-to]' : '[data-bind-to]');
                if (opts.includeSelf)
                    elems = elems.addBack();

                elems.each(function () {
                    var elem = $(this);
                    var val = data[elem.data("bind-to")];
                    if (val !== undefined) {

                        if (opts.animate) {
                            elem.html(val)
                                .addClass('data-binding')
                                .one(Prefixer.event.animationEnd, function (e) {
                                    elem.removeClass('data-binding');
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

		/**
		 * Copyright 2012, Digital Fusion
		 * Licensed under the MIT license.
		 * http://teamdf.com/jquery-plugins/license/
		 *
		 * @author Sam Sehnert
		 * @desc A small plugin that checks whether elements are within
		 *       the user visible viewport of a web browser.
		 *       only accounts for vertical position, not horizontal.
		*/
        visible: function (partial, hidden, direction, container) {
            if (this.length < 1)
                return false;

            // Set direction default to 'both'.
            direction = direction || 'both';

            var $t = this.length > 1 ? this.eq(0) : this,
                isContained = typeof container !== 'undefined' && container !== null,
                $c = isContained ? $(container) : $w,
                wPosition = isContained ? $c.offset() : 0,
                t = $t.get(0),
                vpWidth = $c.outerWidth(),
                vpHeight = $c.outerHeight(),
                clientSize = hidden === true ? t.offsetWidth * t.offsetHeight : true;

            var rec = t.getBoundingClientRect(),
                tViz = isContained ?
                    rec.top - wPosition.top >= 0 && rec.top < vpHeight + wPosition.top :
                    rec.top >= 0 && rec.top < vpHeight,
                bViz = isContained ?
                    rec.bottom - wPosition.top > 0 && rec.bottom <= vpHeight + wPosition.top :
                    rec.bottom > 0 && rec.bottom <= vpHeight,
                lViz = isContained ?
                    rec.left - wPosition.left >= 0 && rec.left < vpWidth + wPosition.left :
                    rec.left >= 0 && rec.left < vpWidth,
                rViz = isContained ?
                    rec.right - wPosition.left > 0 && rec.right < vpWidth + wPosition.left :
                    rec.right > 0 && rec.right <= vpWidth,
                vV = partial ? tViz || bViz : tViz && bViz,
                hV = partial ? lViz || rViz : lViz && rViz,
                vVisible = (rec.top < 0 && rec.bottom > vpHeight) ? true : vV,
                hVisible = (rec.left < 0 && rec.right > vpWidth) ? true : hV;

            if (direction === 'both')
                return clientSize && vVisible && hVisible;
            else if (direction === 'vertical')
                return clientSize && vVisible;
            else if (direction === 'horizontal')
                return clientSize && hVisible;
        },

        moreLess: function () {
            return this.each(function () {
                var el = $(this);

                // iOS Safari freaks out when a YouTube video starts playing while the block is collapsed:
                // the video disapperars after a while! Other video embeds like Vimeo seem to behave correctly.
                // So: shit on moreLess in this case.
                if (Modernizr.touchevents && /iPhone|iPad/.test(navigator.userAgent)) {
                    var containsToxicEmbed = el.find("iframe[src*='youtube.com']").length > 0;
                    if (containsToxicEmbed) {
                        el.removeClass('more-less');
                        return;
                    }
                }

                var inner = el.find('> .more-block');

                function getActualHeight() {
                    return inner.length > 0 ? inner.outerHeight(false) : el.outerHeight(false);
                }

                var actualHeight = getActualHeight();

                if (actualHeight === 0) {
                    el.evenIfHidden(function () {
                        actualHeight = getActualHeight();
                    });
                }

                var maxHeight = el.data('max-height') || 260;

                if (actualHeight <= maxHeight) {
                    el.css('max-height', 'none');
                    return;
                }
                else {
                    el.css('max-height', maxHeight + 'px');
                    el.addClass('collapsed');
                }

                el.on('click', '.btn-text-expander', function (e) {
                    e.preventDefault();
                    if ($(this).hasClass('btn-text-expander--expand')) {
                        el.addClass('expanded').removeClass('collapsed');
                    }
                    else {
                        el.addClass('collapsed').removeClass('expanded');
                    }
                    return false;
                });

                var expander = el.find('.btn-text-expander--expand');
                if (expander.length === 0) {
                    el.append('<a href="#" class="btn-text-expander btn-text-expander--expand"><i class="fa fa fa-angle-double-down pr-2"></i><span>' + Res['Products.Longdesc.More'] + '</span></a>');
                }

                var collapser = el.find('.btn-text-expander--collapse');
                if (collapser.length === 0) {
                    el.append('<a href="#" class="btn-text-expander btn-text-expander--collapse"><i class="fa fa fa-angle-double-up pr-2"></i><span>' + Res['Products.Longdesc.Less'] + '</span></a>');
                }
            });
        },

        // Element must be decorated with visibilty:hidden
        masonryGrid: function (itemSelector, callback) {

            return this.each(function () {

                var self = $(this);
                var grid = self[0];
                var allItems = self.find(".card");

                var viewport = ResponsiveBootstrapToolkit;
                if (viewport.is('<=sm')) {
                    self.css("visibility", "visible");
                    return false;
                }

                self.addClass("masonry-grid");

                // first call so aos can be initialized correctly
                resizeAllGridItems();

                self.imagesLoaded(function () {
                    // second call to get correct size if pictures weren't loaded on the first call
                    resizeAllGridItems();
                    self.css("visibility", "visible");

                    if (typeof callback === 'function') {
                        _.defer(function () {
                            callback.call(this);
                        });
                    }
                });

                function resizeGridItem(item) {
                    var innerItem = item.querySelector(itemSelector);
                    var computedStyle = window.getComputedStyle(grid);
                    var rowHeight = parseInt(computedStyle.getPropertyValue('grid-auto-rows'));
                    var rowGap = parseInt(computedStyle.getPropertyValue('grid-row-gap'));
                    var rowSpan = Math.ceil((innerItem.getBoundingClientRect().height + rowGap) / (rowHeight + rowGap));
                    item.style.gridRowEnd = "span " + rowSpan;
                    innerItem.style.height = "100%";
                }

                function resizeAllGridItems() {
                    allItems.each(function () {
                        resizeGridItem($(this)[0]);
                    });
                }

                var timeout;

                $w.on("resize", function () {
                    if (timeout) {
                        window.cancelAnimationFrame(timeout);
                    }

                    timeout = window.requestAnimationFrame(resizeAllGridItems);
                });
            });
        }
    }); // $.fn.extend

    // Shorter aliases
    $.fn.gap = $.fn.cushioning;
    $.fn.hgap = $.fn.horizontalCushioning;
    $.fn.vgap = $.fn.verticalCushioning;

})(jQuery);