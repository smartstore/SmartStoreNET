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
            else if (item.find(".nav-link").is("#service-tab")) {
                navigateToService();
            }
            else if (item.find(".nav-link").is("#category-tab")) {
                navigateToHomeLayer();
            }
            else if (item.parents(".tab-pane").is("#ocm-categories") || item.parents('.category-container').length) {

                navigateToMenuItem(entityId ? entityId : 0, item.hasClass("navigate-back") ? "left" : "right");
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
	            var categoryContainer = menu.find(".category-container");
	            var firstCall = categoryContainer.length == 0;
	            var categoryTab = entityId != 0 ? menu : menu.find("#ocm-categories");

	            if (firstCall) {

	                if (entityId != 0)
	                    categoryTab.append(wrapAjaxResponse(response, direction, " in"));
                    else 
	                    categoryTab.append(response);
	            }
	            else {

	                var categoryContainerSlideIn;

	                if (entityId != 0)
	                {   
	                    categoryContainerSlideIn = $(wrapAjaxResponse(response, direction, "")).appendTo(categoryTab);
	                }
	                else
	                {
	                    // TODO: get rid of this call
	                    categoryContainer.remove();
	                    navigateToHomeLayer();
	                    return;
	                }
	                
	                var categoryContainerSlideOut = menu.find(".ocm-home-layer").length != 0 ? menu.find(".ocm-home-layer") : menu.find(".ocm-nav-layer:first");

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

	            menu.html(response);
	            menu.find("#category-tab").tab('show');
	            
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

    function navigateToManufacturer() {

        var tabContent = menu.find("#manufacturer-tab");
        var manuTab = $("#ocm-manufacturers");
        var isInitialized = tabContent.data("initialized");

        if (isInitialized) {
            tabContent.tab('show');
            return;
        }

        $.ajax({
            cache: false,
            url: menu.data("url-manufacturer"),
            type: 'POST',
            success: function (response) {
                manuTab.html(response);
                tabContent.tab('show');
                tabContent.data("initialized", true);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
            },
            complete: function () { }
        });
    }

    function navigateToService() {

        var menuContent = $(".menubar-section .menubar").clone();
        var tabContent = menu.find("#service-tab");
        var serviceTab = $("#ocm-service");
        var isInitialized = tabContent.data("initialized");
        
        if (isInitialized) {
            tabContent.tab('show');
            return;
        }

        // hide currency & language selectors 
        menuContent.find(".currency-selector, .language-selector").addClass("hidden-xs-up");

        // open dropdown elements initially
        menuContent.find(".dropdown").addClass("open");

        serviceTab.html(menuContent);
        tabContent.data("initialized", true);
        tabContent.tab('show');

        return;
    }

    function wrapAjaxResponse(response, direction, first) {
        var responseHtml = "";

        responseHtml += '<div class="ocm-nav-layer offcanvas-scrollable slide-in-from-' + direction + first + '">';
        responseHtml += response;
        responseHtml += '</div>';

        return responseHtml;
    }

	return {

	    initMenu: function () {

	        var offcanvasMenu = $('#offcanvas-menu');
	        var menuContent = $(".menubar-section .menubar");
	        var selectedMenuItemId = $(".megamenu .navbar-nav").data("selected-menu-item");

	        if (selectedMenuItemId == 0) {
	            navigateToHomeLayer();
	        }
	        else {
	            navigateToMenuItem(selectedMenuItemId);
	        }

	        isInitialised = true;
	        return;
	    },

	    initFooter: function () {

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
	            var selected = link.data("selected") ? ' selected="selected" ' : '';
	            languageOptions += '<option value="' + link.attr("href") + '"' + selected + '>' + link.text() + '</option>';
	        });

	        $(currencySelector).find(".dropdown-item").each(function () {
	            var link = $(this);
	            var selected = link.data("selected") ? ' selected="selected" ' : '';
	            currencyOptions += '<option value="' + link.attr("href") + '"' + selected + '>' + link.text() + '</option>';
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