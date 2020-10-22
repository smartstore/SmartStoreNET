(function ($, window, document, undefined) {

    function RangeSlider(element, options) {
        var self = this;

        this.element = element;
        var el = this.el = $(element);
        var slider = this.slider = el.find('> .form-control-range[data-target]');
        var opts = this.options = $.extend({}, options);

        function refreshBubblePosition(e, bubble, track) {
            bubble = bubble || el.find('> .range-value');
            ready = el.hasClass('ready');

            if (bubble.length === 0 || el.length === 0) {
                el.addClass('ready');
                return;
            }

            if (!ready && !el.is(':visible')) {
                return;
            }

            if (!ready) {
                // calc Y position only once (when visible but not ready yet)
                var labelTop = Math.floor((slider.position().top + (slider.height() / 2)) - (bubble.height() / 2));
                bubble.css('top', labelTop + 'px');
                el.addClass('ready');
            }

            // Always refresh X position
            var min = parseFloat(slider.prop('min')),
                max = parseFloat(slider.prop('max')),
                range = max - min,
                ratio = el.width() / range,
                n = parseFloat(slider.val()) - min;

            if (SmartStore.globalization.culture.isRTL) {
                bubble.css('right', (n * ratio) + 'px').attr('data-placement', n > range / 2 ? 'right' : 'left');
            }
            else {
                bubble.css('left', (n * ratio) + 'px').attr('data-placement', n > range / 2 ? 'left' : 'right');
            }

        }

        function updateSlider(e) {
            // Move invariant value from slider to an associated hidden field
            // as formatted value. Client validation will fail otherwise.
            var val = slider.val(),
                bubble = el.find('> .range-value'),
                fmt = el.data('format');

            el
                .css('--slider-value', val)
                .find('.range-value-inner')
                .text(fmt === '{0}' ? fmt.format(val) : eval(fmt.format(val)));

            if (self.initialized) {
                var g = SmartStore.globalization,
                    nf = g.culture.numberFormat,
                    formatted = val.replace('.', nf["."]);
                $(slider.data('target')).val(formatted).trigger('change');
            }

            refreshBubblePosition(e, bubble);
        }

        this.init = function () {
            if (slider.length === 0)
                return;

            // on input
            slider.on('input', updateSlider);

            // align bubble
            el.on('mouseenter', refreshBubblePosition);

            updateSlider();
        };

        this.initialized = false;
        this.init();
        this.initialized = true;
    }

    $.fn.rangeSlider = function (options) {
        return this.each(function () {
            if (!$.data(this, 'rangeSlider')) {
                $.data(this, 'rangeSlider', new RangeSlider(this, options));
            }
        });
    };

})(jQuery, this, document);

