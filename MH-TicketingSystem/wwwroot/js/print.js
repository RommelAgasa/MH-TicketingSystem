document.addEventListener('DOMContentLoaded', function () {
    // Attach click event listener to the Print button
    document.getElementById('printButton').addEventListener('click', printTable);

    function printTable() {
        var printContents = document.getElementById('ticketTable').outerHTML;

        // Open a new window for printing
        var printWindow = window.open('', '', 'height=600,width=800');

        // Create the header content
        var headerContent = `
            <header>
                <div style="display: flex; justify-content: space-between; align-items: center;">
                    <h3>Ticket Report</h3>
                    <img src="/img/02_left_metrohealthlogo_blue.png" alt="Company Logo" style="width: 100px; height: auto;" />
                </div>
            </header>
        `;

        // Build the complete HTML content with header and table
        var completeHtml = `
            <!DOCTYPE html>
            <html>
            <head>
                <title>Metro Health Ticket Report</title>
                <style>
                    table { width: 100%; border-collapse: collapse; }
                    th, td { border: 1px solid #000; padding: 8px; text-align: left; }
                </style>
            </head>
            <body>
                ${headerContent}
                ${printContents}
            </body>
            </html>
        `;

        // Write the complete HTML content to the print window
        printWindow.document.write(completeHtml);
        printWindow.document.close();

        // Wait for the new window to load completely before printing
        printWindow.onload = function () {
            printWindow.focus();
            printWindow.print();
            printWindow.close();
        };
    }
});


