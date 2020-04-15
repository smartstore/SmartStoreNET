// Creates dashboard charts with Chart.js
;
(function () {
    var root = $('html');
    var colorPrimary = root.css('--primary');
    var colorSuccess = root.css('--success');
    var colorWarning = root.css('--warning');
    var colorDanger = root.css('--danger');
    var fontFamily = root.css('--font-family-sans-serif');

    createIncompleteOrdersCharts = function (textNoIncomplete, textNotShipped, textNotPayed, textNewOrders) {
        var dataSets = $('#incomplete-orders-report').data('chart');

        for (var i = 0; i < dataSets.Reports.length; i++) {
            if (dataSets.Reports[i].Data[0].Quantity == 0 && dataSets.Reports[i].Data[1].Quantity == 0) {
                dataSets.Reports[i].Data[2].Quantity = 1
            }
        }

        // Chart config
        var orders_config = {
            responsive: true,
            responsiveAnimationDuration: 0,
            maintainAspectRatio: false,
            cutoutPercentage: 91,
            rotation: -0.5 * Math.PI,
            circumfernce: 2 * Math.PI,
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 1000,
                easing: 'easeInOutSine',
            },
            layout: {
                padding: {
                    left: 0,
                    right: 0,
                    top: 10,
                    bottom: 10
                }
            },
            hover: {
                animationDuration: 250,
            },
            legend: false,
            tooltips: {
                enabled: true,
                mode: 'nearest',
                intersect: true,
                titleFontFamily: fontFamily,
                bodyFontFamily: fontFamily,
                xPadding: 10,
                yPadding: 8,
                caretPadding: 6,
                caretSize: 8,
                cornerRadius: 4,
                titleMarginBottom: 8,
                bodySpacing: 5,
                callbacks: {
                    label: function (item, data) {
                        if (item.index == 2 && data.datasets[0].data[0] == 0 && data.datasets[0].data[1] == 0) {
                            return " " + textNoIncomplete;
                        }
                        return " " + data.labels[item.index] + ": " + dataSets.Reports[this._chart.id + alreadyExistingCharts].Data[item.index].QuantityFormatted
                            + ", " + dataSets.Reports[this._chart.id + alreadyExistingCharts].Data[item.index].AmountFormatted;
                    },
                }
            },
        };

        var dataDay = {
            labels: [textNotShipped, textNotPayed, textNewOrders],
            datasets: [{
                data: [dataSets.Reports[0].Data[0].Quantity, dataSets.Reports[0].Data[1].Quantity, dataSets.Reports[0].Data[2].Quantity],
                backgroundColor: [colorDanger, colorWarning, colorPrimary],
                borderAlign: 'center',
                borderColor: '#fff',
                borderWidth: 5,
                weight: 1,
                hoverBackgroundColor: [colorDanger, colorWarning, colorPrimary],
                hoverBorderColor: '#fff',
                hoverBorderWidth: 0,
            }],
        };
        var chartDay = new Chart(
            $('#incomplete-orders-chart-day').get(0).getContext('2d'),
            {
                type: 'doughnut',
                data: dataDay,
                options: orders_config
            }
        );
        // This value is needed for mapping tooltip data
        var alreadyExistingCharts = chartDay.id;

        var dataWeek = {
            labels: [textNotShipped, textNotPayed, textNewOrders],
            datasets: [{
                data: [dataSets.Reports[1].Data[0].Quantity, dataSets.Reports[1].Data[1].Quantity, dataSets.Reports[1].Data[2].Quantity],
                backgroundColor: [colorDanger, colorWarning, colorPrimary],
                borderAlign: 'center',
                borderColor: '#fff',
                borderWidth: 5,
                weight: 1,
                hoverBackgroundColor: [colorDanger, colorWarning, colorPrimary],
                hoverBorderColor: '#fff',
                hoverBorderWidth: 0,
            }],
        };
        var chartWeek = new Chart(
            $('#incomplete-orders-chart-week').get(0).getContext('2d'),
            {
                type: 'doughnut',
                data: dataWeek,
                options: orders_config
            }
        );

        var dataMonth = {
            labels: [textNotShipped, textNotPayed, textNewOrders],
            datasets: [{
                data: [dataSets.Reports[2].Data[0].Quantity, dataSets.Reports[2].Data[1].Quantity, dataSets.Reports[2].Data[2].Quantity],
                backgroundColor: [colorDanger, colorWarning, colorPrimary],
                borderAlign: 'center',
                borderColor: '#fff',
                borderWidth: 5,
                weight: 1,
                hoverBackgroundColor: [colorDanger, colorWarning, colorPrimary],
                hoverBorderColor: '#fff',
                hoverBorderWidth: 0,
            }],
        };
        var chartMonth = new Chart(
            $('#incomplete-orders-chart-month').get(0).getContext('2d'),
            {
                type: 'doughnut',
                data: dataMonth,
                options: orders_config
            }
        );

        var dataOverall = {
            labels: [textNotShipped, textNotPayed, textNewOrders],
            datasets: [{
                data: [dataSets.Reports[3].Data[0].Quantity, dataSets.Reports[3].Data[1].Quantity, dataSets.Reports[3].Data[2].Quantity],
                backgroundColor: [colorDanger, colorWarning, colorPrimary],
                borderAlign: 'center',
                borderColor: '#fff',
                borderWidth: 5,
                weight: 1,
                hoverBackgroundColor: [colorDanger, colorWarning, colorPrimary],
                hoverBorderColor: '#fff',
                hoverBorderWidth: 0,
            }],
        };
        var chartOverall = new Chart(
            $('#incomplete-orders-chart-overall').get(0).getContext('2d'),
            {
                type: 'doughnut',
                data: dataOverall,
                options: orders_config
            }
        );
    }

    createOrdersChart = function (textCancelled, textPending, textProcessing, textComplete, textOrders) {
        var ordersData = $('#orders-report').data('chart');
        var percentageElement = $("#orders-delta-percentage");
        var chevronElement = $("#orders-delta-percentage-chevron");
        var sumElement = $("#orders-sum-amount");
        var ordersChartElement = $('#orders-chart');
        var orders_ctx = ordersChartElement.get(0).getContext('2d');
        var currentPeriod = 0;

        var cancelledGradient = orders_ctx.createLinearGradient(0, 0, 0, ordersChartElement.parent().height());
        cancelledGradient.addColorStop(0, root.css('--chart-color-danger'));
        cancelledGradient.addColorStop(1, root.css('--chart-color-danger-light'));

        var pendingGradient = orders_ctx.createLinearGradient(0, 0, 0, ordersChartElement.parent().height());
        pendingGradient.addColorStop(0, root.css('--chart-color-warning'));
        pendingGradient.addColorStop(1, root.css('--chart-color-warning-light'));

        var processingGradient = orders_ctx.createLinearGradient(0, 0, 0, ordersChartElement.parent().height());
        processingGradient.addColorStop(0, root.css('--chart-color-success'));
        processingGradient.addColorStop(1, root.css('--chart-color-success-light'));

        var completeGradient = orders_ctx.createLinearGradient(0, 0, 0, ordersChartElement.parent().height());
        completeGradient.addColorStop(0, root.css('--chart-color-primary'));
        completeGradient.addColorStop(1, root.css('--chart-color-primary-light'));

        // Chart config
        var order_config = {
            type: 'line',
            data: {
                labels: ordersData.Reports[0].Labels,
                datasets: [{
                    label: textCancelled,
                    data: ordersData.Reports[0].DataSets[0].Amount,
                    borderColor: colorDanger,
                    backgroundColor: cancelledGradient,
                    pointBackgroundColor: colorDanger,
                    pointHoverBackgroundColor: colorDanger,
                    pointHoverBorderColor: 'transparent',
                }, {
                    label: textPending,
                    data: ordersData.Reports[0].DataSets[1].Amount,
                    borderColor: colorWarning,
                    backgroundColor: pendingGradient,
                    hidden: true,
                    pointBackgroundColor: colorWarning,
                    pointHoverBackgroundColor: colorWarning,
                    pointHoverBorderColor: 'transparent',
                }, {
                    label: textProcessing,
                    data: ordersData.Reports[0].DataSets[2].Amount,
                    borderColor: colorSuccess,
                    backgroundColor: processingGradient,
                    hidden: true,
                    pointBackgroundColor: colorSuccess,
                    pointHoverBackgroundColor: colorSuccess,
                    pointHoverBorderColor: 'transparent',
                }, {
                    label: textComplete,
                    data: ordersData.Reports[0].DataSets[3].Amount,
                    borderColor: colorPrimary,
                    backgroundColor: completeGradient,
                    pointBackgroundColor: colorPrimary,
                    pointHoverBackgroundColor: colorPrimary,
                    pointHoverBorderColor: 'transparent',
                }]
            },
            options: {
                responsive: true,
                responsiveAnimationDuration: 0,
                maintainAspectRatio: false,
                stacked: true,
                animation: {
                    duration: 400,
                    hide: {
                        visible: {
                            type: true,
                            easing: 'easeInOutSine'
                        },
                    },
                    easing: 'easeInOutSine',
                },
                hover: {
                    mode: 'nearest',
                    intersect: false,
                    animationDuration: 250,
                },
                layout: {
                    padding: {
                        left: 0,
                        right: 0,
                        top: 6,
                        bottom: 0
                    }
                },
                legend: false,
                legendCallback: function (chart) {
                    var text = [];
                    text.push('<ul class="' + chart.id + '-legend">');
                    for (var i = chart.data.datasets.length - 1; i >= 0; i--) {
                        if (chart.data.datasets[i].hidden) {
                            text.push('<li class="hidden"><span class="legend" style="background-color:' + chart.data.datasets[i].borderColor + '"></span>');
                        }
                        else {
                            text.push('<li><span class="legend" style="background-color:' + chart.data.datasets[i].borderColor + '"></span>');
                        }

                        if (chart.data.labels[i]) {
                            text.push('<span>' + chart.data.datasets[i].label + '</span>');
                            text.push('<span class="font-weight-500 pl-1 total-amount">' + ordersData.Reports[currentPeriod].DataSets[i].TotalAmount + '</span>');
                        }
                        text.push('</li>');
                    }
                    text.push('</ul>');
                    return text.join("");
                },
                elements: {
                    point: {
                        radius: 0,
                        hoverRadius: 6,
                        hitRadius: 3,
                    },
                    line: {
                        borderWidth: 1,
                        lineTension: 0.2,
                        fill: true,
                    }
                },
                tooltips: {
                    enabled: true,
                    mode: 'nearest',
                    intersect: false,
                    titleFontFamily: fontFamily,
                    titleFontSize: 13,
                    bodyFontFamily: fontFamily,
                    bodyFontSize: 12,
                    xPadding: 12,
                    yPadding: 10,
                    caretPadding: 6,
                    caretSize: 8,
                    cornerRadius: 4,
                    titleMarginBottom: 8,
                    bodySpacing: 5,
                    callbacks: {
                        label: function (item, data) {
                            return " " + textOrders + ": " + ordersData.Reports[currentPeriod].DataSets[item.datasetIndex].FormattedQuantity[item.index]
                                + "    " + ordersData.Reports[currentPeriod].DataSets[item.datasetIndex].FormattedAmount[item.index];
                        },
                        labelColor: function (tooltipItem, chart) {
                            var dataset = chart.config.data.datasets[tooltipItem.datasetIndex];
                            return {
                                backgroundColor: dataset.borderColor,
                            }
                        },
                    },
                },
                scales: {
                    yAxes: [{
                        display: false,
                        stacked: true,
                    }],
                    xAxes: [{
                        display: false,
                        gridLines: {
                            display: false,
                        },
                        scaleLabel: {
                            display: false,
                            padding: 0,
                        },
                    }]
                },
                title: {
                    display: false,
                }
            },
        }

        var ordersChart = new Chart(orders_ctx, order_config);
        setPercentageDelta(currentPeriod);
        var $ordersLegendElement = $("#orders-chart-legend").get(0);
        createLegend();

        // EventHandler to display selected period data     
        $('input[type=radio][name=orders-toggle]').on('change', function () {
            setChartData($('input:radio[name=orders-toggle]:checked').data("period"));
        });

        function setChartData(period) {
            ordersChart.destroy();
            order_config.data.labels = ordersData.Reports[period].Labels;
            for (var i = 0; i < order_config.data.datasets.length; i++) {
                order_config.data.datasets[i].data = ordersData.Reports[period].DataSets[i].Amount;
            }
            ordersChart = new Chart(orders_ctx, order_config);
            setPercentageDelta(period, ordersData);
            currentPeriod = period;
            createLegend();
        }

        function setPercentageDelta(period) {
            var val = ordersData.Reports[period].PercentageDelta;
            if (val < 0) {
                chevronElement.addClass("negative");
                chevronElement.removeClass("d-none");
                percentageElement.removeClass("text-success");
                percentageElement.addClass("text-danger");
            }
            else if (val > 0) {
                chevronElement.removeClass("negative");
                chevronElement.removeClass("d-none");
                percentageElement.addClass("text-success");
                percentageElement.removeClass("text-danger");
            }
            else {
                chevronElement.addClass("d-none")
            }
            var delta = val == 0 ? "" : val < 0 ? "-" + Math.abs(val) + "%" : "+" + Math.abs(val) + "%"; // TODO: format value on server
            percentageElement.html(delta);
            sumElement.html(ordersData.Reports[period].TotalAmount);
        }

        // Custom chart legend
        function createLegend() {
            $($ordersLegendElement).html(ordersChart.generateLegend());
            var legendItems = $ordersLegendElement.getElementsByTagName('li');
            for (var i = 0; i < legendItems.length; i++) {
                legendItems[i].addEventListener("click", legendClickCallback, false);
            }
        }

        // Custom chart legend callback
        function legendClickCallback(event) {
            event = event || window.event;
            var target = event.target || event.srcElement;
            while (target.nodeName !== 'LI') {
                target = target.parentElement;
            }
            var parent = target.parentElement;
            var chartId = parseInt(parent.classList[0].split("-")[0], 10);
            var chart = Chart.instances[chartId];
            var index = (ordersChart.data.datasets.length - 1) - Array.prototype.slice.call(parent.children).indexOf(target);
            var meta = chart.getDatasetMeta(index);
            if (chart.data.datasets[index].hidden) {
                target.classList.remove('hidden');
            }
            else {
                target.classList.add('hidden');
            }
            meta.hidden = !chart.data.datasets[index].hidden;
            chart.data.datasets[index].hidden = !chart.data.datasets[index].hidden;
            chart.update();
        }
    }

    createCustomersChart = function (textRegistrations, textRegistrationsShort) {
        var customersData = $('#customers-report').data('chart');
        var percentageElement = $("#customers-delta-percentage");
        var chevronElement = $("#customers-delta-percentage-chevron");
        var sumElement = $("#customer-quantity-total");
        var customersChartElement = $('#customers-chart');
        var customers_ctx = customersChartElement.get(0).getContext('2d');
        var currentPeriod = 0;

        var successGradient = customers_ctx.createLinearGradient(0, 0, 0, customersChartElement.parent().height());
        successGradient.addColorStop(0, root.css('--chart-color-success'));
        successGradient.addColorStop(1, root.css('--chart-color-success-light'));

        // Chart config
        var customer_config = {
            type: 'line',
            data: {
                labels: customersData.Reports[0].Labels,
                datasets: [{
                    label: textRegistrations,
                    data: customersData.Reports[0].DataSets[0].Quantity,
                    borderColor: colorSuccess,
                    backgroundColor: successGradient,
                    pointBackgroundColor: colorSuccess,
                    pointHoverBackgroundColor: colorSuccess,
                    pointHoverBorderColor: 'transparent',
                }]
            },
            options: {
                responsive: true,
                responsiveAnimationDuration: 0,
                maintainAspectRatio: false,
                stacked: true,
                animation: {
                    duration: 400,
                    easing: 'easeInOutSine',
                    hide: {
                        visible: {
                            type: true,
                            easing: 'easeInOutSine'
                        },
                    },
                },
                hover: {
                    mode: 'nearest',
                    intersect: false,
                    animationDuration: 250,
                },
                layout: {
                    padding: {
                        left: 0,
                        right: 0,
                        top: 7,
                        bottom: 0
                    }
                },
                legend: false,
                elements: {
                    point: {
                        radius: 0,
                        hoverRadius: 6,
                        hitRadius: 3,
                    },
                    line: {
                        borderWidth: 1,
                        lineTension: 0.2,
                        fill: true,
                    }
                },
                tooltips: {
                    enabled: true,
                    mode: 'nearest',
                    intersect: false,
                    titleFontFamily: fontFamily,
                    titleFontSize: 13,
                    bodyFontFamily: fontFamily,
                    bodyFontSize: 12,
                    xPadding: 12,
                    yPadding: 10,
                    caretPadding: 6,
                    caretSize: 8,
                    cornerRadius: 4,
                    titleMarginBottom: 8,
                    bodySpacing: 5,
                    callbacks: {
                        label: function (item, data) {
                            return " " + textRegistrationsShort + ":  "
                                + customersData.Reports[currentPeriod].DataSets[item.datasetIndex].FormattedQuantity[item.index];
                        },
                        labelColor: function (tooltipItem, chart) {
                            var dataset = chart.config.data.datasets[tooltipItem.datasetIndex];
                            return {
                                backgroundColor: dataset.borderColor,
                            }
                        },
                    },
                },
                scales: {
                    yAxes: [{
                        display: false,
                        stacked: true,
                    }],
                    xAxes: [{
                        display: false,
                        gridLines: {
                            display: false,
                        },
                        scaleLabel: {
                            display: false,
                            padding: 0,
                        },
                    }]
                },
                title: {
                    display: false,
                }
            },
        }

        var customersChart = new Chart(customers_ctx, customer_config);
        setPercentageDelta(currentPeriod);

        // EventHandler to display selected period data       
        $('input[type=radio][name=customers-toggle]').on('change', function () {
            setChartData($('input:radio[name=customers-toggle]:checked').data("period"));
        });

        function setChartData(period) {
            customersChart.destroy();
            customer_config.data.labels = customersData.Reports[period].Labels;
            for (var i = 0; i < customer_config.data.datasets.length; i++) {
                customer_config.data.datasets[i].data = customersData.Reports[period].DataSets[i].Quantity;
            }
            customersChart = new Chart(customers_ctx, customer_config);
            setPercentageDelta(period, customersData);
            currentPeriod = period;
        }

        function setPercentageDelta(period) {
            var val = customersData.Reports[period].PercentageDelta;
            if (val < 0) {
                chevronElement.addClass("negative");
                chevronElement.removeClass("d-none");
                percentageElement.removeClass("text-success");
                percentageElement.addClass("text-danger");
            }
            else if (val > 0) {
                chevronElement.removeClass("negative");
                chevronElement.removeClass("d-none");
                percentageElement.addClass("text-success");
                percentageElement.removeClass("text-danger");
            }
            else {
                chevronElement.addClass("d-none")
            }
            var delta = val == 0 ? "" : val < 0 ? "-" + Math.abs(val) + "%" : "+" + Math.abs(val) + "%";
            percentageElement.html(delta);
            sumElement.html(customersData.Reports[period].TotalAmount);
        }
    }
})(jQuery, this, document);