(function ($, window, document, undefined) {

    var _initialized = false;

    function refreshBubblePosition(e, wrapper, el, bubble, track) {
        wrapper = wrapper || $(this);
        el = el || wrapper.find('> .form-control-range[data-target]');
        bubble = bubble || wrapper.find('> .range-value');
        ready = wrapper.hasClass('ready');

        if (bubble.length === 0 || el.length === 0) {
            wrapper.addClass('ready');
            return;
        }

        if (!ready && !wrapper.is(':visible')) {
            return;
        }

        if (!ready) {
            // calc Y position only once (when visible but not ready yet)
            var labelTop = Math.floor((el.position().top + (el.height() / 2)) - (bubble.height() / 2));
            bubble.css('top', labelTop + 'px');
            wrapper.addClass('ready');
        }

        // Always refresh X position
        var min = parseFloat(el.prop('min')),
            max = parseFloat(el.prop('max')),
            range = max - min,
            ratio = wrapper.width() / range,
            n = parseFloat(el.val()) - min;

        bubble.css('left', (n * ratio) + 'px').attr('data-placement', n > range / 2 ? 'left' : 'right');
    }

    function updateSlider(e) {
        // move invariant value from slider to an associated hidden field
        // as formatted value. Client validation will fail otherwise.
        var el = $(this),
            val = el.val(),
            wrapper = el.parent(),
            bubble = wrapper.find('> .range-value');

        wrapper
            .css('--slider-value', val)
            .find('.range-value-inner')
            .text(wrapper.data('format').format(val));

        if (_initialized) {
            var g = SmartStore.globalization,
                nf = g.culture.numberFormat,
                formatted = val.replace('.', nf["."]);
            $(el.data('target')).val(formatted).trigger('change');
        }

        refreshBubblePosition(e, wrapper, el, bubble);
    }

    // on document ready
	$(function () {
        var selector = '.range-slider > .form-control-range[data-target]';

        // initial
        $(selector).each(updateSlider);

        // on input
        $(document).on('input', selector, updateSlider);

        // align bubble
        $(document).on('mouseenter', '.range-slider', refreshBubblePosition);

        _initialized = true;
    });

})( jQuery, this, document );

