document.addEventListener("DOMContentLoaded", function () {
    const itemsPerPage = 5; // Number of items to display per page
    const ticketsContainer = document.getElementById('tickets');
    // Collect the ticket card elements
    const tickets = Array.from(ticketsContainer.querySelectorAll('#ticketContainer'));
    const pagination = document.querySelector(".pagination"); // Select the pagination container
    const totalPages = Math.ceil(tickets.length / itemsPerPage);

    function showPage(page) {
        const start = (page - 1) * itemsPerPage;
        const end = start + itemsPerPage;

        // Show only tickets for the current page
        tickets.forEach((ticket, index) => {
            ticket.style.display = index >= start && index < end ? "block" : "none";
        });

        // Update active pagination link
        pagination.querySelectorAll(".page-item.page-number").forEach((link, index) => {
            link.classList.toggle("active", index === page - 1);
        });
    }

    function createPagination() {
        const prevButton = pagination.querySelector(".page-item:first-child"); // Previous button
        const nextButton = pagination.querySelector(".page-item:last-child"); // Next button
        const maxVisiblePages = 10; // Limit of visible pages

        function renderPageNumbers(currentPage) {
            // Remove old page numbers
            pagination.querySelectorAll(".page-item.page-number").forEach((item) => item.remove());

            const startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
            const endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

            // Adjust startPage if endPage is near the total
            const adjustedStartPage = Math.max(1, endPage - maxVisiblePages + 1);

            // Create new page numbers
            for (let i = adjustedStartPage; i <= endPage; i++) {
                const li = document.createElement("li");
                li.className = "page-item page-number" + (i === currentPage ? " active" : "");
                li.innerHTML = `<a class="page-link" href="#">${i}</a>`;
                li.addEventListener("click", function (e) {
                    e.preventDefault();
                    showPage(i);
                    renderPageNumbers(i);
                });

                // Insert before the Next button
                pagination.insertBefore(li, nextButton);
            }

            // Add ellipses if needed
            if (startPage > 1) {
                const li = document.createElement("li");
                li.className = "page-item page-number";
                li.innerHTML = `<span class="page-link">...</span>`;
                pagination.insertBefore(li, pagination.querySelector(".page-item.page-number"));
            }

            if (endPage < totalPages) {
                const li = document.createElement("li");
                li.className = "page-item page-number";
                li.innerHTML = `<span class="page-link">...</span>`;
                pagination.insertBefore(li, nextButton);
            }
        }

        // Add event listeners for Previous and Next buttons
        prevButton.addEventListener("click", function (e) {
            e.preventDefault();
            const currentPage = getActivePage();
            if (currentPage > 1) {
                showPage(currentPage - 1);
                renderPageNumbers(currentPage - 1);
            }
        });

        nextButton.addEventListener("click", function (e) {
            e.preventDefault();
            const currentPage = getActivePage();
            if (currentPage < totalPages) {
                showPage(currentPage + 1);
                renderPageNumbers(currentPage + 1);
            }
        });

        // Initial rendering of page numbers
        renderPageNumbers(1);
    }

    function getActivePage() {
        const activePage = pagination.querySelector(".page-item.page-number.active");
        return activePage ? +activePage.textContent : 1;
    }

    // Initialize pagination
    if (tickets.length > 0) {
        createPagination();
        showPage(1); // Show the first page
    } else {
        pagination.style.display = "none"; // Hide pagination if no tickets
    }
});

