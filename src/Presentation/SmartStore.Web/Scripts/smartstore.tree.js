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
        nodeState: '',  // allow-deny
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
        var root = $(context);
        var labelHtml = '<label class="smtree-label"><span class="smtree-text"></span></label>';
        var noLeafHtml = '<div class="smtree-inner"><span class="smtree-expander-container smtree-expander"></span>' + labelHtml + '</div>';
        var leafHtml = '<div class="smtree-inner"><span class="smtree-expander-container"></span>' + labelHtml + '</div>';

        opt = $.extend({}, $.smTree.defaults, opt);
        root.data('smtree-options', opt);

        // Set node HTML.
        root.find('li').each(function () {
            var li = $(this);
            var isLeaf = !li.has('ul').length;

            li.addClass('smtree-node ' + (isLeaf ? 'smtree-leaf' : 'smtree-noleaf'))
                .prepend(isLeaf ? leafHtml : noLeafHtml)
                .find('.smtree-text').html(li.data('label'));
        });

        // Initially expand or collapse nodes.
        root.find('.smtree-noleaf').each(function () {
            expandNode($(this), opt.expanded, opt);
        });

        // Helper lines.
        if (opt.showLines) {
            root.find('ul:first').find('ul')
                .addClass('smtree-hline')
                .prepend('<span class="smtree-vline"></span>');
        }

        if (opt.nodeState === 'allow-deny') {
            // Add state checkbox HTML.
            root.find('.smtree-label').each(function (i, el) {
                var label = $(this);
                var node = label.closest('.smtree-node');
                var value = parseInt(node.data('value'));
                var name = node.data('name');
                var html = '';
                var stateClass = '';

                if (opt.nodeState === 'allow-deny') {
                    if (value === 1) {
                        stateClass = 'deny';
                    }
                    else if (value === 2) {
                        stateClass = 'allow';
                    }

                    html += '<input type="checkbox" name="' + name + '" id="' + name + '" value="' + value + '"' + (value === 2 ? ' checked="checked"' : '') + ' />';
                    html += '<input type="hidden" name="' + name + '" value="' + (value === 0 ? 0 : 1) + '" />';
                    html += '<span class="smtree-state ' + stateClass + '"></span>';
                }

                label.attr('for', name).prepend(html);
            });

            // Set indeterminate property.
            root.find('input[type=checkbox][value=0]').prop('indeterminate', true);

            // Set inherited state.
            root.find('ul:first > .smtree-node').each(function () {
                setInheritedState($(this), 0);
            });
        }

        // Expander click handler.
        root.on('click', '.smtree-expander', function () {
            var node = $(this).closest('.smtree-node');
            expandNode(node, node.hasClass('smtree-collapsed'), opt);
        });

        // State click handler.
        root.on('click', 'input[type=checkbox]', function () {
            var el = $(this);
            var node = el.closest('.smtree-node');
            var state = el.siblings('.smtree-state:first');

            if (opt.nodeState === 'allow-deny') {
                var hIn = el.next();
                var inheritedState = 0;
                state.removeClass('allow deny in-allow in-deny');

                switch (parseInt(el.val())) {
                    case 0:
                        // Indeterminate > checked.
                        el.prop({ checked: true, indeterminate: false, value: 2 });
                        hIn.val(1);
                        state.addClass('allow');
                        inheritedState = 2;
                        break;
                    case 2:
                        // Checked > unchecked.
                        el.prop({ checked: false, indeterminate: false, value: 1 });
                        hIn.val(1);
                        state.addClass('deny');
                        inheritedState = 1;
                        break;
                    case 1:
                    default:
                        // Unchecked > indeterminate.
                        el.prop({ checked: false, indeterminate: true, value: 0 });
                        hIn.val(0);
                        inheritedState = getInheritedState(node);
                        break;
                }

                // Update classes for nodes with inherited state.
                setInheritedState(node, inheritedState);
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

    function setInheritedState(node, inheritedState) {
        if (!node) return;

        var childState = inheritedState;
        var val = parseInt(node.find('> .smtree-inner input[type=checkbox]').val()) || 0;

        if (val > 0) {
            // Has direct state.
            childState = val;
        }
        else {
            // Has no direct state.
            var state = node.find('.smtree-state:first');
            state.removeClass('in-allow in-deny');
            state.addClass(inheritedState === 2 ? 'in-allow' : 'in-deny');
        }

        node.find('> ul > .smtree-node').each(function () {
            setInheritedState($(this), childState);
        });
    }

    function getInheritedState(node) {
        var result = 0;

        if (node) {
            node.parents('.smtree-node').each(function () {
                result = parseInt($(this).find('> .smtree-inner input[type=checkbox]').val()) || 0;
                if (result > 0) {
                    return false;
                }
            });
        }

        return result;
    }

})( jQuery, this, document );