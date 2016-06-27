;(function ( $, window, document, undefined ) {
    
    var pluginName = 'productDetail';
    var galPluginName = "plugin_smartGallery";
    
    function ProductDetail( element, options ) {
    	
		var self = this;

		this.element = element;
		var el = this.el = $(element);

		var meta = $.metadata ? $.metadata.get(element) : {};
		var opts = this.options = $.extend(true, {}, options, meta || {});
		
		this.init = function() {
			
			var opts = this.options;
			
		    this.createGallery(opts.galleryStartIndex);
			
		    $('#pd-gallery-container-outer').throbber({ white: false, small: false, show: false });

		    $('#pd-manufacturer img').css({ 'max-width': opts.galleryWidth });

			// update product data and gallery
		    $(el).find(':input').change(function () {
		    	var inputType = $(this).attr('type');
		    	if (inputType && (inputType === 'file' || inputType === 'submit'))
		    		return this;

		    	var context = $(this).closest('.update-container');

		    	if (context[0]) {
		    	}
		    	else {
		    		context = el;
		    	}

		    	var url = context.attr('data-url');
		    	if (!url) {
		    		return this;
		    	}

		    	$({}).doAjax({
		    		url: url,
		    		data: context.find(':input').serialize(),
		    		callbackSuccess: function (response) {
		    			self.updateDetailData(response, context);

		    			if (context.hasClass('bundle-item')) {
		    				// update bundle price too
		    				$('#TotalPriceUpdateContainer').doAjax({
		    					data: $('#ProductBundleItems').find(':input').serialize(),
		    					callbackSuccess: function (response2) {
		    						self.updateDetailData(response2, $('#AddToCart, #ProductBundleOverview'));
		    					}
		    				});
		    			}

		    		}
		    	});
		    });
			
			return this;
		};

		this.updateDetailData = function (data, context) {
			var gallery = $('#pd-gallery').data(galPluginName),
				referTo = $(context).attr('data-referto');

		    if (data.GalleryHtml) {
		        var cnt = $('#pd-gallery-container');
		        cnt.stop(true, true).transition({ opacity: 0 }, 300, "ease-out", function () {
		        	gallery.reset();
		            cnt.html(data.GalleryHtml);
		            self.createGallery(data.GalleryStartIndex);

		            _.delay(function () {
		                cnt.stop(true, true).transition({ opacity: 1 }, 175, "ease-in");
		            }, 200);
		            
		        });
		    }
		    else if (data.GalleryStartIndex >= 0) {
		        if (data.GalleryStartIndex != gallery.currentIndex) {
		            gallery.showImage(data.GalleryStartIndex);
		        }
		    }

			//update detail data in view
		    if (referTo)
		    	context = $(referTo);

		    //prices
		    var priceBlock = $(context).find('.price-block').addBack();
		    priceBlock.find(".base-price").html(data.Price.Base.Info);
		    priceBlock.find(".old-product-price").html(data.Price.Old.Text);
		    priceBlock.find('.product-price-without-discount').html(data.Price.WithoutDiscount.Text);
		    priceBlock.find('.product-price-with-discount').html(data.Price.WithDiscount.Text);

		    //delivery time
		    var deliveryTime = priceBlock.find(".delivery-time");

		    if (data.Delivery.DisplayAccordingToStock) {
		    	deliveryTime.toggle(true);
		    	deliveryTime.find(".delivery-time-value").html(data.Delivery.Name);
		    	deliveryTime.find(".delivery-time-color")
					.css("background-color", data.Delivery.Color)
					.attr("title", data.Delivery.Name)
					.toggle(data.Stock.Availability.Available);
		    }
		    else {
		    	deliveryTime.find(".delivery-time-value").html(data.Stock.Availability.Text);
		    	deliveryTime.find(".delivery-time-color").toggle(false);
		    	deliveryTime.toggle(data.Stock.Availability.Text.length > 0);
		    }
		     
		    //attributes
		    var attributesBlock = $(context).find('.attributes').addBack();
		    
		    function updateAttrLine(className, newValue) {
		        attrLine = attributesBlock.find(className);
		        if (newValue) {
		            attrLine.find(".value").html(newValue);
		            attrLine.removeClass("hide");
		            attrLine.addClass("in");
		        }
		        else {
		            attrLine.find(".value").html();
		            attrLine.addClass("hide");
		            attrLine.removeClass("in");
		        }
		    }

		    updateAttrLine(".attr-sku", data.Number.Sku.Value);
		    updateAttrLine(".attr-gtin", data.Number.Gtin.Value);
		    updateAttrLine(".attr-mpn", data.Number.Mpn.Value);
		    updateAttrLine(".attr-weight", data.Measure.Weight.Text);
		    updateAttrLine(".attr-length", data.Measure.Length.Text);
		    updateAttrLine(".attr-width", data.Measure.Width.Text);
		    updateAttrLine(".attr-height", data.Measure.Height.Text);

		    if (data.Stock.Quantity.Show)
		    {
		        updateAttrLine(".attr-stock", data.Stock.Availability.Text);
		    }
		    else
		    {
		        updateAttrLine(".attr-stock", "");
		    }

		    context.find('.add-to-cart .form-inline').toggle(data.Stock.Availability.Available);

		    if (data.DynamicThumblUrl && data.DynamicThumblUrl.length > 0) {
		    	$(context).find('.dynamic-image img').attr('src', data.DynamicThumblUrl);
		    }
		};
		
		this.initialized = false;
		this.init();
		this.initialized = true;
	}
	
	ProductDetail.prototype = {
		gallery: null,
		activePictureIndex: 0,
		
		createGallery: function (startIndex) {
			var self  = this;
			var opts = this.options;

			gallery = $('#pd-gallery').smartGallery({
			    height: opts.galleryHeight,
				enableDescription: opts.showImageDescription,
				startIndex: startIndex || 0,
				zoom: {
				    enabled: opts.enableZoom,
                    zoomType: opts.zoomType
				},
				box: {
					enabled: true,
					hidePageScrollbars: false
				}
			});
		}
		
    }

    // the global, default plugin options
	_.provide('$.' + pluginName);
	$[pluginName].defaults = {
	    // Width of the gallery, set to false and it will read the CSS width
	    galleryWidth: null,
	    // Height of the gallery, set to false and it will read the CSS height
	    galleryHeight: null,
        // The max size of the gallery zhumbs
	    galleryThumbSize: 50,
        // The 0-based image index to start the gallery with
	    galleryStartIndex: 0,
        // whether to show the image description
	    showImageDescription: false,
        // whether to enable image zoom
	    enableZoom: true,
        // type of zoom (window | inner | lens)
	    zoomType: "window",
        // url to the ajax method, which loads variant combination data
        updateUrl: null,
	}
	
	$.fn[pluginName] = function( options ) {
		
		return this.each(function() {
		    if (!$.data(this, 'plugin_' + pluginName)) {
		        options = $.extend( true, {}, $[pluginName].defaults, options );
		        $.data(this, 'plugin_' + pluginName, new ProductDetail( this, options ));
		    }
		});
	}
    
})(jQuery, window, document);