(function ($, window, document, undefined) {

    var root = $('#ruleset-root');
    var token = "";

    function enableRuleValueControl(el) {
        var rule = el.closest('.rule');
        var ruleId = rule.data('rule-id');
        var op = rule.find('.rule-operator').data('value');
        var inputElements = rule.find(':input[name="rule-value-' + ruleId + '"], :input[name^="rule-value-' + ruleId + '-"]');

        switch (op) {
            case 'IsEmpty':
            case 'IsNotEmpty':
            case 'IsNotNull':
            case 'IsNull':
                inputElements.prop('disabled', true);
                break;
            default:
                inputElements.prop('disabled', false);
                break;
        }
    }

    function appendToRuleSetBody(ruleSet, html) {
        var target = ruleSet.find('.ruleset-body').first();
        if (target.length === 0) {
            target = $('<div class="ruleset-body"></div>').appendTo(ruleSet);
        }
        target.append(html);
        enableRuleValueControl(target.find('.rule:last'));
        $('#excute-result').addClass('hide');
    }

    function getRuleData() {
        var data = [];

        root.find('.rule').each(function () {
            var rule = $(this);
            var ruleId = rule.data('rule-id');
            var op = rule.find('.rule-operator').data('value');
            var multipleNamePrefix = 'rule-value-' + ruleId + '-';
            var multipleInputElements = rule.find(':input[name^="' + multipleNamePrefix + '"]');
            var value = '';

            if (multipleInputElements.length > 0) {
                var valueObj = {};
                multipleInputElements.each(function () {
                    var el = $(this);
                    var val = el.val();
                    var name = el.attr('name') || '';

                    if (!_.isEmpty(name)) {
                        valueObj[name.replace(multipleNamePrefix, '')] = Array.isArray(val) ? val.join(',') : val;
                    }
                });

                value = JSON.stringify(valueObj);
            }
            else {
                var val = rule.find(':input[name="rule-value-' + ruleId + '"]').val();
                value = Array.isArray(val) ? val.join(',') : val;
            }

            data.push({ ruleId: ruleId, op: op, value: value });
        });

        return data;
    }

    //function showRuleError(ruleId, error) {
    //    var rule = root.find('[data-rule-id=' + ruleId + ']');
    //    var errorContainer = rule.find('.r-rule-error');
    //    var hasError = !_.isEmpty(error);

    //    errorContainer.toggleClass('hide', !hasError);
    //    errorContainer.find('.field-validation-error').text(error || '');

    //    rule.find('.btn-rule-operator')
    //        .toggleClass('btn-info', !hasError)
    //        .toggleClass('btn-danger', hasError);
    //}


    // Initialize.
    root.find('.rule').each(function () {
        var rule = $(this);
        enableRuleValueControl(rule);

        if (rule.data('has-error')) {
            rule.find(':input[name^="rule-value-"]').addClass('input-validation-error');
        }
    });

    $(function () {
        token = $('input[name="__RequestVerificationToken"]').val();
    });

    // Save rule set.
    $(document).on('click', 'button[name="save"]', function (e) {
        var strData = root.data('dirty')
            ? JSON.stringify(getRuleData())
            : '';

        $('#RawRuleData').val(strData);
        return true;
    });


    // Add group.
    $(document).on('click', '.r-add-group', function (e) {
        var parentSet = $(this).closest('.ruleset');
        var parentSetId = parentSet.data('ruleset-id');
        var scope = parentSet.closest('.ruleset-root').data('scope');

        $.ajax({
            cache: false,
            url: root.data('url-addgroup'),
            data: { ruleSetId: parentSetId, scope: scope, __RequestVerificationToken: token },
            type: "POST",
            success: function (html) {
                appendToRuleSetBody(parentSet, html);
                parentSet.find('.ruleset:last select.r-add-rule').selectWrapper();
            }
        });

        return false;
    });

    // Delete group.
    $(document).on('click', '.r-delete-group', function () {
        var parentSet = $(this).closest('.ruleset');
        var refRuleId = parentSet.data('refrule-id');

        $.ajax({
            cache: false,
            url: root.data('url-deletegroup'),
            data: { refRuleId: refRuleId, __RequestVerificationToken: token },
            type: "POST",
            success: function (result) {
                if (result.Success) {
                    parentSet.remove();
                    $('#excute-result').addClass('hide');
                }
            }
        });

        return false;
    });

    // Change rule set operator.
    $(document).on('click', '.ruleset-operator .dropdown-item:not(.disabled)', function (e) {
        e.stopPropagation();
        e.preventDefault();

        var item = $(this);
        var parentSetId = item.closest('.ruleset').data('ruleset-id');
        var operator = item.closest('.ruleset-operator');
        var op = item.data('value');

        $.ajax({
            cache: false,
            url: operator.data('url'),
            data: { ruleSetId: parentSetId, op: op, __RequestVerificationToken: token },
            type: 'POST',
            success: function (result) {
                if (result.Success) {
                    operator.find('input[name=LogicalOperator]').val(op);
                    operator.find('.logical-operator-chooser').removeClass('show');
                    operator.find('.ruleset-op-one').toggleClass('hide', op == 'And').toggleClass('d-flex', op != 'And');
                    operator.find('.ruleset-op-all').toggleClass('hide', op != 'And').toggleClass('d-flex', op == 'And');
                }
            }
        });

        return false;
    });

    // Change rule operator.
    $(document).on('click', 'div.rule-operator .dropdown-item', function () {
        var item = $(this);
        var operator = item.closest('.rule-operator');
        operator.data("value", item.data("value"));
        operator.find(".btn")
            .html('<span class="text-truncate">' + item.text() + '</span>')
            .attr('title', item.text());
        enableRuleValueControl(item);
        onRuleValueChanged();
    });

    // Change state of save rules button.
    $(document).on('change', ':input[name^="rule-value-"]', function () {
        onRuleValueChanged();
    });

    // Save rules.
    $(document).on('click', 'button.ruleset-save', function () {
        var data = getRuleData();
        
        $.ajax({
            cache: false,
            url: root.data('url-updaterules'),
            data: {
                __RequestVerificationToken: token,
                ruleData: data
            },
            type: 'POST',
            success: function (result) {
                if (result.Success) {
                    location.reload();
                }
                else if (!_.isEmpty(result.Message)) {
                    displayNotification(result.Message, 'error');
                }
            }
        });

        return false;
    });

    // Add rule.
    $(document).on('change', '.r-add-rule', function () {
        var select = $(this);
        var ruleType = select.val();
        if (!ruleType)
            return;

        var parentSet = select.closest('.ruleset');
        var parentSetId = parentSet.data('ruleset-id');
        var scope = parentSet.closest('.ruleset-root').data('scope');

        $.ajax({
            cache: false,
            url: root.data('url-addrule'),
            data: { ruleSetId: parentSetId, scope: scope, ruleType: ruleType, __RequestVerificationToken: token },
            type: "POST",
            success: function (html) {
                appendToRuleSetBody(parentSet, html);
                select.val('').trigger('change');
            }
        });

        return false;
    });

    // Delete rule.
    $(document).on('click', '.r-delete-rule', function () {
        var rule = $(this).closest('.rule');
        var ruleId = rule.data('rule-id');

        $.ajax({
            cache: false,
            url: root.data('url-deleterule'),
            data: { ruleId: ruleId, __RequestVerificationToken: token },
            type: "POST",
            success: function (result) {
                if (result.Success) {
                    rule.remove();
                    $('#excute-result').addClass('hide');
                }
            }
        });

        return false;
    });

    // Execute rule.
    $(document).on('click', '#execute-rules', function () {
        var ruleSet = $(".ruleset-root > .ruleset")
        var ruleSetId = ruleSet.data('ruleset-id');

        $.ajax({
            cache: false,
            url: $(this).attr('href'),
            data: { ruleSetId: ruleSetId, __RequestVerificationToken: token },
            type: "POST",
            success: function (result) {
                $('#excute-result')
                    .html(result.Message)
                    .removeClass('hide alert-warning alert-danger')
                    .addClass(result.Success ? 'alert-warning' : 'alert-danger');
            }
        });

        return false;
    });

    // Ruleset hover
    var hoveredRuleset;
    root.on('mousemove', function (e) {
        var ruleset = $(e.target).closest('.ruleset');
        if (ruleset && ruleset.get(0) !== hoveredRuleset) {
            root.find('.ruleset').removeClass('hover');
            ruleset.addClass('hover');
            hoveredRuleset = ruleset.get(0);
        }
    });
    root.on('mouseleave', function (e) {
        root.find('.ruleset').removeClass('hover');
        hoveredRuleset = null;
    });

    //$(document).on('mouseenter', '.ruleset', function () {
    //    root.find('.ruleset').removeClass('hover');
    //    $(this).addClass('hover');
    //});
    //$(document).on('mouseleave', '.ruleset', function (e) {
    //    $(this).removeClass('hover');
    //    var target = $(e.target).closest('.ruleset');
    //    if (target.length) {
    //        target.addClass('.hover');
    //    }
    //    console.log(e.currentTarget, e.relatedTarget);
    //});

})(jQuery, window, document);