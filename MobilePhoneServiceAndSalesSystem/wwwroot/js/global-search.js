(function () {
    let searchModal = null;
    let searchInput = null;
    let searchResults = null;
    let activeIndex = -1;
    let debounceTimer = null;
    let currentResults = [];

    // Initialize search once DOM is fully loaded
    document.addEventListener("DOMContentLoaded", function () {
        const searchBtn = document.getElementById("global-search-btn");
        const modalEl = document.getElementById("globalSearchModal");
        
        if (!modalEl) return;

        searchModal = new bootstrap.Modal(modalEl);
        searchInput = document.getElementById("global-search-input");
        searchResults = document.getElementById("global-search-results");

        if (searchBtn) {
            searchBtn.addEventListener("click", function (e) {
                e.preventDefault();
                searchModal.show();
            });
        }

        document.addEventListener("keydown", function (e) {
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
                e.preventDefault();
                searchModal.show();
            }
        });

        modalEl.addEventListener("shown.bs.modal", function () {
            searchInput.focus();
            loadResults(""); // Load default navigation shortcuts
        });

        modalEl.addEventListener("hidden.bs.modal", function () {
            searchInput.value = "";
            searchResults.innerHTML = "";
            activeIndex = -1;
            currentResults = [];
        });

        searchInput.addEventListener("input", function () {
            clearTimeout(debounceTimer);
            const query = searchInput.value.trim();

            debounceTimer = setTimeout(function () {
                loadResults(query);
            }, 200); // 200ms debounce
        });

        searchInput.addEventListener("keydown", function (e) {
            const items = searchResults.querySelectorAll(".global-search-item");
            if (items.length === 0) return;

            if (e.key === "ArrowDown") {
                e.preventDefault();
                activeIndex++;
                if (activeIndex >= items.length) activeIndex = 0;
                updateActiveItem(items);
            } else if (e.key === "ArrowUp") {
                e.preventDefault();
                activeIndex--;
                if (activeIndex < 0) activeIndex = items.length - 1;
                updateActiveItem(items);
            } else if (e.key === "Enter") {
                e.preventDefault();
                if (activeIndex >= 0 && activeIndex < items.length) {
                    items[activeIndex].click();
                }
            }
        });
    });

    function loadResults(query) {
        searchResults.innerHTML = `
            <div class="text-center py-4 opacity-50">
                <div class="spinner-border spinner-border-sm text-primary me-2" role="status"></div>
                <span>Loading...</span>
            </div>
        `;

        fetch(`/api/search?q=${encodeURIComponent(query)}`)
            .then(response => {
                if (!response.ok) throw new Error("Network response error");
                return response.json();
            })
            .then(data => {
                currentResults = data;
                renderResults(data);
                activeIndex = -1; // Reset active selection index
            })
            .catch(error => {
                console.error("Search error:", error);
                searchResults.innerHTML = `
                    <div class="text-center py-3 text-danger">
                        <i class="bi bi-exclamation-triangle-fill me-2"></i>
                        <span>Error loading search results. Please try again.</span>
                    </div>
                `;
            });
    }

    function renderResults(results) {
        if (!results || results.length === 0) {
            searchResults.innerHTML = `
                <div class="text-center py-4 text-secondary">
                    <i class="bi bi-search display-6 d-block mb-2 opacity-25"></i>
                    <span>No results found. Try another query.</span>
                </div>
            `;
            return;
        }

        const groups = {};
        results.forEach(item => {
            if (!groups[item.category]) {
                groups[item.category] = [];
            }
            groups[item.category].push(item);
        });

        let html = "";
        
        const categories = Object.keys(groups).sort((a, b) => {
            if (a === "Navigation") return -1;
            if (b === "Navigation") return 1;
            return a.localeCompare(b);
        });

        categories.forEach(category => {
            html += `<div class="global-search-category-header">${category}</div>`;
            groups[category].forEach(item => {
                html += `
                    <a href="${item.url}" class="global-search-item list-group-item list-group-item-action">
                        <div class="global-search-item-icon">
                            <i class="bi ${item.icon || 'bi-link-45deg'}"></i>
                        </div>
                        <div class="global-search-item-info">
                            <span class="global-search-item-title">${escapeHtml(item.title)}</span>
                            <span class="global-search-item-desc">${escapeHtml(item.description)}</span>
                        </div>
                        <span class="global-search-item-badge">${escapeHtml(item.category)}</span>
                    </a>
                `;
            });
        });

        searchResults.innerHTML = html;
    }

    function updateActiveItem(items) {
        items.forEach((item, index) => {
            if (index === activeIndex) {
                item.classList.add("active");
                item.scrollIntoView({ block: "nearest" });
            } else {
                item.classList.remove("active");
            }
        });
    }

    function escapeHtml(text) {
        if (!text) return "";
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, function(m) { return map[m]; });
    }
})();
