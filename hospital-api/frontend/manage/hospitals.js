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

    // --- Глобальні константи ---
    const ALL_SPECIALIZATIONS = [
        'Surgeon',
        'Neurologist',
        'Ophthalmologist',
        'Dentist',
        'Radiologist',
        'Gynecologist',
        'Cardiologist'
    ];

    // --- Елементи DOM ---
    const hospitalsTableBody = document.getElementById('hospitals-table-body');
    const addHospitalBtn = document.getElementById('add-hospital-btn');
    const pageError = document.getElementById('page-error');

    // --- Головне модальне вікно ---
    const mainModalEl = document.getElementById('hospital-manage-modal');
    const mainModal = new bootstrap.Modal(mainModalEl);
    const mainModalTitle = document.getElementById('hospital-modal-title');
    const mainModalError = document.getElementById('modal-error');
    const hospitalForm = document.getElementById('hospital-form');
    const hospitalIdInput = document.getElementById('hospital-id');
    const hospitalNameInput = document.getElementById('hospital-name');
    const hospitalAddressInput = document.getElementById('hospital-address');
    const hospitalSpecsContainer = document.getElementById('hospital-specializations-container');
    const saveHospitalBtn = document.getElementById('save-hospital-btn');

    // Вкладки
    const tabMainInfo = new bootstrap.Tab(document.getElementById('tab-main-info'));
    const tabStructure = document.getElementById('tab-structure');
    const tabStructureContent = document.getElementById('tab-structure-content');
    const structureAccordion = document.getElementById('structure-accordion');
    const structureLoading = document.getElementById('structure-loading');

    // --- Допоміжні модальні вікна ---

    // 1. Додати Корпус
    const addBuildingModalEl = document.getElementById('add-building-modal');
    const addBuildingModal = new bootstrap.Modal(addBuildingModalEl);
    const addBuildingForm = document.getElementById('add-building-form');
    const addBuildingError = document.getElementById('add-building-error');

    // 2. Додати Відділення
    const addDepartmentModalEl = document.getElementById('add-department-modal');
    const addDepartmentModal = new bootstrap.Modal(addDepartmentModalEl);
    const addDepartmentForm = document.getElementById('add-department-form');
    const addDepartmentError = document.getElementById('add-department-error');
    const addDepartmentSpecSelect = document.getElementById('add-department-specialization');

    // 3. Додати Палату
    const addRoomModalEl = document.getElementById('add-room-modal');
    const addRoomModal = new bootstrap.Modal(addRoomModalEl);
    const addRoomForm = document.getElementById('add-room-form');
    const addRoomError = document.getElementById('add-room-error');

    // 4. Додати Ліжка
    const addBedsModalEl = document.getElementById('add-beds-modal');
    const addBedsModal = new bootstrap.Modal(addBedsModalEl);
    const addBedsForm = document.getElementById('add-beds-form');
    const addBedsError = document.getElementById('add-beds-error');

    // 5. Редагувати Відділення
    const editDepartmentModalEl = document.getElementById('edit-department-modal');
    const editDepartmentModal = editDepartmentModalEl ? new bootstrap.Modal(editDepartmentModalEl) : null;
    const editDepartmentForm = document.getElementById('edit-department-form');
    const editDepartmentError = document.getElementById('edit-department-error');
    const editDepartmentIdInput = document.getElementById('edit-department-id');
    const editDepartmentNameInput = document.getElementById('edit-department-name');
    const editDepartmentSpecSelect = document.getElementById('edit-department-specialization');
    const editDepartmentBuildingIdInput = document.getElementById('edit-department-building-id');


    // --- Стан ---
    let currentMode = 'create'; // 'create' або 'edit'
    let currentHospitalData = null; // Тут зберігається повне дерево лікарні
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
     * Показує/ховає помилку в головному модальному вікні
     */
    function showMainModalError(message) {
        mainModalError.textContent = message;
        mainModalError.style.display = message ? 'block' : 'none';
    }

    /**
     * Динамічно генерує чекбокси спеціалізацій
     */
    function populateHospitalSpecsCheckboxes(specs = []) {
        hospitalSpecsContainer.innerHTML = ''; // Очищуємо
        ALL_SPECIALIZATIONS.forEach(spec => {
            const isChecked = specs.includes(spec);
            hospitalSpecsContainer.innerHTML += `
                <div class="form-check">
                    <input class="form-check-input" type="checkbox" value="${spec}" id="spec-${spec}" ${isChecked ? 'checked' : ''}>
                    <label class="form-check-label" for="spec-${spec}">
                        ${spec}
                    </label>
                </div>
            `;
        });
    }

    /**
     * Отримує обрані спеціалізації з чекбоксів
     * @returns {string[]} Масив обраних спеціалізацій
     */
    function getSelectedSpecs() {
        const specs = [];
        hospitalSpecsContainer.querySelectorAll('input[type="checkbox"]:checked').forEach(chk => {
            specs.push(chk.value);
        });
        return specs;
    }

    /**
     * Завантажує список лікарень у головну таблицю
     */
    async function loadHospitals() {
        try {
            showPageError(null);
            const hospitals = await apiFetch('/api/hospital');
            hospitalsTableBody.innerHTML = '';

            if (hospitals && hospitals.length > 0) {
                hospitals.forEach(hospital => {
                    const row = document.createElement('tr');
                    const specsHtml = hospital.specializations?.length
                        ? hospital.specializations.map(s => `<span class="badge bg-secondary me-1">${s}</span>`).join(' ')
                        : '<span class="text-muted small">Не вказано</span>';

                    // Кнопки дій в залежності від ролі
                    let actionsHtml = '';
                    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                        actionsHtml = `
                            <button class="btn btn-sm btn-primary" data-action="manage" data-id="${hospital.id}" title="Керувати">
                                <i class="bi bi-pencil-square"></i>
                            </button>
                        `;
                    }
                    if (currentUserRole === 'Admin') {
                        actionsHtml += `
                            <button class="btn btn-sm btn-danger ms-1" data-action="delete" data-id="${hospital.id}" title="Видалити">
                                <i class="bi bi-trash"></i>
                            </button>
                        `;
                    }

                    row.innerHTML = `
                        <td>${hospital.id}</td>
                        <td>${hospital.name}</td>
                        <td>${hospital.address}</td>
                        <td>${specsHtml}</td>
                        <td>${actionsHtml || 'Недоступно'}</td>
                    `;

                    hospitalsTableBody.appendChild(row);
                });
            } else {
                hospitalsTableBody.innerHTML = '<tr><td colspan="5" class="text-center">Лікарні не знайдено.</td></tr>';
            }
        } catch (error) {
            showPageError(`Не вдалося завантажити лікарні: ${error.message}`);
        }
    }

    /**
     * Обробник кліків на головній таблиці (для кнопок "Керувати" та "Видалити")
     */
    function handleTableClick(event) {
        const target = event.target.closest('button');
        if (!target) return;

        const action = target.dataset.action;
        const id = target.dataset.id;

        if (action === 'manage') {
            handleManage(id);
        } else if (action === 'delete') {
            handleDelete(id);
        }
    }

    /**
     * Готує модальне вікно для СТВОРЕННЯ нової лікарні
     */
    function handleCreate() {
        currentMode = 'create';
        currentHospitalData = null; // Немає даних
        showMainModalError(null);

        hospitalForm.reset();
        hospitalIdInput.value = '';
        populateHospitalSpecsCheckboxes([]); // Порожні чекбокси

        mainModalTitle.textContent = 'Створити нову лікарню';
        saveHospitalBtn.textContent = 'Створити і перейти до структури';

        // Налаштування вкладок
        tabStructure.setAttribute('disabled', 'true');
        tabMainInfo.show(); // Примусово показуємо першу вкладку

        mainModal.show();
    }

    /**
     * Готує модальне вікно для КЕРУВАННЯ (Редагування) існуючої лікарні
     */
    async function handleManage(id) {
        currentMode = 'edit';
        showMainModalError(null);

        mainModalTitle.textContent = 'Завантаження даних лікарні...';
        structureAccordion.innerHTML = '';
        structureLoading.style.display = 'block';

        // Вимикаємо вкладку структури, поки дані не завантажено
        tabStructure.setAttribute('disabled', 'true');
        tabMainInfo.show();
        mainModal.show();

        try {
            // ✅ ГОЛОВНИЙ ЗАПИТ: Отримуємо все дерево даних
            const hospital = await apiFetch(`/api/hospital/${id}`);
            currentHospitalData = hospital; // Зберігаємо повні дані

            // 1. Заповнюємо вкладку "Основна інформація"
            hospitalIdInput.value = hospital.id;
            hospitalNameInput.value = hospital.name;
            hospitalAddressInput.value = hospital.address;
            populateHospitalSpecsCheckboxes(hospital.specializations);

            // 2. Рендеримо вкладку "Структура"
            await renderHospitalStructure(hospital);

            // 3. Налаштовуємо модальне вікно
            mainModalTitle.textContent = `Керування: ${hospital.name}`;
            saveHospitalBtn.textContent = 'Зберегти зміни';
            tabStructure.removeAttribute('disabled');

        } catch (error) {
            showMainModalError(`Не вдалося завантажити дані лікарні: ${error.message}`);
        } finally {
            structureLoading.style.display = 'none';
        }
    }

    /**
     * Обробник збереження з вкладки "Основна інформація"
     */
    async function handleHospitalFormSubmit(event) {
        event.preventDefault();
        showMainModalError(null);

        const specs = getSelectedSpecs();
        if (specs.length === 0) {
            showMainModalError('Необхідно обрати хоча б одну спеціалізацію.');
            return;
        }

        const hospitalBody = {
            id: currentHospitalData ? currentHospitalData.id : 0,
            name: hospitalNameInput.value,
            address: hospitalAddressInput.value,
            specializations: specs
        };

        try {
            if (currentMode === 'create') {
                // Створюємо нову лікарню
                const newHospital = await apiFetch('/api/hospital', {
                    method: 'POST',
                    body: JSON.stringify(hospitalBody)
                });

                // Успіх! Переводимо модальне вікно в режим "Редагування"
                currentMode = 'edit';
                currentHospitalData = newHospital; // Зберігаємо щойно створену лікарню
                hospitalIdInput.value = newHospital.id;

                mainModalTitle.textContent = `Керування: ${newHospital.name}`;
                saveHospitalBtn.textContent = 'Зберегти зміни';
                tabStructure.removeAttribute('disabled');

                // Переключаємо на вкладку структури
                bootstrap.Tab.getInstance(tabStructure).show();

                // Рендеримо (порожню) структуру
                await renderHospitalStructure(newHospital);
                loadHospitals(); // Оновлюємо головну таблицю у фоні

            } else {
                // Оновлюємо існуючу лікарню
                await apiFetch(`/api/hospital/${currentHospitalData.id}`, {
                    method: 'PUT',
                    body: JSON.stringify(hospitalBody)
                });

                // Оновлюємо заголовок (на випадок зміни назви)
                mainModalTitle.textContent = `Керування: ${hospitalBody.name}`;
                // Оновлюємо кешовані дані (особливо спеціалізації)
                currentHospitalData.name = hospitalBody.name;
                currentHospitalData.address = hospitalBody.address;
                currentHospitalData.specializations = hospitalBody.specializations;

                // Оновлюємо рендер, бо спеціалізації могли змінитись (впливає на модалки)
                await renderHospitalStructure(currentHospitalData);

                loadHospitals(); // Оновлюємо головну таблицю
                showMainModalError(null); // Сховати помилку
            }
        } catch (error) {
            showMainModalError(`Не вдалося зберегти лікарню: ${error.message}`);
        }
    }

    /**
     * Головна функція рендерингу. Будує дерево акордеонів.
     */
    async function renderHospitalStructure(hospital) {
        structureAccordion.innerHTML = ''; // Очищуємо
        if (!hospital.buildings || hospital.buildings.length === 0) {
            structureAccordion.innerHTML = '<p class="text-center text-muted">Корпуси відсутні. Додайте перший корпус.</p>';
            return;
        }

        // 1. Створюємо масив промісів (кожен запитує дані одного корпусу)
        const buildingPromises = hospital.buildings.map(building =>
            createBuildingAccordionItem(building)
        );

        // 2. Чекаємо, поки ВСІ вони не виконаються паралельно
        const buildingHtmlArray = await Promise.all(buildingPromises);

        // 3. Додаємо весь готовий HTML на сторінку одним махом
        structureAccordion.innerHTML = buildingHtmlArray.join('');
    }

    async function createBuildingAccordionItem(building) {
        const departments = await apiFetch(`/api/hospital/department/building/${building.id}/with-rooms`, {
            method: 'GET'
        });

        console.log(departments.length);

        let departmentsHtml;

        if (departments && departments.length > 0) {
            // 1. Створюємо масив промісів
            const departmentPromises = departments.map(dept => createDepartmentAccordionItem(dept));

            // 2. Чекаємо на виконання ВСІХ промісів
            const resolvedDepartmentsHtmlArray = await Promise.all(departmentPromises);

            // 3. З'єднуємо результати
            departmentsHtml = resolvedDepartmentsHtmlArray.join('');

        } else {
            departmentsHtml = `<div class="p-3 text-muted small">Відділення відсутні.
                             <button class="btn btn-sm btn-outline-success" data-action="add-department" data-building-id="${building.id}">
                                 <i class="bi bi-plus-lg"></i> Додати відділення
                             </button>
                           </div>`;
        }

        // Повертаємо HTML для самого корпусу
        return `
        <div class="accordion-item">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#b-${building.id}">
                    <i class="bi bi-building me-2"></i> Корпус: ${building.name}
                </button>
                <div class="btn-group">
                    <button class="btn btn-sm btn-outline-success" data-action="add-department" data-building-id="${building.id}" title="Додати Відділення">
                        <i class="bi bi-plus-lg"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" data-action="delete-building" data-id="${building.id}" title="Видалити Корпус">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </h2>
            <div id="b-${building.id}" class="accordion-collapse collapse" data-bs-parent="#structure-accordion">
                <div class="accordion-body">
                    <div class="accordion">
                        ${departmentsHtml} </div>
                </div>
            </div>
        </div>`;
    }

    async function createDepartmentAccordionItem(dept) {
        let roomsHtml;

        if (dept.rooms && dept.rooms.length > 0) {
            // 1. Створюємо масив промісів. 
            // Кожен елемент - це виклик createRoomAccordionItem(room), який повертає проміс.
            const roomPromises = dept.rooms.map(room => createRoomAccordionItem(room));

            // 2. Чекаємо, поки ВСІ проміси в масиві не виконаються.
            const resolvedRoomsHtmlArray = await Promise.all(roomPromises);

            // 3. З'єднуємо масив готових HTML-рядків.
            roomsHtml = resolvedRoomsHtmlArray.join('');

        } else {
            // Якщо dept.rooms порожній або відсутній
            roomsHtml = `<div class="p-3 text-muted small">Палати відсутні.
                         <button class="btn btn-sm btn-outline-success" data-action="add-room" data-department-id="${dept.id}">
                             <i class="bi bi-plus-lg"></i> Додати палату
                         </button>
                       </div>`;
        }

        // ВАЖЛИВО: Переконайтеся, що ваш API повертає buildingId для кожного департаменту
        const buildingId = dept.buildingId || (dept.room ? dept.room.department.buildingId : currentHospitalData.buildings.find(b => b.departments.some(d => d.id === dept.id))?.id);

        return `
        <div class="accordion-item">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#d-${dept.id}">
                    <i class="bi bi-heart-pulse me-2"></i> Відділення: ${dept.name} (${dept.specialization})
                </button>
                <div class="btn-group">
                    <button class="btn btn-sm btn-outline-primary" 
                            data-action="edit-department" 
                            data-id="${dept.id}" 
                            data-name="${dept.name}"
                            data-specialization="${dept.specialization}"
                            data-building-id="${buildingId}"
                            data-bs-toggle="modal" 
                            data-bs-target="#edit-department-modal"
                            title="Редагувати Відділення">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-success" data-action="add-room" data-department-id="${dept.id}" title="Додати Палату">
                        <i class="bi bi-plus-lg"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" data-action="delete-department" data-id="${dept.id}" title="Видалити Відділення">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </h2>
            <div id="d-${dept.id}" class="accordion-collapse collapse">
                <div class="accordion-body">
                    <div class="accordion">
                        ${roomsHtml} </div>
                </div>
            </div>
        </div>`;
    }

    async function createRoomAccordionItem(room) {
        // ✅ ВИПРАВЛЕНО: Додано початковий слеш (/)
        const beds = await apiFetch(`/api/hospital/bed/room/${room.id}`, {
            method: 'GET'
        });

        console.log(beds)
        // ✅ ВИПРАВЛЕНО: Використовуємо 'beds.map' (змінну, яку завантажили), 
        // а не 'room.beds.map'
        const bedsHtml = beds?.length
            ? beds.map(createBedItem).join('')
            : 'Ліжка відсутні.';

        const bedCount = beds?.length || 0; // Рахуємо на основі завантажених даних
        const capacityColor = bedCount > room.capacity ? 'text-danger' : (bedCount === room.capacity ? 'text-success' : 'text-warning');
        
        return `
            <div class="accordion-item">
                <h2 class="accordion-header">
                    <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#r-${room.id}">
                        <i class="bi bi-door-open me-2"></i> Палата: ${room.number} 
                        <span class="badge ${capacityColor} ms-2">(${bedCount} / ${room.capacity} ліжок)</span>
                    </button>
                    <div class="btn-group">
                        <button class="btn btn-sm btn-outline-success" data-action="add-beds" data-room-id="${room.id}" data-room-name="${room.number}" data-room-capacity="${room.capacity}" title="Додати Ліжка">
                            <i class="bi bi-plus-lg"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger" data-action="delete-room" data-id="${room.id}" title="Видалити Палату">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </h2>
                <div id="r-${room.id}" class="accordion-collapse collapse">
                    <div class="accordion-body">
                        <ul class="list-group">
                            ${bedsHtml}
                        </ul>
                    </div>
                </div>
            </div>`;
    }

    function createBedItem(bed) {
        patient = bed.isOccupied ? "зайняте" : "вільне"
        return `
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <i class="bi bi-bed me-2"></i> ${patient} (ID: ${bed.id})
                <button class="btn btn-sm btn-outline-danger" data-action="delete-bed" data-id="${bed.id}" title="Видалити Ліжко">
                    <i class="bi bi-trash"></i>
                </button>
            </li>`;
    }

    /**
     * Головний обробник кліків для вкладки "Структура"
     */
    function handleStructureClick(event) {
        const target = event.target.closest('button');
        if (!target) return;

        const action = target.dataset.action;
        const id = target.dataset.id;

        // Зупиняємо спливання, якщо кнопка НЕ data-bs-toggle (щоб не закрити акордеон)
        if (!target.dataset.bsToggle) {
            event.stopPropagation();
        }

        switch (action) {
            case 'add-building':
                openAddBuildingModal();
                break;
            case 'delete-building':
                handleDeleteStructureItem('building', id);
                break;
            case 'add-department':
                openAddDepartmentModal(target.dataset.buildingId);
                break;
            case 'edit-department':
                openEditDepartmentModal(target.dataset); // dataset містить всі data-* атрибути
                break;
            case 'delete-department':
                handleDeleteStructureItem('department', id);
                break;
            case 'add-room':
                openAddRoomModal(target.dataset.departmentId);
                break;
            case 'delete-room':
                handleDeleteStructureItem('room', id);
                break;
            case 'add-beds':
                openAddBedsModal(target.dataset.roomId, target.dataset.roomName, target.dataset.roomCapacity);
                break;
            case 'delete-bed':
                handleDeleteStructureItem('bed', id);
                break;
        }
    }

    /**
     * Оновлює дерево структури (заново запитує дані лікарні)
     */
    async function refreshStructure() {
        if (!currentHospitalData || !currentHospitalData.id) return;

        structureLoading.style.display = 'block';
        structureAccordion.innerHTML = '';
        try {
            const hospital = await apiFetch(`/api/hospital/${currentHospitalData.id}`);
            currentHospitalData = hospital; // Оновлюємо кеш
            await renderHospitalStructure(hospital);
        } catch (error) {
            showMainModalError(`Не вдалося оновити структуру: ${error.message}`);
        } finally {
            structureLoading.style.display = 'none';
        }
    }

    // --- Обробники для ДОПОМІЖНИХ модальних вікон ---

    // КОРПУС
    function openAddBuildingModal() {
        addBuildingForm.reset();
        addBuildingError.style.display = 'none';
        document.getElementById('add-building-hospital-id').value = currentHospitalData.id;
        // Модальне вікно відкривається через data-bs-toggle в HTML
    }

    async function handleAddBuildingSubmit(event) {
        event.preventDefault();
        const hospitalId = document.getElementById('add-building-hospital-id').value;
        const number = document.getElementById('add-building-number').value;

        try {
            await apiFetch('/api/hospital/building', {
                method: 'POST',
                body: JSON.stringify({ name: number, hospitalId: parseInt(hospitalId) })
            });
            addBuildingModal.hide();
            refreshStructure(); // Оновлюємо дерево
        } catch (error) {
            addBuildingError.textContent = error.message;
            addBuildingError.style.display = 'block';
        }
    }

    // ВІДДІЛЕННЯ (ДОДАВАННЯ)
    function openAddDepartmentModal(buildingId) {
        addDepartmentForm.reset();
        addDepartmentError.style.display = 'none';
        document.getElementById('add-department-building-id').value = buildingId;

        // Заповнюємо <select> спеціалізаціями лікарні
        addDepartmentSpecSelect.innerHTML = '<option value="" disabled selected>Оберіть...</option>';
        currentHospitalData.specializations.forEach(spec => {
            addDepartmentSpecSelect.innerHTML += `<option value="${spec}">${spec}</option>`;
        });

        // ✅ ВИПРАВЛЕНО: Кнопка не має data-bs-toggle, тому відкриваємо вручну
        addDepartmentModal.show();
    }

    async function handleAddDepartmentSubmit(event) {
        event.preventDefault();
        const buildingId = document.getElementById('add-department-building-id').value;
        const name = document.getElementById('add-department-name').value;
        const specialization = document.getElementById('add-department-specialization').value;

        if (!specialization) {
            addDepartmentError.textContent = 'Оберіть спеціалізацію';
            addDepartmentError.style.display = 'block';
            return;
        }

        try {
            await apiFetch('/api/hospital/department', {
                method: 'POST',
                body: JSON.stringify({name: name, specialization: specialization, buildingId: parseInt(buildingId)})
            });
            addDepartmentModal.hide();
            refreshStructure(); // Оновлюємо дерево
        } catch (error) {
            addDepartmentError.textContent = error.message;
            addDepartmentError.style.display = 'block';
        }
    }

    // ВІДДІЛЕННЯ (РЕДАГУВАННЯ)
    function openEditDepartmentModal(dataset) {
        if (!editDepartmentModal) {
            console.error("Модальне вікно 'edit-department-modal' не знайдено в HTML!");
            return;
        }

        editDepartmentForm.reset();
        editDepartmentError.style.display = 'none';

        // Заповнюємо поля даними з кнопки
        editDepartmentIdInput.value = dataset.id;
        editDepartmentNameInput.value = dataset.name;
        editDepartmentBuildingIdInput.value = dataset.buildingId;

        // Заповнюємо <select> і обираємо поточну спеціалізацію
        editDepartmentSpecSelect.innerHTML = '<option value="" disabled>Оберіть...</option>';
        currentHospitalData.specializations.forEach(spec => {
            const selected = (spec === dataset.specialization) ? 'selected' : '';
            editDepartmentSpecSelect.innerHTML += `<option value="${spec}" ${selected}>${spec}</option>`;
        });

        // Модальне вікно відкривається через data-bs-toggle на кнопці
    }

    // ОБРОБНИК РЕДАГУВАННЯ ВІДДІЛЕННЯ
    async function handleEditDepartmentSubmit(event) {
        event.preventDefault();

        const id = editDepartmentIdInput.value;
        const name = editDepartmentNameInput.value;
        const specialization = editDepartmentSpecSelect.value;
        const buildingId = editDepartmentBuildingIdInput.value;

        if (!specialization) {
            editDepartmentError.textContent = 'Оберіть спеціалізацію';
            editDepartmentError.style.display = 'block';
            return;
        }

        try {
            await apiFetch(`/api/department/${id}`, {
                method: 'PUT',
                body: JSON.stringify({
                    id: parseInt(id),
                    name,
                    specialization,
                    buildingId: parseInt(buildingId)
                })
            });
            editDepartmentModal.hide();
            refreshStructure(); // Оновлюємо дерево
        } catch (error) {
            editDepartmentError.textContent = error.message;
            editDepartmentError.style.display = 'block';
        }
    }


    // ПАЛАТА
    function openAddRoomModal(departmentId) {
        addRoomForm.reset();
        addRoomError.style.display = 'none';
        document.getElementById('add-room-department-id').value = departmentId;

        // ✅ ВИПРАВЛЕНО: Кнопка не має data-bs-toggle, тому відкриваємо вручну
        addRoomModal.show();
    }

    async function handleAddRoomSubmit(event) {
        event.preventDefault();
        const departmentId = document.getElementById('add-room-department-id').value;
        const number = document.getElementById('add-room-number').value;
        const capacity = parseInt(document.getElementById('add-room-capacity').value, 10);

        try {
            await apiFetch('/api/room', {
                method: 'POST',
                body: JSON.stringify({number, capacity, departmentId: parseInt(departmentId)})
            });
            addRoomModal.hide();
            refreshStructure(); // Оновлюємо дерево
        } catch (error) {
            addRoomError.textContent = error.message;
            addRoomError.style.display = 'block';
        }
    }

    // ЛІЖКА
    function openAddBedsModal(roomId, roomName, roomCapacity) {
        addBedsForm.reset();
        addBedsError.style.display = 'none';
        document.getElementById('add-beds-room-id').value = roomId;
        document.getElementById('add-beds-room-name').textContent = roomName;
        document.getElementById('add-beds-room-capacity').textContent = roomCapacity;
        document.getElementById('add-beds-count').value = 1;
        document.getElementById('add-beds-count').max = roomCapacity; // TODO: треба рахувати вільні

        // ✅ ВИПРАВЛЕНО: Кнопка не має data-bs-toggle, тому відкриваємо вручну
        addBedsModal.show();
    }

    async function handleAddBedsSubmit(event) {
        event.preventDefault();
        const roomId = parseInt(document.getElementById('add-beds-room-id').value, 10);
        const count = parseInt(document.getElementById('add-beds-count').value, 10);

        // TODO: Додати валідацію на основі (capacity - currentBeds)

        try {
            const bedPromises = [];
            for (let i = 1; i <= count; i++) {
                bedPromises.push(apiFetch('/api/hospital/bed', {
                    method: 'POST',
                    body: JSON.stringify({number: `Ліжко-${Date.now() + i}`, roomId}) // Унікальне ім'я
                }));
            }
            await Promise.all(bedPromises);

            addBedsModal.hide();
            refreshStructure(); // Оновлюємо дерево
        } catch (error) {
            addBedsError.textContent = error.message;
            addBedsError.style.display = 'block';
        }
    }

    /**
     * Універсальна функція видалення (для корпусів, відділень і т.д.)
     */
    async function handleDeleteStructureItem(itemType, id) {
        const typeMap = {
            building: {name: 'корпус', endpoint: 'building'},
            department: {name: 'відділення', endpoint: 'department'},
            room: {name: 'палату', endpoint: 'room'},
            bed: {name: 'ліжко', endpoint: 'bed'},
        };
        const itemInfo = typeMap[itemType];
        if (!itemInfo) return;

        if (!confirm(`Ви впевнені, що хочете видалити це ${itemInfo.name} (ID: ${id})? Це видалить ВСІ вкладені елементи!`)) {
            return;
        }

        try {
            showMainModalError(null);
            await apiFetch(`/api/hospital/${itemInfo.endpoint}/${id}`, {method: 'DELETE'});
            refreshStructure(); // Оновлюємо дерево
        } catch (error) {
            showMainModalError(`Помилка видалення: ${error.message}`);
        }
    }

    /**
     * Головна функція видалення лікарні
     */
    async function handleDelete(id) {
        if (!confirm('ВИ ВПЕВНЕНІ?\n\nЦе видалить лікарню, всі її корпуси, відділення, палати та ліжка. Ця дія НЕОБОРОТНА.')) {
            return;
        }

        try {
            await apiFetch(`/api/hospital/${id}`, {method: 'DELETE'});
            loadHospitals();
        } catch (error) {
            showPageError(`Помилка видалення: ${error.message}`);
        }
    }

    // --- Ініціалізація та слухачі подій ---

    // Ініціалізуємо модальні вікна (для ручного керування)
    // (Переміщено вище, щоб мати до них доступ)

    populateHospitalSpecsCheckboxes(); // Заповнюємо чекбокси при завантаженні

    // Головна таблиця
    hospitalsTableBody.addEventListener('click', handleTableClick);

    // Головне модальне вікно
    addHospitalBtn.addEventListener('click', handleCreate);
    hospitalForm.addEventListener('submit', handleHospitalFormSubmit);
    tabStructureContent.addEventListener('click', handleStructureClick);

    // Допоміжні модальні вікна (форми)
    addBuildingForm.addEventListener('submit', handleAddBuildingSubmit);
    addDepartmentForm.addEventListener('submit', handleAddDepartmentSubmit);
    addRoomForm.addEventListener('submit', handleAddRoomSubmit);
    addBedsForm.addEventListener('submit', handleAddBedsSubmit);

    if (editDepartmentForm) {
        editDepartmentForm.addEventListener('submit', handleEditDepartmentSubmit);
    } else {
        console.warn("Форма 'edit-department-form' не знайдена. Редагування не працюватиме.");
    }

    // Перше завантаження даних
    loadHospitals();
});