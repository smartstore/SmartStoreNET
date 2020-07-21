// Creates dashboard charts with Chart.js
SmartStore.Admin.Charts = {
    Create: (function () {
        const root = $('html');
        const colorPrimary = root.css('--primary');
        const colorSuccess = root.css('--success');
        const colorWarning = root.css('--warning');
        const colorDanger = root.css('--danger');
        const fontFamily = root.css('--font-family-sans-serif');

        return {
            IncompleteOrdersCharts: function (dataSets, textFulfilled, textNotShipped, textNotPayed, textNewOrders, textOrders, textAmount) {
                for (let i = 0; i < dataSets.length; i++) {
                    // If there are no incomplete orders for set i > add 1 to new orders (data index 2) so tooltip gets displayed
                    if (dataSets[i].Data[0].Quantity == 0 && dataSets[i].Data[1].Quantity == 0) {
                        dataSets[i].Data[2].Quantity = 1;
                    }
                }

                // Chart config
                const orders_config = {
                    maintainAspectRatio: false,
                    cutoutPercentage: 90,
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
                            left: 8,
                            right: 8,
                            top: 20,
                            bottom: 15
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
                        titleFontSize: 13,
                        bodyFontFamily: fontFamily,
                        bodyFontSize: 13,
                        xPadding: 12,
                        yPadding: 10,
                        caretPadding: 6,
                        caretSize: 8,
                        cornerRadius: 4,
                        titleMarginBottom: 8,
                        bodySpacing: 5,
                        displayColors: false,
                        callbacks: {
                            label: function (item, data) {
                                // If tooltip equals newOrders (index 2) and no other data is avalable, display orders fulfilled text
                                if (item.index == 2
                                    && data.datasets[0].data[0] == 0
                                    && data.datasets[0].data[1] == 0) {
                                    return textFulfilled;
                                }
                                let d = dataSets[this._chart.id + alreadyExistingCharts].Data[item.index];
                                return [textOrders + ":  " + d.QuantityFormatted, textAmount + ":  " + d.AmountFormatted];
                            },
                            title: function (item, data) {
                                if (item[0].index == 2
                                    && data.datasets[0].data[0] == 0
                                    && data.datasets[0].data[1] == 0) {
                                    return;
                                }
                                return data.labels[item[0].index];
                            }
                        }
                    },
                };
                const dataDay = {
                    labels: [textNotShipped, textNotPayed, textNewOrders],
                    datasets: [{
                        data: [dataSets[0].Data[0].Quantity, dataSets[0].Data[1].Quantity, dataSets[0].Data[2].Quantity],
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
                const chartDay = new Chart(
                    $('#incomplete-orders-chart-0').get(0).getContext('2d'),
                    {
                        type: 'doughnut',
                        data: dataDay,
                        options: orders_config
                    }
                );

                // This value is needed for mapping tooltip data
                const alreadyExistingCharts = chartDay.id;
                const dataWeek = {
                    labels: [textNotShipped, textNotPayed, textNewOrders],
                    datasets: [{
                        data: [dataSets[1].Data[0].Quantity, dataSets[1].Data[1].Quantity, dataSets[1].Data[2].Quantity],
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
                const chartWeek = new Chart(
                    $('#incomplete-orders-chart-1').get(0).getContext('2d'),
                    {
                        type: 'doughnut',
                        data: dataWeek,
                        options: orders_config
                    }
                );

                const dataMonth = {
                    labels: [textNotShipped, textNotPayed, textNewOrders],
                    datasets: [{
                        data: [dataSets[2].Data[0].Quantity, dataSets[2].Data[1].Quantity, dataSets[2].Data[2].Quantity],
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
                const chartMonth = new Chart(
                    $('#incomplete-orders-chart-2').get(0).getContext('2d'),
                    {
                        type: 'doughnut',
                        data: dataMonth,
                        options: orders_config
                    }
                );

                const dataOverall = {
                    labels: [textNotShipped, textNotPayed, textNewOrders],
                    datasets: [{
                        data: [dataSets[3].Data[0].Quantity, dataSets[3].Data[1].Quantity, dataSets[3].Data[2].Quantity],
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
                const chartOverall = new Chart(
                    $('#incomplete-orders-chart-3').get(0).getContext('2d'),
                    {
                        type: 'doughnut',
                        data: dataOverall,
                        options: orders_config
                    }
                );
            },
            OrdersChart: function (dataSets, textCancelled, textPending, textProcessing, textComplete, textOrders) {
                const chartElement = $('#orders-report');
                const percentageElement = $("#orders-delta-percentage");
                const chevronElement = $("#orders-delta-percentage-chevron");
                const sumElement = $("#orders-sum-amount");
                const ordersChartElement = $('#orders-chart');
                const ordersChartElementHeight = ordersChartElement.parent().height();
                const orders_ctx = ordersChartElement.get(0).getContext('2d');
                let currentPeriod = 0;

                const cancelledGradient = orders_ctx.createLinearGradient(0, 0, 0, ordersChartElementHeight);
                cancelledGradient.addColorStop(0, chartElement.css('--chart-color-danger'));
                cancelledGradient.addColorStop(1, chartElement.css('--chart-color-danger-light'));

                const pendingGradient = orders_ctx.createLinearGradient(0, 0, 0, ordersChartElementHeight);
                pendingGradient.addColorStop(0, chartElement.css('--chart-color-warning'));
                pendingGradient.addColorStop(1, chartElement.css('--chart-color-warning-light'));

                const processingGradient = orders_ctx.createLinearGradient(0, 0, 0, ordersChartElementHeight);
                processingGradient.addColorStop(0, chartElement.css('--chart-color-success'));
                processingGradient.addColorStop(1, chartElement.css('--chart-color-success-light'));

                const completeGradient = orders_ctx.createLinearGradient(0, 0, 0, ordersChartElementHeight);
                completeGradient.addColorStop(0, chartElement.css('--chart-color-primary'));
                completeGradient.addColorStop(1, chartElement.css('--chart-color-primary-light'));

                // Chart config
                const order_config = {
                    type: 'line',
                    data: {
                        labels: dataSets[0].Labels,
                        datasets: [{
                            label: textCancelled,
                            data: dataSets[0].DataSets[0].Amount,
                            borderColor: colorDanger,
                            backgroundColor: cancelledGradient,
                            pointBackgroundColor: colorDanger,
                            pointHoverBackgroundColor: colorDanger,
                            pointHoverBorderColor: 'transparent',
                        }, {
                            label: textPending,
                            data: dataSets[0].DataSets[1].Amount,
                            borderColor: colorWarning,
                            backgroundColor: pendingGradient,
                            hidden: true,
                            pointBackgroundColor: colorWarning,
                            pointHoverBackgroundColor: colorWarning,
                            pointHoverBorderColor: 'transparent',
                        }, {
                            label: textProcessing,
                            data: dataSets[0].DataSets[2].Amount,
                            borderColor: colorSuccess,
                            backgroundColor: processingGradient,
                            hidden: true,
                            pointBackgroundColor: colorSuccess,
                            pointHoverBackgroundColor: colorSuccess,
                            pointHoverBorderColor: 'transparent',
                        }, {
                            label: textComplete,
                            data: dataSets[0].DataSets[3].Amount,
                            borderColor: colorPrimary,
                            backgroundColor: completeGradient,
                            pointBackgroundColor: colorPrimary,
                            pointHoverBackgroundColor: colorPrimary,
                            pointHoverBorderColor: 'transparent',
                        }]
                    },
                    options: {                     
                        maintainAspectRatio: false,
                        animation: {
                            duration: 400,
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
                                top: 0,
                                bottom: 0
                            }
                        },
                        legend: false,
                        legendCallback: function (chart) {
                            let text = [];
                            text.push('<ul class="' + chart.id + '-legend">');
                            for (let i = chart.data.datasets.length - 1; i >= 0; i--) {
                                let data = chart.data.datasets[i];
                                if (data.hidden) {
                                    text.push('<li class="hidden"><span class="legend" style="background-color:' + data.borderColor + '"></span>');
                                }
                                else {
                                    text.push('<li><span class="legend" style="background-color:' + data.borderColor + '"></span>');
                                }

                                if (chart.data.labels[i]) {
                                    text.push('<span>' + data.label + '</span>');
                                    text.push('<span class="font-weight-500 pl-1 total-amount">'
                                        + dataSets[currentPeriod].DataSets[i].TotalAmountFormatted + '</span>');
                                }
                                text.push('</li>');
                            }
                            text.push('</ul>');
                            return text.join("");
                        },
                        elements: {
                            point: {
                                radius: 0,
                                hoverRadius: 5,
                                hitRadius: 3,
                            },
                            line: {
                                borderWidth: 0.5,
                                cubicInterpolationMode: 'monotone',
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
                                    let d = dataSets[currentPeriod].DataSets[item.datasetIndex];
                                    return " " + textOrders + ": " + d.QuantityFormatted[item.index]
                                        + "    " + d.AmountFormatted[item.index];
                                },
                                labelColor: function (tooltipItem, chart) {
                                    let dataset = chart.config.data.datasets[tooltipItem.datasetIndex];
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
                let ordersChart = new Chart(orders_ctx, order_config);
                setPercentageDelta(currentPeriod);
                const $ordersLegendElement = $("#orders-chart-legend").get(0);
                createLegend();
                setYaxis(ordersChart);
                ordersChart.update();

                // EventHandler to display selected period data     
                $('input[type=radio][name=orders-toggle]').on('change', function () {
                    setChartData($('input:radio[name=orders-toggle]:checked').data("period"));
                });

                function setChartData(period) {
                    ordersChart.destroy();
                    order_config.data.labels = dataSets[period].Labels;
                    for (let i = 0; i < order_config.data.datasets.length; i++) {
                        order_config.data.datasets[i].data = dataSets[period].DataSets[i].Amount;
                    }
                    setYaxis(ordersChart);
                    ordersChart = new Chart(orders_ctx, order_config);
                    setPercentageDelta(period, dataSets);
                    currentPeriod = period;
                    createLegend();
                }

                // Get highest combined yAxis value from not hidden datasets
                function setYaxis(chart) {
                    let sumArr = [];
                    let datasets = chart.data.datasets;
                    for (let i = 0; i < datasets["0"].data.length; i++) {
                        let num = 0;
                        for (let j = 0; j < datasets.length; j++) {
                            num += datasets[j].hidden ? 0 : datasets[j].data[i];
                        }
                        sumArr[i] = num;
                    }
                    let yAxisSize = Math.max(...sumArr);
                    chart.config.options.scales.yAxes[0].ticks.max = yAxisSize == 0 ? 1 : yAxisSize * 1.1;
                }

                function setPercentageDelta(period) {
                    let delta = "";
                    let val = dataSets[period].PercentageDelta;
                    if (val < 0) {
                        chevronElement.addClass("negative");
                        chevronElement.removeClass("d-none");
                        percentageElement.removeClass("text-success");
                        percentageElement.addClass("text-danger");
                        delta = "-" + Math.abs(val) + "%";
                    }
                    else if (val > 0) {
                        chevronElement.removeClass("negative");
                        chevronElement.removeClass("d-none");
                        percentageElement.addClass("text-success");
                        percentageElement.removeClass("text-danger");
                        delta = "+" + Math.abs(val) + "%";;
                    }
                    else {
                        chevronElement.addClass("d-none")
                    }
                    percentageElement.html(delta);
                    sumElement.html(dataSets[period].TotalAmountFormatted);
                }

                // Custom chart legend
                function createLegend() {
                    $($ordersLegendElement).html(ordersChart.generateLegend());
                    let legendItems = $ordersLegendElement.getElementsByTagName('li');
                    for (let i = 0; i < legendItems.length; i++) {
                        legendItems[i].addEventListener("click", legendClickCallback, false);
                    }
                }

                // Custom chart legend callback
                function legendClickCallback(event) {
                    event = event || window.event;
                    let target = event.target || event.srcElement;
                    while (target.nodeName !== 'LI') {
                        target = target.parentElement;
                    }
                    let parent = target.parentElement;
                    let chartId = parseInt(parent.classList[0].split("-")[0], 10);
                    let chart = Chart.instances[chartId];
                    let index = (ordersChart.data.datasets.length - 1) - Array.prototype.slice.call(parent.children).indexOf(target);
                    let meta = chart.getDatasetMeta(index);
                    if (chart.data.datasets[index].hidden) {
                        target.classList.remove('hidden');
                    }
                    else {
                        target.classList.add('hidden');
                    }
                    meta.hidden = !chart.data.datasets[index].hidden;
                    chart.data.datasets[index].hidden = !chart.data.datasets[index].hidden;
                    setYaxis(chart);
                    chart.update();
                }
            },
            CustomersChart: function (dataSets, textRegistrations, textRegistrationsShort) {
                const chartElement = $('#customers-report');
                const percentageElement = $("#customers-delta-percentage");
                const chevronElement = $("#customers-delta-percentage-chevron");
                const sumElement = $("#customer-quantity-total");
                const customersChartElement = $('#customers-chart');
                const customers_ctx = customersChartElement.get(0).getContext('2d');
                let currentPeriod = 0;

                const successGradient = customers_ctx.createLinearGradient(0, 0, 0, customersChartElement.parent().height());
                successGradient.addColorStop(0, chartElement.css('--chart-color-success'));
                successGradient.addColorStop(1, chartElement.css('--chart-color-success-light'));

                // Chart config
                const customer_config = {
                    type: 'line',
                    data: {
                        labels: dataSets[0].Labels,
                        datasets: [{
                            label: textRegistrations,
                            data: dataSets[0].DataSets[0].Quantity,
                            borderColor: colorSuccess,
                            backgroundColor: successGradient,
                            pointBackgroundColor: colorSuccess,
                            pointHoverBackgroundColor: colorSuccess,
                            pointHoverBorderColor: 'transparent',
                        }]
                    },
                    options: {
                        maintainAspectRatio: false,
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
                                top: 0,
                                bottom: 0
                            }
                        },
                        legend: false,
                        elements: {
                            point: {
                                radius: 0,
                                hoverRadius: 5,
                                hitRadius: 3,
                            },
                            line: {
                                borderWidth: .5,
                                cubicInterpolationMode: 'monotone',
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
                                        + dataSets[currentPeriod].DataSets[item.datasetIndex].QuantityFormatted[item.index];
                                },
                                labelColor: function (tooltipItem, chart) {
                                    let dataset = chart.config.data.datasets[tooltipItem.datasetIndex];
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

                                ticks: {
                                    max: getYaxis(dataSets[0]),
                                }
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
                let customersChart = new Chart(customers_ctx, customer_config);
                setPercentageDelta(currentPeriod);

                // EventHandler to display selected period data       
                $('input[type=radio][name=customers-toggle]').on('change', function () {
                    setChartData($('input:radio[name=customers-toggle]:checked').data("period"));
                });

                function setChartData(period) {
                    customersChart.destroy();
                    customer_config.data.labels = dataSets[period].Labels;
                    for (let i = 0; i < customer_config.data.datasets.length; i++) {
                        customer_config.data.datasets[i].data = dataSets[period].DataSets[i].Quantity;
                    }
                    customer_config.options.scales.yAxes[0].ticks.max = getYaxis(dataSets[period]);
                    customersChart = new Chart(customers_ctx, customer_config);
                    setPercentageDelta(period, dataSets);
                    currentPeriod = period;
                }

                function getYaxis(chartData) {
                    let yAxisSize = Math.max(...chartData.DataSets.map(e => Math.max(...e.Quantity)))
                    return yAxisSize == 0 ? 1 : yAxisSize * 1.1;
                }

                function setPercentageDelta(period) {
                    let delta = "";
                    let val = dataSets[period].PercentageDelta;
                    if (val < 0) {
                        chevronElement.addClass("negative");
                        chevronElement.removeClass("d-none");
                        percentageElement.removeClass("text-success");
                        percentageElement.addClass("text-danger");
                        delta = "-" + Math.abs(val) + "%";
                    }
                    else if (val > 0) {
                        chevronElement.removeClass("negative");
                        chevronElement.removeClass("d-none");
                        percentageElement.addClass("text-success");
                        percentageElement.removeClass("text-danger");
                        delta = "+" + Math.abs(val) + "%";
                    }
                    else {
                        chevronElement.addClass("d-none")
                    }
                    percentageElement.html(delta);
                    sumElement.html(dataSets[period].TotalAmountFormatted);
                }
            }
        }
    })()
};