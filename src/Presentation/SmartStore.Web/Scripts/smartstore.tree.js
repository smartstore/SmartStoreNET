/*
*  Project: SmartStore tree.
*  Author: Marcus Gesing, SmartStore AG.
*/
; (function ($, window, document, undefined) {

    var methods = {
        init: function (options) {
            return this.each(function () {
                initialize(this, options);
            });
        },

        expandAll: function () {
            return this.each(function () {
                expandAll(this);
            });
        },
    };

    $.fn.smTree = function (method) {
        return main.apply(this, arguments);
    };

    $.smTree = function () {
        return main.apply($('.smtree:first'), arguments);
    };

    $.smTree.defaults = {
        expanded: false,
        showLines: false,
        nodeState: null,  // allow-deny
        expandedClass: 'fas fa-angle-down',
        collapsedClass: 'fas fa-angle-right',
    };


    function main(method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }

        if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }

        EventBroker.publish("message", { title: 'Method "' + method + '" does not exist on jQuery.smTree', type: "error" });
        return null;
    }

    function initialize(context, opt) {
        var self = $(context);
        var labelHtml = '<label class="smtree-label"><span class="smtree-text"></span></label>';
        var noLeafHtml = '<div class="smtree-inner"><span class="smtree-expander-container smtree-expander"></span>' + labelHtml + '</div>';
        var leafHtml = '<div class="smtree-inner"><span class="smtree-expander-container"></span>' + labelHtml + '</div>';

        opt = $.extend({}, $.smTree.defaults, opt);
        self.data('smtree-options', opt);

        // Set node HTML.
        self.find('li').each(function () {
            var li = $(this);
            var isLeaf = !li.has('ul').length;

            li.addClass('smtree-node ' + (isLeaf ? 'smtree-leaf' : 'smtree-noleaf'))
                .prepend(isLeaf ? leafHtml : noLeafHtml)
                .find('.smtree-text').html(li.data('label'));
        });

        // Initially expand or collapse nodes.
        self.find('.smtree-noleaf').each(function () {
            expandNode($(this), opt.expanded, opt);
        });

        // Helper lines.
        if (opt.showLines) {
            self.find('ul:first').find('ul')
                .addClass('smtree-hline')
                .prepend('<span class="smtree-vline"></span>');
        }

        // Node state checkbox.
        if (opt.nodeState) {
            // Add state HTML.
            self.find('.smtree-label').each(function (i, el) {
                var label = $(this);
                var li = label.closest('.smtree-node');
                var value = parseInt(li.data('value'));
                var name = li.data('name');
                var html = '';

                if (opt.nodeState === 'allow-deny') {
                    if (value === 0) {
                        // ' state-indeterminate';
                    }
                    else if (value === 1) {
                        li.addClass('state-deny');
                    }
                    else if (value === 2) {
                        li.addClass('state-allow');
                    }

                    html += '<input type="checkbox" name="' + name + '" id="' + name + '" value="' + value + '"' + (value === 2 ? ' checked="checked"' : '') + ' />';
                    html += '<input type="hidden" name="' + name + '" value="' + (value === 0 ? 0 : 1) + '" />';
                    html += '<span class="smtree-state"></span>';
                }

                label.attr('for', name).prepend(html);
            });

            // Set indeterminate property.
            if (opt.nodeState === 'allow-deny') {
                self.find('input[type=checkbox][value=0]').prop('indeterminate', true);
            }
        }

        // Expander click handler.
        self.on('click', '.smtree-expander', function () {
            var li = $(this).closest('.smtree-node');
            expandNode(li, li.hasClass('smtree-collapsed'), opt);
        });

        // State click handler.
        self.on('click', 'input[type=checkbox]', function () {
            var el = $(this);

            if (opt.nodeState === 'allow-deny') {
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
    }

    function expandAll(context) {
        var self = $(context);
        var opt = self.data('smtree-options') || $.smTree.defaults;
        var expand = !(opt.expanded || false);

        self.find('.smtree-noleaf').each(function () {
            expandNode($(this), expand, opt);
        });

        opt.expanded = expand;
    }

    function expandNode(node, expand, opt) {
        if (expand) {
            node.children('ul').show();
            node.removeClass('smtree-collapsed').addClass('smtree-expanded');
        }
        else {
            node.children('ul').hide();
            node.removeClass('smtree-expanded').addClass('smtree-collapsed');
        }
        node.find('.smtree-inner:first .smtree-expander').html('<i class="' + (expand ? opt.expandedClass : opt.collapsedClass) + '"></i>');
    }

})( jQuery, this, document );