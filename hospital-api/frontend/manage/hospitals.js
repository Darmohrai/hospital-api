// manage/hospitals.js

// Глобальна змінна для модального вікна
let hospitalModal;

document.addEventListener('DOMContentLoaded', () => {
    // 1. Перевірка безпеки: чи маємо ми тут бути?
    const role = getUserRole();
    if (role !== 'Admin' && role !== 'Operator') {
        // Якщо ні, повертаємо на головну
        alert('У вас немає доступу до цього ресурсу.');
        window.location.href = '../index.html';
        return;
    }

    // 2. Ініціалізуємо модальне вікно Bootstrap
    hospitalModal = new bootstrap.Modal(document.getElementById('hospital-modal'));

    // 3. Завантажуємо список лікарень
    loadHospitals();

    // 4. Налаштовуємо обробники

    // Обробник для форми (Create/Update)
    document.getElementById('hospital-form').addEventListener('submit', handleFormSubmit);

    // Обробник для кнопки "Додати" (очищує форму)
    document.getElementById('add-hospital-btn').addEventListener('click', () => {
        document.getElementById('hospital-form').reset(); // Скидаємо поля
        document.getElementById('hospital-id').value = ''; // Скидаємо ID
        document.getElementById('hospital-modal-title').textContent = 'Додати лікарню';
    });

    // Обробник для кнопок в таблиці (Edit/Delete)
    document.getElementById('hospitals-table-body').addEventListener('click', handleTableClick);
});

/**
 * 1. READ (Завантажити список)
 * Запитує дані з GET /api/hospital
 */
async function loadHospitals() {
    const tableBody = document.getElementById('hospitals-table-body');
    tableBody.innerHTML = '<tr><td colspan="4">Завантаження...</td></tr>'; // Тимчасовий рядок

    try {
        const hospitals = await apiFetch('/api/hospital'); //

        tableBody.innerHTML = ''; // Очищуємо

        if (hospitals.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="4">Лікарні не знайдено.</td></tr>';
            return;
        }

        hospitals.forEach(hospital => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${hospital.id}</td>
                <td>${hospital.name}</td>
                <td>${hospital.address}</td>
                <td>
                    <button class="btn btn-warning btn-sm btn-edit" data-id="${hospital.id}">
                        Редагувати
                    </button>
                    <button class="btn btn-danger btn-sm btn-delete" data-id="${hospital.id}">
                        Видалити
                    </button>
                </td>
            `;
            tableBody.appendChild(row);
        });

    } catch (error) {
        tableBody.innerHTML = `<tr><td colspan="4" class="text-danger">Помилка завантаження: ${error.message}</td></tr>`;
    }
}

/**
 * 2. CREATE / UPDATE (Обробник форми)
 * Викликає POST /api/hospital або PUT /api/hospital/{id}
 */
async function handleFormSubmit(event) {
    event.preventDefault();

    const hospitalId = document.getElementById('hospital-id').value;
    const hospitalData = {
        name: document.getElementById('hospital-name').value,
        address: document.getElementById('hospital-address').value
        // Якщо в моделі Hospital є інші обов'язкові поля, їх треба додати сюди
    };

    // Якщо ID є, то це Update (PUT), інакше Create (POST)
    const isUpdate = !!hospitalId;
    const method = isUpdate ? 'PUT' : 'POST';
    // Для PUT нам треба додати ID в URL
    const endpoint = isUpdate ? `/api/hospital/${hospitalId}` : '/api/hospital';

    // Для PUT запиту DTO має включати ID
    if (isUpdate) {
        hospitalData.id = parseInt(hospitalId);
    }

    try {
        await apiFetch(endpoint, {
            method: method,
            body: JSON.stringify(hospitalData),
        });

        hospitalModal.hide(); // Ховаємо модалку
        loadHospitals(); // Оновлюємо таблицю

    } catch (error) {
        alert(`Помилка збереження: ${error.message}`);
    }
}

/**
 * 3. DELETE / Початок UPDATE (Обробник кнопок у таблиці)
 */
async function handleTableClick(event) {
    const target = event.target;
    const id = target.dataset.id; // Отримуємо ID з кнопки

    // 4. DELETE
    if (target.classList.contains('btn-delete')) {
        if (confirm(`Ви впевнені, що хочете видалити лікарню з ID ${id}?`)) {
            try {
                await apiFetch(`/api/hospital/${id}`, { method: 'DELETE' });
                loadHospitals(); // Оновлюємо таблицю
            } catch (error) {
                alert(`Помилка видалення: ${error.message}`);
            }
        }
    }

    // 5. UPDATE (заповнення форми)
    if (target.classList.contains('btn-edit')) {
        try {
            // Отримуємо свіжі дані цієї лікарні
            const hospital = await apiFetch(`/api/hospital/${id}`);

            // Заповнюємо форму
            document.getElementById('hospital-id').value = hospital.id;
            document.getElementById('hospital-name').value = hospital.name;
            document.getElementById('hospital-address').value = hospital.address;

            // Налаштовуємо модалку
            document.getElementById('hospital-modal-title').textContent = 'Редагувати лікарню';
            hospitalModal.show();

        } catch (error) {
            alert(`Помилка завантаження даних: ${error.message}`);
        }
    }
}