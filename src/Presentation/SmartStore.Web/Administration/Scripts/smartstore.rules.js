(function ($, window, document, undefined) {

    function enableRuleValueControl(el) {
        var rule = el.closest('.rule');
        var ruleId = rule.data('rule-id');
        var op = rule.find('.rule-operator').data('value');
        var disable = false;

        switch (op) {
            case 'IsEmpty':
            case 'IsNotEmpty':
            case 'IsNotNull':
            case 'IsNull':
                disable = true;
                break;
        }

        rule.find(':input[name="rule-value-' + ruleId + '"]').prop('disabled', disable);
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


    // Initialize.
    $('#ruleset-root').find('.rule').each(function () {
        enableRuleValueControl($(this));
    });


    // Add group.
    $(document).on('click', '.r-add-group', function (e) {
        var parentSet = $(this).closest('.ruleset');
        var parentSetId = parentSet.data('ruleset-id');
        var scope = parentSet.closest('.ruleset-root').data('scope');

        $.ajax({
            cache: false,
            url: $('#ruleset-root').data('url-addgroup'),
            data: { ruleSetId: parentSetId, scope: scope },
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
            url: $('#ruleset-root').data('url-deletegroup'),
            data: { refRuleId: refRuleId },
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
    $(document).on('click', '.ruleset-operator .dropdown-item', function () {
        var item = $(this);
        var parentSetId = item.closest('.ruleset').data('ruleset-id');
        var operator = item.closest('.ruleset-operator');
        var op = item.data('value');

        $.ajax({
            cache: false,
            url: operator.data('url'),
            data: { ruleSetId: parentSetId, op: op },
            type: "POST",
            success: function () {
                operator.find(".logical-operator").text(operator.data(op == 'And' ? 'res-all' : 'res-one'));
                operator.find(".logical-operator-chooser").removeClass("show");
            }
        });

        return false;
    });

    // Change rule operator.
    $(document).on('click', 'div.rule-operator .dropdown-item', function () {
        var item = $(this);
        var operator = item.closest('.rule-operator');
        operator.data("value", item.data("value"));
        operator.find(".btn").text(item.text());
        enableRuleValueControl(item);
    });

    // Save rule.
    $(document).on('click', '.r-save-rule', function () {
        var rule = $(this).closest('.rule');
        var ruleId = rule.data('rule-id');
        var op = rule.find(".rule-operator").data("value");

        var control = rule.find(':input[name="rule-value-' + ruleId + '"]');
        var value = control.val();
        if (Array.isArray(value)) {
            value = value.join(',');
        }

        $.ajax({
            cache: false,
            url: $('#ruleset-root').data('url-updaterule'),
            data: { ruleId: ruleId, op: op, value: value },
            type: "POST",
            success: function (result) {
                if (result.Success) {
                    location.reload();
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
            url: $('#ruleset-root').data('url-addrule'),
            data: { ruleSetId: parentSetId, scope: scope, ruleType: ruleType },
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
            url: $('#ruleset-root').data('url-deleterule'),
            data: { ruleId: ruleId },
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
            data: { ruleSetId: ruleSetId },
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

})(jQuery, window, document);