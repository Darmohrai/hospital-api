// frontend/js/auth.js

document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');

    const registerEmail = document.getElementById('register-email');
    const registerPassword = document.getElementById('register-password');
    const registerUsername = document.getElementById('register-username');

    // 1. Очищення помилки для ПАРОЛЯ
    if (registerPassword) {
        registerPassword.addEventListener('input', function() {
            this.setCustomValidity('');
        });
    }

    // 2. Очищення помилки для ЛОГІНА
    if (registerUsername) {
        registerUsername.addEventListener('input', function() {
            this.setCustomValidity('');
        });
    }

    // 3. ✅ ОЧИЩЕННЯ ПОМИЛКИ ДЛЯ EMAIL (Виправлення вашої проблеми)
    if (registerEmail) {
        registerEmail.addEventListener('input', function() {
            // Як тільки юзер змінює пошту — прибираємо блокування
            this.setCustomValidity('');
        });
    }
    
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
            // Це малоймовірний сценарій, якщо API працює правильно,
            // але про всяк випадок обробляємо вручну
            throw new Error('Неправильний логін або пароль');
        }

    } catch (error) {
        errorEl.style.display = 'block';

        // ПЕРЕВІРКА: Якщо сервер повернув 401 (Unauthorized)
        if (error.message && (error.message.includes('Unauthorized') || error.message.includes('401'))) {
            errorEl.textContent = 'Неправильний логін або пароль';
        } else {
            // Для всіх інших помилок показуємо оригінальне повідомлення або заглушку
            errorEl.textContent = error.message || 'Помилка логіну.';
        }
    }
}

// =================== РЕЄСТРАЦІЯ (ОНОВЛЕНО) ===================
// =================== РЕЄСТРАЦІЯ (ОНОВЛЕНО З ПЕРЕВІРКОЮ ПАРОЛЯ) ===================
async function handleRegister(event) {
    event.preventDefault();

    const usernameInput = document.getElementById('register-username');
    const emailInput = document.getElementById('register-email');
    const passwordInput = document.getElementById('register-password');

    const roleInputs = document.querySelectorAll('input[name="role"]');
    const selectedRole = document.querySelector('input[name="role"]:checked');

    const messageEl = document.getElementById('register-message');

    // 1. ОЧИЩЕННЯ ПОМИЛОК
    // Скидаємо старі помилки перед новою спробою
    usernameInput.setCustomValidity('');
    emailInput.setCustomValidity(''); // Додано очищення для email
    passwordInput.setCustomValidity('');
    roleInputs.forEach(input => input.setCustomValidity(''));

    if (messageEl) {
        messageEl.style.display = 'none';
        messageEl.textContent = '';
    }

    // 2. ВАЛІДАЦІЯ USERNAME (Тільки латиниця)
    const usernameRegex = /^[a-zA-Z0-9]+$/;
    if (!usernameRegex.test(usernameInput.value)) {
        usernameInput.setCustomValidity('Логін може містити тільки англійські літери та цифри.');
        usernameInput.reportValidity();
        return;
    }

    // 3. ВАЛІДАЦІЯ ПАРОЛЯ
    const password = passwordInput.value;
    let passwordError = '';

    if (password.length < 6) {
        passwordError = 'Пароль має містити мінімум 6 символів.';
    } else if (!/[a-z]/.test(password)) {
        passwordError = 'Пароль має містити хоча б одну малу літеру (a-z).';
    } else if (!/[A-Z]/.test(password)) {
        passwordError = 'Пароль має містити хоча б одну велику літеру (A-Z).';
    } else if (!/[^a-zA-Z0-9]/.test(password)) {
        passwordError = 'Пароль має містити хоча б один спецсимвол (!, @, # тощо).';
    }

    if (passwordError) {
        passwordInput.setCustomValidity(passwordError);
        passwordInput.reportValidity();
        return;
    }

    // 4. ВАЛІДАЦІЯ РОЛІ
    if (!selectedRole) {
        roleInputs[0].setCustomValidity('Будь ласка, оберіть тип реєстрації');
        roleInputs[0].reportValidity();
        return;
    }

    const userName = usernameInput.value;
    const email = emailInput.value;
    const role = selectedRole.value;

    try {
        const data = await apiFetch('/api/auth/register-guest', {
            method: 'POST',
            body: JSON.stringify({
                username: userName,
                email,
                password,
                role: role
            }),
        });

        showToast(data.message || 'Реєстрація успішна!', 'success');
        event.target.reset();

        setTimeout(() => {
            window.location.href = 'login.html';
        }, 1500);

    } catch (error) {
        console.error(error);

        let handled = false; // Прапорець: чи обробили ми помилку специфічно?

        // --- ОБРОБКА СПЕЦИФІЧНИХ ПОМИЛОК ВІД СЕРВЕРА ---
        if (Array.isArray(error)) {

            // 1. Перевірка на дублікат Email
            const emailErr = error.find(e => e.code === 'DuplicateEmail' || e.description.toLowerCase().includes('email'));
            if (emailErr) {
                emailInput.setCustomValidity('Цей email вже зареєстрований. Спробуйте увійти.');
                emailInput.reportValidity(); // Показує хмаринку на полі Email
                handled = true;
            }

            // 2. Перевірка на дублікат Логіну (Username)
            // Робимо це тільки якщо з поштою все ок
            if (!handled) {
                const userErr = error.find(e => e.code === 'DuplicateUserName' || e.description.toLowerCase().includes('user'));
                if (userErr) {
                    usernameInput.setCustomValidity('Це ім\'я користувача вже зайняте.');
                    usernameInput.reportValidity(); // Показує хмаринку на полі Username
                    handled = true;
                }
            }
        }

        // --- ЯКЩО ЦЕ ІНША ПОМИЛКА ---
        // Якщо ми не знайшли специфічної помилки поля, показуємо загальне повідомлення
        if (!handled) {
            let errorText = 'Помилка реєстрації.';

            if (Array.isArray(error)) {
                errorText = error.map(e => e.description).join('\n');
            } else if (error.message) {
                errorText = error.message;
            }

            if (messageEl) {
                messageEl.style.display = 'block';
                messageEl.textContent = errorText;
            } else {
                showToast(errorText, 'danger');
            }
        }
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