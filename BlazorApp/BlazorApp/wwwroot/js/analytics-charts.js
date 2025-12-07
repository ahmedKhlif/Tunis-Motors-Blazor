// Analytics Charts JavaScript
// Store chart instances globally to destroy them before recreating
window.chartInstances = window.chartInstances || {};

// Function to destroy a chart if it exists
function destroyChart(chartId) {
    if (window.chartInstances[chartId]) {
        window.chartInstances[chartId].destroy();
        delete window.chartInstances[chartId];
    }
}

// Function to destroy all charts
window.destroyAllCharts = function() {
    Object.keys(window.chartInstances).forEach(chartId => {
        destroyChart(chartId);
    });
};

// Initialize Doughnut Chart (Brands)
window.initializeDoughnutChart = function(canvasId, labels, data) {
    console.log('Initializing Doughnut Chart:', canvasId, labels, data);
    
    // Destroy existing chart if any
    destroyChart(canvasId);
    
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error('Canvas element not found:', canvasId);
        return false;
    }
    
    const ctx = canvas.getContext('2d');
    
    try {
        window.chartInstances[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: [
                        '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0',
                        '#9966FF', '#FF9F40', '#FF6384', '#C9CBCF'
                    ],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'right',
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                            font: {
                                size: 12
                            }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.label + ': ' + context.parsed + ' cars';
                            }
                        }
                    }
                }
            }
        });
        console.log('Doughnut chart created successfully');
        return true;
    } catch (error) {
        console.error('Error creating doughnut chart:', error);
        return false;
    }
};

// Initialize Bar Chart (Categories)
window.initializeBarChart = function(canvasId, labels, data) {
    console.log('Initializing Bar Chart:', canvasId, labels, data);
    
    // Destroy existing chart if any
    destroyChart(canvasId);
    
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error('Canvas element not found:', canvasId);
        return false;
    }
    
    const ctx = canvas.getContext('2d');
    
    try {
        window.chartInstances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Listings',
                    data: data,
                    backgroundColor: [
                        '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0',
                        '#9966FF', '#FF9F40', '#FF6384', '#C9CBCF',
                        '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0'
                    ],
                    borderColor: [
                        '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0',
                        '#9966FF', '#FF9F40', '#FF6384', '#C9CBCF',
                        '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(0,0,0,0.05)'
                        },
                        ticks: {
                            font: {
                                size: 11
                            }
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            font: {
                                size: 11
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return 'Listings: ' + context.parsed.y;
                            }
                        }
                    }
                }
            }
        });
        console.log('Bar chart created successfully');
        return true;
    } catch (error) {
        console.error('Error creating bar chart:', error);
        return false;
    }
};

// Initialize Line Chart (Revenue)
window.initializeLineChart = function(canvasId, labels, data) {
    console.log('Initializing Line Chart:', canvasId, labels, data);
    
    // Destroy existing chart if any
    destroyChart(canvasId);
    
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error('Canvas element not found:', canvasId);
        return false;
    }
    
    const ctx = canvas.getContext('2d');
    
    try {
        window.chartInstances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Monthly Revenue (TND)',
                    data: data,
                    borderColor: '#10B981',
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#10B981',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointRadius: 5,
                    pointHoverRadius: 7
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(0,0,0,0.05)'
                        },
                        ticks: {
                            callback: function(value) {
                                return value.toLocaleString() + ' TND';
                            },
                            font: {
                                size: 11
                            }
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            font: {
                                size: 11
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return 'Revenue: ' + context.parsed.y.toLocaleString() + ' TND';
                            }
                        }
                    }
                },
                interaction: {
                    intersect: false,
                    mode: 'index'
                }
            }
        });
        console.log('Line chart created successfully');
        return true;
    } catch (error) {
        console.error('Error creating line chart:', error);
        return false;
    }
};

console.log('Analytics charts script loaded successfully');

// Initialize User Growth Area Chart
window.initializeUserGrowthChart = function(canvasId, labels, data) {
    console.log('Initializing User Growth Chart:', canvasId);
    destroyChart(canvasId);
    
    const canvas = document.getElementById(canvasId);
    if (!canvas) return false;
    
    const ctx = canvas.getContext('2d');
    
    try {
        window.chartInstances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'New Users',
                    data: data,
                    borderColor: '#3B82F6',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4,
                    pointRadius: 4,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.05)' },
                        ticks: { stepSize: 1 }
                    },
                    x: { grid: { display: false } }
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return 'Users: ' + context.parsed.y;
                            }
                        }
                    }
                }
            }
        });
        return true;
    } catch (error) {
        console.error('Error creating user growth chart:', error);
        return false;
    }
};

// Initialize Mixed Chart (Revenue vs Listings)
window.initializeComparisonChart = function(canvasId, labels, revenueData, listingsData) {
    console.log('Initializing Comparison Chart:', canvasId);
    destroyChart(canvasId);
    
    const canvas = document.getElementById(canvasId);
    if (!canvas) return false;
    
    const ctx = canvas.getContext('2d');
    
    try {
        window.chartInstances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Revenue (TND)',
                        data: revenueData,
                        backgroundColor: 'rgba(16, 185, 129, 0.6)',
                        borderColor: '#10B981',
                        borderWidth: 2,
                        yAxisID: 'y'
                    },
                    {
                        label: 'Listings',
                        data: listingsData,
                        type: 'line',
                        borderColor: '#EF4444',
                        backgroundColor: 'rgba(239, 68, 68, 0.1)',
                        borderWidth: 2,
                        fill: false,
                        tension: 0.4,
                        yAxisID: 'y1'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                scales: {
                    y: {
                        type: 'linear',
                        display: true,
                        position: 'left',
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Revenue (TND)'
                        }
                    },
                    y1: {
                        type: 'linear',
                        display: true,
                        position: 'right',
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Listings'
                        },
                        grid: {
                            drawOnChartArea: false
                        }
                    }
                }
            }
        });
        return true;
    } catch (error) {
        console.error('Error creating comparison chart:', error);
        return false;
    }
};

// Initialize Horizontal Bar Chart (Revenue Per Brand)
window.initializeRevenuePerBrandChart = function(canvasId, labels, data) {
    console.log('Initializing Revenue Per Brand Chart:', canvasId);
    destroyChart(canvasId);
    
    const canvas = document.getElementById(canvasId);
    if (!canvas) return false;
    
    const ctx = canvas.getContext('2d');
    
    try {
        window.chartInstances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Avg Revenue per Listing',
                    data: data,
                    backgroundColor: 'rgba(139, 92, 246, 0.6)',
                    borderColor: '#8B5CF6',
                    borderWidth: 1
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.05)' },
                        ticks: {
                            callback: function(value) {
                                return value.toLocaleString() + ' TND';
                            }
                        }
                    },
                    y: {
                        grid: { display: false }
                    }
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return 'Avg: ' + context.parsed.x.toLocaleString() + ' TND';
                            }
                        }
                    }
                }
            }
        });
        return true;
    } catch (error) {
        console.error('Error creating revenue per brand chart:', error);
        return false;
    }
};

// Initialize Radar Chart (Category Performance)
window.initializeCategoryRadarChart = function(canvasId, labels, data) {
    console.log('Initializing Category Radar Chart:', canvasId);
    destroyChart(canvasId);
    
    const canvas = document.getElementById(canvasId);
    if (!canvas) return false;
    
    const ctx = canvas.getContext('2d');
    
    try {
        window.chartInstances[canvasId] = new Chart(ctx, {
            type: 'radar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Performance Score',
                    data: data,
                    backgroundColor: 'rgba(102, 126, 234, 0.2)',
                    borderColor: '#667EEA',
                    borderWidth: 2,
                    pointBackgroundColor: '#667EEA',
                    pointBorderColor: '#fff',
                    pointHoverBackgroundColor: '#fff',
                    pointHoverBorderColor: '#667EEA'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    r: {
                        beginAtZero: true,
                        angleLines: {
                            color: 'rgba(0, 0, 0, 0.1)'
                        },
                        grid: {
                            color: 'rgba(0, 0, 0, 0.1)'
                        },
                        pointLabels: {
                            font: {
                                size: 11
                            }
                        },
                        ticks: {
                            backdropColor: 'transparent'
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    }
                }
            }
        });
        return true;
    } catch (error) {
        console.error('Error creating radar chart:', error);
        return false;
    }
};
