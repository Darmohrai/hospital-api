// js/admin.js

document.addEventListener('DOMContentLoaded', () => {
    // 1. Перевірка безпеки: чи маємо ми тут бути?
    const role = getUserRole();
    if (role !== 'Admin') {
        // Якщо це не Адмін, відправляємо на головну
        alert('У вас немає доступу до цього ресурсу.');
        window.location.href = 'index.html';
        return;
    }

    // 2. Якщо це Адмін, завантажуємо запити
    loadUpgradeRequests();

    // 3. "Вішаємо" обробник на *тіло* таблиці для кнопок "Схвалити" / "Відхилити"
    document.getElementById('requests-table-body').addEventListener('click', handleRequestAction);
});

/**
 * 1. READ (Завантажити список запитів)
 * Запитує дані з GET /api/auth/pending-requests
 */
async function loadUpgradeRequests() {
    const tableBody = document.getElementById('requests-table-body');
    tableBody.innerHTML = '<tr><td colspan="5">Завантаження...</td></tr>';

    try {
        // Цей ендпоінт повертає список UpgradeRequestDto
        const requests = await apiFetch('/api/auth/pending-requests');

        tableBody.innerHTML = ''; // Очищуємо

        if (requests.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="5">Нових запитів на підвищення немає.</td></tr>';
            return;
        }

        requests.forEach(req => {
            console.log(req);
            const row = document.createElement('tr');
            // DTO містить id, userName, email, requestDate
            row.innerHTML = `
                <td>${req.requestId}</td>
                <td>${req.userName}</td>
                <td>${new Date(req.requestDate).toLocaleString('uk-UA')}</td>
                <td>
                    <button class="btn btn-success btn-sm btn-approve" data-id="${req.userId}">
                        Схвалити
                    </button>
                    <button class="btn btn-danger btn-sm btn-reject" data-id="${req.userId}">
                        Відхилити
                    </button>
                </td>
            `;
            tableBody.appendChild(row);
        });

    } catch (error) {
        tableBody.innerHTML = `<tr><td colspan="5" class="text-danger">Помилка завантаження: ${error.message}</td></tr>`;
    }
}

/**
 * 2. Обробник для кнопок "Схвалити" (Approve) / "Відхилити" (Reject)
 */
async function handleRequestAction(event) {
    const target = event.target; // Елемент, на який натиснули
    const id = target.dataset.id; // Отримуємо ID з data-id=""

    // Якщо ми натиснули не на кнопку, ігноруємо
    if (!id) return;

    let endpoint = '';
    let actionName = '';

    // Визначаємо, який ендпоінт викликати
    if (target.classList.contains('btn-approve')) {
        endpoint = `/api/auth/approve-guest/${id}`; //
        actionName = 'схвалити';
    } else if (target.classList.contains('btn-reject')) {
        endpoint = `/api/auth/reject-guest/${id}`; //
        actionName = 'відхилити';
    } else {
        return; // Натиснули не на ту кнопку
    }

    // Блокуємо кнопки, щоб уникнути подвійного натискання
    target.disabled = true;

    try {
        // Викликаємо відповідний POST-запит
        await apiFetch(endpoint, { method: 'POST' });

        alert(`Запит ${id} успішно ${actionName}.`);
        loadUpgradeRequests(); // Оновлюємо таблицю, щоб запит зник

    } catch (error) {
        alert(`Помилка: ${error.message}`);
        target.disabled = false; // Розблоковуємо кнопку, якщо була помилка
    }
}