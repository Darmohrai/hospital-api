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

// Використовуємо числові ключі для внутрішньої логіки
const academicDegrees = {
    0: "Немає",
    1: "Кандидат наук",
    2: "Доктор наук"
};

const academicTitles = {
    0: "Немає",
    1: "Доцент",
    2: "Професор"
};

// Змінна для зберігання ролі
let currentUserRole = null;
let doctorModal = null; // Зберігаємо екземпляр Bootstrap Modal
let employmentModal = null; // Модальне вікно для працевлаштування
let allHospitals = []; // Кеш для списку лікарень
let allClinics = []; // Кеш для списку клінік
let allDoctors = []; // Кеш для списку лікарів

// Змінна для поточного запиту
let currentDoctorQuery = '/api/staff/doctors';

// Змінні для динамічних полів
let modalSpecialtySelect = null;
let modalDegreeSelect = null;
let modalTitleSelect = null;
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

    // 4. Завантажуємо лікарні та клініки
    loadInstitutions();

    // Отримуємо елементи динамічної форми
    modalSpecialtySelect = document.getElementById('doctor-specialty');
    modalDegreeSelect = document.getElementById('doctor-degree');
    modalTitleSelect = document.getElementById('doctor-title');
    dynamicHazardPayFields = document.getElementById('dynamic-hazard-pay-fields');
    dynamicExtendedVacationFields = document.getElementById('dynamic-extended-vacation-fields');

    // 5. Налаштування обробників подій
    document.getElementById('filter-btn').addEventListener('click', applyFilters);
    document.getElementById('create-doctor-btn').addEventListener('click', openCreateModal);
    document.getElementById('doctor-form').addEventListener('submit', handleFormSubmit);
    document.getElementById('doctors-table-body').addEventListener('click', handleTableClick);

    document.getElementById('employment-form').addEventListener('submit', handleEmploymentFormSubmit);
    document.getElementById('employment-type').addEventListener('change', handleEmploymentTypeChange);

    if (modalSpecialtySelect) {
        modalSpecialtySelect.addEventListener('change', handleModalSpecialtyChange);
    }

    if (modalDegreeSelect) {
        modalDegreeSelect.addEventListener('change', updateTitleOptions);
    }
});

/**
 * ✅ ВАЖЛИВО: Ця функція виправляє проблему з "None"
 * Перетворює рядки з API ("None", "Candidate") у числа (0, 1, 2)
 */
function normalizeEnum(value) {
    if (typeof value === 'number') return value;
    if (!value) return 0;

    const v = value.toString();
    // Перевірка для Ступенів та Звань
    if (v === "0" || v === "None") return 0;
    if (v === "1" || v === "Candidate" || v === "AssociateProfessor") return 1;
    if (v === "2" || v === "Doctor" || v === "Professor") return 2;

    return 0; // За замовчуванням 0
}

function setupUIBasedOnRole() {
    currentUserRole = getUserRole();
    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
        document.getElementById('create-doctor-btn').style.display = 'block';
        document.getElementById('actions-header').style.display = 'table-cell';
    }
}

function populateSelects() {
    populateSelectOptions('filter-specialty', doctorSpecialties, "Усі");
    // Використовуємо об'єкти academicDegrees/Titles напряму
    populateEnumSelect('filter-degree', academicDegrees, "Усі");
    populateEnumSelect('filter-title', academicTitles, "Усі");

    populateSelectOptions('doctor-specialty', doctorSpecialties, "Оберіть спеціальність...");
    populateEnumSelect('doctor-degree', academicDegrees);
    populateEnumSelect('doctor-title', academicTitles);
}

function populateSelectOptions(selectId, options, defaultOptionText = null) {
    const select = document.getElementById(selectId);
    select.innerHTML = defaultOptionText ? `<option value="">${defaultOptionText}</option>` : '';
    for (const [key, value] of Object.entries(options)) {
        select.innerHTML += `<option value="${key}">${value}</option>`;
    }
}

function populateEnumSelect(selectId, options, defaultOptionText = null) {
    const select = document.getElementById(selectId);
    select.innerHTML = defaultOptionText ? `<option value="">${defaultOptionText}</option>` : '';
    for (const [key, value] of Object.entries(options)) {
        select.innerHTML += `<option value="${key}">${value}</option>`; // key тут буде "0", "1", "2"
    }
}

async function loadDoctors(endpoint) {
    currentDoctorQuery = endpoint;
    try {
        showError(null);
        const doctors = await apiFetch(endpoint);
        allDoctors = doctors;
        renderTable(doctors);
    } catch (error) {
        console.error('Error loading doctors:', error);
        showError(`Не вдалося завантажити лікарів: ${error.message}`);
    }
}

function applyFilters() {
    const specialty = document.getElementById('filter-specialty').value;
    const degree = document.getElementById('filter-degree').value;
    const title = document.getElementById('filter-title').value;

    const params = new URLSearchParams();
    if (specialty) params.append('specialty', specialty);
    if (degree !== "") params.append('degree', degree);
    if (title !== "") params.append('title', title);

    loadDoctors(`/api/staff/doctors?${params.toString()}`);
}

async function renderTable(doctors) {
    const tableBody = document.getElementById('doctors-table-body');
    tableBody.innerHTML = '<tr><td colspan="9" class="text-center">Завантаження даних...</td></tr>';

    if (!doctors || doctors.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="9" class="text-center">Лікарів не знайдено.</td></tr>';
        return;
    }

    try {
        const rowPromises = doctors.map(async (doctor) => {
            let actionsHtml = '';
            if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                actionsHtml += `
                    <button class="btn btn-primary btn-sm me-1" data-id="${doctor.id}" data-action="edit" title="Редагувати">Ред.</button>
                    <button class="btn btn-info btn-sm me-1" data-id="${doctor.id}" data-name="${doctor.fullName}" data-action="assign" title="Призначити">Призначити</button>
                `;
            }
            if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                actionsHtml += `<button class="btn btn-danger btn-sm" data-id="${doctor.id}" data-action="delete" title="Видалити">Видал.</button>`;
            }

            // ✅ ВИПРАВЛЕНО: Нормалізуємо значення перед відображенням
            const degreeLevel = normalizeEnum(doctor.academicDegree);
            const titleLevel = normalizeEnum(doctor.academicTitle);

            const degreeText = academicDegrees[degreeLevel] || "Невідомо";
            const titleText = academicTitles[titleLevel] || "Невідомо";

            let employmentsHtml = '';
            if (doctor.employments && doctor.employments.length > 0) {
                employmentsHtml = doctor.employments.map(emp => {
                    let badgeClass = '', name = '';
                    if (emp.hospitalId) {
                        badgeClass = 'bg-primary';
                        const hospital = allHospitals.find(h => h.id === emp.hospitalId);
                        name = hospital ? hospital.name : `Лікарня #${emp.hospitalId}`;
                    } else if (emp.clinicId) {
                        badgeClass = 'bg-success';
                        const clinic = allClinics.find(c => c.id === emp.clinicId);
                        name = clinic ? clinic.name : `Клініка #${emp.clinicId}`;
                    }

                    let deleteButton = '';
                    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                        deleteButton = `<a href="#" class="text-white text-decoration-none ms-1 fw-bold" data-action="unassign" data-employment-id="${emp.id}">&times;</a>`;
                    }
                    return `<span class="badge ${badgeClass} rounded-pill p-2 me-1 mb-1">${name} ${deleteButton}</span>`;
                }).join(' ');
            } else {
                employmentsHtml = '<span class="text-muted small">Не призначено</span>';
            }

            const specialDoctorInfo = await getSpecialDoctor(doctor);

            return `
                <tr>
                    <td>${doctor.id}</td>
                    <td>${doctor.fullName}</td>
                    <td>${doctorSpecialties[doctor.specialty] || doctor.specialty}</td>
                    <td>${degreeText}</td>
                    <td>${titleText}</td>
                    <td>${doctor.workExperienceYears}</td>
                    <td>${employmentsHtml}</td>
                    <td>${specialDoctorInfo}</td>
                    ${actionsHtml ? `<td>${actionsHtml}</td>` : ''}
                </tr>
            `;
        });

        const rowHtmlStrings = await Promise.all(rowPromises);
        tableBody.innerHTML = rowHtmlStrings.join('');

    } catch (error) {
        console.error("Render error:", error);
        tableBody.innerHTML = `<tr><td colspan="9" class="text-center text-danger">Помилка відображення.</td></tr>`;
    }
}

async function getSpecialDoctor(doctor) {
    try {
        switch (doctor.specialty) {
            case "Surgeon":
                let opS = await apiFetch(`/api/operation/doctor/${doctor.id}`);
                return operationCounter(opS);
            case "Neurologist":
                let extendedVacationDays = await apiFetch(`/api/staff/neurologist/vacation/${doctor.id}`)
                return "Додаткові дні відпустки: " + extendedVacationDays;
            case "Ophthalmologist":
                let extendedVacationDaysO = await apiFetch(`/api/staff/ophthalmologists/vacation/${doctor.id}`)
                return "Додаткові дні відпустки: " + extendedVacationDaysO;
            case "Dentist":
                let opD = await apiFetch(`/api/operation/doctor/${doctor.id}`);
                return operationCounter(opD);
            case "Radiologist":
                let extendedVacationDaysR = await apiFetch(`/api/staff/radiologists/vacation/${doctor.id}`)
                let hazardPay = await apiFetch(`/api/staff/radiologists/hazard/${doctor.id}`)
                return "Додаткові дні відпустки: " + extendedVacationDaysR
                    + "<br>Плата за шкідливість: " + hazardPay;
            case "Gynecologist":
                let opG = await apiFetch(`/api/operation/doctor/${doctor.id}`);
                return operationCounter(opG);
            case "Cardiologist":
                let opC = await apiFetch(`/api/operation/doctor/${doctor.id}`);
                return operationCounter(opC);
            default:
                return "";
        }
    } catch (err) {
        return "";
    }
}

function operationCounter(operations) {
    if (!Array.isArray(operations)) return "";
    let total = operations.length;
    let fatal = operations.filter(o => o.isFatal).length;
    return `Операцій: ${total}<br>Летальних: ${fatal}`;
}

function handleTableClick(event) {
    event.preventDefault();
    const target = event.target;
    const action = target.dataset.action;
    if (!action) return;

    const doctorId = target.closest('tr').querySelector('td').textContent;

    if (action === 'edit') handleEdit(doctorId);
    else if (action === 'delete') handleDelete(doctorId);
    else if (action === 'assign') openAssignModal(doctorId, target.dataset.name);
    else if (action === 'unassign') handleUnassign(target.dataset.employmentId);
}

function openCreateModal() {
    showModalError(null);
    document.getElementById('doctor-form').reset();
    document.getElementById('doctor-id').value = '';
    document.getElementById('modal-title').textContent = 'Створити лікаря';

    document.getElementById('doctor-specialty').disabled = false;
    document.getElementById('doctor-degree').value = "0";

    updateTitleOptions(); // Скидає звання

    document.getElementById('doctor-hazard-pay').value = '1.0';
    document.getElementById('doctor-extended-vacation').value = '0';
    handleModalSpecialtyChange();
    doctorModal.show();
}

async function handleEdit(id) {
    try {
        showModalError(null);
        const doctor = await apiFetch(`/api/staff/doctors/${id}`);
        if (!doctor) { showError('Лікаря не знайдено'); return; }

        document.getElementById('doctor-id').value = doctor.id;
        document.getElementById('modal-title').textContent = `Редагувати: ${doctor.fullName}`;
        document.getElementById('doctor-fullname').value = doctor.fullName;
        document.getElementById('doctor-specialty').value = doctor.specialty;
        document.getElementById('doctor-experience').value = doctor.workExperienceYears;

        // ✅ ВИПРАВЛЕНО: Нормалізуємо значення перед встановленням в select
        const degreeVal = normalizeEnum(doctor.academicDegree);
        const titleVal = normalizeEnum(doctor.academicTitle);

        document.getElementById('doctor-degree').value = degreeVal.toString();

        // Оновлюємо доступні звання перед встановленням
        updateTitleOptions();
        document.getElementById('doctor-title').value = titleVal.toString();

        document.getElementById('doctor-specialty').disabled = true;
        document.getElementById('doctor-hazard-pay').value = doctor.hazardPayCoefficient || '1.0';
        document.getElementById('doctor-extended-vacation').value = doctor.extendedVacationDays || '0';

        handleModalSpecialtyChange();
        doctorModal.show();
    } catch (error) {
        console.error(error);
        showError(error.message);
    }
}

async function handleFormSubmit(event) {
    event.preventDefault();
    const id = document.getElementById('doctor-id').value;
    const specialty = document.getElementById('doctor-specialty').value;

    const doctorDto = {
        fullName: document.getElementById('doctor-fullname').value,
        specialty: specialty,
        workExperienceYears: parseInt(document.getElementById('doctor-experience').value, 10),
        academicDegree: parseInt(document.getElementById('doctor-degree').value, 10),
        academicTitle: parseInt(document.getElementById('doctor-title').value, 10),
    };

    if (specialty === 'Radiologist') {
        doctorDto.hazardPayCoefficient = parseFloat(document.getElementById('doctor-hazard-pay').value);
        doctorDto.extendedVacationDays = parseInt(document.getElementById('doctor-extended-vacation').value, 10);
    } else if (specialty === 'Neurologist' || specialty === 'Ophthalmologist') {
        doctorDto.extendedVacationDays = parseInt(document.getElementById('doctor-extended-vacation').value, 10);
    }

    const endpoint = getApiEndpointForSpecialty(specialty);
    const method = id ? 'PUT' : 'POST';
    const url = id ? `${endpoint}/${id}` : endpoint;

    if (id) doctorDto.id = parseInt(id, 10);

    try {
        showModalError(null);
        await apiFetch(url, { method: method, body: JSON.stringify(doctorDto) });
        doctorModal.hide();
        loadDoctors(currentDoctorQuery);
    } catch (error) {
        showModalError(error.message);
    }
}

async function handleDelete(id) {
    if (!confirm('Видалити цього лікаря?')) return;
    try {
        await apiFetch(`/api/staff/${id}`, { method: 'DELETE' });
        loadDoctors(currentDoctorQuery);
    } catch (error) {
        showError(error.message);
    }
}

function getApiEndpointForSpecialty(specialty) {
    const map = {
        "Surgeon": "/api/staff/surgeons",
        "Neurologist": "/api/staff/neurologist",
        "Ophthalmologist": "/api/staff/ophthalmologists",
        "Dentist": "/api/staff/dentists",
        "Radiologist": "/api/staff/radiologists",
        "Gynecologist": "/api/staff/gynecologists",
        "Cardiologist": "/api/staff/cardiologists"
    };
    return map[specialty] || null;
}

function handleModalSpecialtyChange() {
    const specialty = modalSpecialtySelect.value;
    dynamicHazardPayFields.style.display = 'none';
    dynamicExtendedVacationFields.style.display = 'none';

    if (specialty === 'Radiologist') {
        dynamicHazardPayFields.style.display = 'block';
        dynamicExtendedVacationFields.style.display = 'block';
    } else if (specialty === 'Neurologist' || specialty === 'Ophthalmologist') {
        dynamicExtendedVacationFields.style.display = 'block';
    }
}

function updateTitleOptions() {
    const degree = parseInt(modalDegreeSelect.value || 0, 10);
    modalTitleSelect.innerHTML = `<option value="0">${academicTitles[0]}</option>`;

    if (degree >= 1) modalTitleSelect.innerHTML += `<option value="1">${academicTitles[1]}</option>`; // Доцент
    if (degree === 2) modalTitleSelect.innerHTML += `<option value="2">${academicTitles[2]}</option>`; // Професор

    // Якщо поточне значення недоступне, скидаємо
    const options = Array.from(modalTitleSelect.options).map(o => o.value);
    if (!options.includes(modalTitleSelect.value)) modalTitleSelect.value = "0";
}

// --- ПРАЦЕВЛАШТУВАННЯ ---

async function loadInstitutions() {
    try {
        const [h, c] = await Promise.all([apiFetch('/api/hospital'), apiFetch('/api/clinic')]);
        allHospitals = h;
        allClinics = c;
    } catch (e) { showError("Помилка завантаження закладів."); }
}

function openAssignModal(staffId, staffName) {
    showEmploymentError(null);
    document.getElementById('employment-form').reset();
    document.getElementById('employment-staff-id').value = staffId;
    document.getElementById('employment-staff-name').value = staffName;
    document.getElementById('institution-select-container').style.display = 'none';
    employmentModal.show();
}

function handleEmploymentTypeChange() {
    const type = document.getElementById('employment-type').value;
    const select = document.getElementById('employment-institution-id');
    const container = document.getElementById('institution-select-container');

    let list = (type === 'hospital') ? allHospitals : (type === 'clinic') ? allClinics : [];

    select.innerHTML = list.length ? '<option value="">Оберіть заклад...</option>' : '<option value="">Немає даних</option>';
    list.forEach(i => select.innerHTML += `<option value="${i.id}">${i.name} (ID: ${i.id})</option>`);

    container.style.display = type ? 'block' : 'none';
}

/**
 * ✅ ВИПРАВЛЕНО: Логіка сумісництва з використанням normalizeEnum
 */
async function handleEmploymentFormSubmit(event) {
    event.preventDefault();
    showEmploymentError(null);

    const staffId = parseInt(document.getElementById('employment-staff-id').value, 10);
    const type = document.getElementById('employment-type').value;
    const instId = parseInt(document.getElementById('employment-institution-id').value, 10);

    if (!staffId || !type || !instId) {
        showEmploymentError('Заповніть усі поля.');
        return;
    }

    const doctor = allDoctors.find(d => d.id === staffId);
    if (!doctor) return showEmploymentError("Помилка даних лікаря.");

    const employments = doctor.employments || [];

    // 1. Перевірка дублікатів
    if (employments.some(e => (type === 'hospital' && e.hospitalId === instId) || (type === 'clinic' && e.clinicId === instId))) {
        return showEmploymentError("Лікар вже працює в цьому закладі.");
    }

    // 2. Логіка сумісництва
    // ✅ Перетворюємо "None"/"Professor" у 0/2 для коректного порівняння
    const titleLevel = normalizeEnum(doctor.academicTitle);

    if (titleLevel === 0) { // Немає звання
        if (type === 'hospital' && employments.some(e => e.hospitalId)) {
            return showEmploymentError("Без звання можна працювати лише в одній лікарні.");
        }
        if (type === 'clinic' && employments.some(e => e.clinicId)) {
            return showEmploymentError("Без звання можна працювати лише в одній поліклініці.");
        }
    }
    // Якщо titleLevel >= 1, обмежень немає

    try {
        await apiFetch('/api/employment', {
            method: 'POST',
            body: JSON.stringify({ staffId, hospitalId: type === 'hospital' ? instId : null, clinicId: type === 'clinic' ? instId : null })
        });
        employmentModal.hide();
        loadDoctors(currentDoctorQuery);
    } catch (e) { showEmploymentError(e.message); }
}

async function handleUnassign(id) {
    if (!confirm('Видалити призначення?')) return;
    try { await apiFetch(`/api/employment/${id}`, { method: 'DELETE' }); loadDoctors(currentDoctorQuery); }
    catch (e) { showError(e.message); }
}

function showError(msg) { const e = document.getElementById('error-alert'); e.innerText = msg || ''; e.style.display = msg ? 'block' : 'none'; }
function showModalError(msg) { const e = document.getElementById('modal-error-alert'); e.innerText = msg || ''; e.style.display = msg ? 'block' : 'none'; }
function showEmploymentError(msg) { const e = document.getElementById('employment-error-alert'); e.innerText = msg || ''; e.style.display = msg ? 'block' : 'none'; }