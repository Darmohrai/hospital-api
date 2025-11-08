// js/api.js

// Ця адреса з твого launchSettings.json (профіль http)
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
 * Автоматично додає токен авторизації.
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

        // Обробка помилок
        if (!response.ok) {
            // Спробуємо отримати тіло помилки, яке надсилає ASP.NET
            const errorData = await response.json().catch(() => ({
                title: "Unknown error",
                detail: response.statusText
            }));

            // Формуємо зрозуміле повідомлення про помилку
            let errorMessage = errorData.title || 'Помилка API';
            if(errorData.detail) errorMessage += `: ${errorData.detail}`;

            // Обробка помилок валідації (якщо вони є)
            if(errorData.errors) {
                const validationErrors = Object.values(errorData.errors).flat().join(', ');
                errorMessage += `: ${validationErrors}`;
            }

            console.error('API Error Response:', errorData);
            throw new Error(errorMessage);
        }

        // Якщо відповідь '204 No Content' (наприклад, для DELETE),
        // fetch не повертає тіло, тому .json() викличе помилку.
        if (response.status === 204) {
            return null; // Повертаємо null, щоб позначити успішне виконання
        }

        // Якщо все добре і є тіло, повертаємо JSON
        return response.json();

    } catch (error) {
        console.error('Fetch Error:', error.message);
        // "Прокидаємо" помилку далі, щоб її можна було зловити в UI
        throw error;
    }
}