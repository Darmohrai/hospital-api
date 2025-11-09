document.addEventListener('DOMContentLoaded', () => {

    // --- Перевірка залежностей ---
    if (typeof apiFetch === 'undefined') {
        console.error("КРИТИЧНА ПОМИЛКА: 'apiFetch' функцію не знайдено.");
        alert("Помилка: Не вдалося завантажити API. Перевірте консоль.");
        return;
    }
    if (typeof bootstrap === 'undefined' || typeof bootstrap.Modal === 'undefined') {
        console.error("КРИТИЧНА ПОМИЛКА: 'bootstrap' не завантажено.");
        alert("Помилка: Не вдалося завантажити Bootstrap. Модальні вікна не працюватимуть.");
        return;
    }

    // --- Елементи DOM ---
    const clinicsTableBody = document.getElementById('clinics-table-body');
    const addClinicBtn = document.getElementById('add-clinic-btn');
    const pageError = document.getElementById('page-error');

    // --- Модальне вікно ---
    const mainModalEl = document.getElementById('clinic-manage-modal');
    const mainModal = new bootstrap.Modal(mainModalEl);
    const mainModalTitle = document.getElementById('clinic-modal-title');
    const mainModalError = document.getElementById('modal-error');
    const clinicForm = document.getElementById('clinic-form');
    const clinicIdInput = document.getElementById('clinic-id');
    const clinicNameInput = document.getElementById('clinic-name');
    const clinicAddressInput = document.getElementById('clinic-address');
    const clinicHospitalIdSelect = document.getElementById('clinic-hospital-id');
    const saveClinicBtn = document.getElementById('save-clinic-btn');

    // --- Стан ---
    let currentMode = 'create'; // 'create' або 'edit'
    let currentUserRole = getUserRole(); // Припускаємо, що ця функція є в auth.js

    // --- Функції ---

    /**
     * Показує повідомлення про помилку на сторінці
     */
    function showPageError(message) {
        pageError.textContent = message;
        pageError.style.display = message ? 'block' : 'none';
    }

    /**
     * Показує/ховає помилку в модальному вікні
     */
    function showMainModalError(message) {
        mainModalError.textContent = message;
        mainModalError.style.display = message ? 'block' : 'none';
    }

    /**
     * Завантажує список лікарень у випадаючий список
     * @param {number | null} selectedId - ID лікарні, яка має бути обрана
     */
    async function loadHospitalsForSelect(selectedId = null) {
        try {
            // Отримуємо список лікарень (припускаємо, що це простий DTO)
            const hospitals = await apiFetch('/api/hospital');

            clinicHospitalIdSelect.innerHTML = '<option value="">Не прив\'язано</option>'; // Очищуємо + опція "не обрано"

            if (hospitals && hospitals.length > 0) {
                hospitals.forEach(hospital => {
                    const isSelected = hospital.id == selectedId;
                    clinicHospitalIdSelect.innerHTML += `
                        <option value="${hospital.id}" ${isSelected ? 'selected' : ''}>
                            ID: ${hospital.id} - ${hospital.name}
                        </option>
                    `;
                });
            }
        } catch (error) {
            console.error("Не вдалося завантажити лікарні для select:", error);
            showMainModalError("Не вдалося завантажити список лікарень.");
        }
    }

    /**
     * Завантажує список поліклінік у головну таблицю
     */
    async function loadClinics() {
        try {
            showPageError(null);
            // Використовуємо ендпоінт, який повертає ClinicDto
            const clinics = await apiFetch('/api/clinic');
            clinicsTableBody.innerHTML = '';

            if (clinics && clinics.length > 0) {
                clinics.forEach(clinic => {
                    const row = document.createElement('tr');

                    // Кнопки дій в залежності від ролі
                    let actionsHtml = '';
                    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                        actionsHtml = `
                            <button class="btn btn-sm btn-primary" data-action="edit" data-id="${clinic.id}" title="Редагувати">
                                <i class="bi bi-pencil-square"></i>
                            </button>
                        `;
                    }
                    if (currentUserRole === 'Admin') {
                        actionsHtml += `
                            <button class="btn btn-sm btn-danger ms-1" data-action="delete" data-id="${clinic.id}" title="Видалити">
                                <i class="bi bi-trash"></i>
                            </button>
                        `;
                    }

                    row.innerHTML = `
                        <td>${clinic.id}</td>
                        <td>${clinic.name}</td>
                        <td>${clinic.address}</td>
                        <td>${actionsHtml || 'Недоступно'}</td>
                    `;

                    clinicsTableBody.appendChild(row);
                });
            } else {
                clinicsTableBody.innerHTML = '<tr><td colspan="4" class="text-center">Поліклініки не знайдено.</td></tr>';
            }
        } catch (error) {
            showPageError(`Не вдалося завантажити поліклініки: ${error.message}`);
        }
    }

    /**
     * Обробник кліків на головній таблиці (для кнопок "Редагувати" та "Видалити")
     */
    function handleTableClick(event) {
        const target = event.target.closest('button');
        if (!target) return;

        const action = target.dataset.action;
        const id = target.dataset.id;

        if (action === 'edit') {
            handleEditClinic(id);
        } else if (action === 'delete') {
            handleDeleteClinic(id);
        }
    }

    /**
     * Готує модальне вікно для СТВОРЕННЯ нової поліклініки
     */
    async function handleCreateClinic() {
        currentMode = 'create';
        showMainModalError(null);
        clinicForm.reset();
        clinicIdInput.value = '';

        mainModalTitle.textContent = 'Створити нову поліклініку';
        saveClinicBtn.textContent = 'Створити';

        // Завантажуємо лікарні для вибору
        await loadHospitalsForSelect(null);

        mainModal.show();
    }

    /**
     * Готує модальне вікно для РЕДАГУВАННЯ існуючої поліклініки
     */
    async function handleEditClinic(id) {
        currentMode = 'edit';
        showMainModalError(null);
        clinicForm.reset();

        mainModalTitle.textContent = 'Завантаження даних...';
        saveClinicBtn.textContent = 'Зберегти зміни';
        mainModal.show();

        try {
            // Згідно контролера, GetById повертає повну модель Clinic
            const clinic = await apiFetch(`/api/clinic/${id}`);

            if (clinic) {
                // 1. Заповнюємо поля
                clinicIdInput.value = clinic.id;
                clinicNameInput.value = clinic.name;
                clinicAddressInput.value = clinic.address;

                // 2. Завантажуємо лікарні і одразу обираємо потрібну
                await loadHospitalsForSelect(clinic.hospitalId);

                // 3. Оновлюємо заголовок
                mainModalTitle.textContent = `Редагування: ${clinic.name}`;
            } else {
                showMainModalError(`Поліклініку з ID ${id} не знайдено.`);
            }

        } catch (error) {
            showMainModalError(`Не вдалося завантажити дані поліклініки: ${error.message}`);
            mainModal.hide();
        }
    }

    /**
     * Обробник збереження форми (Створення або Оновлення)
     */
    async function handleClinicFormSubmit(event) {
        event.preventDefault();
        showMainModalError(null);

        // Збираємо дані з форми
        const name = clinicNameInput.value;
        const address = clinicAddressInput.value;
        // ParseInt, або null, якщо обрано "Не прив'язано"
        const hospitalId = clinicHospitalIdSelect.value ? parseInt(clinicHospitalIdSelect.value, 10) : null;

        try {
            if (currentMode === 'create') {
                // Створюємо нову поліклініку (використовуємо CreateClinicDto)
                const createDto = { name, address, hospitalId };

                await apiFetch('/api/clinic', {
                    method: 'POST',
                    body: JSON.stringify(createDto)
                });

            } else {
                // Оновлюємо існуючу (використовуємо повну модель Clinic, як очікує Update)
                const id = parseInt(clinicIdInput.value, 10);
                const updateModel = { id, name, address, hospitalId };

                await apiFetch(`/api/clinic/${id}`, {
                    method: 'PUT',
                    body: JSON.stringify(updateModel)
                });
            }

            // Успіх
            mainModal.hide();
            loadClinics(); // Оновлюємо головну таблицю

        } catch (error) {
            showMainModalError(`Не вдалося зберегти поліклініку: ${error.message}`);
        }
    }

    /**
     * Головна функція видалення поліклініки
     */
    async function handleDeleteClinic(id) {
        if (!confirm(`Ви впевнені, що хочете видалити поліклініку з ID: ${id}?`)) {
            return;
        }

        try {
            await apiFetch(`/api/clinic/${id}`, { method: 'DELETE' });
            loadClinics();
        } catch (error) {
            showPageError(`Помилка видалення: ${error.message}`);
        }
    }

    // --- Ініціалізація та слухачі подій ---

    // Головна таблиця
    clinicsTableBody.addEventListener('click', handleTableClick);

    // Головне модальне вікно
    addClinicBtn.addEventListener('click', handleCreateClinic);
    clinicForm.addEventListener('submit', handleClinicFormSubmit);

    // Перше завантаження даних
    loadClinics();
});