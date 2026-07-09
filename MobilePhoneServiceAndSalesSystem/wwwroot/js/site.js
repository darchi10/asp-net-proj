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
						animateListItems(customerResults);
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
						animateListItems(technicianResults);
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
						animateListItems(productResults);
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
						animateListItems(sparePartResults);
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
						animateListItems(phoneResults);
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
						animateListItems(orderResults);
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
						animateListItems(repairJobResults);
					});
			}, 250);
		});
	}

	// Advanced JavaScript Animations using Web Animations API
	function animateListItems(container) {
		if (!container) return;
		const items = container.querySelectorAll(".col");
		items.forEach((item, index) => {
			item.animate(
				[
					{ opacity: 0, transform: "translateY(20px)" },
					{ opacity: 1, transform: "translateY(0)" }
				],
				{
					duration: 400,
					delay: index * 50,
					easing: "ease-out",
					fill: "forwards"
				}
			);
		});
	}

	// Initial animations for cards on load
	document.querySelectorAll(".retail-card, .card").forEach((card, index) => {
		card.animate(
			[
				{ opacity: 0, transform: "scale(0.95) translateY(10px)" },
				{ opacity: 1, transform: "scale(1) translateY(0)" }
			],
			{
				duration: 500,
				delay: index * 100,
				easing: "cubic-bezier(0.34, 1.56, 0.64, 1)",
				fill: "forwards"
			}
		);
	});

	// DatePicker Initialization
	function initDatePickers() {
		document.querySelectorAll(".datetime-picker-wrapper").forEach(wrapper => {
			const input = wrapper.querySelector(".datetime-picker-input");
			const hidden = wrapper.querySelector(".datetime-picker-value");
			const culture = wrapper.dataset.culture;
			const includeTime = wrapper.dataset.includeTime === "true";
			const format = wrapper.dataset.format;

			flatpickr(input, {
				locale: culture,
				enableTime: includeTime,
				dateFormat: format,
				time_24hr: true,
				onChange: function (selectedDates, dateStr, instance) {
					if (selectedDates.length > 0) {
						// Set ISO string to hidden input for server-side binding
						const date = selectedDates[0];
						// Correct for timezone offset to send UTC or local as expected
						const offset = date.getTimezoneOffset() * 60000;
						const localISOTime = (new Date(date - offset)).toISOString().slice(0, -1);
						hidden.value = localISOTime;
					} else {
						hidden.value = "";
					}
					hidden.dispatchEvent(new Event("change", { bubbles: true }));
				}
			});
		});
	}

	initDatePickers();
})();

// =================================
// MOBILE RESPONSIVE ENHANCEMENTS
// =================================

(function() {
	// Auto-collapse navbar on mobile after link click
	const navbarToggler = document.querySelector('.navbar-toggler');
	const navbarCollapse = document.querySelector('.navbar-collapse');
	
	if (navbarToggler && navbarCollapse) {
		document.querySelectorAll('.navbar-nav .nav-link').forEach(link => {
			link.addEventListener('click', function() {
				const opensDropdown = this.classList.contains('dropdown-toggle') || this.getAttribute('data-bs-toggle') === 'dropdown';
				const isPlaceholderLink = this.getAttribute('href') === '#';

				if (!opensDropdown && !isPlaceholderLink && window.innerWidth < 576 && navbarCollapse.classList.contains('show')) {
					navbarToggler.click();
				}
			});
		});
	}

	// Add visual feedback for touch events
	document.querySelectorAll('.card, .btn, .nav-link').forEach(element => {
		element.addEventListener('touchstart', function() {
			this.style.opacity = '0.8';
		}, { passive: true });

		element.addEventListener('touchend', function() {
			setTimeout(() => {
				this.style.opacity = '';
			}, 150);
		}, { passive: true });
	});

	// Smooth scroll behavior for anchor links
	document.querySelectorAll('a[href^="#"]').forEach(anchor => {
		anchor.addEventListener('click', function(e) {
			const targetId = this.getAttribute('href');
			if (targetId !== '#' && targetId !== '#!') {
				const target = document.querySelector(targetId);
				if (target) {
					e.preventDefault();
					target.scrollIntoView({ behavior: 'smooth', block: 'start' });
				}
			}
		});
	});

	// Detect viewport size changes and adjust layout
	let resizeTimer;
	window.addEventListener('resize', function() {
		clearTimeout(resizeTimer);
		resizeTimer = setTimeout(function() {
			// Re-trigger any layout-dependent calculations
			document.body.classList.toggle('mobile', window.innerWidth < 768);
		}, 250);
	});

	// Initial check
	document.body.classList.toggle('mobile', window.innerWidth < 768);

	// Handle orientation change
	window.addEventListener('orientationchange', function() {
		setTimeout(function() {
			// Force repaint after orientation change
			document.body.style.display = 'none';
			document.body.offsetHeight; // Force reflow
			document.body.style.display = '';
		}, 100);
	});

	// Improve form input focus on mobile
	if ('ontouchstart' in window) {
		document.querySelectorAll('input, textarea, select').forEach(input => {
			input.addEventListener('focus', function() {
				// Scroll input into view on mobile
				setTimeout(() => {
					this.scrollIntoView({ behavior: 'smooth', block: 'center' });
				}, 300);
			});
		});
	}

	// Add pull-to-refresh indicator (visual only)
	let touchStartY = 0;
	let touchEndY = 0;

	document.addEventListener('touchstart', function(e) {
		touchStartY = e.touches[0].clientY;
	}, { passive: true });

	document.addEventListener('touchmove', function(e) {
		touchEndY = e.touches[0].clientY;
	}, { passive: true });

	// Optimize scroll performance
	let ticking = false;
	document.addEventListener('scroll', function() {
		if (!ticking) {
			window.requestAnimationFrame(function() {
				// Add scroll-based effects here if needed
				const navbar = document.querySelector('.navbar');
				if (navbar) {
					navbar.classList.toggle('scrolled', window.scrollY > 50);
				}
				ticking = false;
			});
			ticking = true;
		}
	}, { passive: true });

})();


// iOS Safari viewport height fix
function setVH() {
	const vh = window.innerHeight * 0.01;
	document.documentElement.style.setProperty('--vh', `${vh}px`);
}

setVH();
window.addEventListener('resize', setVH);
window.addEventListener('orientationchange', function() {
	setTimeout(setVH, 100);
});
