// frontend/manage/tracking.js

// Глобальні змінні (кеш)
let currentUserRole = null;
let trackingModal = null;
let allClinics = [];
let allHospitals = [];
let allDoctors = []; // Очікується, що містить .employments
let allPatients = []; // Очікується, що містить .clinicId та .hospitalId

// Глобальні змінні (дані)
let allAppointments = [];
let allAdmissions = [];
let allOperations = [];

/**
 * Головна функція, що виконується при завантаженні сторінки
 */
document.addEventListener('DOMContentLoaded', () => {
    // 1. Перевірка залежностей
    if (typeof apiFetch === 'undefined') {
        console.error("КРИТИЧНА ПОМИЛКА: 'apiFetch' не знайдено.");
        alert("Помилка: Не вдалося завантажити API модуль. Перезавантажте сторінку.");
        return;
    }
    if (typeof bootstrap === 'undefined' || typeof bootstrap.Modal === 'undefined') {
        console.error("КРИТИЧНА ПОМИЛКА: 'bootstrap' не знайдено.");
        alert("Помилка: Не вдалося завантажити інтерфейс. Перезавантажте сторінку.");
        return;
    }

    // Ініціалізація модального вікна
    const modalEl = document.getElementById('tracking-modal');
    if (modalEl) {
        trackingModal = new bootstrap.Modal(modalEl);
    }

    setupUIBasedOnRole();
    loadInitialData();
    setupEventListeners();
});

/**
 * Перевіряє роль користувача та налаштовує UI
 */
function setupUIBasedOnRole() {
    currentUserRole = getUserRole(); // Функція з auth.js

    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
        const btnGroup = document.getElementById('create-buttons-group');
        if (btnGroup) btnGroup.style.display = 'block';

        // Показуємо колонки "Дії" для всіх таблиць
        const headers = ['actions-header-app', 'actions-header-adm', 'actions-header-op'];
        headers.forEach(id => {
            const el = document.getElementById(id);
            if (el) el.style.display = 'table-cell';
        });
    }
}

/**
 * Налаштування обробників подій
 */
function setupEventListeners() {
    // Кнопки "Створити"
    const btnApp = document.getElementById('create-appointment-btn');
    if (btnApp) btnApp.addEventListener('click', () => openCreateModal('appointment'));

    const btnAdm = document.getElementById('create-admission-btn');
    if (btnAdm) btnAdm.addEventListener('click', () => openCreateModal('admission'));

    const btnOp = document.getElementById('create-operation-btn');
    if (btnOp) btnOp.addEventListener('click', () => openCreateModal('operation'));

    // Форма
    const form = document.getElementById('tracking-form');
    if (form) form.addEventListener('submit', handleFormSubmit);

    // Каскадні дропдауни
    document.getElementById('tracking-patient')?.addEventListener('change', handlePatientChange);
    document.getElementById('tracking-location')?.addEventListener('change', handleLocationChange);
    document.getElementById('tracking-hospital')?.addEventListener('change', handleHospitalChange);

    // Обробники кліків на таблицях (делегування)
    document.getElementById('appointments-table-body')?.addEventListener('click', (e) => handleTableClick(e, 'appointment'));
    document.getElementById('admissions-table-body')?.addEventListener('click', (e) => handleTableClick(e, 'admission'));
    document.getElementById('operations-table-body')?.addEventListener('click', (e) => handleTableClick(e, 'operation'));
}


/**
 * Завантажує всі необхідні початкові дані (кеш)
 */
async function loadInitialData() {
    try {
        // 1. Завантажуємо кеш (пацієнти, лікарі, локації)
        const [patients, doctors, clinics, hospitals] = await Promise.all([
            apiFetch('/api/patient/all-with-associations'),
            apiFetch('/api/staff/doctors'),
            apiFetch('/api/clinic'),
            apiFetch('/api/hospital')
        ]);

        allPatients = patients || [];
        allDoctors = doctors || [];
        allClinics = clinics || [];
        allHospitals = hospitals || [];

        // Заповнюємо головний список пацієнтів
        const patientSelect = document.getElementById('tracking-patient');
        if (patientSelect) {
            patientSelect.innerHTML = '<option value="">Оберіть пацієнта...</option>';
            allPatients.forEach(p => {
                patientSelect.innerHTML += `<option value="${p.id}">${p.id}: ${p.fullName}</option>`;
            });
        }

        // Заповнюємо список лікарень (для госпіталізації)
        const hospitalSelect = document.getElementById('tracking-hospital');
        if (hospitalSelect) {
            hospitalSelect.innerHTML = '<option value="">Оберіть лікарню...</option>';
            allHospitals.forEach(h => {
                hospitalSelect.innerHTML += `<option value="${h.id}">${h.name}</option>`;
            });
        }

        // 2. Завантажуємо дані для таблиць
        const [appointments, admissions, operations] = await Promise.all([
            apiFetch('/api/appointment'),
            apiFetch('/api/admission'),
            apiFetch('/api/operation')
        ]);

        allAppointments = appointments || [];
        allAdmissions = admissions || [];
        allOperations = operations || [];

        // 3. Рендеримо таблиці
        renderAllTables();

    } catch (error) {
        showError(`Не вдалося завантажити дані: ${error.message}`);
    }
}

// ===================================================================
// ЛОГІКА РЕНДЕРИНГУ ТАБЛИЦЬ
// ===================================================================

function renderAllTables() {
    renderAppointmentsTable(allAppointments);
    renderAdmissionsTable(allAdmissions);
    renderOperationsTable(allOperations);
}

function renderAppointmentsTable(data) {
    const tableBody = document.getElementById('appointments-table-body');
    if (!tableBody) return;
    tableBody.innerHTML = '';

    if (!data || data.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="7" class="text-center">Записів не знайдено.</td></tr>';
        return;
    }

    data.forEach(item => {
        const patient = allPatients.find(p => p.id === item.patientId);
        const doctor = allDoctors.find(d => d.id === item.doctorId);

        let location = '<span class="text-danger">Н/Д</span>';
        if (item.clinicId) {
            const clinic = allClinics.find(c => c.id === item.clinicId);
            location = `(К) ${clinic?.name || 'Н/Д'}`;
        } else if (item.hospitalId) {
            const hospital = allHospitals.find(h => h.id === item.hospitalId);
            location = `(Л) ${hospital?.name || 'Н/Д'}`;
        }

        const actions = (currentUserRole === 'Admin' || currentUserRole === 'Operator') ? `
            <td>
                <button class="btn btn-primary btn-sm me-1" data-id="${item.id}" data-action="edit" title="Редагувати">Ред.</button>
                <button class="btn btn-danger btn-sm" data-id="${item.id}" data-action="delete" title="Видалити">Видал.</button>
            </td>` : '';

        const row = `
            <tr>
                <td>${item.id}</td>
                <td>${patient?.fullName || 'Н/Д'}</td>
                <td>${doctor?.fullName || 'Н/Д'}</td>
                <td>${location}</td>
                <td>${new Date(item.visitDateTime).toLocaleString()}</td>
                <td>${item.summary ? item.summary.substring(0, 50) : '...'}</td>
                ${actions}
            </tr>`;
        tableBody.innerHTML += row;
    });
}

function renderAdmissionsTable(data) {
    const tableBody = document.getElementById('admissions-table-body');
    if (!tableBody) return;
    tableBody.innerHTML = '';

    if (!data || data.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="7" class="text-center">Госпіталізацій не знайдено.</td></tr>';
        return;
    }

    data.forEach(item => {
        const patient = allPatients.find(p => p.id === item.patientId);
        const doctor = allDoctors.find(d => d.id === item.attendingDoctorId);
        const hospital = allHospitals.find(h => h.id === item.hospitalId);

        const actions = (currentUserRole === 'Admin' || currentUserRole === 'Operator') ? `
            <td>
                <button class="btn btn-warning btn-sm me-1" data-id="${item.id}" data-action="edit" title="Виписати / Редагувати">Виписати</button>
                <button class="btn btn-danger btn-sm" data-id="${item.id}" data-action="delete" title="Видалити запис">Видал.</button>
            </td>` : '';

        const dischargeDate = item.dischargeDate ? new Date(item.dischargeDate).toLocaleString() : '<span class="badge bg-success">В стаціонарі</span>';

        const row = `
            <tr>
                <td>${item.id}</td>
                <td>${patient?.fullName || 'Н/Д'}</td>
                <td>${hospital?.name || 'Н/Д'}</td>
                <td>${doctor?.fullName || 'Н/Д'}</td>
                <td>${new Date(item.admissionDate).toLocaleString()}</td>
                <td>${dischargeDate}</td>
                ${actions}
            </tr>`;
        tableBody.innerHTML += row;
    });
}

function renderOperationsTable(data) {
    const tableBody = document.getElementById('operations-table-body');
    if (!tableBody) return;
    tableBody.innerHTML = '';

    if (!data || data.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="8" class="text-center">Операцій не знайдено.</td></tr>';
        return;
    }

    data.forEach(item => {
        const patient = allPatients.find(p => p.id === item.patientId);
        const doctor = allDoctors.find(d => d.id === item.doctorId);

        let location = '<span class="text-danger">Н/Д</span>';
        if (item.clinicId) {
            const clinic = allClinics.find(c => c.id === item.clinicId);
            location = `(К) ${clinic?.name || 'Н/Д'}`;
        } else if (item.hospitalId) {
            const hospital = allHospitals.find(h => h.id === item.hospitalId);
            location = `(Л) ${hospital?.name || 'Н/Д'}`;
        }

        const result = item.isFatal ? '<span class="badge bg-danger">Фатальний</span>' : '<span class="badge bg-success">Успішно</span>';

        const actions = (currentUserRole === 'Admin' || currentUserRole === 'Operator') ? `
            <td>
                <button class="btn btn-primary btn-sm me-1" data-id="${item.id}" data-action="edit" title="Редагувати">Ред.</button>
                <button class="btn btn-danger btn-sm" data-id="${item.id}" data-action="delete" title="Видалити">Видал.</button>
            </td>` : '';

        const row = `
            <tr>
                <td>${item.id}</td>
                <td>${patient?.fullName || 'Н/Д'}</td>
                <td>${doctor?.fullName || 'Н/Д'}</td>
                <td>${location}</td>
                <td>${new Date(item.date).toLocaleString()}</td>
                <td>${item.type}</td>
                <td>${result}</td>
                ${actions}
            </tr>`;
        tableBody.innerHTML += row;
    });
}

// ===================================================================
// ЛОГІКА МОДАЛЬНОГО ВІКНА ТА ФОРМ
// ===================================================================

/**
 * Відкриває модальне вікно для СТВОРЕННЯ
 */
function openCreateModal(mode) {
    showModalError(null);
    document.getElementById('tracking-form').reset();

    // Скидаємо 'required' та 'disabled'
    document.querySelectorAll('#tracking-form [required]').forEach(el => el.required = false);
    document.querySelectorAll('#tracking-form input, #tracking-form select').forEach(el => el.disabled = false);

    document.getElementById('tracking-id').value = '';
    document.getElementById('tracking-mode').value = mode;

    setupModalForMode(mode);

    // Скидаємо залежні дропдауни
    resetLocationSelect(true);
    resetDoctorSelect(true);

    trackingModal.show();
}

/**
 * Асинхронно відкриває модальне вікно для РЕДАГУВАННЯ
 */
async function handleEdit(id, mode) {
    showModalError(null);
    document.getElementById('tracking-form').reset();
    document.getElementById('tracking-id').value = id;
    document.getElementById('tracking-mode').value = mode;

    // Скидаємо disabled перед налаштуванням
    document.querySelectorAll('#tracking-form input, #tracking-form select').forEach(el => el.disabled = false);

    setupModalForMode(mode, true); // true = isEdit

    try {
        const endpoint = getEndpoint(mode);
        const item = await apiFetch(`${endpoint}/${id}`);

        // Заповнюємо пацієнта (і запускаємо каскад)
        document.getElementById('tracking-patient').value = item.patientId;
        await handlePatientChange();

        if (mode === 'appointment') {
            const locationValue = item.clinicId ? `clinic-${item.clinicId}` : `hospital-${item.hospitalId}`;
            document.getElementById('tracking-location').value = locationValue;
            await handleLocationChange();

            document.getElementById('tracking-doctor').value = item.doctorId;
            document.getElementById('tracking-datetime').value = toLocalISOString(new Date(item.visitDateTime));
            document.getElementById('tracking-summary').value = item.summary;

        } else if (mode === 'admission') {
            // ✅ ЛОГІКА ВИПИСКИ (DISCHARGE)
            document.getElementById('modal-title').textContent = `Виписати пацієнта`;
            document.getElementById('tracking-hospital').value = item.hospitalId;
            await handleHospitalChange();

            document.getElementById('tracking-doctor').value = item.attendingDoctorId;
            document.getElementById('tracking-datetime').value = toLocalISOString(new Date(item.admissionDate));

            // Блокуємо поля, які не можна змінювати при виписці
            document.getElementById('tracking-patient').disabled = true;
            document.getElementById('tracking-hospital').disabled = true;
            document.getElementById('tracking-doctor').disabled = true;
            document.getElementById('tracking-datetime').disabled = true; // Дата госпіталізації

            // Дата виписки
            if (item.dischargeDate) {
                document.getElementById('tracking-dischargedate').value = toLocalISOString(new Date(item.dischargeDate));
            } else {
                // Якщо ще не виписаний, ставимо поточний час
                document.getElementById('tracking-dischargedate').value = toLocalISOString(new Date());
            }
            // Робимо дату виписки обов'язковою
            document.getElementById('tracking-dischargedate').required = true;


        } else if (mode === 'operation') {
            const locationValue = item.clinicId ? `clinic-${item.clinicId}` : `hospital-${item.hospitalId}`;
            document.getElementById('tracking-location').value = locationValue;
            await handleLocationChange();

            document.getElementById('tracking-doctor').value = item.doctorId;
            document.getElementById('tracking-datetime').value = toLocalISOString(new Date(item.date));
            document.getElementById('tracking-optype').value = item.type;
            document.getElementById('tracking-isfatal').checked = item.isFatal;
        }

        trackingModal.show();
    } catch (error) {
        showError(`Не вдалося завантажити дані для редагування: ${error.message}`);
    }
}


/**
 * Налаштовує вигляд модального вікна (поля, заголовки)
 */
function setupModalForMode(mode, isEdit = false) {
    const titlePrefix = isEdit ? 'Редагувати' : 'Створити';

    // Отримуємо всі керовані поля
    const patientSelect = document.getElementById('tracking-patient');
    const locationSelect = document.getElementById('tracking-location');
    const hospitalSelect = document.getElementById('tracking-hospital');
    const doctorSelect = document.getElementById('tracking-doctor');
    const datetimeInput = document.getElementById('tracking-datetime');
    const opTypeInput = document.getElementById('tracking-optype');

    // Ховаємо всі специфічні групи полів
    document.querySelectorAll('.form-group-specific').forEach(el => el.style.display = 'none');

    // Скидаємо 'required' для специфічних полів
    locationSelect.required = false;
    hospitalSelect.required = false;
    opTypeInput.required = false;

    // Загальні поля
    patientSelect.disabled = isEdit;
    patientSelect.required = true;
    doctorSelect.required = true;
    datetimeInput.required = true;

    document.getElementById('form-group-patient').style.display = 'block';
    document.getElementById('form-group-doctor').style.display = 'block';
    document.getElementById('form-group-datetime').style.display = 'block';


    if (mode === 'appointment') {
        document.getElementById('modal-title').textContent = `${titlePrefix} запис на прийом`;
        document.getElementById('tracking-doctor-label').textContent = 'Лікар';
        document.getElementById('tracking-datetime-label').textContent = 'Дата/Час візиту';
        document.getElementById('form-group-location').style.display = 'block';
        document.getElementById('form-group-summary').style.display = 'block';
        locationSelect.required = true;

    } else if (mode === 'admission') {
        document.getElementById('modal-title').textContent = `${titlePrefix} госпіталізацію`;
        document.getElementById('tracking-doctor-label').textContent = 'Лікуючий лікар';
        document.getElementById('tracking-datetime-label').textContent = 'Дата госпіталізації';

        document.getElementById('form-group-hospital').style.display = 'block';

        // Поле виписки показуємо завжди в режимі admission
        document.getElementById('form-group-dischargedate').style.display = 'block';

        hospitalSelect.required = true;

    } else if (mode === 'operation') {
        document.getElementById('modal-title').textContent = `${titlePrefix} операцію`;
        document.getElementById('tracking-doctor-label').textContent = 'Лікар (Хірург)';
        document.getElementById('tracking-datetime-label').textContent = 'Дата/Час операції';
        document.getElementById('form-group-location').style.display = 'block';
        document.getElementById('form-group-optype').style.display = 'block';
        document.getElementById('form-group-isfatal').style.display = 'block';
        locationSelect.required = true;
        opTypeInput.required = true;
    }
}

// ===================================================================
// ЛОГІКА КАСКАДНИХ ДРОПДАУНІВ
// ===================================================================

function handlePatientChange() {
    const patientId = document.getElementById('tracking-patient').value;
    resetLocationSelect();
    resetDoctorSelect();

    if (!patientId) {
        resetLocationSelect(true);
        return;
    }

    const patient = allPatients.find(p => p.id == patientId);
    if (!patient) return;

    const locationSelect = document.getElementById('tracking-location');

    // Безпечна перевірка clinicId та hospitalId
    if (patient.clinicId) {
        // Якщо об'єкт clinic не підвантажився, шукаємо в allClinics
        const clinic = patient.clinic || allClinics.find(c => c.id === patient.clinicId);
        if (clinic) {
            locationSelect.innerHTML += `<option value="clinic-${clinic.id}">${clinic.name} (Поліклініка)</option>`;
        }
    }

    if (patient.hospitalId) {
        const hospital = patient.hospital || allHospitals.find(h => h.id === patient.hospitalId);
        if (hospital) {
            locationSelect.innerHTML += `<option value="hospital-${hospital.id}">${hospital.name} (Лікарня)</option>`;
        }
    }
}

function handleLocationChange() {
    const locationValue = document.getElementById('tracking-location').value;
    resetDoctorSelect();

    if (!locationValue) return;

    const [type, id] = locationValue.split('-');
    const locationId = parseInt(id, 10);

    let availableDoctors = [];

    if (type === 'clinic') {
        availableDoctors = allDoctors.filter(doc =>
            doc.employments?.some(emp => emp.clinicId === locationId)
        );
    } else if (type === 'hospital') {
        availableDoctors = allDoctors.filter(doc =>
            doc.employments?.some(emp => emp.hospitalId === locationId)
        );
    }

    populateDoctorSelect(availableDoctors);
}

function handleHospitalChange() {
    const hospitalId = document.getElementById('tracking-hospital').value;
    resetDoctorSelect();

    if (!hospitalId) return;

    const locationId = parseInt(hospitalId, 10);

    const availableDoctors = allDoctors.filter(doc =>
        doc.employments?.some(emp => emp.hospitalId === locationId)
    );

    populateDoctorSelect(availableDoctors);
}


function populateDoctorSelect(doctors) {
    const doctorSelect = document.getElementById('tracking-doctor');
    if (doctors.length === 0) {
        doctorSelect.innerHTML = '<option value="">Немає лікарів у цьому місці</option>';
        return;
    }

    doctorSelect.innerHTML = '<option value="">Оберіть лікаря...</option>';
    doctors.forEach(doc => {
        doctorSelect.innerHTML += `<option value="${doc.id}">${doc.fullName} (${doc.specialty})</option>`;
    });
}


// ===================================================================
// ЛОГІКА ЗБЕРЕЖЕННЯ / ВИДАЛЕННЯ
// ===================================================================

/**
 * Обробник відправки форми (Створення / Оновлення)
 */
async function handleFormSubmit(event) {
    event.preventDefault();
    showModalError(null);

    const id = document.getElementById('tracking-id').value;
    const mode = document.getElementById('tracking-mode').value;

    // ✅ СПЕЦІАЛЬНА ЛОГІКА ДЛЯ ВИПИСКИ (DISCHARGE)
    if (mode === 'admission' && id) {
        const dischargeDateInput = document.getElementById('tracking-dischargedate').value;
        if (!dischargeDateInput) {
            showModalError("Будь ласка, вкажіть дату виписки.");
            return;
        }

        const dischargeDateIso = new Date(dischargeDateInput).toISOString();
        const endpoint = `/api/admission/${id}/discharge?dischargeDate=${dischargeDateIso}`;

        try {
            await apiFetch(endpoint, { method: 'PUT' });
            trackingModal.hide();

            allAdmissions = await apiFetch('/api/admission');
            renderAdmissionsTable(allAdmissions);
            // Оновити статус пацієнтів, бо хтось звільнився
            allPatients = await apiFetch('/api/patient/all-with-associations');

        } catch (error) {
            console.error('Error discharging:', error);
            showModalError(`Не вдалося виписати: ${error.message}`);
        }
        return;
    }

    // --- СТАНДАРТНА ЛОГІКА ДЛЯ ІНШИХ ТИПІВ ---

    const method = id ? 'PUT' : 'POST';
    let endpoint = getEndpoint(mode);
    if (id) endpoint += `/${id}`;

    let dto;
    try {
        dto = buildDto(mode);
        if (id) dto.id = parseInt(id, 10);
    } catch (error) {
        showModalError(error.message); // Тут буде українізована помилка з buildDto
        return;
    }

    try {
        await apiFetch(endpoint, { method: method, body: JSON.stringify(dto) });
        trackingModal.hide();

        // Оновлюємо дані в таблицях
        if (mode === 'appointment') {
            allAppointments = await apiFetch('/api/appointment');
            renderAppointmentsTable(allAppointments);
        } else if (mode === 'admission') {
            allAdmissions = await apiFetch('/api/admission');
            renderAdmissionsTable(allAdmissions);
            allPatients = await apiFetch('/api/patient/all-with-associations');
        } else if (mode === 'operation') {
            allOperations = await apiFetch('/api/operation');
            renderOperationsTable(allOperations);
        }

    } catch (error) {
        console.error('Error saving:', error);
        showModalError(`Не вдалося зберегти: ${error.message}`);
    }
}

/**
 * Збирає DTO з форми
 */
function buildDto(mode) {
    const patientId = parseInt(document.getElementById('tracking-patient').value, 10);
    if (!patientId) throw new Error("Пацієнт не обраний.");

    const doctorId = parseInt(document.getElementById('tracking-doctor').value, 10);
    if (!doctorId) throw new Error("Лікар не обраний.");

    const dateTime = document.getElementById('tracking-datetime').value;
    if (!dateTime) throw new Error("Дата/Час не вказані.");
    const isoDateTime = new Date(dateTime).toISOString();

    if (mode === 'appointment') {
        const locationValue = document.getElementById('tracking-location').value;
        if (!locationValue) throw new Error("Місце не обране.");

        const [type, locIdStr] = locationValue.split('-');
        const locId = parseInt(locIdStr, 10);

        return {
            patientId: patientId,
            doctorId: doctorId,
            visitDateTime: isoDateTime,
            summary: document.getElementById('tracking-summary').value,
            clinicId: (type === 'clinic') ? locId : null,
            hospitalId: (type === 'hospital') ? locId : null
        };
    }
    else if (mode === 'admission') {
        const hospitalId = parseInt(document.getElementById('tracking-hospital').value, 10);
        if (!hospitalId) throw new Error("Лікарня не обрана.");

        // При створенні нової госпіталізації дата виписки зазвичай null
        const dischargeDate = document.getElementById('tracking-dischargedate').value;

        return {
            patientId: patientId,
            attendingDoctorId: doctorId,
            hospitalId: hospitalId,
            admissionDate: isoDateTime,
            dischargeDate: dischargeDate ? new Date(dischargeDate).toISOString() : null
        };
    }
    else if (mode === 'operation') {
        const locationValue = document.getElementById('tracking-location').value;
        if (!locationValue) throw new Error("Місце не обране.");

        const [type, locIdStr] = locationValue.split('-');
        const locId = parseInt(locIdStr, 10);

        const opType = document.getElementById('tracking-optype').value;
        if (!opType) throw new Error("Тип операції не вказано.");

        return {
            patientId: patientId,
            doctorId: doctorId,
            date: isoDateTime,
            type: opType,
            isFatal: document.getElementById('tracking-isfatal').checked,
            clinicId: (type === 'clinic') ? locId : null,
            hospitalId: (type === 'hospital') ? locId : null
        };
    }

    throw new Error('Внутрішня помилка: Невідомий режим форми.');
}

/**
 * Обробник кліків на кнопках в таблиці
 */
function handleTableClick(event, mode) {
    const target = event.target.closest('button');
    if (!target) return;

    const action = target.dataset.action;
    if (!action) return;

    const id = target.dataset.id;
    if (!id) return;

    if (action === 'edit') {
        handleEdit(id, mode);
    } else if (action === 'delete') {
        handleDelete(id, mode);
    }
}

/**
 * Видалення запису
 */
async function handleDelete(id, mode) {
    if (!confirm('Ви впевнені, що хочете видалити цей запис?')) {
        return;
    }
    try {
        showError(null);
        let endpoint = getEndpoint(mode);
        await apiFetch(`${endpoint}/${id}`, { method: 'DELETE' });

        // Перезавантажуємо дані
        if (mode === 'appointment') {
            allAppointments = await apiFetch('/api/appointment');
            renderAppointmentsTable(allAppointments);
        } else if (mode === 'admission') {
            allAdmissions = await apiFetch('/api/admission');
            renderAdmissionsTable(allAdmissions);
        } else if (mode === 'operation') {
            allOperations = await apiFetch('/api/operation');
            renderOperationsTable(allOperations);
        }

    } catch (error) {
        console.error('Error deleting:', error);
        showError(`Не вдалося видалити: ${error.message}`);
    }
}


// ===================================================================
// ДОПОМІЖНІ ФУНКЦІЇ
// ===================================================================

function getEndpoint(mode) {
    if (mode === 'appointment') return '/api/appointment';
    if (mode === 'admission') return '/api/admission';
    if (mode === 'operation') return '/api/operation';
    throw new Error('Невірний режим роботи (invalid mode)');
}

function resetLocationSelect(showDefault = false) {
    const select = document.getElementById('tracking-location');
    select.innerHTML = showDefault ? '<option value="">Спочатку оберіть пацієнта</option>' : '<option value="">Оберіть місце...</option>';
}

function resetDoctorSelect(showDefault = false) {
    const select = document.getElementById('tracking-doctor');
    select.innerHTML = showDefault ? '<option value="">Спочатку оберіть місце</option>' : '<option value="">Оберіть лікаря...</option>';
}

// Конвертує дату в формат, який розуміє <input type="datetime-local">
function toLocalISOString(date) {
    const offset = date.getTimezoneOffset() * 60000;
    const localISOTime = (new Date(date - offset)).toISOString().slice(0, 16);
    return localISOTime;
}

// --- Функції показу помилок ---

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