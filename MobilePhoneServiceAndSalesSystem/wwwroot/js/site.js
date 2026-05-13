// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(function () {
	if (window.jQuery && jQuery.validator) {
		jQuery.validator.setDefaults({
			ignore: ":hidden:not(.autocomplete-id)"
		});
	}

	function initAutocomplete(root) {
		const input = root.querySelector(".autocomplete-input");
		const hidden = root.querySelector(".autocomplete-id");
		const menu = root.querySelector(".autocomplete-menu");
		const endpoint = root.dataset.endpoint;
		const minLength = parseInt(root.dataset.minLength || "2", 10);
		let activeRequest = null;

		function clearMenu() {
			menu.innerHTML = "";
		}

		function renderItems(items) {
			if (!items.length) {
				menu.innerHTML = "<div class=\"autocomplete-empty\">No results</div>";
				return;
			}

			menu.innerHTML = items
				.map(item => `
					<button type="button" class="autocomplete-item" data-id="${item.id}" data-text="${item.text}">
						${item.text}
					</button>
				`)
				.join("");
		}

		input.addEventListener("input", function () {
			const term = input.value.trim();
			hidden.value = "";
			clearMenu();

			if (term.length < minLength) {
				return;
			}

			if (activeRequest) {
				activeRequest.abort();
			}

			const controller = new AbortController();
			activeRequest = controller;

			fetch(`${endpoint}?term=${encodeURIComponent(term)}`, { signal: controller.signal })
				.then(response => response.ok ? response.json() : [])
				.then(items => {
					if (controller.signal.aborted) {
						return;
					}
					renderItems(Array.isArray(items) ? items : []);
				})
				.catch(() => {
					if (controller.signal.aborted) {
						return;
					}
					clearMenu();
				});
		});

		menu.addEventListener("click", function (event) {
			const item = event.target.closest(".autocomplete-item");
			if (!item) {
				return;
			}

			hidden.value = item.dataset.id;
			input.value = item.dataset.text;
			clearMenu();
			input.dispatchEvent(new Event("change", { bubbles: true }));
		});

		document.addEventListener("click", function (event) {
			if (!root.contains(event.target)) {
				clearMenu();
			}
		});
	}

	document.querySelectorAll(".autocomplete").forEach(initAutocomplete);

	const customerSearchInput = document.getElementById("customer-search");
	const customerResults = document.getElementById("customer-results");
	if (customerSearchInput && customerResults) {
		let timer = null;
		customerSearchInput.addEventListener("input", function () {
			clearTimeout(timer);
			const term = customerSearchInput.value.trim();
			timer = setTimeout(function () {
				fetch(`/customers/search-list?term=${encodeURIComponent(term)}`)
					.then(response => response.ok ? response.text() : "")
					.then(html => {
						customerResults.innerHTML = html;
					});
			}, 250);
		});
	}

	const technicianSearchInput = document.getElementById("technician-search");
	const technicianResults = document.getElementById("technician-results");
	if (technicianSearchInput && technicianResults) {
		let timer = null;
		technicianSearchInput.addEventListener("input", function () {
			clearTimeout(timer);
			const term = technicianSearchInput.value.trim();
			timer = setTimeout(function () {
				fetch(`/technicians/search-list?term=${encodeURIComponent(term)}`)
					.then(response => response.ok ? response.text() : "")
					.then(html => {
						technicianResults.innerHTML = html;
					});
			}, 250);
		});
	}

	const productSearchInput = document.getElementById("product-search");
	const productResults = document.getElementById("product-results");
	if (productSearchInput && productResults) {
		let timer = null;
		productSearchInput.addEventListener("input", function () {
			clearTimeout(timer);
			const term = productSearchInput.value.trim();
			timer = setTimeout(function () {
				fetch(`/products/search-list?term=${encodeURIComponent(term)}`)
					.then(response => response.ok ? response.text() : "")
					.then(html => {
						productResults.innerHTML = html;
					});
			}, 250);
		});
	}

	const sparePartSearchInput = document.getElementById("spare-part-search");
	const sparePartResults = document.getElementById("spare-part-results");
	if (sparePartSearchInput && sparePartResults) {
		let timer = null;
		sparePartSearchInput.addEventListener("input", function () {
			clearTimeout(timer);
			const term = sparePartSearchInput.value.trim();
			timer = setTimeout(function () {
				fetch(`/spare-parts/search-list?term=${encodeURIComponent(term)}`)
					.then(response => response.ok ? response.text() : "")
					.then(html => {
						sparePartResults.innerHTML = html;
					});
			}, 250);
		});
	}

	const phoneSearchInput = document.getElementById("phone-search");
	const phoneResults = document.getElementById("phone-results");
	if (phoneSearchInput && phoneResults) {
		let timer = null;
		phoneSearchInput.addEventListener("input", function () {
			clearTimeout(timer);
			const term = phoneSearchInput.value.trim();
			timer = setTimeout(function () {
				fetch(`/phones/search-list?term=${encodeURIComponent(term)}`)
					.then(response => response.ok ? response.text() : "")
					.then(html => {
						phoneResults.innerHTML = html;
					});
			}, 250);
		});
	}

	const orderSearchInput = document.getElementById("order-search");
	const orderResults = document.getElementById("order-results");
	if (orderSearchInput && orderResults) {
		let timer = null;
		orderSearchInput.addEventListener("input", function () {
			clearTimeout(timer);
			const term = orderSearchInput.value.trim();
			timer = setTimeout(function () {
				fetch(`/orders/search-list?term=${encodeURIComponent(term)}`)
					.then(response => response.ok ? response.text() : "")
					.then(html => {
						orderResults.innerHTML = html;
					});
			}, 250);
		});
	}

	const repairJobSearchInput = document.getElementById("repair-job-search");
	const repairJobResults = document.getElementById("repair-job-results");
	if (repairJobSearchInput && repairJobResults) {
		let timer = null;
		repairJobSearchInput.addEventListener("input", function () {
			clearTimeout(timer);
			const term = repairJobSearchInput.value.trim();
			timer = setTimeout(function () {
				fetch(`/repair-jobs/search-list?term=${encodeURIComponent(term)}`)
					.then(response => response.ok ? response.text() : "")
					.then(html => {
						repairJobResults.innerHTML = html;
					});
			}, 250);
		});
	}
})();
