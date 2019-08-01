/*
*  Project: SmartStore tree.
*  Author: Marcus Gesing, SmartStore AG.
*/
; (function ($, window, document, undefined) {
    var pluginName = 'smTree';

    function SmTree(element, options) {
        var self = this;
        this.element = element;

        options = $.extend(defaults, options);

        self.init(options);
    }

    SmTree.prototype = {

        init: function (opt) {
            var self = this;
            var noLeafHtml = '<div class="smtree-inner">'
                + '<span class="smtree-expander"></span>'
                + '<span class="smtree-label"><span class="smtree-state"></span><span class="smtree-text"></span></span>'
                + '</div>';
            var leafHtml = '<div class="smtree-inner">'
                + '<span class="smtree-label"><span class="smtree-state"></span><span class="smtree-text"></span></span>'
                + '</div>';

            // Set item HTML.
            $(self.element).find('li').each(function () {
                var li = $(this);
                var isLeaf = !li.has('ul').length;

                li.addClass('smtree-node ' + (isLeaf ? 'smtree-leaf' : 'smtree-noleaf'))
                    .prepend(isLeaf ? leafHtml : noLeafHtml)
                    .find('.smtree-text').html(li.data('label'));
            });                

            // Initially expand or reduce nodes.
            $(self.element).find('.smtree-noleaf').each(function () {
                self._expand($(this), opt.isExpanded, opt);
            });

            // Expand/reduce.
            $(self.element).on('click', '.smtree-expander', function () {
                var li = $(this).closest('.smtree-node');
                self._expand(li, li.hasClass('smtree-reduced'), opt);
            });
        },

        _expand: function (node, expand, opt) {
            node.children('ul').toggle(expand);
            node.toggleClass('smtree-expanded', expand);
            node.toggleClass('smtree-reduced', !expand);
            node.find('.smtree-inner:first .smtree-expander').html('<i class="' + (expand ? opt.expandedClass : opt.reducedClass) + '"></i>');
        }
    };

    // The global, default plugin options.
    var defaults = {
        isExpanded: false,
        expandedClass: 'fas fa-chevron-down',
        reducedClass: 'fas fa-chevron-right',
    };

    $[pluginName] = { defaults: defaults };

    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, pluginName)) {
                options = $.extend({}, $[pluginName].defaults, options);
                $.data(this, pluginName, new SmTree(this, options));
            }
        });
    };

})( jQuery, this, document );