document.addEventListener('DOMContentLoaded', () => {
    setupSharedUI();
});

function setupSharedUI() {
    if (isLoggedIn()) {
        const anonymousLinks = document.getElementById('anonymous-links');
        if (anonymousLinks) {
            anonymousLinks.style.display = 'none';
        }

        const authBlock = document.getElementById('authenticated-user');
        if (authBlock) {
            authBlock.style.display = 'block';
        }

        const userGreeting = document.getElementById('user-greeting');
        if (userGreeting) {
            userGreeting.textContent = `Вітаємо, ${getUserName()}!`;
        }

        const logoutButton = document.getElementById('logout-button');
        if (logoutButton) {
            logoutButton.addEventListener('click', logout);
        }

        const role = getUserRole();

        const navReports = document.getElementById('nav-reports');
        const navCrud = document.getElementById('nav-crud');
        const navAdmin = document.getElementById('nav-admin');

        switch (role) {
            case 'Guest':
                break;

            case 'Authorized':
                if (navReports) navReports.style.display = 'block';
                break;

            case 'Operator':
                if (navReports) navReports.style.display = 'block';
                if (navCrud) navCrud.style.display = 'block';
                break;

            case 'Admin':
                if (navReports) navReports.style.display = 'block';
                if (navCrud) navCrud.style.display = 'block';
                if (navAdmin) navAdmin.style.display = 'block';
                break;
        }

        const guestContent = document.getElementById('guest-content');
        const publicContent = document.getElementById('public-content');

        if (role === 'Guest' && guestContent && publicContent) {
            guestContent.style.display = 'block';
            publicContent.style.display = 'none';

            const upgradeButton = document.getElementById('request-upgrade-button');
            if (upgradeButton) {
                upgradeButton.addEventListener('click', handleUpgradeRequest);
            }
        }

    }
}

async function handleUpgradeRequest() {
    const msgEl = document.getElementById('guest-message');
    const button = document.getElementById('request-upgrade-button');

    if (!msgEl || !button) return;

    button.disabled = true;

    try {
        await apiFetch('/api/auth/request-upgrade', { method: 'POST' });

        msgEl.textContent = 'Запит успішно надіслано адміністратору.';
        msgEl.className = 'text-success mt-2';
    } catch (error) {
        msgEl.textContent = `Помилка: ${error.message}`;
        msgEl.className = 'text-danger mt-2';
        button.disabled = false;
    }
}

async function loadNavigation() {
    const header = document.querySelector('header');
    if (!header) return;

    const role = getUserRole();

    let navLinks = `
        <li class="nav-item">
            <a class="nav-link" href="/index.html">Головна</a>
        </li>
    `;

    let authLinks = '';

    if (role) {
        if (role === 'Admin' || role === 'Operator' || role === 'Authorized') {
            navLinks += `
                <li class="nav-item">
                    <a class="nav-link" href="/reports.html">Звіти</a>
                </li>
            `;
        }

        if (role === 'Admin' || role === 'Operator') {
            navLinks += `
                <li class="nav-item dropdown">
                    <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
                        Керування
                    </a>
                    <ul class="dropdown-menu">
                        <li><a class="dropdown-item" href="/manage/hospitals.html">Лікарні</a></li>
                        <li><a class="dropdown-item" href="/manage/patients.html">Пацієнти</a></li>
                        <li><a class="dropdown-item" href="/manage/doctors.html">Лікарі</a></li>
                    </ul>
                </li>
            `;
        }

        if (role === 'Admin') {
            navLinks += `
                <li class="nav-item">
                    <a class="nav-link" href="/admin.html">Адмін-панель</a>
                </li>
            `;
        }

        authLinks = `
            <div id="authenticated-user" class="text-light">
                <span class="navbar-text me-3">Вітаємо, ${getUserName()}</span>
                <button class="btn btn-outline-warning" id="logout-button">Вийти</button>
            </div>
        `;
    } else {
        authLinks = `
            <div id="anonymous-links">
                <a href="/login.html" class="btn btn-outline-light me-2">Увійти</a>
                <a href="/register.html" class="btn btn-warning">Зареєструватися</a>
            </div>
        `;
    }

    const navHtml = `
        <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
            <div class="container-fluid">
                <a class="navbar-brand" href="/index.html">МедСистема</a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarContent">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarContent">
                    <ul class="navbar-nav me-auto mb-2 mb-lg-0">
                        ${navLinks}
                    </ul>
                    <div class="d-flex" id="auth-section">
                        ${authLinks}
                    </div>
                </div>
            </div>
        </nav>
    `;

    header.innerHTML = navHtml;

    const logoutButton = document.getElementById('logout-button');
    if (logoutButton) {
        logoutButton.addEventListener('click', logout);
    }
}