// Переконуємось, що DOM завантажено
document.addEventListener('DOMContentLoaded', () => {

    // --- Отримання елементів DOM ---
    const staffTableBody = document.getElementById('staff-table-body');
    const addStaffBtn = document.getElementById('add-staff-btn');
    const actionsHeader = document.getElementById('actions-header');
    const pageError = document.getElementById('page-error');

    // Елементи модального вікна Створення/Редагування
    const staffModalElement = document.getElementById('staff-manage-modal');
    const staffModal = new bootstrap.Modal(staffModalElement);
    const staffModalTitle = document.getElementById('staff-modal-title');
    const staffForm = document.getElementById('staff-form');
    const modalError = document.getElementById('modal-error');

    // Поля форми (основні)
    const staffIdField = document.getElementById('staff-id');
    const fullNameField = document.getElementById('fullName');
    const experienceField = document.getElementById('experience');
    const roleField = document.getElementById('role');

    // Поля форми (місце працевлаштування)
    const radioNone = document.getElementById('workplaceNone');
    const radioHospital = document.getElementById('workplaceHospital');
    const radioClinic = document.getElementById('workplaceClinic');

    const hospitalContainer = document.getElementById('hospital-select-container');
    const clinicContainer = document.getElementById('clinic-select-container');
    const hospitalSelect = document.getElementById('hospitalSelect');
    const clinicSelect = document.getElementById('clinicSelect');

    // Елементи модального вікна Профілю
    const profileModalElement = document.getElementById('profile-modal');
    const profileModal = new bootstrap.Modal(profileModalElement);
    const profileContent = document.getElementById('profile-summary-content');

    // 1. Перевірка ролі користувача
    const userRole = getUserRole(); //
    const isAdminOrOperator = userRole === 'Admin' || userRole === 'Operator';

    if (isAdminOrOperator) {
        addStaffBtn.style.display = 'block';
        actionsHeader.style.display = 'table-cell';
    }

    // --- ЛОГІКА РОБОТИ З МІСЦЯМИ РОБОТИ ---

    // Функція завантаження списків лікарень та клінік
    async function loadWorkplaces() {
        try {
            //
            // Виконуємо паралельні запити для швидкості
            const [hospitals, clinics] = await Promise.all([
                apiFetch('/api/hospital'), // Припускаємо, що такий ендпоінт є
                apiFetch('/api/clinic')    // Припускаємо, що такий ендпоінт є
            ]);

            // Заповнюємо Select лікарень
            hospitalSelect.innerHTML = '<option value="">-- Оберіть лікарню --</option>';
            if (hospitals && Array.isArray(hospitals)) {
                hospitals.forEach(h => {
                    hospitalSelect.innerHTML += `<option value="${h.id}">${h.name}</option>`;
                });
            }

            // Заповнюємо Select поліклінік
            clinicSelect.innerHTML = '<option value="">-- Оберіть поліклініку --</option>';
            if (clinics && Array.isArray(clinics)) {
                clinics.forEach(c => {
                    clinicSelect.innerHTML += `<option value="${c.id}">${c.name}</option>`;
                });
            }

        } catch (error) {
            console.error("Не вдалося завантажити списки закладів:", error);
        }
    }

    // Обробка перемикання радіо-кнопок
    function handleWorkplaceToggle() {
        if (radioHospital.checked) {
            hospitalContainer.style.display = 'block';
            clinicContainer.style.display = 'none';
            clinicSelect.value = ""; // Скидаємо вибір іншого типу
        } else if (radioClinic.checked) {
            hospitalContainer.style.display = 'none';
            clinicContainer.style.display = 'block';
            hospitalSelect.value = ""; // Скидаємо вибір іншого типу
        } else {
            // Обрано "None"
            hospitalContainer.style.display = 'none';
            clinicContainer.style.display = 'none';
            hospitalSelect.value = "";
            clinicSelect.value = "";
        }
    }

    // Додаємо слухачі подій на радіо-кнопки
    radioNone.addEventListener('change', handleWorkplaceToggle);
    radioHospital.addEventListener('change', handleWorkplaceToggle);
    radioClinic.addEventListener('change', handleWorkplaceToggle);

    // Завантажуємо списки при старті сторінки
    loadWorkplaces();


    // 2. Функція завантаження списку персоналу
    async function loadStaff() {
        staffTableBody.innerHTML = '<tr><td colspan="5" class="text-center">Завантаження...</td></tr>';

        try {
            const staffList = await apiFetch('/api/staff/support');

            staffTableBody.innerHTML = '';

            if (!staffList || staffList.length === 0) {
                staffTableBody.innerHTML = '<tr><td colspan="5" class="text-center">Персонал не знайдено.</td></tr>';
                return;
            }

            for (const staff of staffList) {
                const row = document.createElement('tr');

                let actionsHtml = `
                    
                `;

                if (isAdminOrOperator) {
                    actionsHtml += `
                        <button class="btn btn-sm btn-warning btn-edit ms-1" data-id="${staff.id}" title="Редагувати">
                            <i class="bi bi-pencil-fill"></i>
                        </button>
                        <button class="btn btn-sm btn-danger btn-delete ms-1" data-id="${staff.id}" title="Видалити">
                            <i class="bi bi-trash-fill"></i>
                        </button>
                    `;
                }

                const actionsColumnDisplay = (isAdminOrOperator || actionsHtml.includes('btn-profile')) ? '' : 'style="display: none;"';
                
                let obj = await apiFetch(`/api/employment/staff/${staff.id}`);
                let workName;
                if(obj[0] !== undefined) {
                    workName = obj[0].hospital ? obj[0].hospital.name : obj[0].clinic.name;
                }

                
                row.innerHTML = `
                    <td>${staff.id}</td>
                    <td>${staff.fullName}</td>
                    <td>${mapRoleToString(staff.role)}</td>
                    <td>${staff.workExperienceYears}</td>
                    <td>${workName}</td>
                    <td ${actionsColumnDisplay}>${actionsHtml}</td>
                `;

                staffTableBody.appendChild(row);
            }

            if (!isAdminOrOperator && staffList.length > 0) {
                actionsHeader.style.display = 'table-cell';
            }

        } catch (error) {
            showError(pageError, `Помилка завантаження: ${error.message}`);
        }
    }

    // 3. Обробник форми створення/редагування
    staffForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideError(modalError);

        const id = staffIdField.value;

        // Збираємо дані форми
        const dto = {
            fullName: fullNameField.value,
            workExperienceYears: parseInt(experienceField.value),
            role: parseInt(roleField.value),

            // Додаємо нові поля: якщо обрано відповідний тип, беремо значення селекту
            hospitalId: (radioHospital.checked && hospitalSelect.value) ? parseInt(hospitalSelect.value) : null,
            clinicId: (radioClinic.checked && clinicSelect.value) ? parseInt(clinicSelect.value) : null
        };

        if (id) {
            dto.id = parseInt(id); // Додаємо ID для оновлення
        }

        try {
            if (id) {
                // PUT /api/staff/support/{id}
                await apiFetch(`/api/staff/support/${id}`, {
                    method: 'PUT',
                    body: JSON.stringify(dto)
                });
            } else {
                // POST /api/staff/support
                await apiFetch('/api/staff/support', {
                    method: 'POST',
                    body: JSON.stringify(dto)
                });
            }

            staffModal.hide(); // Ховаємо модальне вікно
            await loadStaff(); // Оновлюємо таблицю

        } catch (error) {
            showError(modalError, `Помилка збереження: ${error.message}`);
        }
    });

    // 4. Обробка кнопок в таблиці
    staffTableBody.addEventListener('click', async (e) => {
        const target = e.target.closest('button');
        if (!target) return;

        const id = target.dataset.id;

        // Кнопка ПРОФІЛЬ
        if (target.classList.contains('btn-profile')) {
            try {
                const summary = await apiFetch(`/api/staff/support/${id}/profile-summary`);
                if (summary) {
                    profileContent.textContent = summary;
                    profileModal.show();
                }
            } catch (error) {
                showError(pageError, 'Не вдалося завантажити профіль.');
            }
        }

        // Кнопка РЕДАГУВАТИ
        if (target.classList.contains('btn-edit') && isAdminOrOperator) {
            staffModalTitle.textContent = 'Редагування співробітника';
            staffForm.reset();
            hideError(modalError);

            // Скидаємо інтерфейс вибору місця роботи
            radioNone.checked = true;
            handleWorkplaceToggle();

            try {
                // Отримуємо розширені дані співробітника (включаючи hospitalId/clinicId)
                const staff = await apiFetch(`/api/staff/support/${id}`);

                if (staff) {
                    staffIdField.value = staff.id;
                    fullNameField.value = staff.fullName;
                    experienceField.value = staff.workExperienceYears;
                    roleField.value = staff.role;

                    // Заповнюємо дані про місце роботи
                    if (staff.hospitalId) {
                        radioHospital.checked = true;
                        handleWorkplaceToggle(); // Показуємо селект
                        hospitalSelect.value = staff.hospitalId;
                    } else if (staff.clinicId) {
                        radioClinic.checked = true;
                        handleWorkplaceToggle(); // Показуємо селект
                        clinicSelect.value = staff.clinicId;
                    } else {
                        radioNone.checked = true;
                        handleWorkplaceToggle();
                    }

                    staffModal.show();
                }
            } catch (error) {
                showError(modalError, `Помилка завантаження даних: ${error.message}`);
            }
        }

        // Кнопка ВИДАЛИТИ
        if (target.classList.contains('btn-delete') && isAdminOrOperator) {
            if (confirm(`Ви впевнені, що хочете видалити співробітника ID ${id}?`)) {
                try {
                    await apiFetch(`/api/staff/support/${id}`, { method: 'DELETE' });
                    await loadStaff();
                } catch (error) {
                    showError(pageError, `Помилка видалення: ${error.message}`);
                }
            }
        }
    });

    // 5. Кнопка "Додати співробітника"
    addStaffBtn.addEventListener('click', () => {
        staffModalTitle.textContent = 'Додати нового співробітника';
        staffForm.reset();
        staffIdField.value = '';

        // Скидаємо UI до "Без прив'язки"
        radioNone.checked = true;
        handleWorkplaceToggle();

        hideError(modalError);
    });

    // 6. Допоміжна функція для відображення ролі
    function mapRoleToString(roleValue) {
        const roles = {
            "None": 'Не обрано',
            "Nurse": 'Медсестра',
            "Orderly": 'Санітар',
            "Technician": 'Технік',
            "LabAssistant": 'Лаборант',
            "Receptionist": 'Реєстратор',
            "Administrator": 'Адміністратор',
            "Cleaner": 'Прибиральник',
            "Porter": "Кур'єр/носильник",
            "Other": 'Інше'
        };
        return roles[roleValue] || 'Невідома роль';
    }

    function showError(element, message) {
        element.textContent = message;
        element.style.display = 'block';
    }

    function hideError(element) {
        element.textContent = '';
        element.style.display = 'none';
    }

    // Запуск
    loadStaff();
});