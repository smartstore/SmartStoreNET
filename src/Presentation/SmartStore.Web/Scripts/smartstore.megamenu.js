(function ($) {

    $.fn.extend({
        megaMenu: function (settings) {

            var defaults = {
                productRotatorInterval: 4000,
                productRotatorDuration: 300,
                productRotatorCycle: false,
                productRotatorAjaxUrl: ""
            };

			var rtl = SmartStore.globalization.culture.isRTL,
				marginX = rtl ? 'margin-right' : 'margin-left';

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

				function alignDrop(popper, drop, container) {
					var nav = $(".navbar-nav", container),
						left,
						right,
						popperWidth = popper.width(),
						dropWidth = drop.width(),
						containerWidth = container.width();

					if (!rtl) {
						left = Math.ceil(popper.position().left + parseInt(nav.css('margin-left')));
						right = "auto";

						if (left < 0) {
							left = 0;
						}
						else if (left + dropWidth > containerWidth) {
							left = "auto";
							right = 0;
						}
					}
					else {
						left = "auto";
						right = Math.ceil(containerWidth - (popper.position().left + popperWidth));

						if (right < 0) {
							right = 0;
						}
						else if (right + dropWidth > containerWidth) {
							left = 0;
							right = "auto";
						}
					}

					if (popperWidth > dropWidth) {
						// ensure that drop is not smaller than popper
						drop.width(popperWidth);
					}

					drop.toggleClass("ar", (rtl && left == "auto") || _.isNumber(right));

					if (_.isNumber(left)) left = left + "px";
					if (_.isNumber(right)) right = right + "px";

					// jQuery does not accept "!important"
					drop[0].style.setProperty('left', left, 'important');
					drop[0].style.setProperty('right', right, 'important');
				}

                // correct dropdown position
                if (isSimple) {
					var event = Modernizr.touchevents ? "click" : "mouseenter";

                    navElems.on(event, function (e) {
						var navItem = $(this);
						var targetSelector = navItem.find(".nav-link").data("target");
						if (!targetSelector)
							return;

						var drop = $(targetSelector).find(".dropdown-menu");
						if (!drop.length)
							return;

						alignDrop(navItem, drop, megamenu);
                    });
				}

                megamenuContainer.evenIfHidden(function (el) {
                    var scrollCorrection = null;
                    var lastVisibleElem = null;
                    var firstVisibleElem = null;
                    var isFirstItemVisible = true;
                    var isLastItemVisible = false;

                    megamenuContainer.find('ul').wrap('<div class="nav-slider" style="overflow:hidden; position:relative;" />');

					var navSlider = $(".nav-slider", megamenu);
					var nav = $(".navbar-nav", navSlider);

                    if (!Modernizr.touchevents) {
                        megamenuNext.on('click', function (e) {
							e.preventDefault();
                            scrollToNextInvisibleNavItem(false);
                        });

						megamenuPrev.on('click', function (e) {
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
						var offset = Math.abs(parseFloat(nav.css(marginX)));

                        // 30 = offset for arrows
                        // if 'backwards': scroll to the left position of the current item 
						var newMarginStart = rtl
							? leftPos - offset - 31
							: (leftPos * -1) + 31;

                        if ((!rtl && !backwards) || (rtl && backwards)) {
                            // if 'forward': scroll to the right position of the current item 
							var rightPos = leftPos + nextItem.outerWidth(true) + 1;
							newMarginStart = rtl
								? (nav.width() - rightPos - (nextItem[0].previousElementSibling ? 30 : 0)) * -1
								: navSlider.width() - rightPos - (nextItem[0].nextElementSibling ? 30 : 0);
                        }

						newMarginStart = Math.min(0, newMarginStart);

						nav.css(marginX, Math.ceil(newMarginStart) + 'px').one(Prefixer.event.transitionEnd, function (e) {
                            // performs UI update after end of animation (.one(trans...))
                            updateNavState();
                        });
                    }

                    function findFirstVisibleNavItem(fromStart) {
                        var navItems = navElems;
						if (!fromStart) {
                            // turn nav items around as we start iteration from the right side
                            navItems = $($.makeArray(navElems).reverse());
                        }

                        var result;
                        var cntWidth = navSlider.width();
						var curMarginStart = rtl ? 0 : parseFloat(nav.css(marginX));

                        function isInView(pos) {
							var realPos = pos + curMarginStart;
                            return realPos >= 0 && realPos < cntWidth;
                        }

                        navItems.each(function (i, el) {
                            // iterates all nav items from the left OR the right side and breaks loop once the left AND the right edges fall into the viewport
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

						var curMarginStart = parseFloat(nav.css(marginX));

                        if (realNavWidth > megamenu.width()) {
                            // nav items don't fit in the megamenu container: display next arrow 
                            megamenu.addClass('megamenu-blend--next');
                        }

						if (curMarginStart < 0) {
                            // user has scrolled: show prev arrow 
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
							updateNavState();
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

                    	megamenuDropdownContainer.find('.mega-menu-product-rotator > .artlist-grid').each(function(i, el) {
                    		try {
								$(this).slick('unslick');
								$(this).attr('data-slick', '{"dots": false, "autoplay": true}');
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
                });

                function initRotator(containerId) {
                    var container = $(containerId);
                    var catId = container.data("id");
                    var displayRotator = container.data("display-rotator");

                    // reinit slick product rotator
                    container.find('.mega-menu-product-rotator > .artlist-grid').each(function (i, el) {
                        try {
							$(this).slick('unslick');
							$(this).attr('data-slick', '{"dots": false, "autoplay": true}');
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

									var list = container.find('.mega-menu-product-rotator > .artlist-grid');
									list.attr('data-slick', '{"dots": false, "autoplay": true}');

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