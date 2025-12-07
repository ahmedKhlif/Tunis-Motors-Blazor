// Analytics Export Functions
window.downloadAnalyticsReport = function(format, jsonData) {
    try {
        const data = JSON.parse(jsonData);
        
        if (format === 'pdf') {
            downloadPDFReport(data);
        } else if (format === 'excel') {
            downloadExcelReport(data);
        }
    } catch (error) {
        console.error('Error in downloadAnalyticsReport:', error);
    }
};

function downloadPDFReport(data) {
    // Create a printable HTML document
    const printWindow = window.open('', '_blank');
    
    const htmlContent = `
        <!DOCTYPE html>
        <html>
        <head>
            <title>Analytics Report - ${data.GeneratedDate}</title>
            <style>
                body {
                    font-family: Arial, sans-serif;
                    padding: 40px;
                    color: #333;
                }
                .header {
                    text-align: center;
                    margin-bottom: 30px;
                    border-bottom: 3px solid #D10024;
                    padding-bottom: 20px;
                }
                .header h1 {
                    color: #D10024;
                    margin: 0;
                }
                .header p {
                    color: #666;
                    margin: 5px 0;
                }
                .section {
                    margin: 30px 0;
                }
                .section h2 {
                    color: #D10024;
                    border-bottom: 2px solid #eee;
                    padding-bottom: 10px;
                    margin-bottom: 20px;
                }
                .metrics {
                    display: grid;
                    grid-template-columns: repeat(2, 1fr);
                    gap: 20px;
                    margin: 20px 0;
                }
                .metric-card {
                    border: 1px solid #ddd;
                    padding: 20px;
                    border-radius: 8px;
                    background: #f9f9f9;
                }
                .metric-card h3 {
                    margin: 0 0 10px 0;
                    color: #666;
                    font-size: 14px;
                    font-weight: normal;
                }
                .metric-card .value {
                    font-size: 28px;
                    font-weight: bold;
                    color: #D10024;
                }
                .metric-card .sub-value {
                    font-size: 12px;
                    color: #999;
                    margin-top: 5px;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                    margin: 20px 0;
                }
                table th {
                    background: #D10024;
                    color: white;
                    padding: 12px;
                    text-align: left;
                }
                table td {
                    padding: 10px 12px;
                    border-bottom: 1px solid #eee;
                }
                table tr:nth-child(even) {
                    background: #f9f9f9;
                }
                .footer {
                    margin-top: 50px;
                    text-align: center;
                    color: #999;
                    font-size: 12px;
                    border-top: 1px solid #eee;
                    padding-top: 20px;
                }
                @media print {
                    .no-print { display: none; }
                }
            </style>
        </head>
        <body>
            <div class="header">
                <h1><i class="fa fa-car"></i> Tunisia Motors - Analytics Report</h1>
                <p>Generated on ${data.GeneratedDate}</p>
            </div>

            <div class="section">
                <h2><i class="fa fa-chart-bar"></i> Key Performance Indicators</h2>
                <div class="metrics">
                    <div class="metric-card">
                        <h3>Total Revenue</h3>
                        <div class="value">${data.TotalRevenue} TND</div>
                    </div>
                    <div class="metric-card">
                        <h3>Monthly Revenue</h3>
                        <div class="value">${data.MonthlyRevenue} TND</div>
                        <div class="sub-value">Current month</div>
                    </div>
                    <div class="metric-card">
                        <h3>Average Monthly Revenue</h3>
                        <div class="value">${data.AverageMonthlyRevenue} TND</div>
                        <div class="sub-value">Last 12 months</div>
                    </div>
                    <div class="metric-card">
                        <h3>Revenue Growth</h3>
                        <div class="value">${data.RevenueGrowth}</div>
                        <div class="sub-value">vs last month</div>
                    </div>
                </div>
            </div>

            <div class="section">
                <h2><i class="fa fa-trophy"></i> Top Performers</h2>
                <div class="metrics">
                    <div class="metric-card">
                        <h3>Best Selling Brand</h3>
                        <div class="value">${data.TopBrand}</div>
                        <div class="sub-value">${data.TopBrandCount} listings</div>
                    </div>
                    <div class="metric-card">
                        <h3>Top Category</h3>
                        <div class="value">${data.TopCategory}</div>
                        <div class="sub-value">${data.TopCategoryCount} listings</div>
                    </div>
                    <div class="metric-card">
                        <h3>Total Listings</h3>
                        <div class="value">${data.TotalListings}</div>
                    </div>
                </div>
            </div>

            ${data.Brands && data.Brands.length > 0 ? `
            <div class="section">
                <h2><i class="fa fa-car"></i> Popular Brands</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Rank</th>
                            <th>Brand</th>
                            <th>Listings</th>
                            <th>Market Share</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${data.Brands.map((brand, index) => {
                            const total = data.Brands.reduce((sum, b) => sum + b.Count, 0);
                            const share = ((brand.Count / total) * 100).toFixed(1);
                            return `
                                <tr>
                                    <td>${index + 1}</td>
                                    <td>${brand.Brand}</td>
                                    <td>${brand.Count}</td>
                                    <td>${share}%</td>
                                </tr>
                            `;
                        }).join('')}
                    </tbody>
                </table>
            </div>
            ` : ''}

            ${data.Categories && data.Categories.length > 0 ? `
            <div class="section">
                <h2><i class="fa fa-tags"></i> Category Distribution</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Rank</th>
                            <th>Category</th>
                            <th>Listings</th>
                            <th>Percentage</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${data.Categories.map((category, index) => {
                            const total = data.Categories.reduce((sum, c) => sum + c.Count, 0);
                            const percentage = ((category.Count / total) * 100).toFixed(1);
                            return `
                                <tr>
                                    <td>${index + 1}</td>
                                    <td>${category.Category}</td>
                                    <td>${category.Count}</td>
                                    <td>${percentage}%</td>
                                </tr>
                            `;
                        }).join('')}
                    </tbody>
                </table>
            </div>
            ` : ''}

            ${data.MonthlyRevenueData && data.MonthlyRevenueData.length > 0 ? `
            <div class="section">
                <h2><i class="fa fa-dollar-sign"></i> Monthly Revenue Trends</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Month</th>
                            <th>Revenue (TND)</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${data.MonthlyRevenueData.map(month => `
                            <tr>
                                <td>${month.Month}</td>
                                <td>${month.Revenue.toLocaleString('en-US', {minimumFractionDigits: 2, maximumFractionDigits: 2})}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
            ` : ''}

            <div class="footer">
                <p>© ${new Date().getFullYear()} Tunisia Motors. All rights reserved.</p>
                <p>This report is confidential and intended for authorized personnel only.</p>
            </div>

            <div class="no-print" style="text-align: center; margin-top: 30px;">
                <button onclick="window.print()" style="background: #D10024; color: white; border: none; padding: 12px 30px; font-size: 16px; border-radius: 5px; cursor: pointer;">
                    Print Report
                </button>
                <button onclick="window.close()" style="background: #666; color: white; border: none; padding: 12px 30px; font-size: 16px; border-radius: 5px; cursor: pointer; margin-left: 10px;">
                    Close
                </button>
            </div>
        </body>
        </html>
    `;
    
    printWindow.document.write(htmlContent);
    printWindow.document.close();
}

function downloadExcelReport(data) {
    // Create CSV content
    let csvContent = "Tunisia Motors - Analytics Report\n";
    csvContent += `Generated: ${data.Summary.GeneratedDate}\n\n`;
    
    // Summary Section
    csvContent += "SUMMARY\n";
    csvContent += "Metric,Value\n";
    csvContent += `Total Revenue,${data.Summary.TotalRevenue} TND\n`;
    csvContent += `Monthly Revenue,${data.Summary.MonthlyRevenue} TND\n`;
    csvContent += `Average Monthly Revenue,${data.Summary.AverageMonthlyRevenue} TND\n`;
    csvContent += `Revenue Growth,${data.Summary.RevenueGrowth}%\n`;
    csvContent += `Total Listings,${data.Summary.TotalListings}\n`;
    csvContent += `Total Brands,${data.Summary.TotalBrands}\n`;
    csvContent += `Total Categories,${data.Summary.TotalCategories}\n\n`;
    
    // Top Performers
    csvContent += "TOP PERFORMERS\n";
    csvContent += "Metric,Value\n";
    csvContent += `Best Selling Brand,${data.TopPerformers.TopBrand}\n`;
    csvContent += `Top Brand Listings,${data.TopPerformers.TopBrandCount}\n`;
    csvContent += `Top Category,${data.TopPerformers.TopCategory}\n`;
    csvContent += `Top Category Listings,${data.TopPerformers.TopCategoryCount}\n\n`;
    
    // Brands
    if (data.Brands && data.Brands.length > 0) {
        csvContent += "POPULAR BRANDS\n";
        csvContent += "Brand,Listings\n";
        data.Brands.forEach(brand => {
            csvContent += `${brand.Brand},${brand.Count}\n`;
        });
        csvContent += "\n";
    }
    
    // Categories
    if (data.Categories && data.Categories.length > 0) {
        csvContent += "CATEGORY DISTRIBUTION\n";
        csvContent += "Category,Listings\n";
        data.Categories.forEach(category => {
            csvContent += `${category.Category},${category.Count}\n`;
        });
        csvContent += "\n";
    }
    
    // Monthly Revenue
    if (data.MonthlyRevenueData && data.MonthlyRevenueData.length > 0) {
        csvContent += "MONTHLY REVENUE\n";
        csvContent += "Month,Revenue (TND)\n";
        data.MonthlyRevenueData.forEach(month => {
            csvContent += `${month.Month},${month.Revenue}\n`;
        });
    }
    
    // Create blob and download
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    
    link.setAttribute('href', url);
    link.setAttribute('download', `Analytics_Report_${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

console.log('Analytics export script loaded successfully');
