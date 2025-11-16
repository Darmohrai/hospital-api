// Глобальні змінні для зручності
const doctorSpecialties = {
    "Surgeon": "Хірург",
    "Neurologist": "Невролог",
    "Ophthalmologist": "Окуліст", // (Офтальмолог)
    "Dentist": "Стоматолог",
    "Radiologist": "Рентгенолог",
    "Gynecologist": "Гінеколог",
    "Cardiologist": "Кардіолог"
};

// Ключі тепер є числами (у вигляді рядків), що відповідають C# Enum
const academicDegrees = {
    "0": "Немає",       // AcademicDegree.None
    "1": "Кандидат наук", // AcademicDegree.Candidate
    "2": "Доктор наук"  // AcademicDegree.Doctor
};

// Ключі тепер є числами (у вигляді рядків), що відповідають C# Enum
const academicTitles = {
    "0": "Немає",            // AcademicTitle.None
    "1": "Доцент",           // AcademicTitle.AssociateProfessor
    "2": "Професор"          // AcademicTitle.Professor
};

// Змінна для зберігання ролі
let currentUserRole = null;
let doctorModal = null; // Зберігаємо екземпляр Bootstrap Modal
let employmentModal = null; // Модальне вікно для працевлаштування
let allHospitals = []; // Кеш для списку лікарень
let allClinics = []; // Кеш для списку клінік

// ✅ Змінна для поточного запиту (щоб оновлення працювало з фільтрами)
let currentDoctorQuery = '/api/staff/doctors';

// ✅ НОВИЙ БЛОК: Змінні для динамічних полів
let modalSpecialtySelect = null;
let dynamicHazardPayFields = null;
let dynamicExtendedVacationFields = null;

document.addEventListener('DOMContentLoaded', () => {
    // Ініціалізація модальних вікон
    doctorModal = new bootstrap.Modal(document.getElementById('doctor-modal'));
    employmentModal = new bootstrap.Modal(document.getElementById('employment-modal'));

    // 1. Перевірка ролі та налаштування UI
    setupUIBasedOnRole();

    // 2. Заповнення випадаючих списків
    populateSelects();

    // 3. Завантаження всіх лікарів при старті
    loadDoctors(currentDoctorQuery);

    // 4. Завантажуємо лікарні та клініки для модального вікна
    loadInstitutions();

    // ✅ НОВИЙ БЛОК: Отримуємо елементи динамічної форми
    modalSpecialtySelect = document.getElementById('doctor-specialty');
    dynamicHazardPayFields = document.getElementById('dynamic-hazard-pay-fields');
    dynamicExtendedVacationFields = document.getElementById('dynamic-extended-vacation-fields');

    // 5. Налаштування обробників подій
    document.getElementById('filter-btn').addEventListener('click', applyFilters);
    document.getElementById('create-doctor-btn').addEventListener('click', openCreateModal);
    document.getElementById('doctor-form').addEventListener('submit', handleFormSubmit);
    document.getElementById('doctors-table-body').addEventListener('click', handleTableClick);

    // Нові обробники для модального вікна "Призначити"
    document.getElementById('employment-form').addEventListener('submit', handleEmploymentFormSubmit);
    document.getElementById('employment-type').addEventListener('change', handleEmploymentTypeChange);

    // ✅ НОВИЙ БЛОК: Обробник для зміни спеціальності в модальному вікні
    if (modalSpecialtySelect) {
        modalSpecialtySelect.addEventListener('change', handleModalSpecialtyChange);
    }
});

/**
 * Перевіряє роль користувача та налаштовує UI
 */
function setupUIBasedOnRole() {
    currentUserRole = getUserRole(); // Функція з auth.js

    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
        document.getElementById('create-doctor-btn').style.display = 'block';
        document.getElementById('actions-header').style.display = 'table-cell';
    }
}

/**
 * Заповнює всі <select> на сторінці
 */
function populateSelects() {
    // Фільтри
    populateSelectOptions('filter-specialty', doctorSpecialties, "Усі");
    populateSelectOptions('filter-degree', academicDegrees, "Усі");
    populateSelectOptions('filter-title', academicTitles, "Усі");

    // Модальна форма
    populateSelectOptions('doctor-specialty', doctorSpecialties, "Оберіть спеціальність...");
    populateSelectOptions('doctor-degree', academicDegrees);
    populateSelectOptions('doctor-title', academicTitles);
}

/**
 * Допоміжна функція для заповнення <select>
 * @param {string} selectId - ID of the <select> element
 * @param {object} options - Key-value pair object
 * @param {string} defaultOptionText - Text for the default (empty value) option
 */
function populateSelectOptions(selectId, options, defaultOptionText = null) {
    const select = document.getElementById(selectId);
    if (defaultOptionText) {
        select.innerHTML = `<option value="">${defaultOptionText}</option>`;
    } else {
        select.innerHTML = '';
    }

    for (const [key, value] of Object.entries(options)) {
        select.innerHTML += `<option value="${key}">${value}</option>`;
    }
}

/**
 * Завантажує лікарів з API (з урахуванням фільтрів)
 * @param {string} endpoint - The API endpoint to fetch from
 */
async function loadDoctors(endpoint) {
    currentDoctorQuery = endpoint; // ✅ Зберігаємо поточний запит
    try {
        showError(null); // Ховаємо помилку
        const doctors = await apiFetch(endpoint);
        renderTable(doctors);
    } catch (error) {
        console.error('Error loading doctors:', error);
        showError(`Не вдалося завантажити лікарів: ${error.message}`);
    }
}

/**
 * Застосовує фільтри та перезавантажує список
 */
function applyFilters() {
    const specialty = document.getElementById('filter-specialty').value;
    const degree = document.getElementById('filter-degree').value;
    const title = document.getElementById('filter-title').value;

    const params = new URLSearchParams();
    if (specialty) {
        params.append('specialty', specialty);
    }
    if (degree !== "") { // Більш явна перевірка, що значення не є "Усі"
        params.append('degree', degree);
    }
    if (title !== "") { // Більш явна перевірка, що значення не є "Усі"
        params.append('title', title);
    }

    const queryString = params.toString();
    loadDoctors(`/api/staff/doctors?${queryString}`);
}

/**
 * Рендерить таблицю лікарів
 * @param {Array<object>} doctors - Масив об'єктів лікарів
 */
function renderTable(doctors) {
    const tableBody = document.getElementById('doctors-table-body');
    tableBody.innerHTML = ''; // Очищаємо таблицю

    if (!doctors || doctors.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="8" class="text-center">Лікарів не знайдено.</td></tr>';
        return;
    }

    doctors.forEach(doctor => {
        const row = document.createElement('tr');

        let actionsHtml = '';
        if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
            actionsHtml += `
                <button class="btn btn-primary btn-sm me-1" data-id="${doctor.id}" data-action="edit" title="Редагувати профіль">
                    Ред.
                </button>
                <button class="btn btn-info btn-sm me-1" data-id="${doctor.id}" data-name="${doctor.fullName}" data-action="assign" title="Призначити в заклад">
                    Призначити
                </button>
            `;
        }
        if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
            actionsHtml += `
                <button class="btn btn-danger btn-sm" data-id="${doctor.id}" data-action="delete" title="Видалити лікаря">
                    Видал.
                </button>`;
        }

        const degreeText = academicDegrees[doctor.academicDegree] || doctor.academicDegree;
        const titleText = academicTitles[doctor.academicTitle] || doctor.academicTitle;

        // ✅ ОНОВЛЕНА ЛОГІКА: Форматуємо місця роботи з кнопками видалення
        let employmentsHtml = '';
        if (doctor.employments && doctor.employments.length > 0) {
            employmentsHtml = doctor.employments.map(emp => {
                let badgeClass = '';
                let name = '';

                if (emp.hospitalId) {
                    badgeClass = 'bg-primary';
                    const hospital = allHospitals.find(h => h.id === emp.hospitalId);
                    name = hospital ? hospital.name : `Лікарня #${emp.hospitalId}`;
                } else if (emp.clinicId) {
                    badgeClass = 'bg-success';
                    const clinic = allClinics.find(c => c.id === emp.clinicId);
                    name = clinic ? clinic.name : `Клініка #${emp.clinicId}`;
                }

                // Додаємо кнопку видалення
                let deleteButton = '';
                if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                    deleteButton = `
                        <a href="#" class="text-white text-decoration-none" 
                           style="margin-left: 5px; font-weight: bold; vertical-align: middle;"
                           data-action="unassign"
                           data-employment-id="${emp.id}" 
                           title="Видалити призначення">&times;</a>
                    `;
                }

                return `<span class="badge ${badgeClass} rounded-pill p-2 ps-3 me-1 mb-1">${name} ${deleteButton}</span>`;

            }).join(' ');
        } else {
            employmentsHtml = '<span class="text-muted small">Не призначено</span>';
        }


        row.innerHTML = `
            <td>${doctor.id}</td>
            <td>${doctor.fullName}</td>
            <td>${doctorSpecialties[doctor.specialty] || doctor.specialty}</td>
            <td>${degreeText}</td>
            <td>${titleText}</td>
            <td>${doctor.workExperienceYears}</td>
            <td>${employmentsHtml}</td>
            ${actionsHtml ? `<td>${actionsHtml}</td>` : ''}
        `;
        tableBody.appendChild(row);
    });
}

/**
 * Обробник кліків на кнопках "Редагувати", "Видалити", "Призначити", "Видалити призначення"
 */
function handleTableClick(event) {
    // Зупиняємо стандартну поведінку (для посилань #)
    event.preventDefault();

    const target = event.target; // Елемент, на який клікнули
    const action = target.dataset.action;

    if (!action) return; // Клікнули не на кнопку дії

    const doctorId = target.closest('tr').querySelector('td').textContent; // ID лікаря з рядка

    if (action === 'edit') {
        handleEdit(doctorId);
    } else if (action === 'delete') {
        handleDelete(doctorId);
    } else if (action === 'assign') {
        const name = target.dataset.name;
        openAssignModal(doctorId, name);
    } else if (action === 'unassign') { // ✅ НОВА ДІЯ
        const employmentId = target.dataset.employmentId;
        handleUnassign(employmentId);
    }
}

/**
 * Відкриває модальне вікно для створення
 */
function openCreateModal() {
    showModalError(null);
    document.getElementById('doctor-form').reset();
    document.getElementById('doctor-id').value = '';
    document.getElementById('modal-title').textContent = 'Створити лікаря';

    document.getElementById('doctor-specialty').disabled = false;
    document.getElementById('doctor-degree').value = "0";
    document.getElementById('doctor-title').value = "0";

    // ✅ НОВИЙ БЛОК: Скидаємо та ховаємо динамічні поля
    document.getElementById('doctor-hazard-pay').value = '1.0';
    document.getElementById('doctor-extended-vacation').value = '0';
    handleModalSpecialtyChange(); // Викликаємо, щоб сховати всі поля

    doctorModal.show();
}

/**
 * Завантажує дані лікаря та відкриває модальне вікно для редагування
 * @param {number} id - ID лікаря
 */
async function handleEdit(id) {
    try {
        showModalError(null);
        // Запит йде на загальний ендпоінт, що повертає повну модель лікаря
        const doctor = await apiFetch(`/api/staff/doctors/${id}`);
        if (!doctor) {
            showError('Не вдалося знайти лікаря.');
            return;
        }

        document.getElementById('doctor-id').value = doctor.id;
        document.getElementById('modal-title').textContent = `Редагувати: ${doctor.fullName}`;
        document.getElementById('doctor-fullname').value = doctor.fullName;
        document.getElementById('doctor-specialty').value = doctor.specialty;
        document.getElementById('doctor-experience').value = doctor.workExperienceYears;
        document.getElementById('doctor-degree').value = doctor.academicDegree;
        document.getElementById('doctor-title').value = doctor.academicTitle;
        document.getElementById('doctor-specialty').disabled = true;

        // ✅ НОВИЙ БЛОК: Заповнюємо динамічні поля, якщо вони є
        // Використовуємо || (або), щоб уникнути 'null' або 'undefined' в полях
        // і встановити значення за замовчуванням
        document.getElementById('doctor-hazard-pay').value = doctor.hazardPayCoefficient || '1.0';
        document.getElementById('doctor-extended-vacation').value = doctor.extendedVacationDays || '0';

        // Показуємо потрібні поля для цієї спеціальності
        handleModalSpecialtyChange();

        doctorModal.show();

    } catch (error) {
        console.error('Error fetching doctor details:', error);
        showError(`Помилка: ${error.message}`);
    }
}

/**
 * ✅✅✅ ВИПРАВЛЕНО: Обробник відправки форми (Створення / Оновлення)
 */
async function handleFormSubmit(event) {
    event.preventDefault();
    const id = document.getElementById('doctor-id').value;
    const specialty = document.getElementById('doctor-specialty').value;

    // 1. Збираємо базові дані (які є у всіх лікарів)
    const doctorDto = {
        fullName: document.getElementById('doctor-fullname').value,
        workExperienceYears: parseInt(document.getElementById('doctor-experience').value, 10),
        academicDegree: parseInt(document.getElementById('doctor-degree').value, 10),
        academicTitle: parseInt(document.getElementById('doctor-title').value, 10),
    };

    // 2. ✅✅✅ НОВИЙ ВИПРАВЛЕНИЙ БЛОК: Додаємо унікальні дані на основі C# класів,
    // які ви надіслали в останньому повідомленні.
    switch (specialty) {
        case 'Dentist':
            // Dentist.cs має HazardPayCoefficient
            doctorDto.hazardPayCoefficient = parseFloat(document.getElementById('doctor-hazard-pay').value);
            break;
        case 'Radiologist':
            // Radiologist.cs має обидва поля
            doctorDto.hazardPayCoefficient = parseFloat(document.getElementById('doctor-hazard-pay').value);
            doctorDto.extendedVacationDays = parseInt(document.getElementById('doctor-extended-vacation').value, 10);
            break;
        case 'Neurologist':
            // Neurologist.cs має ExtendedVacationDays
            doctorDto.extendedVacationDays = parseInt(document.getElementById('doctor-extended-vacation').value, 10);
            break;
        case 'Ophthalmologist':
            // Ophthalmologist.cs має ExtendedVacationDays
            doctorDto.extendedVacationDays = parseInt(document.getElementById('doctor-extended-vacation').value, 10);
            break;
        // Surgeon, Gynecologist, Cardiologist не мають дод. полів
    }
    // ✅✅✅ КІНЕЦЬ НОВОГО БЛОКУ

    const endpoint = getApiEndpointForSpecialty(specialty);
    if (!endpoint) {
        showModalError(`Невідома спеціальність: ${specialty}`);
        return;
    }

    const method = id ? 'PUT' : 'POST';
    const url = id ? `${endpoint}/${id}` : endpoint;

    if (id) {
        doctorDto.id = parseInt(id, 10);
    }

    try {
        showModalError(null);
        if (method === 'POST') {
            await apiFetch(url, { method: 'POST', body: JSON.stringify(doctorDto) });
        } else {
            await apiFetch(url, { method: 'PUT', body: JSON.stringify(doctorDto) });
        }

        doctorModal.hide();
        loadDoctors(currentDoctorQuery); // ✅ ОНОВЛЕННЯ

    } catch (error) {
        console.error('Error saving doctor:', error);
        showModalError(`Не вдалося зберегти: ${error.message}`);
    }
}

/**
 * Видалення лікаря (профілю)
 * @param {number} id - ID лікаря
 */
async function handleDelete(id) {
    if (!confirm('Ви впевнені, що хочете видалити цього лікаря? Ця дія видалить його профіль та всі призначення.')) {
        return;
    }

    try {
        showError(null);
        await apiFetch(`/api/staff/${id}`, { method: 'DELETE' });
        loadDoctors(currentDoctorQuery); // ✅ ОНОВЛЕННЯ
    } catch (error) {
        console.error('Error deleting doctor:', error);
        showError(`Не вдалося видалити: ${error.message}`);
    }
}

/**
 * Повертає правильний API-ендпоінт на основі спеціальності
 * @param {string} specialty - Ключ спеціальності (напр. "Surgeon")
 * @returns {string} The API endpoint path
 */
function getApiEndpointForSpecialty(specialty) {
    // Ця функція вже була коректною і містила Ophthalmologist
    switch (specialty) {
        case "Surgeon": return "/api/staff/surgeons";
        case "Neurologist": return "/api/staff/neurologists";
        case "Ophthalmologist": return "/api/staff/ophthalmologists";
        case "Dentist": return "/api/staff/dentists";
        case "Radiologist": return "/api/staff/radiologists";
        case "Gynecologist": return "/api/staff/gynecologists";
        case "Cardiologist": return "/api/staff/cardiologists";
        default:
            console.error(`Unknown specialty: ${specialty}`);
            return null;
    }
}

// --- ✅✅✅ ВИПРАВЛЕНА ФУНКЦІЯ: Показ/приховування динамічних полів ---
/**
 * Обробляє зміну спеціальності в модальному вікні,
 * показуючи або приховуючи додаткові поля.
 * Логіка базується на C# класах з останнього повідомлення.
 */
function handleModalSpecialtyChange() {
    const specialty = modalSpecialtySelect.value;

    // 1. Спершу ховаємо всі динамічні поля
    dynamicHazardPayFields.style.display = 'none';
    dynamicExtendedVacationFields.style.display = 'none';

    // 2. Показуємо потрібні поля на основі спеціальності
    switch (specialty) {
        case 'Dentist':
            // Dentist.cs має HazardPayCoefficient
            dynamicHazardPayFields.style.display = 'block';
            break;
        case 'Radiologist':
            // Radiologist.cs має обидва поля
            dynamicHazardPayFields.style.display = 'block';
            dynamicExtendedVacationFields.style.display = 'block';
            break;
        case 'Neurologist':
            // Neurologist.cs має ExtendedVacationDays
            dynamicExtendedVacationFields.style.display = 'block';
            break;
        case 'Ophthalmologist':
            // Ophthalmologist.cs має ExtendedVacationDays
            dynamicExtendedVacationFields.style.display = 'block';
            break;
        // Surgeon, Gynecologist, Cardiologist 
        // не мають унікальних полів для форми
    }
}


// --- ФУНКЦІЇ ДЛЯ ПРАЦЕВЛАШТУВАННЯ ---

/**
 * Асинхронно завантажує списки лікарень та клінік
 */
async function loadInstitutions() {
    try {
        const [hospitals, clinics] = await Promise.all([
            apiFetch('/api/hospital'),
            apiFetch('/api/clinic')
        ]);
        allHospitals = hospitals;
        allClinics = clinics;
        console.log('Лікарні та клініки завантажено');
    } catch (error) {
        console.error('Не вдалося завантажити списки лікарень/клінік:', error);
        showError(`Не вдалося завантажити списки лікарень/клінік: ${error.message}. Функція призначення не працюватиме.`);
    }
}

/**
 * Відкриває модальне вікно призначення (Employment)
 * @param {string} staffId - ID лікаря
 * @param {string} staffName - ПІБ лікаря
 */
function openAssignModal(staffId, staffName) {
    showEmploymentError(null);
    document.getElementById('employment-form').reset();
    document.getElementById('employment-staff-id').value = staffId;
    document.getElementById('employment-staff-name').value = staffName;

    document.getElementById('institution-select-container').style.display = 'none';
    document.getElementById('employment-institution-id').innerHTML = '';

    employmentModal.show();
}

/**
 * Обробляє зміну типу закладу (Лікарня / Поліклініка)
 */
function handleEmploymentTypeChange() {
    const type = document.getElementById('employment-type').value;
    const institutionSelect = document.getElementById('employment-institution-id');
    const container = document.getElementById('institution-select-container');

    let optionsList = [];

    if (type === 'hospital') {
        optionsList = allHospitals;
    } else if (type === 'clinic') {
        optionsList = allClinics;
    }

    institutionSelect.innerHTML = '<option value="">Оберіть заклад...</option>';
    if (optionsList.length > 0) {
        optionsList.forEach(item => {
            institutionSelect.innerHTML += `<option value="${item.id}">${item.name} (ID: ${item.id})</option>`;
        });
        container.style.display = 'block';
    } else if (type) {
        institutionSelect.innerHTML = '<option value="">Не знайдено закладів цього типу</option>';
        container.style.display = 'block';
    } else {
        container.style.display = 'none';
    }
}

/**
 * Обробник відправки форми працевлаштування
 */
async function handleEmploymentFormSubmit(event) {
    event.preventDefault();
    showEmploymentError(null);

    const staffId = parseInt(document.getElementById('employment-staff-id').value, 10);
    const type = document.getElementById('employment-type').value;
    const institutionIdValue = document.getElementById('employment-institution-id').value;

    if (!staffId || !type || !institutionIdValue) {
        showEmploymentError('Будь ласка, заповніть усі поля.');
        return;
    }

    const institutionId = parseInt(institutionIdValue, 10);

    const employmentDto = {
        staffId: staffId,
        hospitalId: type === 'hospital' ? institutionId : null,
        clinicId: type === 'clinic' ? institutionId : null
    };

    try {
        await apiFetch('/api/employment', {
            method: 'POST',
            body: JSON.stringify(employmentDto)
        });

        employmentModal.hide();
        loadDoctors(currentDoctorQuery); // ✅ ОНОВЛЕННЯ

    } catch (error) {
        console.error('Error saving employment:', error);
        showEmploymentError(`Не вдалося призначити: ${error.message}`);
    }
}

/**
 * ✅ НОВА ФУНКЦІЯ: Видалення призначення (Employment)
 * @param {number} employmentId - ID запису про працевлаштування
 */
async function handleUnassign(employmentId) {
    if (!confirm('Ви впевнені, що хочете видалити це призначення?')) {
        return;
    }

    try {
        showError(null);
        await apiFetch(`/api/employment/${employmentId}`, { method: 'DELETE' });
        loadDoctors(currentDoctorQuery); // ✅ ОНОВЛЕННЯ
    } catch (error) {
        console.error('Error deleting employment:', error);
        showError(`Не вдалося видалити призначення: ${error.message}`);
    }
}


// --- Допоміжні функції для показу помилок ---
function showError(message) {
    const errorEl = document.getElementById('error-alert');
    if (message) {
        errorEl.textContent = message;
        errorEl.style.display = 'block';
    } else {
        errorEl.style.display = 'none';
    }
}

function showModalError(message) {
    const errorEl = document.getElementById('modal-error-alert');
    if (message) {
        errorEl.textContent = message;
        errorEl.style.display = 'block';
    } else {
        errorEl.style.display = 'none';
    }
}

function showEmploymentError(message) {
    const errorEl = document.getElementById('employment-error-alert');
    if (message) {
        errorEl.textContent = message;
        errorEl.style.display = 'block';
    } else {
        errorEl.style.display = 'none';
    }
}