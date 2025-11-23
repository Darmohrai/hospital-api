// frontend/js/api.js

// Ця адреса має відповідати вашому бекенду (з launchSettings.json)
const API_BASE_URL = 'http://localhost:5027';

/**
 * Отримує JWT токен з localStorage.
 * @returns {string | null} Збережений токен.
 */
function getToken() {
    return localStorage.getItem('jwtToken');
}

/**
 * Головна функція для виконання запитів до API.
 * Автоматично додає токен авторизації та обробляє помилки українською.
 * @param {string} endpoint - Шлях до API (наприклад, '/api/patient')
 * @param {RequestInit} options - Стандартні опції для fetch (method, body, etc.)
 * @returns {Promise<any>} Розпарсені JSON-дані з відповіді.
 */
async function apiFetch(endpoint, options = {}) {
    const token = getToken();

    // Налаштовуємо заголовки
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers, // Додаємо будь-які інші заголовки з options
    };

    // Якщо токен існує, додаємо його в заголовок Authorization
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...options, // Передаємо всі опції (method, body, etc.)
            headers: headers, // Передаємо наші налаштовані заголовки
        });

        // ==========================================
        // ОБРОБКА ПОМИЛОК HTTP
        // ==========================================
        if (!response.ok) {
            // Спробуємо розпарсити JSON помилку від сервера
            let errorData;
            try {
                errorData = await response.json();
            } catch (e) {
                // Якщо сервер не повернув JSON (наприклад, просто впав 500 html або текст)
                errorData = null;
            }

            // 1. Якщо це масив помилок (Identity часто так повертає, наприклад при реєстрації)
            if (Array.isArray(errorData)) {
                // Прокидаємо масив далі, щоб специфічні форми (реєстрація) могли його обробити
                throw errorData;
            }

            // 2. Стандартні HTTP коди - робимо їх зрозумілими для користувача
            if (response.status === 401) {
                throw new Error("Сесія закінчилась або ви не авторизовані. Будь ласка, увійдіть знову.");
            }
            if (response.status === 403) {
                throw new Error("У вас недостатньо прав для виконання цієї дії.");
            }
            if (response.status === 404) {
                throw new Error("Запитуваний ресурс не знайдено (Помилка 404).");
            }
            if (response.status >= 500) {
                throw new Error("Сталася внутрішня помилка сервера. Спробуйте пізніше.");
            }

            // 3. Формуємо повідомлення з того, що надіслав сервер (якщо є ProblemDetails)
            let errorMessage = 'Виникла невідома помилка під час запиту.';

            if (errorData) {
                // ProblemDetails зазвичай має поле 'title' або 'detail'
                if (errorData.title) errorMessage = errorData.title;
                if (errorData.detail) errorMessage += `: ${errorData.detail}`;

                // Валідаційні помилки (errors: { Field: ["Error"] })
                if (errorData.errors) {
                    const validationErrors = Object.values(errorData.errors).flat().join(', ');
                    errorMessage = `Перевірте введені дані: ${validationErrors}`;
                }
            } else {
                // Якщо JSON немає, беремо статус текст
                errorMessage = `Помилка сервера: ${response.status} ${response.statusText}`;
            }

            console.error('API Error Response:', errorData || response.statusText);
            throw new Error(errorMessage);
        }

        // Якщо відповідь '204 No Content' (наприклад, для DELETE або успішного PUT без тіла)
        if (response.status === 204) {
            return null;
        }

        // Якщо все добре і є тіло, повертаємо JSON
        return response.json();

    } catch (error) {
        // Якщо це помилка мережі (сервер вимкнено або немає інтернету)
        if (error.name === 'TypeError' && error.message === 'Failed to fetch') {
            console.error('Network Error:', error);
            throw new Error("Не вдалося з'єднатися з сервером. Перевірте підключення до інтернету або спробуйте пізніше.");
        }

        if (!Array.isArray(error)) {
            console.error('Fetch Error:', error.message || error);
        }
        // "Прокидаємо" помилку далі, щоб UI міг її показати
        throw error;
    }
}