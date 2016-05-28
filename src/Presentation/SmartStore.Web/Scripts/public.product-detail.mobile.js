;(function ( $, window, document, undefined ) {
    
    var pluginName = 'productDetailMobile';
    
    function productDetailMobile(element, options) {
    	
		var self = this;

		this.element = element;
		var el = this.el = $(element);

		var meta = $.metadata ? $.metadata.get(element) : {};
		var opts = this.options = $.extend(true, {}, options, meta || {});
		
		this.init = function() {
			
			var opts = this.options;
			
			// update product data and gallery
		    $(el).find(':input').change(function () {
		    	var context = $(this).closest('.update-container');

		    	if (context[0]) {		// associated or bundled item
		    	}
		    	else {
		    		context = el;
		    	}

		    	context.doAjax({
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
			var referTo = $(context).attr('data-referto');

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
		        if (newValue && newValue != "") {
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
		    updateAttrLine(".attr-stock", data.Stock.Availability.Text);

		    UpdateMainImage(imageUrls[data.GalleryStartIndex]);

		    context.find('.add-to-cart .form-inline').toggle(data.Stock.Availability.Available);

		};
		
		this.initialized = false;
		this.init();
		this.initialized = true;
	}
	
    productDetailMobile.prototype = {
		activePictureIndex: 0
    }

    // the global, default plugin options
	_.provide('$.' + pluginName);
	$[pluginName].defaults = {
        // whether to show the image description
	    showImageDescription: false,
        // url to the ajax method, which loads variant combination data
        updateUrl: null,
	}
	
	$.fn[pluginName] = function( options ) {
		
		return this.each(function() {
		    if (!$.data(this, 'plugin_' + pluginName)) {
		        options = $.extend( true, {}, $[pluginName].defaults, options );
		        $.data(this, 'plugin_' + pluginName, new productDetailMobile(this, options));
		    }
		});
	}
    
})(jQuery, window, document);