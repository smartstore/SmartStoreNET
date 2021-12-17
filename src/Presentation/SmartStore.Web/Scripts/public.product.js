; (function ($, window, document, undefined) {

    var pluginName = 'productDetail';
    var galPluginName = "smartGallery";

    function ProductDetail(element, options) {

        var self = this;

        this.element = element;
        var el = this.el = $(element);

        var meta = $.metadata ? $.metadata.get(element) : {};
        var opts = this.options = $.extend(true, {}, options, meta || {});

        this.init = function () {
            var opts = this.options;

            this.createGallery(opts.galleryStartIndex);

            // Update product data and gallery
            $(el).on('change', ':input:not(.skip-update)', function (e) {
                var inputCtrl = $(this);
                var ctx = inputCtrl.closest('.update-container');
                var isTouchSpin = inputCtrl.parent(".bootstrap-touchspin").length > 0;
                var isFileUpload = inputCtrl.data("fileupload");
                var isDateTime = inputCtrl.hasClass("date-part");

                if (ctx.length === 0) {
                    // It's an associated or bundled item.
                    ctx = el;
                }

                ctx.doAjax({
                    data: ctx.find(':input').serialize(),
                    callbackSuccess: function (response) {
                        self.updateDetailData(response, ctx, isTouchSpin, isFileUpload, isDateTime);

                        if (ctx.hasClass('pd-bundle-item')) {
                            // Update bundle price too.
                            $('#main-update-container').doAjax({
                                data: $('.pd-bundle-items').find(':input').serialize(),
                                callbackSuccess: function (response2) {
                                    self.updateDetailData(response2, $('#main-update-container'), isTouchSpin, isFileUpload, isDateTime);
                                }
                            });
                        }
                    }
                });
            });

            return this;
        };

        this.updateDetailData = function (data, ctx, isTouchSpin, isFileUpload, isDateTime) {
            var gallery = $('#pd-gallery').data(galPluginName);

            // Image gallery needs special treatment
            if (!isFileUpload) {
                if (data.GalleryHtml) {
                    var cnt = $('#pd-gallery-container');
                    gallery.reset();
                    cnt.html(data.GalleryHtml);
                    self.createGallery(data.GalleryStartIndex);
                }
                else if (data.GalleryStartIndex >= 0) {
                    if (data.GalleryStartIndex !== gallery.currentIndex) {
                        gallery.goTo(data.GalleryStartIndex);
                    }
                }
            }

            ctx.find('[data-partial]').each(function (i, el) {
                // Iterate all elems with [data-partial] attribute...
                var $el = $(el);
                var partial = $el.data('partial');
                
                if (partial && !(isTouchSpin && partial === 'OfferActions') && !(isDateTime && partial === 'Variants')) {
                    // ...fetch the updated html from the corresponding AJAX result object's properties
                    if (data.Partials && data.Partials.hasOwnProperty(partial)) {
                        if (partial === 'Variants' || partial === 'BundleItemVariants') {
                            $el.find('[data-toggle=tooltip], .tooltip-toggle').tooltip('hide');
                        }

                        var updatedHtml = data.Partials[partial] || "";
                        // ...and update the inner html
                        $el.html($(updatedHtml.trim()));
                    }
                }
            });

            applyCommonPlugins(ctx);

            ctx.find(".pd-tierprices").html(data.Partials["TierPrices"]);

            if (data.DynamicThumblUrl && data.DynamicThumblUrl.length > 0) {
                $(ctx).find('.pd-dyn-thumb').attr('src', data.DynamicThumblUrl);
            }

            // trigger event for plugins devs to subscribe
            $('#main-update-container').trigger("updated");
        };

        this.initialized = false;
        this.init();
        this.initialized = true;
    }

    ProductDetail.prototype = {
        gallery: null,
        activePictureIndex: 0,

        createGallery: function (startIndex) {
            var self = this;
            var opts = this.options;

            this.gallery = $('#pd-gallery').smartGallery({
                startIndex: startIndex || 0,
                zoom: {
                    enabled: opts.enableZoom
                },
                box: {
                    enabled: true,
                    hidePageScrollbars: false
                }
            });
        }
    };

    // the global, default plugin options
    _.provide('$.' + pluginName);

    $[pluginName].defaults = {
        // The 0-based image index to start the gallery with
        galleryStartIndex: 0,
        // whether to enable image zoom
        enableZoom: true,
        // url to the ajax method, which loads variant combination data
        updateUrl: null,
    };

    $.fn[pluginName] = function (options) {

        return this.each(function () {
            if (!$.data(this, 'plugin_' + pluginName)) {
                options = $.extend(true, {}, $[pluginName].defaults, options);
                $.data(this, 'plugin_' + pluginName, new ProductDetail(this, options));
            }
        });
    };

})(jQuery, window, document);