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
            var labelHtml = '<label class="smtree-label"><span class="smtree-text"></span></label>';
            var noLeafHtml = '<div class="smtree-inner"><span class="smtree-expander-container smtree-expander"></span>' + labelHtml + '</div>';
            var leafHtml = '<div class="smtree-inner"><span class="smtree-expander-container"></span>' + labelHtml + '</div>';

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
                self._expand($(this), opt.expanded, opt);
            });

            // Helper lines.
            if (opt.showLines) {
                $(self.element).find('ul:first').find('ul')
                    .addClass('smtree-hline')
                    .prepend('<span class="smtree-vline"></span>');
            }

            // Node state checkbox.
            if (opt.nodeState) {
                // Add state HTML.
                $(self.element).find('.smtree-label').each(function (i, el) {
                    var label = $(this);
                    var li = label.closest('.smtree-node');
                    var value = parseInt(li.data('value'));
                    var name = li.data('name');
                    var html = '';

                    if (opt.nodeState === 'tri') {
                        html += '<input type="checkbox" name="' + name + '" id="' + name + '" value="' + value + '"' + (value === 2 ? ' checked="checked"' : '') + ' />';
                        html += '<input type="hidden" name="' + name + '" value="' + (value == 0 ? 0 : 1) + '" />';
                    }

                    label.attr('for', name).prepend(html + '<span class="smtree-state"></span>');
                });

                // Set indeterminate property.
                if (opt.nodeState === 'tri') {
                    $(self.element).find('input[type=checkbox][value=0]').prop('indeterminate', true);
                }
            }

            // Expander click handler.
            $(self.element).on('click', '.smtree-expander', function () {
                var li = $(this).closest('.smtree-node');
                self._expand(li, li.hasClass('smtree-reduced'), opt);
            });

            // State click handler.
            $(self.element).on('click', 'input[type=checkbox]', function () {
                var el = $(this);

                if (opt.nodeState === 'tri') {
                    var hIn = el.next();
                    switch (parseInt(el.val())) {
                        case 0:
                            // Indeterminate > checked.
                            el.prop({ checked: true, indeterminate: false, value: 2 });
                            hIn.val(1);
                            break;
                        case 2:
                            // Checked > unchecked.
                            el.prop({ checked: false, indeterminate: false, value: 1 });
                            hIn.val(1);
                            break;
                        case 1:
                        default:
                            // Unchecked > indeterminate.
                            el.prop({ checked: false, indeterminate: true, value: 0 });
                            hIn.val(0);
                            break;
                    }
                }
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
        expanded: false,
        showLines: false,
        nodeState: null,  // 'tri'
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