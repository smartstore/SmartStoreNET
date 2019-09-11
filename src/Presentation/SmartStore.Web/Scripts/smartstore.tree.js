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

    $.fn.tree = function (method) {
        return main.apply(this, arguments);
    };

    $.tree = function () {
        return main.apply($('.tree:first'), arguments);
    };

    $.tree.defaults = {
        expanded: false,    // Whether initially expand tree.
        showLines: false,   // Whether to show helper lines.
        readOnly: false,    // Whether state changed are enabled.
        nodeState: '',      // on-off
        expandedClass: 'fas fa-angle-down',
        collapsedClass: 'fas fa-angle-right',
        leafClass: 'tree-leaf left-align',
        stateTitles: ['', '', '']
    };


    function main(method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }

        if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }

        EventBroker.publish("message", { title: 'Method "' + method + '" does not exist on jQuery.tree', type: "error" });
        return null;
    }

    function initialize(context, opt) {
        var root = $(context);

        opt = $.extend({}, $.tree.defaults, opt);
        root.data('tree-options', opt);

        var labelHtml = '<label class="tree-label' + (opt.readOnly ? '' : ' tree-control') + '"><span class="tree-text"></span></label>';
        var noLeafHtml = '<div class="tree-inner"><span class="tree-expander-container tree-expander"></span>' + labelHtml + '</div>';
        var leafHtml = '<div class="tree-inner"><span class="tree-expander-container"></span>' + labelHtml + '</div>';

        // Set node HTML.
        root.find('li').each(function () {
            var li = $(this);
            var isLeaf = !li.has('ul').length;

            li.addClass('tree-node ' + (isLeaf ? opt.leafClass : 'tree-noleaf'))
                .prepend(isLeaf ? leafHtml : noLeafHtml)
                .find('.tree-text').html(li.data('label'));
        });

        // Initially expand or collapse nodes.
        root.find('.tree-noleaf').each(function () {
            expandNode($(this), opt.expanded, opt);
        });

        // Helper lines.
        if (opt.showLines) {
            root.find('ul:first').find('ul')
                .addClass('tree-hline')
                .prepend('<span class="tree-vline"></span>');
        }

        if (opt.nodeState === 'on-off') {
            // Add state checkbox HTML.
            root.find('.tree-label').each(function (i, el) {
                var label = $(this);
                var node = label.closest('.tree-node');
                var value = parseInt(node.data('value')) || 0;
                var name = node.data('name');
                var html = '';
                var stateClass = '';

                if (value === 1) {
                    stateClass = 'off';
                }
                else if (value === 2) {
                    stateClass = 'on';
                }

                if (!opt.readOnly) {
                    label.attr('for', name);

                    html += '<input type="checkbox" name="' + name + '" id="' + name + '" value="' + value + '"' + (value === 2 ? ' checked="checked"' : '') + ' />';
                    html += '<input type="hidden" name="' + name + '" value="' + (value === 0 ? 0 : 1) + '" />';
                }
                html += '<span class="tree-state ' + stateClass + '" title="' + opt.stateTitles[value] + '"></span>';

                label.prepend(html);
            });

            if (!opt.readOnly) {
                // Set indeterminate property.
                root.find('input[type=checkbox][value=0]').prop('indeterminate', true);

                // Set inherited state.
                root.find('ul:first > .tree-node').each(function () {
                    setInheritedState($(this), 0);
                });
            }
        }

        // Expander click handler.
        root.on('click', '.tree-expander', function () {
            var node = $(this).closest('.tree-node');
            expandNode(node, node.hasClass('tree-collapsed'), opt);
        });

        // State click handler.
        root.on('click', 'input[type=checkbox]', function () {
            var el = $(this);
            var node = el.closest('.tree-node');
            var state = el.siblings('.tree-state:first');

            if (opt.nodeState === 'on-off') {
                var hIn = el.next();
                var inheritedState = 0;
                state.removeClass('on off in-on in-off');

                switch (parseInt(el.val())) {
                    case 0:
                        // Indeterminate > checked.
                        el.prop({ checked: true, indeterminate: false, value: 2 });
                        hIn.val(1);
                        state.addClass('on').attr('title', opt.stateTitles[2]);
                        inheritedState = 2;
                        break;
                    case 2:
                        // Checked > unchecked.
                        el.prop({ checked: false, indeterminate: false, value: 1 });
                        hIn.val(1);
                        state.addClass('off').attr('title', opt.stateTitles[1]);
                        inheritedState = 1;
                        break;
                    case 1:
                    default:
                        // Unchecked > indeterminate.
                        el.prop({ checked: false, indeterminate: true, value: 0 });
                        hIn.val(0);
                        state.attr('title', opt.stateTitles[0]);
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
        var opt = self.data('tree-options') || $.tree.defaults;
        var expand = !(opt.expanded || false);

        self.find('.tree-noleaf').each(function () {
            expandNode($(this), expand, opt);
        });

        opt.expanded = expand;
    }

    function expandNode(node, expand, opt) {
        if (expand) {
            node.children('ul').show();
            node.removeClass('tree-collapsed').addClass('tree-expanded');
        }
        else {
            node.children('ul').hide();
            node.removeClass('tree-expanded').addClass('tree-collapsed');
        }
        node.find('.tree-inner:first .tree-expander').html('<i class="' + (expand ? opt.expandedClass : opt.collapsedClass) + '"></i>');
    }

    function setInheritedState(node, inheritedState) {
        if (!node) return;

        var childState = inheritedState;
        var val = parseInt(node.find('> .tree-inner input[type=checkbox]').val()) || 0;

        if (val > 0) {
            // Is directly on.
            childState = val;
        }
        else {
            // Is not directly on.
            var state = node.find('.tree-state:first');
            state.removeClass('in-on in-off');
            state.addClass(inheritedState === 2 ? 'in-on' : 'in-off');
        }

        node.find('> ul > .tree-node').each(function () {
            setInheritedState($(this), childState);
        });
    }

    function getInheritedState(node) {
        var result = 0;

        if (node) {
            node.parents('.tree-node').each(function () {
                result = parseInt($(this).find('> .tree-inner input[type=checkbox]').val()) || 0;
                if (result > 0) {
                    return false;
                }
            });
        }

        return result;
    }

})( jQuery, this, document );