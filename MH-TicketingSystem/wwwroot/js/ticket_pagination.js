document.addEventListener("DOMContentLoaded", function () {
    const itemsPerPage = 5; // Number of items to display per page
    const tickets = Array.from(document.querySelectorAll(".card-body > a")); // Select all ticket links
    const pagination = document.querySelector(".pagination"); // Select the pagination container
    const totalPages = Math.ceil(tickets.length / itemsPerPage);

    function showPage(page) {
        const start = (page - 1) * itemsPerPage;
        const end = start + itemsPerPage;

        // Hide all tickets
        tickets.forEach((ticket, index) => {
            ticket.style.display = index >= start && index < end ? "block" : "none";
        });

        // Update active pagination link
        const pageLinks = pagination.querySelectorAll(".page-item");
        pageLinks.forEach((link, index) => {
            if (index === page) {
                link.classList.add("active");
            } else {
                link.classList.remove("active");
            }
        });
    }

    function createPagination() {
        const nextButton = pagination.querySelector('.page-item:last-child'); // Select the last page-item as the "Next" button
        const prevButton = pagination.querySelector('.page-item:first-child'); // Select the first page-item as the "Previous" button

        // Remove existing page links (except Next and Previous)
        const pageLinks = Array.from(pagination.querySelectorAll(".page-item"));
        pageLinks.forEach((link, index) => {
            if (link !== prevButton && link !== nextButton) {
                link.remove();
            }
        });

        // Create new page links
        for (let i = 1; i <= totalPages; i++) {
            const li = document.createElement("li");
            li.className = "page-item";
            li.innerHTML = `<a class="page-link" href="#">${i}</a>`;
            li.addEventListener("click", function (e) {
                e.preventDefault();
                showPage(i);
            });

            // Insert the new page link before the "Next" button
            pagination.insertBefore(li, nextButton);
        }

        // Add event listeners for Next and Previous
        nextButton.addEventListener("click", function (e) {
            e.preventDefault();
            const currentPage = +pagination.querySelector(".page-item.active a")?.textContent || 1;
            if (currentPage < totalPages) {
                showPage(currentPage + 1);
            }
        });

        prevButton.addEventListener("click", function (e) {
            e.preventDefault();
            const currentPage = +pagination.querySelector(".page-item.active a")?.textContent || 1;
            if (currentPage > 1) {
                showPage(currentPage - 1);
            }
        });
    }


    // Initialize pagination and show the first page
    createPagination();
    showPage(1);
});