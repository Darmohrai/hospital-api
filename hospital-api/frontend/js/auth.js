// js/auth.js

// Цей код виконається, коли HTML-документ буде повністю завантажено
document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');

    // --- ВАЖЛИВЕ ВИПРАВЛЕННЯ ---
    // Ця логіка спрацює ТІЛЬКИ якщо ми на сторінці логіну АБО реєстрації
    if (loginForm || registerForm) {

        // Якщо ми ВЖЕ залогінені, нам не треба бути на цих сторінках
        if (isLoggedIn()) {
            window.location.href = 'index.html';
            return; // Зупиняємо виконання скрипту
        }

        // "Вішаємо" обробники, тільки якщо форми існують
        if (loginForm) {
            loginForm.addEventListener('submit', handleLogin);
        }
        if (registerForm) {
            registerForm.addEventListener('submit', handleRegister);
        }
    }
});

/**
 * Обробник відправки форми логіну
 */
async function handleLogin(event) {
    event.preventDefault(); // Запобігаємо перезавантаженню сторінки

    // Використовуємо 'login-username' згідно з твоїм LoginDto.cs
    const username = document.getElementById('login-username').value;
    const password = document.getElementById('login-password').value;
    const errorEl = document.getElementById('login-error');

    errorEl.style.display = 'none';
    errorEl.textContent = '';

    try {
        // Використовуємо наш apiFetch
        const data = await apiFetch('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password }),
        });

        // УСПІХ!
        if (data.token) {
            localStorage.setItem('jwtToken', data.token);
            // Перенаправляємо на головну сторінку, як ти і хотів
            window.location.href = 'index.html';
        } else {
            showLoginError('Помилка: Не вдалося отримати токен.');
        }

    } catch (error) {
        // Показуємо помилку (напр. 'Invalid credentials')
        showLoginError(error.message);
    }
}

/**
 * Обробник відправки форми реєстрації "Гостя"
 */
async function handleRegister(event) {
    event.preventDefault();

    // Поля з твого RegisterDto.cs
    const userName = document.getElementById('register-username').value;
    const email = document.getElementById('register-email').value;
    const password = document.getElementById('register-password').value;
    const messageEl = document.getElementById('register-message');

    messageEl.style.display = 'none';
    messageEl.textContent = '';

    try {
        // Викликаємо ендпоінт реєстрації "Гостя"
        await apiFetch('/api/auth/register-guest', {
            method: 'POST',
            body: JSON.stringify({ userName, email, password }),
        });

        // УСПІХ!
        showMessage('Реєстрація успішна! Тепер ви можете увійти.', 'success');
        event.target.reset();

    } catch (error) {
        // Показуємо помилку (напр. 'Passwords must have at least one non-alphanumeric character...')
        showMessage(error.message, 'danger');
    }
}

// Допоміжні функції для показу повідомлень
function showLoginError(message) {
    const errorEl = document.getElementById('login-error');
    errorEl.textContent = message;
    errorEl.style.display = 'block';
}

function showMessage(message, type = 'danger') {
    const messageEl = document.getElementById('register-message');
    messageEl.textContent = message;
    messageEl.className = `alert alert-${type} mt-3`;
    messageEl.style.display = 'block';
}


// --- ГЛОБАЛЬНІ ФУНКЦІЇ (доступні на всіх сторінках) ---

/**
 * Функція виходу
 */
function logout() {
    localStorage.removeItem('jwtToken'); // Просто чистимо токен
    window.location.href = 'index.html'; // Повертаємо на головну
}

/**
 * Перевіряє, чи є токен в сховищі
 * @returns {boolean}
 */
function isLoggedIn() {
    return !!localStorage.getItem('jwtToken');
}

/**
 * Декодує JWT токен, щоб отримати дані (наприклад, роль)
 * @returns {object | null} Payload токена
 */
function decodeToken() {
    try {
        const token = localStorage.getItem('jwtToken');
        if (!token) return null;

        const payloadBase64 = token.split('.')[1];
        const payloadJson = atob(payloadBase64); // Декодуємо з Base64
        return JSON.parse(payloadJson);

    } catch (e) {
        console.error('Error decoding token:', e);
        logout(); // Якщо токен "битий", просто виходимо з системи
        return null;
    }
}

/**
 * Отримує роль поточного користувача з токена
 * @returns {string | null} Роль (Admin, Operator, Authorized, Guest)
 */
function getUserRole() {
    const payload = decodeToken();
    if (!payload) return null;

    // ASP.NET Identity за замовчуванням зберігає роль у "claim" з такою назвою
    const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const role = payload[roleClaim];

    if (Array.isArray(role)) {
        return role[0];
    }
    return role;
}

/**
 * Отримує ім'я (username) користувача з токена
 * @returns {string}
 */
function getUserName() {
    const payload = decodeToken();
    if (!payload) return 'User';

    // ASP.NET Identity зберігає ім'я тут
    const nameClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
    return payload[nameClaim] || 'User';
}