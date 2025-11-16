// frontend/js/auth.js

document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');

    // --- Виконуємо тільки якщо на сторінці логін або реєстрація
    if (loginForm || registerForm) {
        if (isLoggedIn()) {
            window.location.href = 'index.html';
            return;
        }

        if (loginForm) loginForm.addEventListener('submit', handleLogin);
        if (registerForm) registerForm.addEventListener('submit', handleRegister);
    }

    initForgotPassword(); // Ініціалізація модалки Forgot Password
});

// =================== ЛОГІН ===================
async function handleLogin(event) {
    event.preventDefault();

    const username = document.getElementById('login-username').value;
    const password = document.getElementById('login-password').value;
    const errorEl = document.getElementById('login-error');

    errorEl.style.display = 'none';
    errorEl.textContent = '';

    try {
        const data = await apiFetch('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password }),
        });

        if (data.token) {
            localStorage.setItem('jwtToken', data.token);
            showToast('Вхід успішний!', 'success');
            setTimeout(() => window.location.href = 'index.html', 500);
        } else {
            showToast('Помилка: Не вдалося отримати токен.', 'danger');
        }

    } catch (error) {
        errorEl.style.display = 'block';
        errorEl.textContent = error.message || 'Помилка логіну.';
    }
}

// =================== РЕЄСТРАЦІЯ (ОНОВЛЕНО) ===================
async function handleRegister(event) {
    event.preventDefault();

    const userName = document.getElementById('register-username').value;
    const email = document.getElementById('register-email').value;
    const password = document.getElementById('register-password').value;
    const messageEl = document.getElementById('register-message'); // Елемент для виводу помилок

    // ✅ Отримуємо значення ролі з radio-кнопок (які ви додали в register.html)
    const roleInput = document.querySelector('input[name="role"]:checked');
    
    messageEl.style.display = 'none';
    messageEl.textContent = '';

    // ✅ Валідація на клієнті, що роль обрано
    if (!roleInput) {
        messageEl.style.display = 'block';
        messageEl.textContent = 'Будь ласка, оберіть тип реєстрації.';
        return;
    }
    const role = roleInput.value;

    try {
        // ✅ Змінено URL на /api/auth/register
        // ✅ Додано 'role' в тіло запиту
        const data = await apiFetch('/api/auth/register-guest', {
            method: 'POST',
            body: JSON.stringify({
                username: userName,
                email,
                password,
                role: role // ✅ Нове поле
            }),
        });

        // ✅ Використовуємо динамічне повідомлення з бекенду
        showToast(data.message || 'Реєстрація успішна!', 'success');
        event.target.reset();

        // ✅ Перенаправляємо на сторінку логіну
        setTimeout(() => {
            window.location.href = 'login.html';
        }, 1500); // 1.5с, щоб користувач встиг прочитати toast

    } catch (error) {
        // ✅ Відображаємо помилку в формі (аналогічно до handleLogin)
        messageEl.style.display = 'block';
        messageEl.textContent = error.message || 'Помилка реєстрації.';
    }
}


// =================== ФОРГОТ ПАРОЛЬ ===================
function initForgotPassword() {
    const forgotPasswordLink = document.getElementById('forgotPasswordLink');
    if (!forgotPasswordLink) return;

    const forgotModalEl = document.getElementById('forgot-password-modal');
    const forgotModal = new bootstrap.Modal(forgotModalEl);
    const sendResetBtn = document.getElementById('send-reset-link');

    forgotPasswordLink.addEventListener('click', (e) => {
        e.preventDefault();
        forgotModal.show();
    });

    sendResetBtn.addEventListener('click', async () => {
        const emailInput = document.getElementById('forgot-email');
        const email = emailInput.value.trim();
        if (!email) {
            showToast('Будь ласка, введіть email!', 'danger');
            return;
        }

        try {
            const data = await apiFetch('/api/auth/forgot-password', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email }),
            });

            forgotModal.hide();
            showToast('Посилання для скидання пароля надіслано!', 'success');

            setTimeout(() => {
                window.location.href = `reset-password.html?email=${data.email}&token=${data.token}`;
            }, 1500);

        } catch (error) {
            forgotModal.hide();
            showToast(error.message || 'Користувача з таким email не знайдено.', 'danger');
        }
    });
}

// =================== TOAST ===================
function showToast(message, type = 'info') {
    const container = document.querySelector('.toast-container');
    if (!container) return;

    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-bg-${type} border-0`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    container.appendChild(toastEl);

    const bsToast = new bootstrap.Toast(toastEl, { delay: 3000 });
    bsToast.show();

    toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
}

// =================== JWT & AUTH HELPERS ===================
function logout() {
    localStorage.removeItem('jwtToken');
    window.location.href = 'index.html';
}

function isLoggedIn() {
    return !!localStorage.getItem('jwtToken');
}

function decodeToken() {
    try {
        const token = localStorage.getItem('jwtToken');
        if (!token) return null;
        const payloadBase64 = token.split('.')[1];
        const payloadJson = atob(payloadBase64);
        return JSON.parse(payloadJson);
    } catch (e) {
        console.error('Error decoding token:', e);
        logout();
        return null;
    }
}

function getUserRole() {
    const payload = decodeToken();
    if (!payload) return null;
    const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const role = payload[roleClaim];
    return Array.isArray(role) ? role[0] : role;
}

function getUserName() {
    const payload = decodeToken();
    if (!payload) return 'User';
    const nameClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
    return payload[nameClaim] || 'User';
}