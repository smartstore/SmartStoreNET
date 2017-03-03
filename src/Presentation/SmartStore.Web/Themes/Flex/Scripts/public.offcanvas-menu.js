;

/*
** OffCanvasMenu
*/

var AjaxMenu = (function ($, window, document, undefined) {

    var isInitialised = false;
    var viewport = ResponsiveBootstrapToolkit;
    var selectedMenuItemId = 0;
    var menu = $("#offcanvas-menu #menu-container");

    $(function () {

        // if viewport <lg 
        if (viewport.is('<lg')) {
            AjaxMenu.initMenu();
        }
        
        // listen to viewport changed event and init if appropriate
        $(window).resize(
            viewport.changed(function () {
                if (viewport.is('<lg') && !isInitialised) {
                    AjaxMenu.initMenu();
                }
            })
        );

        // listen to clicks inside of #offcanvas-menu
        menu.on('click', '.nav-item', function (e) {
            
            var item = $(this);
            var entityId = item.data("id");
            var isAjaxNavigation = item.data("ajax");

            if (isAjaxNavigation == false) {
                // let event bubble up, so normal navigation via href attribute takes effect
                return true;
            }
            
            e.preventDefault();

            if (item.find(".nav-link").is("#manufacturer-tab")) {
                navigateToManufacturer();
            }
            else if (item.find(".nav-link").is("#help-tab")) {
                navigateToHelp();
            }
            else {
                navigateToMenuItem(entityId ? entityId : 0, item.hasClass("back-to-parent-cat") ? "right" : "left");
            }

            // for stopping event propagation
            return false;
        });
	});

    function navigateToMenuItem(entityId, direction) {

        // TODO: show throbber while elements are being loaded

	    $.ajax({
	        cache: false,
	        url: menu.data("url-item"),
	        data: { "categoryId": entityId },
	        type: 'POST',
	        success: function (response) {

	            // replace current menu content with response 
	            var categoryContainer = $(".category-container");
	            var firstCall = categoryContainer.length == 0;
	            var categoryTab = entityId != 0 ? menu : $("#ocm-categories");

                
	            if (firstCall) {
	                categoryTab.append(response);
	            }
	            else {
                  
	                var responseHtml = "";
	                var categoryContainerSlideIn;

	                if (entityId != 0)
	                {
                        
	                    responseHtml += '<div class="ocm-nav-layer slide-in-from-' + direction + '">';
	                    responseHtml += '   <div class="offcanvas-menu-subcat-header text-xs-right">';
	                    responseHtml += '       <button class="btn btn-secondary btn-flat btn-to-danger btn-lg btn-icon offcanvas-closer fs-h2">&#215;</button>';
	                    responseHtml += '   </div>';
	                    responseHtml +=     response;
	                    responseHtml += '</div>';

	                    categoryContainerSlideIn = $(responseHtml).appendTo(categoryTab);
	                }
	                else
	                {
	                    // TODO: get rid of this call
	                    categoryContainer.remove();
	                    navigateToHomeLayer();
	                    return;
	                }
	                
	                var categoryContainerSlideOut = $(".ocm-home-layer").length != 0 ? $(".ocm-home-layer") : $(".ocm-nav-layer:first");

	                _.delay(function () {
	                    categoryContainerSlideIn.addClass("in");
	                    categoryContainerSlideOut
                            .removeClass("in")
                            .addClass("out to-" + direction);
	                }, 100);

	                if (direction == "left") {
                        
	                }
	                else {
	                    
	                }

	                // remove slid container after transition
	                categoryContainerSlideIn.one(Prefixer.event.transitionEnd, function (e) {
	                    categoryContainerSlideOut.remove();
	                });
	            }
	        },
	        error: function (jqXHR, textStatus, errorThrown) {
	        	console.log(errorThrown);
	        },
	        complete: function () { }
	    });
	}

    function navigateToHomeLayer() {

	    $.ajax({
	        cache: false,
	        url: menu.data("url-home"),
	        type: 'POST',
	        success: function (response) {
	            var ocMenu = $("#menu-container");
	            ocMenu.html(response);

	            $('#offcanvas-menu #category-tab').tab('show');

	            // navigate to home
	            navigateToMenuItem(0);

	            AjaxMenu.initFooter();
	        },
	        error: function (jqXHR, textStatus, errorThrown) {
	            console.log(errorThrown);
	        },
	        complete: function () { }
	    });
	}

    // TODO: mit home layer zusammenlegen
    function navigateToManufacturer() {

        $.ajax({
            cache: false,
            url: menu.data("url-manufacturer"),
            type: 'POST',
            success: function (response) {

                var manuTab = $("#ocm-manufacturers");
                manuTab.html(response);

                $('#offcanvas-menu #manufacturer-tab').tab('show');

                // TODO: set initialized var and don't do requests twice
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
            },
            complete: function () { }
        });
    }

    function navigateToHelp() {

        var menuContent = $(".menubar-section .menubar");
        var tabContent = menu.find("#help-tab");
        var helpTab = $("#ocm-help");
        var isInitialized = tabContent.data("initialized");
        var response = '';

        if (isInitialized) {
            tabContent.tab('show');
            return;
        }
        
        // TODO: do it for every .dropdown & .menubar-link (if .menubar-link isn't within .dropdown)

        // dropdown service-links open
        var tempHelp = $(".service-links", menuContent);
        var menuTitle = tempHelp.find(".menubar-link > span").text();

        // menu title
        response += '<div class="category-info"><div class="category-name">' + menuTitle + '</div></div>';

        response += '<ul class="nav navbar-nav">';

        // foreach .dropdown 
        tempHelp.find(".dropdown-item").each(function (index) {
            var navItem = $(this).clone();

            navItem.removeClass("dropdown-item");
            navItem.addClass("nav-link");

            response += '<li class="sub-cat nav-item" data-ajax="false">' + navItem.outerHtml() + '</li>';
        });

        response += "</ul>";

        helpTab.html(response);
        tabContent.data("initialized", true);
        tabContent.tab('show');
    }

	return {

	    initMenu: function () {

	        var offcanvasMenu = $('#offcanvas-menu');
	        var menuContent = $(".menubar-section .menubar");
	        var selectedMenuItemId = $(".megamenu .navbar-nav").data("selected-menu-item");

	        if (selectedMenuItemId == 0)
	        {
	            navigateToHomeLayer();
	        }
	        else {
	            navigateToMenuItem(selectedMenuItemId);
	        }

	        isInitialised = true;
	        return;
	    },

	    initFooter: function () {

	        // TODO: don't forget to adapt elems or even blend them out when there's just one or no elem

	        var footer = $(".offcanvas-menu-footer");
	        var languageSelector = $(".menubar-section .language-selector");
	        var currencySelector = $(".menubar-section .currency-selector");
	        var ocmLanguageSelector = $("#ocm-language-selector", footer);
	        var ocmCurrencySelector = $("#ocm-currency-selector", footer);
	        var selectTitleLanguage = $(".menubar-link > span", languageSelector).text();
	        var selectTitleCurrency = $(".menubar-link > span", currencySelector).text();
	        var languageOptions = "";
	        var currencyOptions = "";

	        $(languageSelector).find(".dropdown-item").each(function () {
	            var link = $(this);
	            languageOptions += '<option value="' + link.attr("href") + '">' + link.text() + '</option>';
	        });

	        $(currencySelector).find(".dropdown-item").each(function () {
	            var link = $(this);
	            currencyOptions += '<option value="' + link.attr("href") + '">' + link.text() + '</option>';
	        });

	        $("span", ocmLanguageSelector).text(selectTitleLanguage);
	        $("span", ocmCurrencySelector).text(selectTitleCurrency);

	        $(".form-control", ocmLanguageSelector).append(languageOptions);
	        $(".form-control", ocmCurrencySelector).append(currencyOptions);

            // on change navigate to value 
	        $(footer).find(".form-control").on("change", function (e) {
	            var select = $(this);
	            window.setLocation(select.val());
	        });
	    }
	}
})(jQuery, this, document);