(function ($) {
    // Depends on:
    // bootstrap 4
    // jquery.scrollTo.js

    $.fn.extend({
        megaMenu: function (settings) {

            var defaults = {
                productRotatorInterval: 4000,
                productRotatorDuration: 300,
                productRotatorCycle: false,
                productRotatorAjaxUrl: ""
            };

            var settings = $.extend(defaults, settings);

            return this.each(function () {

                var megamenu = $(this);
                var megamenuNext = $(".megamenu-navigate-next", megamenu);
                var megamenuPrev = $(".megamenu-navigate-prev", megamenu);
                var megamenuContainer = $('.megamenu-dropdown-container');

                megamenuContainer.on('show.bs.dropdown', function (e) {
                    var id = e.target.id;
                    var link = $('.megamenu .nav-item [data-target="#' + id + '"]');
                    link.closest("li").addClass("active");

                    // fix elevateZoom z-index problem
                    $(".zoomContainer").css("z-index", "0")
                });

                megamenuContainer.on('hide.bs.dropdown', function (e) {
                    var id = e.target.id;
                    var link = $('.megamenu .nav-item [data-target="#' + id + '"]');
                    link.closest("li").removeClass("active");

                    // fix elevateZoom z-index problem
                    $(".zoomContainer").css("z-index", "999")
                });

                $(".dropdown-submenu", megamenu).click(function () {

                    var clicked_elem = $(event.target).parent();
                    var wasOpened = clicked_elem.hasClass("active");

                    if (!wasOpened) {
                        initRotator($(event.target).data("target"));
                    }
                });

                // prevent dropdowns from closing when clicking inside, to make product rotator work
                $(document).on('click', '.dropdown-menu', function (e) {
                    e.stopPropagation();
                });

                // show scroll buttons when menu items don't fit into screen
                $(window).resize(function () {

                    var liWidth = 0;
                    $(".megamenu li", megamenu).each(function () {
                        liWidth += $(this).width();
                    });

                    if (liWidth > megamenu.width()) {
                        megamenu.addClass("show-scroll-buttons");
                    }
                    else {
                        megamenu.removeClass("show-scroll-buttons");
                    }
                }).trigger('resize');

                //oh, oh, oh, oh, can't touch this ;-/
                var hammertime = new Hammer($(".megamenu")[0]);
                hammertime.add(new Hammer.Pan({ direction: Hammer.DIRECTION_HORIZONTAL, threshold: 80, pointers: 1 }));

                hammertime.on('panend', function (ev) {
                    if (ev.direction == Hammer.DIRECTION_LEFT) {
                        megamenuNext.trigger('click');
                    }
                    if (ev.direction == Hammer.DIRECTION_RIGHT) {
                        megamenuPrev.trigger('click');
                    }
                });

                megamenu.evenIfHidden(function (el) {

                    var scrollCorrection = null;
                    var lastVisibleElem = null;
                    var firstVisibleElem = null;
                    var isFirstItemVisible = true;
                    var isLastItemVisible = false;

                    megamenu.find('ul').wrap('<div class="nav-slider" style="overflow: hidden;position: relative;" />');

                    getCurrentNavigationElements();

                    megamenuNext.click(function () {

                        scrollCorrection = "+=" + ($(lastVisibleElem).position().left + 30 + $(lastVisibleElem).width() - $(".nav-slider").width()) + "px";

                        $(".nav-slider").scrollTo(scrollCorrection, 400, {
                            onAfter: function () {
                                getCurrentNavigationElements();
                            }
                        });
                    });

                    megamenuPrev.click(function () {
                        $(".nav-slider").scrollTo(firstVisibleElem, 400, {
                            offset: { left: -20 },
                            onAfter: function () {
                                getCurrentNavigationElements();
                            }
                        });
                    });

                    function getCurrentNavigationElements() {
                        firstVisibleElem = null;
                        var p = $(".nav-slider", megamenu);
                        var items = megamenu.find(".nav-item");

                        items.each(function (i, val) {

                            var el = $(val);

                            if ((el.offset().left > p.offset().left) && firstVisibleElem == null) {
                                firstVisibleElem = $(val).prev();

                                if (firstVisibleElem.position().left == 0)
                                    isFirstItemVisible = true;
                                else
                                    isFirstItemVisible = false;
                            }

                            // if visible
                            if (el.offset().left + el.width() > p.offset().left + p.width()) {

                                lastVisibleElem = $(val);

                                if (parseInt(el.offset().left) + parseInt(el.width()) == parseInt(p.offset().left) + parseInt(p.width()))
                                    isLastItemVisible = true;
                                else
                                    isLastItemVisible = false;

                                // we've got everything we need, so get out of here
                                return false;
                            }
                        });

                        // show or hide navigation buttons depending on whether first or last navitems are displayed
                        if (isFirstItemVisible)
                            megamenuPrev.css("display", "none");
                        else
                            megamenuPrev.css("display", "block");

                        if (isLastItemVisible)
                            megamenuNext.css("display", "none");
                        else
                            megamenuNext.css("display", "block");
                    }
                });

                function initRotator(containerId) {

                    var container = $(containerId);
                    var catId = container.data("entity-id");

                    if ($(".pl-slider", container).length == 0 && catId != null) {

                        var rotatorColumn = $(".rotator-" + catId);

                        // init throbber
                        rotatorColumn.height(300).throbber({ white: true, small: true, message: '' });

                        // wait a little to imply hard work is going on ;-)
                        setTimeout(function () {

                            $.ajax({
                                cache: false,
                                type: "POST",
                                url: settings.productRotatorAjaxUrl,
                                data: { "catId": catId },
                                success: function (data) {

                                    // add html view
                                    rotatorColumn.html(data);

                                    // init scrolling
                                    var scrollableProductList = $(".mega-menu-product-rotator.scroll", container);
                                    var plSlider = $(".pl-slider", container);

                                    scrollableProductList.productListScroller({
                                        interval: settings.productRotatorInterval,
                                        cycle: settings.productRotatorCycle,
                                        duration: settings.productRotatorDuration
                                    });

                                    // add buttons
                                    container.find(".sb").scrollButton({
                                        nearSize: 36,
                                        farSize: "50%",
                                        target: $(".pl-slider", container),
                                        showButtonAlways: true,
                                        autoPosition: true,
                                        position: "inside",
                                        offset: -100,
                                        handleCorners: true,
                                        smallIcons: true,
                                        isBtnGroup: true,
                                        btnType: "primary"
                                    });

                                    // set article item width (important for mobile devices)
                                    $(".item-box", rotatorColumn).css({ "min-width": rotatorColumn.width(), "max-width": rotatorColumn.width() });

                                    // and now its hammertime
                                    var hammertime = new Hammer(scrollableProductList[0]);
                                    hammertime.add(new Hammer.Pan({ direction: Hammer.DIRECTION_HORIZONTAL, threshold: 80, pointers: 1 }));

                                    hammertime.on('panend', function (ev) {
                                        if (ev.direction == Hammer.DIRECTION_LEFT) {
                                            plSlider.trigger('next');
                                        }
                                        if (ev.direction == Hammer.DIRECTION_RIGHT) {
                                            plSlider.trigger('prev');
                                        }
                                    });
                                }
                            });
                        }, 1000);
                    }
                }
            })
        }
    });
})(jQuery);