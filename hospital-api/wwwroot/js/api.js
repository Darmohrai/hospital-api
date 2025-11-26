const API_BASE_URL = '';

function getToken() {
    return localStorage.getItem('jwtToken');
}

async function apiFetch(endpoint, options = {}) {
    const token = getToken();

    const headers = {
        'Content-Type': 'application/json',
        ...options.headers,
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    try {
        console.log(API_BASE_URL + endpoint);
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...options,
            headers: headers,
        });

        if (!response.ok) {
            let errorData;
            try {
                errorData = await response.json();
            } catch (e) {
                errorData = null;
            }

            if (Array.isArray(errorData)) {
                throw errorData;
            }

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

            let errorMessage = 'Виникла невідома помилка під час запиту.';

            if (errorData) {
                if (errorData.message) {
                    errorMessage = errorData.message;
                }
                else {
                    if (errorData.title) errorMessage = errorData.title;
                    if (errorData.detail) errorMessage += `: ${errorData.detail}`;

                    if (errorData.errors) {
                        const validationErrors = Object.values(errorData.errors).flat().join(', ');
                        errorMessage = `Перевірте введені дані: ${validationErrors}`;
                    }
                }
            } else {
                errorMessage = `Помилка сервера: ${response.status} ${response.statusText}`;
            }

            console.error('API Error Response:', errorData || response.statusText);
            throw new Error(errorMessage);
        }

        if (response.status === 204) {
            return null;
        }

        return response.json();

    } catch (error) {
        
        if (error.name === 'TypeError' && error.message === 'Failed to fetch') {
            console.error('Network Error:', error);
            throw new Error("Не вдалося з'єднатися з сервером. Перевірте підключення до інтернету або спробуйте пізніше.");
        }

        if (!Array.isArray(error)) {
            console.error('Fetch Error:', error.message || error);
        }
        throw error;
    }
}