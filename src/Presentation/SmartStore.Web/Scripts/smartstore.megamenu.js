(function ($) {

    $.fn.extend({
        megaMenu: function (settings) {

            var defaults = {
                productRotatorInterval: 4000,
                productRotatorDuration: 300,
                productRotatorCycle: false,
                productRotatorAjaxUrl: ""
            };

            settings = $.extend(defaults, settings);

            return this.each(function () {
                var megamenuContainer = $(this);
                var megamenu = $(".megamenu", megamenuContainer);
                var isSimple = megamenu.hasClass("simple");
                var megamenuNext = $(".megamenu-nav--next", megamenuContainer);
                var megamenuPrev = $(".megamenu-nav--prev", megamenuContainer);
                var megamenuDropdownContainer = $('.megamenu-dropdown-container');
                var navElems = megamenu.find(".navbar-nav .nav-item");	// li
                var zoomContainer = $(".zoomContainer");		// needed to fix elevateZoom z-index problem e.g. in product detail gallery
                var closingTimeout = 0;							// timeout to handle delay of dropdown closing
                var openTimeout;								// timeout to handle opening attempts in tryOpen
                var tempLink; 		// for temporary storage of the link that was passed into tryOpen, its needed to determine whether a new link was passed into tryOpen so the timeout for the old link can be cleared

                function closeMenuHandler(link, closeImmediatly) {
                    closingTimeout = setTimeout(function () { closeNow(link) }, 250);
                }

                function closeNow(link) {
                    $(link.data("target")).removeClass("show");

                    if (link.hasClass("dropdown-toggle")) {
                        link.closest("li").removeClass("active");
                    }

                    zoomContainer.css("z-index", "999");
                }

                function tryOpen(link) {
                	// if new link was passed into function clear tryOpen-timeout
                    if (tempLink && link.data("target") != tempLink.data("target")) {
                        clearTimeout(openTimeout);
                    }

                    // just open if there are no open menus, else wait and try again as long as there is a menu open
                    if (navElems.hasClass('active') || megamenuDropdownContainer.hasClass('show')) {
                        tempLink = link;
                        openTimeout = setTimeout(function () { tryOpen(link); }, 50);
                    }
                    else {
                    	clearTimeout(openTimeout);

                    	$(link.data("target")).addClass("show");

                    	if (link.hasClass("dropdown-toggle")) {
                    		link.closest("li").addClass("active");
                    	}

                    	initRotator(link.data("target"));

                    	zoomContainer.css("z-index", "0");          	
                    }
                }

                if (Modernizr.touchevents) {

                    // Handle opening events for touch devices
                    megamenuContainer.on('clickoutside', function (e) {
                        closeNow($(".nav-item.active .nav-link"));
                    });

                    navElems.on("click", function (e) {
                        var link = $(this).find(".nav-link");
                        var opendMenuSelector = $(".nav-item.active .nav-link").data("target");

                        if (opendMenuSelector != link.data("target")) {
                            e.preventDefault();
                            closeNow($(".nav-item.active .nav-link"));
                            tryOpen(link);
                        }
                    });
                }
                else {

                    // Handle opening events for desktop workstations

                    $(".dropdown-menu", megamenuContainer).on('mouseenter', function (e) {
                        clearTimeout(closingTimeout);
                    })
                    .on('mouseleave', function () {
                        var targetId = $(this).parent().attr("id");
                        var link = megamenu.find("[data-target='#" + targetId + "']");

                        closeMenuHandler(link);
                    });

                    navElems.on("mouseenter", function () {
                        var link = $(this).find(".nav-link");

                        // if correct dropdown is already open then don't open it again
                        var openedMenuSelector = $(".nav-item.active .nav-link").data("target");
                        var isActive = navElems.is(".active");

                        if ($(this).hasClass("nav-item") && openedMenuSelector != link.data("target")) {
                            closeNow($(".nav-item.active .nav-link"));
                        }
                        else if (openedMenuSelector == link.data("target")) {
                            clearTimeout(closingTimeout);
                            return;
                        }

                        // if one menu is already open, it means the user is currently using the menu, so either ...
                        if (isActive) {
                            // ... open at once 
                            tryOpen(link);
                        }
                        else {
                            // ... or open delayed
                            openTimeout = setTimeout(function () { tryOpen(link); }, 300);
                        }
                    })
                    .on("mouseleave", function () {

                        clearTimeout(openTimeout);

                        var link = $(this).find(".nav-link");
                        closeMenuHandler(link);
                    });
                }

                // correct dropdown position
                if (isSimple) {

                    var event = Modernizr.touchevents ? "click" : "mouseenter";

                    navElems.on(event, function (e) {
                        var navItem = $(this);
                        var opendMenu = $(navItem.find(".nav-link").data("target")).find(".dropdown-menu");
                        var offsetLeft = (navItem.offset().left - megamenu.offset().left) + 10;

                        if (offsetLeft < 0) {
                            offsetLeft = 0;
                        }
                        else if (offsetLeft + opendMenu.width() > megamenu.width()) {
                            offsetLeft = megamenu.width() - opendMenu.width();
                        }
                        else if (navItem.width() > opendMenu.width()) {
                            offsetLeft = offsetLeft + (navItem.width() - opendMenu.width()) + 5;
                        }

                        opendMenu.css("left", offsetLeft);
                    });
                }

                megamenuContainer.evenIfHidden(function (el) {
                    var scrollCorrection = null;
                    var lastVisibleElem = null;
                    var firstVisibleElem = null;
                    var isFirstItemVisible = true;
                    var isLastItemVisible = false;

                    megamenuContainer.find('ul').wrap('<div class="nav-slider" style="overflow: hidden;position: relative;" />');

                    // 
                    getCurrentNavigationElements();

                    var nav = $('.megamenu .navbar-nav');
                    var navSlider = $('.megamenu .nav-slider');
                    updateNavState();

                    if (!Modernizr.touchevents) {

                        megamenuNext.click(function (e) {
                            e.preventDefault();
                            scrollToNextInvisibleNavItem(false);
                        });

                        megamenuPrev.click(function (e) {
                            e.preventDefault();
                            scrollToNextInvisibleNavItem(true);
                        });
                    }

                    function scrollToNextInvisibleNavItem(backwards) {
                        // determine the first completely visible nav item (either from left or right side, depending on 'backwards')
                        var firstVisible = findFirstVisibleNavItem(backwards);

                        // depending on 'backwards': take next or previous nav item (it's not visible yet and should scroll into the visible area now)  
                        var nextItem = backwards
							? firstVisible.prev()
							: firstVisible.next();

                        if (nextItem.length == 0)
                            return;

                        // determine left pos of the item 
                        var leftPos = nextItem.position().left;

                        // 30 = offset for arrows
                        // if 'backwards == true': scroll to the left position of the current item 
                        var newMarginLeft = (leftPos * -1) + 1 + 30;
                        if (!backwards) {
                            // if 'backwards == true': scroll to the right position of the current item 
                            var rightPos = leftPos + nextItem.outerWidth(true) + 1;
                            newMarginLeft = navSlider.width() - rightPos - (nextItem[0].nextElementSibling ? 30 : 0);
                        }

                        newMarginLeft = Math.min(0, newMarginLeft);

                        nav.css('margin-left', Math.ceil(newMarginLeft) + 'px').one(Prefixer.event.transitionEnd, function (e) {
                            // performs UI update after end of animation (.one(trans...))
                            updateNavState();
                        });
                    }

                    function findFirstVisibleNavItem(fromLeft) {
                        var navItems = navElems;
                        if (!fromLeft) {
                            // turn nav items around as we start iteration from the right side
                            navItems = $($.makeArray(navElems).reverse());
                        }

                        var result;
                        var cntWidth = navSlider.width();
                        var curMarginLeft = parseFloat(nav.css('margin-left'));

                        function isInView(pos) {
                            var realPos = pos + curMarginLeft;
                            return realPos >= 0 && realPos < cntWidth;
                        }

                        navItems.each(function (i, el) {

                            // iterates all nav items from the left OR the right side and leaves the intartion once the left AND the right edge are displayed within the viewpoint
                            var navItem = $(el);
                            var leftPos = navItem.position().left;
                            var leftIn = isInView(leftPos);
                            if (leftIn) {
                                var rightIn = isInView(leftPos + navItem.outerWidth(true));
                                if (rightIn) {
                                    result = navItem;
                                    return false;
                                }
                            }
                        });

                        return result;
                    }

                    function updateNavState() {
                        // updates megamenu status: arrows etc.
                        var realNavWidth = 0;
                        navElems.each(function (i, el) { realNavWidth += parseFloat($(this).outerWidth(true)); });

                        var curMarginLeft = parseFloat(nav.css('margin-left'));

                        if (realNavWidth > megamenu.width()) {
                            // nav items don't fit in the megamenu container: display next arrow 
                            megamenu.addClass('megamenu-blend--next');
                        }

                        if (curMarginLeft < 0) {
                            // user has scrolled to the right: show prev arrow 
                            megamenu.addClass('megamenu-blend--prev');

                            // determine whether we're at the end
                            var endReached = nav.width() >= realNavWidth;
                            if (endReached)
                                megamenu.removeClass('megamenu-blend--next')
                            else
                                megamenu.addClass('megamenu-blend--next');
                        }
                        else {
                            // we're at the beginning: fade out prev arrow
                            megamenu.removeClass('megamenu-blend--prev');
                        }
                    }

                    // on touch
                    if (Modernizr.touchevents) {
                        megamenu.tapstart(function () {
                            closeNow($(".nav-item.active .nav-link"));
                        }).tapend(function () {
                            getCurrentNavigationElements();
                        });
                    }

                    function onPageResized() {
                    	updateNavState();

                    	var liWidth = 0;
                    	navElems.each(function () { liWidth += $(this).width(); });

                    	if (liWidth > megamenuContainer.width()) {
                    		megamenuContainer.addClass("show-scroll-buttons");
                    	}
                    	else {
                    		megamenuContainer.removeClass("show-scroll-buttons");
                    	}

                    	getCurrentNavigationElements();

                    	megamenuDropdownContainer.find('.mega-menu-product-rotator > .artlist-grid').each(function(i, el) {
                    		try {
                    			$(this).slick('unslick');
                    			applyCommonPlugins($(this).closest('.rotator-content'));
                    		}
							catch (err) { }
                    	});
                    }

                	// show scroll buttons when menu items don't fit into screen
                    EventBroker.subscribe("page.resized", function (msg, viewport) {
                    	onPageResized();
                    });
                    onPageResized();

                    function getCurrentNavigationElements() {
                        firstVisibleElem = null;
                        isLastItemVisible = false;

                        var p = $(".nav-slider", megamenuContainer);

                        navElems.each(function (i, val) {

                            var el = $(val);

                            if ((el.offset().left > p.offset().left) && firstVisibleElem == null) {
                                firstVisibleElem = el.prev();

                                if (firstVisibleElem.position().left == 0)
                                    isFirstItemVisible = true;
                                else
                                    isFirstItemVisible = false;
                            }

                            // if visible
                            if (parseInt(el.offset().left) + parseInt(el.width()) == parseInt(p.offset().left) + parseInt(p.width())) {

                                lastVisibleElem = el;

                                if (parseInt(el.offset().left) + parseInt(el.width()) == parseInt(p.offset().left) + parseInt(p.width()))
                                    isLastItemVisible = true;
                                else
                                    isLastItemVisible = false;

                                // we've got everything we need, so get out of here
                                return false;
                            }
                        });

                        // show or hide navigation buttons depending on whether first or last navitems are displayed
                        if (!isFirstItemVisible) {
                            megamenu.addClass("megamenu-blend--prev");
                        }
                        else {
                            megamenu.removeClass("megamenu-blend--prev");
                        }

                        if (!isLastItemVisible) {
                            megamenu.addClass("megamenu-blend--next");
                        }
                        else {
                            megamenu.removeClass("megamenu-blend--next");
                        }
                    }
                });

                function initRotator(containerId) {
                    var container = $(containerId);
                    var catId = container.data("id");
                    var displayRotator = container.data("display-rotator");

                    // reinit slick product rotator
                    container.find('.mega-menu-product-rotator > .artlist-grid').each(function (i, el) {
                        try {
                            $(this).slick('unslick');
                            applyCommonPlugins($(this).closest('.rotator-content'));
                        }
                        catch (err) { }
                    });

                    //if ($(".pl-slider", container).length == 0 && catId != null && displayRotator) {
                    if (catId != null && displayRotator) {

                        var rotatorColumn = $(".rotator-" + catId);

                        // clear content & init throbber
                        rotatorColumn.find(".rotator-content")
							.html('<div class="placeholder"></div>')
							.throbber({ white: true, small: true, message: '' });

                        // wait a little to imply hard work is going on ;-)
                        setTimeout(function () {

                            $.ajax({
                                cache: false,
                                type: "POST",
                                url: settings.productRotatorAjaxUrl,
                                data: { "catId": catId },
                                success: function (data) {

                                    // add html view
                                    rotatorColumn.find(".rotator-content").html(data);

                                	// Init carousel
                                    applyCommonPlugins(container);

                                    if (container.hasClass("show")) {
                                        container.data("display-rotator", false);
                                    }
                                }
                            });
                        }, 1000);
                    }
                    else {
                        container.find(".placeholder").addClass("empty");
                    }
                }
            })
        }
    });
})(jQuery);