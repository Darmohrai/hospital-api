document.addEventListener('DOMContentLoaded', () => {

    // --- Перевірка залежностей ---
    if (typeof apiFetch === 'undefined') { /* ... */ return; }
    if (typeof bootstrap === 'undefined') { /* ... */ return; }
    if (typeof TomSelect === 'undefined') { /* ... */ return; }

    // ✅ 1. ДОДАЄМО СПИСОК ПРОФІЛІВ
    // Ти можеш з часом завантажувати цей список з API,
    // але поки почнемо з константи.
    const ALL_LAB_PROFILES = [
        { value: 'Загальний аналіз крові', text: 'Загальний аналіз крові' },
        { value: 'Біохімічний аналіз крові', text: 'Біохімічний аналіз крові' },
        { value: 'Аналіз сечі', text: 'Аналіз сечі' },
        { value: 'Гормональна панель', text: 'Гормональна панель' },
        { value: 'Аналіз на інфекції', text: 'Аналіз на інфекції' },
        { value: 'Цитологія', text: 'Цитологія' },
    ];


    // --- Елементи DOM ---
    const laboratoriesTableBody = document.getElementById('laboratories-table-body');
    const addLaboratoryBtn = document.getElementById('add-laboratory-btn');
    const pageError = document.getElementById('page-error');

    // --- Модальне вікно ---
    const mainModalEl = document.getElementById('laboratory-manage-modal');
    const mainModal = new bootstrap.Modal(mainModalEl);
    const mainModalTitle = document.getElementById('laboratory-modal-title');
    const mainModalError = document.getElementById('modal-error');
    const laboratoryForm = document.getElementById('laboratory-form');
    const laboratoryIdInput = document.getElementById('laboratory-id');
    const laboratoryNameInput = document.getElementById('laboratory-name');
    // ❌ (Видалено: laboratoryProfileInput)
    const saveLaboratoryBtn = document.getElementById('save-laboratory-btn');

    // --- Стан ---
    let currentMode = 'create';
    let currentUserRole = getUserRole();
    let allHospitals = [];
    let allClinics = [];

    // ✅ 2. ДОДАЄМО ЗМІННУ ДЛЯ TomSelect ПРОФІЛІВ
    let tomSelectHospitals, tomSelectClinics, tomSelectProfiles;

    // --- Функції ---

    function showPageError(message) { /* ... */ }
    function showMainModalError(message) { /* ... */ }

    /**
     * ✅ 3. ОНОВЛЕНО: Ініціалізуємо ВСІ три TomSelect
     */
    async function initializeSelects() {
        try {
            // Запити для лікарень та клінік
            [allHospitals, allClinics] = await Promise.all([
                apiFetch('/api/hospital'),
                apiFetch('/api/clinic')
            ]);

            // Ініціалізуємо Tom Select для Лікарень (без створення)
            tomSelectHospitals = new TomSelect('#laboratory-hospitals', {
                plugins: ['remove_button'],
                options: allHospitals.map(h => ({ value: h.id, text: h.name })),
                create: false,
                preload: true
            });

            // Ініціалізуємо Tom Select для Клінік (без створення)
            tomSelectClinics = new TomSelect('#laboratory-clinics', {
                plugins: ['remove_button'],
                options: allClinics.map(c => ({ value: c.id, text: c.name })),
                create: false,
                preload: true
            });

            // ✅ Ініціалізуємо Tom Select для Профілів (ЗІ СТВОРЕННЯМ)
            // Це дозволить обирати зі списку АБО вписувати нові
            tomSelectProfiles = new TomSelect('#laboratory-profile-select', {
                plugins: ['remove_button'],
                options: ALL_LAB_PROFILES, // Використовуємо наш список
                create: true, // Дозволяє додавати нові елементи
                preload: true
            });

        } catch (error) {
            console.error("Не вдалося завантажити дані для селекторів:", error);
            showMainModalError("Не вдалося завантажити списки лікарень та клінік.");
        }
    }

    /**
     * Завантажує список лабораторій у головну таблицю
     */
    async function loadLaboratories() {
        // ... (Ця функція залишається БЕЗ ЗМІН, 
        // оскільки вона вже коректно рендерила `lab.profile` як список)
        try {
            showPageError(null);
            const laboratories = await apiFetch('/api/laboratory');
            laboratoriesTableBody.innerHTML = '';

            if (laboratories && laboratories.length > 0) {
                laboratories.forEach(lab => {
                    const row = document.createElement('tr');

                    const profilesHtml = lab.profile?.length
                        ? lab.profile.map(p => `<span class="badge bg-info me-1">${p}</span>`).join(' ')
                        : '<span class="text-muted small">Немає</span>';

                    const hospitalsHtml = lab.hospitals?.length
                        ? lab.hospitals.map(h => `<span class="badge bg-success me-1">${h.name}</span>`).join(' ')
                        : '';
                    const clinicsHtml = lab.clinics?.length
                        ? lab.clinics.map(c => `<span class="badge bg-primary me-1">${c.name}</span>`).join(' ')
                        : '';

                    let actionsHtml = '';
                    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                        actionsHtml = `<button class="btn btn-sm btn-primary" data-action="edit" data-id="${lab.id}" title="Редагувати"><i class="bi bi-pencil-square"></i></button>`;
                    }
                    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                        actionsHtml += `<button class="btn btn-sm btn-danger ms-1" data-action="delete" data-id="${lab.id}" title="Видалити"><i class="bi bi-trash"></i></button>`;
                    }

                    row.innerHTML = `
                        <td>${lab.id}</td>
                        <td>${lab.name}</td>
                        <td>${profilesHtml}</td>
                        <td>${hospitalsHtml} ${clinicsHtml}</td>
                        <td>${actionsHtml || 'Недоступно'}</td>
                    `;
                    laboratoriesTableBody.appendChild(row);
                });
            } else {
                laboratoriesTableBody.innerHTML = '<tr><td colspan="5" class="text-center">Лабораторії не знайдено.</td></tr>';
            }
        } catch (error) {
            showPageError(`Не вдалося завантажити лабораторії: ${error.message}`);
        }
    }

    function handleTableClick(event) {
        // ... (Без змін)
        const target = event.target.closest('button');
        if (!target) return;
        const action = target.dataset.action;
        const id = target.dataset.id;

        if (action === 'edit') {
            handleEditLaboratory(id);
        } else if (action === 'delete') {
            handleDeleteLaboratory(id);
        }
    }

    /**
     * ✅ 4. ОНОВЛЕНО: Готує модальне вікно для СТВОРЕННЯ
     */
    async function handleCreateLaboratory() {
        currentMode = 'create';
        showMainModalError(null);
        laboratoryForm.reset();
        laboratoryIdInput.value = '';

        mainModalTitle.textContent = 'Створити нову лабораторію';
        saveLaboratoryBtn.textContent = 'Створити';

        // Очищуємо всі селектори
        if (tomSelectHospitals) tomSelectHospitals.clear();
        if (tomSelectClinics) tomSelectClinics.clear();
        if (tomSelectProfiles) tomSelectProfiles.clear();

        mainModal.show();
    }

    /**
     * ✅ 5. ОНОВЛЕНО: Готує модальне вікно для РЕДАГУВАННЯ
     */
    async function handleEditLaboratory(id) {
        currentMode = 'edit';
        showMainModalError(null);
        laboratoryForm.reset();

        mainModalTitle.textContent = 'Завантаження даних...';
        saveLaboratoryBtn.textContent = 'Зберегти зміни';
        mainModal.show();

        try {
            // GetById тепер повертає { ..., profile: ["Аналіз 1", "Аналіз 2"], ... }
            const lab = await apiFetch(`/api/laboratory/${id}`);

            if (lab) {
                // 1. Заповнюємо поля
                laboratoryIdInput.value = lab.id;
                laboratoryNameInput.value = lab.name;
                // ❌ (Поле профілю тепер - select)

                // 2. Встановлюємо значення для TomSelect
                if (tomSelectHospitals) tomSelectHospitals.setValue(lab.hospitalIds || []);
                if (tomSelectClinics) tomSelectClinics.setValue(lab.clinicIds || []);

                // Встановлюємо профілі.
                // Нам потрібно додати нові опції, якщо їх немає у списку
                if (tomSelectProfiles) {
                    (lab.profile || []).forEach(profileName => {
                        tomSelectProfiles.addOption({ value: profileName, text: profileName });
                    });
                    tomSelectProfiles.setValue(lab.profile || []);
                }

                mainModalTitle.textContent = `Редагування: ${lab.name}`;
            } else {
                showMainModalError(`Лабораторію з ID ${id} не знайдено.`);
            }
        } catch (error) {
            showMainModalError(`Не вдалося завантажити дані: ${error.message}`);
            mainModal.hide();
        }
    }

    /**
     * ✅ 6. ОНОВЛЕНО: Обробник збереження форми
     */
    async function handleLaboratoryFormSubmit(event) {
        event.preventDefault();
        showMainModalError(null);

        // Отримуємо значення з TomSelect
        const hospitalIds = Array.from(tomSelectHospitals.getValue()).map(v => parseInt(v, 10));
        const clinicIds = Array.from(tomSelectClinics.getValue()).map(v => parseInt(v, 10));
        const profiles = Array.from(tomSelectProfiles.getValue()); // Це вже список рядків

        // Збираємо дані у формат DTO
        const dto = {
            name: laboratoryNameInput.value,
            profile: profiles, // <-- Передаємо список рядків
            hospitalIds: hospitalIds,
            clinicIds: clinicIds
        };

        try {
            if (currentMode === 'create') {
                await apiFetch('/api/laboratory', {
                    method: 'POST',
                    body: JSON.stringify(dto)
                });
            } else {
                const id = parseInt(laboratoryIdInput.value, 10);
                await apiFetch(`/api/laboratory/${id}`, {
                    method: 'PUT',
                    body: JSON.stringify(dto)
                });
            }

            mainModal.hide();
            loadLaboratories(); // Оновлюємо таблицю

        } catch (error) {
            showMainModalError(`Не вдалося зберегти: ${error.message}`);
        }
    }

    async function handleDeleteLaboratory(id) {
        // ... (Без змін)
        if (!confirm(`Ви впевнені, що хочете видалити лабораторію з ID: ${id}?`)) {
            return;
        }
        try {
            await apiFetch(`/api/laboratory/${id}`, { method: 'DELETE' });
            loadLaboratories();
        } catch (error) {
            showPageError(`Помилка видалення: ${error.message}`);
        }
    }

    // --- Ініціалізація ---
    addLaboratoryBtn.addEventListener('click', handleCreateLaboratory);
    laboratoriesTableBody.addEventListener('click', handleTableClick);
    laboratoryForm.addEventListener('submit', handleLaboratoryFormSubmit);

    async function initializePage() {
        // Запускаємо обидва завантаження паралельно
        // (initializeSelects тепер містить завантаження)
        await Promise.all([
            initializeSelects(), // Завантажить дані та ініціалізує ВСІ Tom Select
            loadLaboratories()   // Завантажить таблицю
        ]);
    }

    initializePage();
});