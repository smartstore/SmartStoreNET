// Creates dashboard charts with Chart.js
SmartStore.Admin.Charts = {
    Create: (function () {
        const style = getComputedStyle(document.documentElement);
        const colorPrimary = style.getPropertyValue('--primary');
        const colorSuccess = style.getPropertyValue('--success');
        const colorWarning = style.getPropertyValue('--warning');
        const colorDanger = style.getPropertyValue('--danger');
        const fontFamily = style.getPropertyValue('--font-family-sans-serif');

        const customTooltip = function (tooltip) {
            const canvas = this._chart.canvas;
            const chartElement = canvas.closest('.report');
            // Get tooltip of chart element
            let tooltipEl = document.getElementById(chartElement.id).querySelector('.chart-tooltip');
            if (!tooltipEl) {
                tooltipEl = document.createElement('div');
                tooltipEl.classList.add('chart-tooltip');
                chartElement.append(tooltipEl);
            }

            // Hide if no tooltip visible
            if (tooltip.opacity === 0) {
                tooltipEl.style.opacity = 0;
                return;
            }

            // Set chart and caret position
            tooltipEl.classList.remove('top', 'bottom', 'center', 'left', 'right');
            if (tooltip.yAlign) {
                tooltipEl.classList.add(tooltip.yAlign);
            }
            if (tooltip.xAlign) {
                tooltipEl.classList.add(tooltip.xAlign);
            }

            function getBody(bodyItem) {
                return bodyItem.lines;
            }

            // Create tooltip html
            if (tooltip.body) {
                const titleLines = tooltip.title || [];
                const bodyLines = tooltip.body.map(getBody);
                let innerHtml = '';

                titleLines.forEach(function (title) {
                    innerHtml += '<div class="chart-tooltip-title">' + title + '</div>';
                });

                bodyLines.forEach(function (body, i) {
                    const colors = tooltip.labelColors[i];
                    const style = 'background:' + colors.backgroundColor;
                    const indicator = '<span class="chart-tooltip-indicator" style="' + style + '"></span>';
                    innerHtml += '<div class="chart-tooltip-body">' + indicator + body + '</div>';
                });

                tooltipEl.innerHTML = innerHtml;
            }

            // Display tooltip and set position
            const position = canvas.getBoundingClientRect();
            const canvasHeight = parseInt(getComputedStyle(canvas).getPropertyValue('height'));
            tooltipEl.style.opacity = 1;
            tooltipEl.style.left = position.left + tooltip.caretX + 'px';
            tooltipEl.style.marginTop = tooltip.caretY - canvasHeight + 'px';
        };

        const chartToggleListener = function (el, setChartData) {
            el.querySelectorAll(".btn-dashboard")
                .forEach(function (value, index) {
                    value.addEventListener("click", function (event) {
                        setChartData(index);
                    })
                });
        }

        const setPercentageDelta = function (datasets, chevronEl, percentEl, sumEl) {
            let delta = '';
            if (datasets.PercentageDelta < 0) {
                chevronEl.classList.add('negative');
                chevronEl.classList.remove('d-none');
                percentEl.classList.remove('text-success');
                percentEl.classList.add('text-danger');
                delta = '-' + Math.abs(datasets.PercentageDelta) + '%';
            }
            else if (datasets.PercentageDelta > 0) {
                chevronEl.classList.remove('negative');
                chevronEl.classList.remove('d-none');
                percentEl.classList.add('text-success');
                percentEl.classList.remove('text-danger');
                delta = '+' + Math.abs(datasets.PercentageDelta) + '%';;
            }
            else {
                chevronEl.classList.add('d-none');
            }
            percentEl.innerText = delta;
            sumEl.innerText = datasets.TotalAmountFormatted;
        }

        return {
            IncompleteOrdersCharts: function (dataSets, textFulfilled, textNotShipped, textNotPayed, textNewOrders, textOrders, textAmount) {
                for (let i = 0; i < dataSets.length; i++) {
                    // If there are no incomplete orders for set i then add 1 to new orders (data index 2) so tooltip gets displayed
                    if (dataSets[i].Data[0].Quantity == 0 && dataSets[i].Data[1].Quantity == 0) {
                        dataSets[i].Data[2].Quantity = 1;
                    }
                }

                const chartConfig = {
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
                                // If tooltip is newOrders (index 2) and no other data is available, display orders fulfilled text
                                if (item.index == 2
                                    && data.datasets[0].data[0] == 0
                                    && data.datasets[0].data[1] == 0) {
                                    return textFulfilled;
                                }
                                const d = dataSets[this._chart.id + alreadyExistingCharts].Data[item.index];
                                return [textOrders + ':  ' + d.QuantityFormatted, textAmount + ':  ' + d.AmountFormatted];
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
                    document.getElementById('incomplete-orders-chart-0').getContext('2d'),
                    {
                        type: 'doughnut',
                        data: dataDay,
                        options: chartConfig
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
                    document.getElementById('incomplete-orders-chart-1').getContext('2d'),
                    {
                        type: 'doughnut',
                        data: dataWeek,
                        options: chartConfig
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
                    document.getElementById('incomplete-orders-chart-2').getContext('2d'),
                    {
                        type: 'doughnut',
                        data: dataMonth,
                        options: chartConfig
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
                    document.getElementById('incomplete-orders-chart-3').getContext('2d'),
                    {
                        type: 'doughnut',
                        data: dataOverall,
                        options: chartConfig
                    }
                );
            },
            OrdersChart: function (dataSets, textCancelled, textPending, textProcessing, textComplete, textOrders) {
                const reportElement = document.getElementById('orders-report');
                const reportStyle = getComputedStyle(reportElement);
                const percentageElement = document.getElementById('orders-delta-percentage');
                const chevronElement = document.getElementById('orders-delta-percentage-chevron');
                const sumElement = document.getElementById('orders-sum-amount');
                const chartElement = document.getElementById('orders-chart');
                const chartContext = chartElement.getContext('2d');
                let currentPeriod = 0;

                const cancelledGradient = chartContext.createLinearGradient(0, 0, 0, chartElement.height);
                cancelledGradient.addColorStop(0, reportStyle.getPropertyValue('--chart-color-danger'));
                cancelledGradient.addColorStop(1, reportStyle.getPropertyValue('--chart-color-danger-light'));

                const pendingGradient = chartContext.createLinearGradient(0, 0, 0, chartElement.height);
                pendingGradient.addColorStop(0, reportStyle.getPropertyValue('--chart-color-warning'));
                pendingGradient.addColorStop(1, reportStyle.getPropertyValue('--chart-color-warning-light'));

                const processingGradient = chartContext.createLinearGradient(0, 0, 0, chartElement.height);
                processingGradient.addColorStop(0, reportStyle.getPropertyValue('--chart-color-success'));
                processingGradient.addColorStop(1, reportStyle.getPropertyValue('--chart-color-success-light'));

                const completeGradient = chartContext.createLinearGradient(0, 0, 0, chartElement.height);
                completeGradient.addColorStop(0, reportStyle.getPropertyValue('--chart-color-primary'));
                completeGradient.addColorStop(1, reportStyle.getPropertyValue('--chart-color-primary-light'));

                const chartConfig = {
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
                                const data = chart.data.datasets[i];
                                if (data.hidden) {
                                    text.push('<li class="hidden"><span class="legend" style="background-color:' + data.borderColor + '"></span>');
                                }
                                else {
                                    text.push('<li><span class="legend" style="background-color:' + data.borderColor + '"></span>');
                                }

                                if (chart.data.labels[i]) {
                                    text.push('<span>' + data.label + '</span>');
                                    text.push('<span class="font-weight-medium pl-1 total-amount">'
                                        + dataSets[currentPeriod].DataSets[i].TotalAmountFormatted + '</span>');
                                }
                                text.push('</li>');
                            }
                            text.push('</ul>');
                            return text.join('');
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
                            enabled: false,
                            mode: 'nearest',
                            intersect: false,
                            callbacks: {
                                label: function (item, data) {
                                    const d = dataSets[currentPeriod].DataSets[item.datasetIndex];
                                    return '<span>' + textOrders + ': </span><span class="ml-1">' + d.QuantityFormatted[item.index]
                                        + '</span><span class="ml-2 pl-1">' + d.AmountFormatted[item.index] + '</span>';
                                },
                                labelColor: function (tooltipItem, chart) {
                                    const dataset = chart.config.data.datasets[tooltipItem.datasetIndex];
                                    return {
                                        backgroundColor: dataset.borderColor,
                                    }
                                },
                            },
                            custom: customTooltip,
                        },
                        scales: {
                            yAxes: [{
                                display: false,
                                stacked: true,

                                ticks: {
                                    max: 1,
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
                let chart = new Chart(chartContext, chartConfig);
                setPercentageDelta(dataSets[currentPeriod], chevronElement, percentageElement, sumElement);

                const legendElement = document.getElementById('orders-chart-legend');
                createLegend();
                setYaxis();
                chart.update();
                chartToggleListener(reportElement, setChartData);

                function setChartData(period) {
                    if (currentPeriod == period) return;
                    chart.destroy();
                    chartConfig.data.labels = dataSets[period].Labels;
                    for (let i = 0; i < chartConfig.data.datasets.length; i++) {
                        chartConfig.data.datasets[i].data = dataSets[period].DataSets[i].Amount;
                    }
                    chart = new Chart(chartContext, chartConfig);
                    setPercentageDelta(dataSets[period], chevronElement, percentageElement, sumElement);
                    currentPeriod = period;
                    createLegend();
                    setYaxis();
                    chart.update();
                }

                // Get highest combined yAxis value (+ 10% margin) from not hidden datasets
                function setYaxis() {
                    const sumArr = [];
                    const datasets = chart.data.datasets;
                    for (let i = 0; i < datasets['0'].data.length; i++) {
                        let num = 0;
                        for (let j = 0; j < datasets.length; j++) {
                            num += datasets[j].hidden ? 0 : datasets[j].data[i];
                        }
                        sumArr[i] = num;
                    }
                    const yAxisSize = Math.max(...sumArr);
                    chart.config.options.scales.yAxes[0].ticks.max = yAxisSize == 0 ? 1 : yAxisSize * 1.1;
                }

                // Custom chart legend
                function createLegend() {
                    legendElement.innerHTML = chart.generateLegend();
                    const legendItems = legendElement.getElementsByTagName('li');
                    for (let i = 0; i < legendItems.length; i++) {
                        legendItems[i].addEventListener('click', legendClickCallback, false);
                        const dataset = chart.data.datasets[i];
                        dataset.hidden = Math.max(...dataset.data) <= 0 ? true : false;
                        if (dataset.hidden) {
                            legendItems[legendItems.length - i - 1].classList.add('hidden');
                            legendItems[legendItems.length - i - 1].classList.add('inactive');
                        }
                        else {
                            legendItems[legendItems.length - i - 1].classList.remove('hidden');
                            legendItems[legendItems.length - i - 1].classList.remove('inactive');
                        }
                    }
                }

                // Custom chart legend callback
                function legendClickCallback() {
                    const chartId = parseInt(this.parentElement.classList[0].split('-')[0], 10);
                    const chart = Chart.instances[chartId];
                    const index = (chart.data.datasets.length - 1) - Array.prototype.slice.call(this.parentElement.children).indexOf(this);
                    const dataset = chart.data.datasets[index];
                    dataset.hidden = Math.max(...chart.data.datasets[index].data) <= 0 ? true : !dataset.hidden;
                    if (dataset.hidden) {
                        this.classList.add('hidden');
                    }
                    else {
                        this.classList.remove('hidden');
                    }
                    setYaxis();
                    chart.update();
                }
            },
            CustomersChart: function (dataSets, textRegistrations, textRegistrationsShort) {
                const reportElement = document.getElementById('customers-report');
                const reportStyle = getComputedStyle(reportElement);
                const percentageElement = document.getElementById('customers-delta-percentage');
                const chevronElement = document.getElementById('customers-delta-percentage-chevron');
                const sumElement = document.getElementById('customer-quantity-total');
                const chartElement = document.getElementById('customers-chart');
                const chartContext = chartElement.getContext('2d');
                let currentPeriod = 0;

                const successGradient = chartContext.createLinearGradient(0, 0, 0, chartElement.height);
                successGradient.addColorStop(0, reportStyle.getPropertyValue('--chart-color-success'));
                successGradient.addColorStop(1, reportStyle.getPropertyValue('--chart-color-success-light'));

                const chartConfig = {
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
                                borderWidth: 0.5,
                            }
                        },
                        tooltips: {
                            enabled: false,
                            mode: 'nearest',
                            intersect: false,
                            callbacks: {
                                label: function (item, data) {
                                    return '<span>' + textRegistrationsShort + ': </span><span class="ml-1">'
                                        + dataSets[currentPeriod].DataSets[item.datasetIndex].QuantityFormatted[item.index] + '</span>';
                                },
                                labelColor: function (tooltipItem, chart) {
                                    const dataset = chart.config.data.datasets[tooltipItem.datasetIndex];
                                    return {
                                        backgroundColor: dataset.borderColor,
                                    }
                                },
                            },
                            custom: customTooltip,
                        },
                        scales: {
                            yAxes: [{
                                display: false,
                                stacked: true,

                                ticks: {
                                    max: 1,
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
                let chart = new Chart(chartContext, chartConfig);
                setPercentageDelta(dataSets[currentPeriod], chevronElement, percentageElement, sumElement);
                setYaxis();
                chart.update();
                chartToggleListener(reportElement, setChartData);

                function setChartData(period) {
                    chart.destroy();
                    chartConfig.data.labels = dataSets[period].Labels;
                    for (let i = 0; i < chartConfig.data.datasets.length; i++) {
                        chartConfig.data.datasets[i].data = dataSets[period].DataSets[i].Quantity;
                    }
                    setYaxis();
                    chart = new Chart(chartContext, chartConfig);
                    setPercentageDelta(dataSets[period], chevronElement, percentageElement, sumElement);
                    currentPeriod = period;
                }

                // Get highest yAxis value (+ 10% margin)
                function setYaxis() {
                    const yAxisSize = Math.max(...chart.config.data.datasets['0'].data)
                    chart.config.options.scales.yAxes[0].ticks.max = yAxisSize == 0 ? 1 : yAxisSize * 1.1;
                }
            }
        }
    })()
};