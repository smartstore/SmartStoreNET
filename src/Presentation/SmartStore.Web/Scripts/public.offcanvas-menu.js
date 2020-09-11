"use strict";

(function ($, window, document, undefined) {
    var viewport = ResponsiveBootstrapToolkit;

    function wrapLayer(response, addClasses, id) {
        return [
            '<div class="ocm-nav-layer layer offcanvas-scrollable ' + addClasses + '" data-id="' + id + '">',
            response,
            '</div>'
        ].join('');
    };

    var AjaxMenu = (function () {
        function AjaxMenu(navbar, menuContainer) {
            this.initialized = false;

            this.navbar = $(navbar);
            this.container = $(menuContainer);
            this.url = this.container.data('url');

            var meta = $('head > meta[property="sm:pagedata"]');

            // currentNode = the current page
            this.currentNode = JSON.parse(meta.attr('content') || '{}');
        }

        AjaxMenu.prototype.initialize = function () {
            var self = this;

            if (!this.initialized && !this._loading) {
                // Load the layer for the current node
                this._loadLayer(this.currentNode.id, function (layer) {
                    // selectedNode = the parent node that was clicked by user
                    self.selectedNodeId = self.currentNode.parentId;

                    // Add the layers' response html to the container element
                    layer.addClass('show').prependTo(self.container);

                    // Activate first tab if any
                    layer.find('[data-toggle="tab"]').first().trigger('click');

                    self.initialized = true;
                });

                // Tab click event
                this.container.on('click', '[data-toggle="tab"]', function (e) {
                    var item = $(e.currentTarget);

                    if (!item.data('initialized')) {
                        e.preventDefault();

                        var paneId = item.attr('href');
                        var callback = function () {
                            item.data('initialized', true);
                            item.tab('show');
                        };

                        if (paneId === '#ocm-brands') {
                            self._initBrandsTab(item.data('url'), $(paneId), callback);
                        }
                        else if (paneId === '#ocm-service') {
                            self._initServiceTab($(paneId), callback);
                        }

                        return false;
                    }
                });

                // Menu click event (parent node item & back button)
                this.container.on('click', '.ocm-link', function (e) {
                    var el = $(this);

                    if (!el.data("ajax"))
                        return true;

                    var li = el.parent();
                    var dir = li.is(".ocm-back") ? "out" : "in";

                    if (dir !== 'out' && li.is(".animating")) {
                        // prevent double clicks
                        return false;
                    }

                    e.preventDefault();
                    li.addClass("animating");

                    var nodeId = el.data("id");
                    self.navigateToLayer(nodeId || 0, dir, function (layer) {
                        li.removeClass("animating");
                    });

                    return false;
                });
            }
        };

        AjaxMenu.prototype._loadLayer = function (nodeId, callback) {
            var self = this;

            this._loading = true;
            $.ajax({
                cache: false,
                url: this.url,
                data: { currentNodeId: this.currentNode.id, targetNodeId: nodeId },
                type: 'POST',
                success: function (response) {
                    var layer = $(response);
                    var isHomeLayer = layer.is('.ocm-home-layer');

                    // Submenus (.ocm-menu) must be wrapped with an '.ocm-nav-layer' element
                    // The home layer has a wrapper already (.ocm-home-layer)
                    if (!isHomeLayer) {
                        layer = $(wrapLayer(response, "", nodeId));
                    }

                    if (isHomeLayer) {
                        // When loading the home layer the first time,
                        // we need to initialize stuff.

                        // Init footer controls
                        self._initFooter(layer.find('.offcanvas-menu-footer'));
                    }

                    callback(layer);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log(errorThrown);
                },
                complete: function () {
                    self._loading = undefined;
                }
            });
        };

        AjaxMenu.prototype._initFooter = function (footer) {
            if (footer.length === 0)
                return;

            var langSelector = $(".menubar-section .language-selector");
            var currencySelector = $(".menubar-section .currency-selector");
            var ocmLangSelector = $("#ocm-language-selector", footer);
            var ocmCurrencySelector = $("#ocm-currency-selector", footer);
            var displayCurrencySelector = currencySelector.length > 0;
            var displayLangSelector = langSelector.length > 0;

            if (!displayCurrencySelector && !displayLangSelector) {
                return;
            }
            else {
                footer.removeClass("d-none");
            }

            // Converts all .dropdown-item elements of a dropdown menu to an <option> array
            var dropdownToOptions = function (dd) {
                var opts = [];
                $(dd).find(".dropdown-item").each(function () {
                    var link = $(this);
                    var selected = link.data("selected") ? ' selected="selected" ' : '';
                    opts.push('<option value="' + link.attr("href") + '"' + selected + '>' + link.data("abbreviation") + '</option>');
                });

                return opts;
            };

            if (displayCurrencySelector) {
                ocmCurrencySelector.removeClass("d-none");
                $(".form-control", ocmCurrencySelector).append(dropdownToOptions(currencySelector).join(''));
            }

            if (displayLangSelector) {
                ocmLangSelector.removeClass("d-none");
                $(".form-control", ocmLangSelector).append(dropdownToOptions(langSelector).join(''));
            }

            // on change navigate to value 
            $(footer).on('change', '.form-control', function (e) {
                window.setLocation($(this).val());
            });
        };

        AjaxMenu.prototype._initServiceTab = function (pane, callback) {
            // Menubar is the top menu (above logo section)
            var menubar = $(".menubar-section .menubar").clone().removeClass('navbar navbar-slide');

            // remove currency & language selectors 
            menubar.find(".currency-selector, .language-selector").remove();

            // remove data-toggle attributes
            menubar.find("[data-toggle=dropdown]").removeAttr("data-toggle");

            // open MyAccount dropdown initially
            var myAccount = menubar.find("#menubar-my-account");
            myAccount.find(".dropdown").addClass("openend");
            myAccount.find(".dropdown-menu").addClass("show");

            // place MyAccount menu on top
            menubar.prepend(myAccount);

            menubar.find(".dropdown-item").one("click", function (e) {
                e.stopPropagation();
            });

            // handle dropdown opening
            pane.on("click", ".dropdown > .menubar-link", function (e) {
                var dropdown = $(this).parent();
                var dropdownMenu = dropdown.find(".dropdown-menu");

                if (dropdownMenu.length === 0)
                    return true;

                e.preventDefault();

                dropdown.toggleClass("openend");
                dropdownMenu.toggleClass("show");

                return false;
            });

            pane.html(menubar);

            callback();
        };

        AjaxMenu.prototype._initBrandsTab = function (url, pane, callback) {
            var self = this;

            $.ajax({
                cache: false,
                url: url,
                type: 'POST',
                success: function (response) {
                    pane.html(response);

                    if (self.currentNode.type === 'brand' && self.currentNode.id) {
                        pane.find("li[data-id='" + self.currentNode.id + "']").addClass("selected");
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log(errorThrown);
                },
                complete: function () {
                    callback();
                }
            });
        };

        AjaxMenu.prototype.navigateToLayer = function (nodeId, dir, callback) {
            var self = this,
                currentLayer = $(".layer.show", self.container),
                nextLayer = currentLayer.next(),
                prevLayer = currentLayer.prev();

            var finalize = function (layer) {
                layer.one(Prefixer.event.transitionEnd, function (e) {
                    callback(layer);
                    self.selectedNodeId = nodeId;
                });
            };

            if (dir === "out") {
                // Check whether a previous layer exists (if it exists, it is always the right one to navigate to)
                if (prevLayer.length) {
                    // special treatment when navigating back to home layer
                    if (prevLayer.is(".ocm-home-layer")) {
                        prevLayer
                            .find(".ocm-nav-layer")
                            .removeClass("offcanvas-scrollable ocm-nav-layer layer");
                    }

                    currentLayer.removeClass("show");
                    prevLayer.addClass("show");
                    finalize(prevLayer);
                    return;
                }
                // If no previous layer exists, make ajax call and prepend response
            }
            else if (dir === "in") {
                // Check whether a next layer exists and if it has the same id as the element to which the user is navigating to
                if (nextLayer.data("id") === nodeId) {
                    currentLayer.removeClass("show");
                    nextLayer.addClass("show");
                    finalize(nextLayer);
                    return;
                }
                else {
                    // The layer to navigate to doesn't exist, so we remove all subsequent layers to build a new clean chain
                    currentLayer.nextAll().remove();
                }
            }

            // Get the data from server
            this._loadLayer(nodeId, function (layer) {
                var elSlideIn,
                    elSlideOut = currentLayer;

                elSlideIn = dir === "out"
                    ? layer.prependTo(self.container)
                    : layer.appendTo(self.container);

                // Activate first tab if any
                layer.find('[data-toggle="tab"]').first().tab('show');

                _.delay(function () {
                    elSlideIn.addClass("show");
                    if (dir) elSlideOut.removeClass("show");
                }, 100);

                if (!dir) {
                    elSlideIn = $(".ocm-home-layer");
                    elSlideOut = nextLayer;

                    elSlideIn
                        .addClass("show")
                        .find(".ocm-nav-layer")
                        .removeClass("offcanvas-scrollable ocm-nav-layer layer");

                    elSlideOut.removeClass("show");
                }

                finalize(elSlideIn);
            });
        };

        return AjaxMenu;
    })();

    $(function () {
        var ajaxMenu = SmartStore.AjaxMenu = new AjaxMenu('#menu-main', '#offcanvas-menu-container');

        // if viewport <lg 
        if (viewport.is('<lg')) {
            ajaxMenu.initialize();
        }
        else {
            // Listen to viewport changed event and init if appropriate
            var token = EventBroker.subscribe("page.resized", function (msg, viewport) {
                if (!ajaxMenu.initialized && viewport.is('<lg')) {
                    ajaxMenu.initialize();
                    // Once initialized, there's no need to listen to resized event anymore.
                    EventBroker.unsubscribe(token);
                }
            });
        }

    });
})(jQuery, this, document);