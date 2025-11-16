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

    // Поля форми
    const staffIdField = document.getElementById('staff-id');
    const fullNameField = document.getElementById('fullName');
    const experienceField = document.getElementById('experience');
    const roleField = document.getElementById('role');

    // Елементи модального вікна Профілю
    const profileModalElement = document.getElementById('profile-modal');
    const profileModal = new bootstrap.Modal(profileModalElement);
    const profileContent = document.getElementById('profile-summary-content');

    // 1. Перевірка ролі користувача (з auth.js)
    const userRole = getUserRole(); // [cite: hospital-api/frontend/js/auth.js]
    const isAdminOrOperator = userRole === 'Admin' || userRole === 'Operator'; // [cite: hospital-api/Controllers/StaffControllers/SupportStaffController.cs]

    if (isAdminOrOperator) {
        // Показуємо кнопку "Додати" та колонку "Дії"
        addStaffBtn.style.display = 'block';
        actionsHeader.style.display = 'table-cell';
    }

    // 2. Функція завантаження списку персоналу (GET /)
    async function loadStaff() {
        staffTableBody.innerHTML = '<tr><td colspan="5" class="text-center">Завантаження...</td></tr>';

        try {
            // [cite: hospital-api/Controllers/StaffControllers/SupportStaffController.cs] (GET /api/staff/support)
            // Викликаємо apiFetch з одним аргументом для GET-запиту
            const staffList = await apiFetch('/api/staff/support'); // [cite: hospital-api/frontend/js/api.js]

            staffTableBody.innerHTML = ''; // Очищуємо таблицю

            if (!staffList || staffList.length === 0) {
                staffTableBody.innerHTML = '<tr><td colspan="5" class="text-center">Персонал не знайдено.</td></tr>';
                return;
            }

            staffList.forEach(staff => {
                const row = document.createElement('tr');

                // Динамічно додаємо кнопки "Дії"
                let actionsHtml = `
                    <button class="btn btn-sm btn-info btn-profile" data-id="${staff.id}" title="Профіль">
                        <i class="bi bi-person-lines-fill"></i>
                    </button>
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

                // Визначаємо, чи потрібно показувати колонку "Дії"
                const actionsColumnDisplay = (isAdminOrOperator || actionsHtml.includes('btn-profile')) ? '' : 'style="display: none;"';

                let roleName = '';

                switch (staff.role) {
                    case 'None':
                        roleName = 'Не обрано';
                        break;
                    case 'Nurse':
                        roleName = 'Медсестра';
                        break;
                    case 'Orderly':
                        roleName = 'Санітар';
                        break;
                    case 'Technician':
                        roleName = 'Технік';
                        break;
                    case 'LabAssistant':
                        roleName = 'Лаборант';
                        break;
                    case 'Receptionist':
                        roleName = 'Реєстратор';
                        break;
                    case 'Administrator':
                        roleName = 'Адміністратор';
                        break;
                    case 'Cleaner':
                        roleName = 'Прибиральник';
                        break;
                    case 'Porter':
                        roleName = 'Кур\'єр/носильник';
                        break;
                    case 'Other':
                        roleName = 'Інше';
                        break;
                    default:
                        roleName = 'Невідома роль';
                        break;
                }
                
                row.innerHTML = `
                    <td>${staff.id}</td>
                    <td>${staff.fullName}</td>
                    <td>${roleName}</td>
                    <td>${staff.workExperienceYears}</td>
                    <td ${actionsColumnDisplay}>${actionsHtml}</td>
                `;

                staffTableBody.appendChild(row);
            });

            // Якщо ми додавали кнопки профілю для не-адмінів, показуємо заголовок
            if (!isAdminOrOperator && staffList.length > 0) {
                actionsHeader.style.display = 'table-cell';
            }

        } catch (error) {
            showError(pageError, `Помилка завантаження: ${error.message}`);
        }
    }

    // 3. Обробник форми створення/редагування (POST / | PUT /{id})
    staffForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideError(modalError);

        const id = staffIdField.value;
        const dto = {
            fullName: fullNameField.value,
            workExperienceYears: parseInt(experienceField.value),
            role: parseInt(roleField.value) // [cite: hospital-api/DTOs/Staff/CreateSupportStaffDto.cs]
        };

        try {

            // --- 💡 ПОЧАТОК ВИПРАВЛЕННЯ ---
            // Ваш api.js очікує (endpoint, options)

            if (id) {
                // --- ОНОВЛЕННЯ (PUT) ---
                dto.id = parseInt(id); // Додаємо ID для оновлення

                // [cite: hospital-api/Controllers/StaffControllers/SupportStaffController.cs] (PUT /api/staff/support/{id})
                // Виправлено: передаємо options як другий аргумент
                await apiFetch(`/api/staff/support/${id}`, {
                    method: 'PUT',
                    body: JSON.stringify(dto)
                });
            } else {
                // --- СТВОРЕННЯ (POST) ---

                // [cite: hospital-api/Controllers/StaffControllers/SupportStaffController.cs] (POST /api/staff/support)
                // Виправлено: передаємо options як другий аргумент
                await apiFetch('/api/staff/support', {
                    method: 'POST',
                    body: JSON.stringify(dto)
                });
            }
            // --- 💡 КІНЕЦЬ ВИПРАВЛЕННЯ ---

            staffModal.hide(); // Ховаємо модальне вікно
            await loadStaff(); // Оновлюємо таблицю

        } catch (error) {
            showError(modalError, `Помилка збереження: ${error.message}`);
        }
    });

    // 4. Обробка кнопок в таблиці (Профіль, Редагування, Видалення)
    staffTableBody.addEventListener('click', async (e) => {
        const target = e.target.closest('button');
        if (!target) return; // Клікнули не по кнопці

        const id = target.dataset.id;

        // (GET /{id}/profile-summary)
        if (target.classList.contains('btn-profile')) {
            // [cite: hospital-api/Controllers/StaffControllers/SupportStaffController.cs] (GET {id}/profile-summary)
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

        // (GET /{id}) -> для форми Редагування
        if (target.classList.contains('btn-edit') && isAdminOrOperator) {
            staffModalTitle.textContent = 'Редагування співробітника';
            staffForm.reset();
            hideError(modalError);

            try {
                // [cite: hospital-api/Controllers/StaffControllers/SupportStaffController.cs] (GET /api/staff/support/{id})
                const staff = await apiFetch(`/api/staff/support/${id}`);
                if (staff) {
                    staffIdField.value = staff.id;
                    fullNameField.value = staff.fullName;
                    experienceField.value = staff.workExperienceYears;
                    roleField.value = staff.role;
                    staffModal.show();
                }
            } catch (error) {
                showError(modalError, `Помилка завантаження даних: ${error.message}`);
            }
        }

        // (DELETE /{id})
        if (target.classList.contains('btn-delete') && isAdminOrOperator) {
            if (confirm(`Ви впевнені, що хочете видалити співробітника ID ${id}?`)) {
                try {
                    // [cite: hospital-api/Controllers/StaffControllers/SupportStaffController.cs] (DELETE /api/staff/support/{id})
                    // Виправлено: передаємо options як другий аргумент
                    await apiFetch(`/api/staff/support/${id}`, { method: 'DELETE' });
                    await loadStaff(); // Оновлюємо таблицю
                } catch (error) {
                    showError(pageError, `Помилка видалення: ${error.message}`);
                }
            }
        }
    });

    // 5. Обробник кнопки "Додати співробітника"
    addStaffBtn.addEventListener('click', () => {
        staffModalTitle.textContent = 'Додати нового співробітника';
        staffForm.reset();
        staffIdField.value = ''; // Переконуємось, що ID порожній
        hideError(modalError);
    });

    // 6. Допоміжна функція для відображення ролі
    function mapRoleToString(roleValue) {
        // Оновлено відповідно до вашого C# Enum SupportRole
        const roles = {
            0: 'Не обрано',
            1: 'Медсестра',
            2: 'Санітар',
            3: 'Технік',
            4: 'Лаборант',
            5: 'Реєстратор',
            6: 'Адміністратор',
            7: 'Прибиральник',
            8: "Кур'єр/носильник",
            9: 'Інше'
        };
        return roles[roleValue] || 'Невідома роль';
    }

    // --- Допоміжні функції для помилок ---
    function showError(element, message) {
        element.textContent = message;
        element.style.display = 'block';
    }

    function hideError(element) {
        element.textContent = '';
        element.style.display = 'none';
    }

    // 7. Початкове завантаження даних
    loadStaff();
});