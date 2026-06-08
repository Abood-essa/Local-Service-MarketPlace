// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Self-executing theme initialization to run as early as possible
(function () {
    const savedTheme = localStorage.getItem('khidmati-theme');
    if (savedTheme) {
        document.documentElement.setAttribute('data-theme', savedTheme);
    } else {
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        document.documentElement.setAttribute('data-theme', prefersDark ? 'dark' : 'light');
    }
})();

// Bind theme toggle functions
document.addEventListener('DOMContentLoaded', function () {
    const toggles = document.querySelectorAll('.theme-toggle');

    function updateToggleUI(theme) {
        toggles.forEach(btn => {
            const icon = btn.querySelector('i');
            if (icon) {
                if (theme === 'dark') {
                    icon.className = 'fa-solid fa-sun';
                } else {
                    icon.className = 'fa-solid fa-moon';
                }
            }
            // If it is a checkbox/switch toggle
            const checkbox = btn.querySelector('input[type="checkbox"]') || (btn.tagName === 'INPUT' && btn.type === 'checkbox' ? btn : null);
            if (checkbox) {
                checkbox.checked = (theme === 'dark');
            }
        });
    }

    // Initialize toggle icons/checkboxes on page load
    const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
    updateToggleUI(currentTheme);

    // Click handler for toggle buttons
    window.toggleKhidmatiTheme = function () {
        const current = document.documentElement.getAttribute('data-theme') || 'light';
        const nextTheme = current === 'dark' ? 'light' : 'dark';
        
        document.documentElement.setAttribute('data-theme', nextTheme);
        localStorage.setItem('khidmati-theme', nextTheme);
        updateToggleUI(nextTheme);
    };

    // Attach click listeners to all matching items
    toggles.forEach(btn => {
        if (btn.tagName === 'INPUT' && btn.type === 'checkbox') {
            btn.addEventListener('change', window.toggleKhidmatiTheme);
        } else {
            btn.addEventListener('click', function (e) {
                // If it is a label wrapper containing checkbox, let the checkbox change handle it
                if (btn.querySelector('input[type="checkbox"]')) return;
                e.preventDefault();
                window.toggleKhidmatiTheme();
            });
        }
    });

    // Scroll-to-top functionality
    const scrollBtn = document.getElementById('scrollTopBtn');
    if (scrollBtn) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 300) {
                scrollBtn.style.display = 'flex';
            } else {
                scrollBtn.style.display = 'none';
            }
        });
        scrollBtn.addEventListener('click', function () {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
    }

    // ==========================================
    // Table Filtering Feature
    // ==========================================

    // Helper: Toggle empty table message
    function toggleEmptyTableMessage(tableId, rows) {
        const tableBody = document.querySelector(`#${tableId} tbody`);
        if (!tableBody) return;

        let visibleCount = 0;
        rows.forEach(r => {
            if (!r.classList.contains('d-none')) {
                visibleCount++;
            }
        });

        let emptyRow = document.getElementById(`${tableId}-empty-row`);
        if (visibleCount === 0) {
            if (!emptyRow) {
                const colCount = document.querySelectorAll(`#${tableId} thead th`).length;
                emptyRow = document.createElement('tr');
                emptyRow.id = `${tableId}-empty-row`;
                emptyRow.innerHTML = `
                    <td colspan="${colCount}" class="text-center py-4 text-muted">
                        <div class="my-3">
                            <i class="fas fa-search fa-2x mb-3 text-muted opacity-50"></i>
                            <p class="mb-0 fw-500">No matching records found</p>
                            <small class="text-muted">Try adjusting your filters or search terms.</small>
                        </div>
                    </td>
                `;
                tableBody.appendChild(emptyRow);
            }
        } else {
            if (emptyRow) {
                emptyRow.remove();
            }
        }
    }

    // 1. Users Page Filter
    const userSearchInput = document.getElementById('userSearchInput');
    const userRoleFilter = document.getElementById('userRoleFilter');
    const userStatusFilter = document.getElementById('userStatusFilter');
    const userVerificationFilter = document.getElementById('userVerificationFilter');

    if (userSearchInput || userRoleFilter || userStatusFilter || userVerificationFilter) {
        const filterUsersTable = function() {
            const searchVal = userSearchInput ? userSearchInput.value.toLowerCase().trim() : '';
            const roleVal = userRoleFilter ? userRoleFilter.value : '';
            const statusVal = userStatusFilter ? userStatusFilter.value : '';
            const verificationVal = userVerificationFilter ? userVerificationFilter.value : '';

            const rows = document.querySelectorAll('.user-row');
            rows.forEach(row => {
                const name = row.getAttribute('data-name') || '';
                const email = row.getAttribute('data-email') || '';
                const phone = row.getAttribute('data-phone') || '';
                const roles = (row.getAttribute('data-roles') || '').split(',');
                const status = row.getAttribute('data-status') || '';
                const verification = row.getAttribute('data-verification') || '';

                const matchesSearch = !searchVal || 
                    name.includes(searchVal) || 
                    email.includes(searchVal) || 
                    phone.includes(searchVal);

                const matchesRole = !roleVal || roles.includes(roleVal);
                const matchesStatus = !statusVal || status === statusVal;
                const matchesVerification = !verificationVal || verification === verificationVal;

                if (matchesSearch && matchesRole && matchesStatus && matchesVerification) {
                    row.style.display = '';
                    row.classList.remove('d-none');
                } else {
                    row.style.display = 'none';
                    row.classList.add('d-none');
                }
            });

            toggleEmptyTableMessage('users-table', rows);
        };

        if (userSearchInput) userSearchInput.addEventListener('input', filterUsersTable);
        if (userRoleFilter) userRoleFilter.addEventListener('change', filterUsersTable);
        if (userStatusFilter) userStatusFilter.addEventListener('change', filterUsersTable);
        if (userVerificationFilter) userVerificationFilter.addEventListener('change', filterUsersTable);
    }

    // 2. Categories Page Filter
    const categorySearchInput = document.getElementById('categorySearchInput');
    if (categorySearchInput) {
        const filterCategoriesTable = function() {
            const searchVal = categorySearchInput.value.toLowerCase().trim();
            const rows = document.querySelectorAll('.category-row');
            
            rows.forEach(row => {
                const name = row.getAttribute('data-name') || '';
                const description = row.getAttribute('data-description') || '';
                
                const matchesSearch = !searchVal || name.includes(searchVal) || description.includes(searchVal);
                
                if (matchesSearch) {
                    row.style.display = '';
                    row.classList.remove('d-none');
                } else {
                    row.style.display = 'none';
                    row.classList.add('d-none');
                }
            });
            
            toggleEmptyTableMessage('categories-table', rows);
        };

        categorySearchInput.addEventListener('input', filterCategoriesTable);
    }

    // 3. Service Requests Page Filter
    const requestSearchInput = document.getElementById('requestSearchInput');
    const requestCategoryFilter = document.getElementById('requestCategoryFilter');
    const requestStatusFilter = document.getElementById('requestStatusFilter');
    const requestMinPrice = document.getElementById('requestMinPrice');
    const requestMaxPrice = document.getElementById('requestMaxPrice');

    if (requestSearchInput || requestCategoryFilter || requestStatusFilter || requestMinPrice || requestMaxPrice) {
        // Populate Categories dynamically from rows
        if (requestCategoryFilter) {
            const categories = new Set();
            document.querySelectorAll('.request-row').forEach(row => {
                const cat = row.getAttribute('data-category');
                if (cat) categories.add(cat);
            });

            Array.from(categories).sort().forEach(cat => {
                const opt = document.createElement('option');
                opt.value = cat;
                opt.textContent = cat;
                requestCategoryFilter.appendChild(opt);
            });
        }

        const filterRequestsTable = function() {
            const searchVal = requestSearchInput ? requestSearchInput.value.toLowerCase().trim() : '';
            const categoryVal = requestCategoryFilter ? requestCategoryFilter.value : '';
            const statusVal = requestStatusFilter ? requestStatusFilter.value : '';
            const minPriceVal = requestMinPrice ? parseFloat(requestMinPrice.value) || null : null;
            const maxPriceVal = requestMaxPrice ? parseFloat(requestMaxPrice.value) || null : null;

            const rows = document.querySelectorAll('.request-row');
            rows.forEach(row => {
                const customer = row.getAttribute('data-customer') || '';
                const title = row.getAttribute('data-title') || '';
                const city = row.getAttribute('data-city') || '';
                const category = row.getAttribute('data-category') || '';
                const status = row.getAttribute('data-status') || '';
                const priceMin = parseFloat(row.getAttribute('data-price-min')) || null;
                const priceMax = parseFloat(row.getAttribute('data-price-max')) || null;

                const matchesSearch = !searchVal || 
                    customer.includes(searchVal) || 
                    title.includes(searchVal) || 
                    city.includes(searchVal);

                const matchesCategory = !categoryVal || category === categoryVal;
                const matchesStatus = !statusVal || status === statusVal;

                // Price Filtering
                let matchesPrice = true;
                if (minPriceVal !== null || maxPriceVal !== null) {
                    if (priceMin === null && priceMax === null) {
                        matchesPrice = false;
                    } else {
                        const effMin = priceMin !== null ? priceMin : 0;
                        const effMax = priceMax !== null ? priceMax : effMin;
                        
                        if (minPriceVal !== null && effMax < minPriceVal) {
                            matchesPrice = false;
                        }
                        if (maxPriceVal !== null && effMin > maxPriceVal) {
                            matchesPrice = false;
                        }
                    }
                }

                if (matchesSearch && matchesCategory && matchesStatus && matchesPrice) {
                    row.style.display = '';
                    row.classList.remove('d-none');
                } else {
                    row.style.display = 'none';
                    row.classList.add('d-none');
                }
            });

            toggleEmptyTableMessage('requests-table', rows);
        };

        if (requestSearchInput) requestSearchInput.addEventListener('input', filterRequestsTable);
        if (requestCategoryFilter) requestCategoryFilter.addEventListener('change', filterRequestsTable);
        if (requestStatusFilter) requestStatusFilter.addEventListener('change', filterRequestsTable);
        if (requestMinPrice) requestMinPrice.addEventListener('input', filterRequestsTable);
        if (requestMaxPrice) requestMaxPrice.addEventListener('input', filterRequestsTable);
    }

    // ==========================================
    // Generic SweetAlert2 Confirm Interceptor
    // ==========================================
    
    // Intercept forms with data-confirm on submit
    $(document).on('submit', 'form[data-confirm]', function (e) {
        var form = this;
        if (form.dataset.confirmed) {
            return true;
        }
        e.preventDefault();
        var message = form.getAttribute('data-confirm');
        var primaryColor = getComputedStyle(document.documentElement).getPropertyValue('--kh-primary').trim() || '#00685f';
        Swal.fire({
            title: 'Are you sure?',
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: primaryColor,
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                form.dataset.confirmed = "true";
                form.submit();
            }
        });
        return false;
    });

    // Intercept buttons/links/inputs with data-confirm on click
    $(document).on('click', '[data-confirm]', function (e) {
        if (this.tagName === 'FORM') return;
        
        var form = this.closest('form');
        if (form && form.hasAttribute('data-confirm')) return;

        var element = this;
        if (element.dataset.confirmed) {
            return true;
        }
        e.preventDefault();
        var message = element.getAttribute('data-confirm');
        var primaryColor = getComputedStyle(document.documentElement).getPropertyValue('--kh-primary').trim() || '#00685f';
        Swal.fire({
            title: 'Are you sure?',
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: primaryColor,
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                element.dataset.confirmed = "true";
                element.click();
            }
        });
        return false;
    });
});
