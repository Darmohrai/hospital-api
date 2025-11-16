// Глобальні змінні
let currentUserRole = null;
let patientModal = null; // Екземпляр Bootstrap Modal для Пацієнта
let allClinics = []; // Кеш для списку клінік
let bedModal = null; // Екземпляр Bootstrap Modal для Ліжок
let doctorModal = null; // Модальне вікно для Лікарів
let allDoctors = []; // Кеш для списку лікарів (з їх employments)
let allPatients = []; // ✅ НОВЕ: Кеш для списку пацієнтів
let allHospitals = [];

/**
 * Головна функція, що виконується при завантаженні сторінки
 */
document.addEventListener('DOMContentLoaded', () => {
    // Ініціалізуємо ВСІ модальні вікна
    patientModal = new bootstrap.Modal(document.getElementById('patient-modal'));
    bedModal = new bootstrap.Modal(document.getElementById('assign-bed-modal'));
    doctorModal = new bootstrap.Modal(document.getElementById('assign-doctor-modal'));

    setupUIBasedOnRole();
    loadInitialData();

    // Налаштування обробників подій
    document.getElementById('create-patient-btn').addEventListener('click', openCreateModal);
    document.getElementById('patient-form').addEventListener('submit', handleFormSubmit);
    document.getElementById('patients-table-body').addEventListener('click', handleTableClick);
    document.getElementById('assign-bed-form').addEventListener('submit', handleBedAssignSubmit);
    document.getElementById('assign-doctor-form').addEventListener('submit', handleDoctorAssignSubmit);
});

/**
 * Перевіряє роль користувача та налаштовує UI
 */
function setupUIBasedOnRole() {
    currentUserRole = getUserRole();

    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
        document.getElementById('create-patient-btn').style.display = 'block';
        document.getElementById('actions-header').style.display = 'table-cell';
    }
}

/**
 * Завантажує клініки, лікарів, а потім пацієнтів.
 */
async function loadInitialData() {
    try {
        // Паралельно завантажуємо клініки та лікарів
        const [clinics, doctors, hospitals] = await Promise.all([
            apiFetch('/api/clinic'),
            apiFetch('/api/staff/doctors'), // Цей запит має повертати лікарів з їх .Employments
            apiFetch('/api/hospital')
        ]);

        // Обробка клінік
        allClinics = clinics;
        const clinicSelect = document.getElementById('patient-clinic');
        clinicSelect.innerHTML = '<option value="">Оберіть поліклініку...</option>';
        allClinics.forEach(clinic => {
            clinicSelect.innerHTML += `<option value="${clinic.id}">${clinic.name}</option>`;
        });



        allHospitals = hospitals;
        const hospitalSelect = document.getElementById('patient-hospital');
        hospitalSelect.innerHTML = '<option value="">Не госпіталізовано</option>'; // Дозволяємо null
        allHospitals.forEach(hospital => {
            hospitalSelect.innerHTML += `<option value="${hospital.id}">${hospital.name}</option>`;
        });

        // ✅ ОНОВЛЕННЯ: Просто зберігаємо лікарів. Не заповнюємо список.
        allDoctors = doctors;

        // Тепер завантажуємо пацієнтів
        await loadPatients();

    } catch (error) {
        showError(`Критична помилка завантаження даних: ${error.message}`);
    }
}

/**
 * Завантажує пацієнтів з API.
 */
async function loadPatients() {
    try {
        showError(null);
        const patients = await apiFetch('/api/patient'); // Має викликати GetAllWithAssociationsAsync

        // ✅ ОНОВЛЕННЯ: Зберігаємо пацієнтів у кеш
        allPatients = patients;

        renderTable(patients);
    } catch (error) {
        console.error('Error loading patients:', error);
        showError(`Не вдалося завантажити пацієнтів: ${error.message}`);
    }
}

/**
 * Рендерить (відображає) таблицю пацієнтів
 */
function renderTable(patients) {
    const tableBody = document.getElementById('patients-table-body');
    tableBody.innerHTML = '';

    if (!patients || patients.length === 0) {
        // ✅ ВИПРАВЛЕНО: colspan="10" (9 колонок даних + 1 дії)
        tableBody.innerHTML = '<tr><td colspan="10" class="text-center">Пацієнтів не знайдено.</td></tr>';
        return;
    }

    patients.forEach(patient => {
        const row = document.createElement('tr');

        let actionsHtml = '';
        if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
            actionsHtml = `
                <button class="btn btn-primary btn-sm me-1" data-id="${patient.id}" data-action="edit" title="Редагувати">
                    Ред.
                </button>
            `;
        }
        if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
            actionsHtml += `
                <button class="btn btn-danger btn-sm" data-id="${patient.id}" data-action="delete" title="Видалити">
                    Видал.
                </button>`;
        }

        const dobText = patient.dateOfBirth ? patient.dateOfBirth.split('T')[0] : 'Н/Д';
        const healthStatusText = patient.healthStatus || 'Н/Д';
        const tempText = patient.temperature ? `${patient.temperature}°C` : 'Н/Д';

        const clinic = allClinics.find(c => c.id === patient.clinicId);
        const clinicText = clinic ? clinic.name : `<span class="text-danger">ID: ${patient.clinicId}</span>`;

        // Ця логіка у вас була, але вона не використовувалась у innerHTML
        let hospitalText = '<span class="text-muted small">Н/Д</span>';
        if (patient.hospitalId) {
            // patient.hospital має бути завантажений через GetAllWithAssociationsAsync
            if (patient.hospital) {
                hospitalText = patient.hospital.name;
            } else {
                // Фоллбек, якщо асоціація не завантажилась, але ID є
                const hospital = allHospitals.find(h => h.id === patient.hospitalId);
                hospitalText = hospital ? hospital.name : `<span class="text-danger">ID: ${patient.hospitalId}</span>`;
            }
        }

        // Логіка для лікаря
        let doctorText = '';
        if (patient.assignedDoctor) {
            doctorText = `
                <div class="d-flex justify-content-between align-items-center">
                    <span class="badge bg-info text-dark">
                        ${patient.assignedDoctor.fullName}
                    </span>
            `;
            if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                doctorText += `
                    <button class="btn btn-warning btn-sm ms-2" 
                            data-action="remove-doctor" 
                            data-id="${patient.id}" 
                            title="Відкріпити лікаря">
                        &times;
                    </button>
                `;
            }
            doctorText += '</div>';
        } else {
            doctorText = '<span class="text-muted small">Немає</span>';
            if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                doctorText += `
                    <button class="btn btn-success btn-sm ms-2" 
                            data-action="assign-doctor" 
                            data-id="${patient.id}" 
                            data-name="${patient.fullName}"
                            title="Призначити лікаря">
                        +
                    </button>
                `;
            }
        }

        // Логіка для ліжка
        let bedText = '';
        if (patient.bed) {
            const bedLocation = `(Кім. ${patient.bed.room.number}, ${patient.bed.room.department.name})`;
            bedText = `
                <div class="d-flex justify-content-between align-items-center">
                    <span class="badge bg-success">
                        Ліжко #${patient.bed.id} 
                        ${bedLocation}
                    </span>
            `;
            if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                bedText += `
                    <button class="btn btn-warning btn-sm ms-2" 
                            data-action="remove-bed" 
                            data-id="${patient.id}"  /* ✅ ВИПРАВЛЕНО: Потрібен patientId для /unassign-bed */
                            title="Звільнити ліжко">
                        &times;
                    </button>
                `;
            }
            bedText += '</div>';
        } else {
            bedText = '<span class="text-muted small">Немає</span>';
            // ✅ Кнопку "призначити ліжко" показуємо, ТІЛЬКИ ЯКЩО пацієнт в лікарні
            if ((currentUserRole === 'Admin' || currentUserRole === 'Operator') && patient.hospitalId) {
                bedText += `
                    <button class="btn btn-info btn-sm ms-2" 
                            data-action="assign-bed" 
                            data-id="${patient.id}" 
                            data-name="${patient.fullName}"
                            title="Призначити ліжко">
                        +
                    </button>
                `;
            }
        }

        // ✅ ВИПРАВЛЕНО: Додано <td>${hospitalText}</td> згідно вашого HTML
        row.innerHTML = `
            <td>${patient.id}</td>
            <td>${patient.fullName}</td>
            <td>${dobText}</td>
            <td>${healthStatusText}</td>
            <td>${tempText}</td>
            <td>${clinicText}</td>
            <td>${hospitalText}</td> <td>${doctorText}</td>
            <td>${bedText}</td>
            ${actionsHtml ? `<td>${actionsHtml}</td>` : ''}
        `;
        tableBody.appendChild(row);
    });
}

/**
 * Обробник кліків на кнопках в таблиці
 */
function handleTableClick(event) {
    const target = event.target.closest('button');
    if (!target) return;

    const action = target.dataset.action;
    if (!action) return;

    // ✅ ВИПРАВЛЕНО: Завжди беремо ID з data-id
    const patientId = target.dataset.id;
    if (!patientId) return; // Захист від кнопок без data-id

    if (action === 'edit') {
        handleEdit(patientId);
    } else if (action === 'delete') {
        handleDelete(patientId);
    } else if (action === 'assign-bed') {
        const patientName = target.dataset.name;
        openAssignBedModal(patientId, patientName);
    } else if (action === 'remove-bed') {
        // const bedId = target.dataset.bedId; // Це не потрібно, patientId достатньо
        handleRemoveBed(patientId);
    } else if (action === 'assign-doctor') {
        // ✅ ОНОВЛЕННЯ: Більше не передаємо 'patientName'
        openAssignDoctorModal(patientId);
    } else if (action === 'remove-doctor') {
        handleRemoveDoctor(patientId);
    }
}

/**
 * Відкриває модальне вікно для створення нового пацієнта
 */
function openCreateModal() {
    showModalError(null);
    document.getElementById('patient-form').reset();
    document.getElementById('patient-id').value = '';
    document.getElementById('modal-title').textContent = 'Створити пацієнта';

    document.getElementById('patient-clinic').value = "";
    document.getElementById('patient-hospital').value = ""; // ✅ Скидаємо лікарню
    document.getElementById('patient-temperature').value = "36.6";

    patientModal.show();
}

/**
 * Завантажує дані пацієнта та відкриває модальне вікно для редагування
 */
async function handleEdit(id) {
    try {
        showModalError(null);
        // Використовуємо кеш, бо він надійніший
        const patient = allPatients.find(p => p.id == id);

        if (!patient) {
            showError('Не вдалося знайти пацієнта в кеші.');
            return;
        }

        document.getElementById('patient-id').value = patient.id;
        document.getElementById('modal-title').textContent = `Редагувати: ${patient.fullName}`;

        document.getElementById('patient-clinic').value = patient.clinicId;
        // ✅ ВИПРАВЛЕНО: Додано встановлення значення лікарні
        document.getElementById('patient-hospital').value = patient.hospitalId || "";

        document.getElementById('patient-fullname').value = patient.fullName;
        document.getElementById('patient-dob').value = patient.dateOfBirth.split('T')[0];
        document.getElementById('patient-healthstatus').value = patient.healthStatus;
        document.getElementById('patient-temperature').value = patient.temperature;

        patientModal.show();

    } catch (error) {
        console.error('Error fetching patient details:', error);
        showError(`Помилка: ${error.message}`);
    }
}

/**
 * Обробник відправки форми (Створення / Оновлення Пацієнта)
 */
async function handleFormSubmit(event) {
    event.preventDefault();
    const id = document.getElementById('patient-id').value;

    const dobString = document.getElementById('patient-dob').value;
    const dobUtc = new Date(dobString).toISOString();

    // ✅ ВИПРАВЛЕНО: Зчитуємо hospitalId з форми
    const hospitalIdValue = document.getElementById('patient-hospital').value;
    const hospitalIdParsed = hospitalIdValue ? parseInt(hospitalIdValue, 10) : null;

    const patientDto = {
        clinicId: parseInt(document.getElementById('patient-clinic').value, 10),
        fullName: document.getElementById('patient-fullname').value,
        dateOfBirth: dobUtc,
        healthStatus: document.getElementById('patient-healthstatus').value,
        temperature: parseFloat(document.getElementById('patient-temperature').value),

        // ✅ ВИПРАВЛЕНО: Встановлюємо hospitalId з форми, а не null
        hospitalId: hospitalIdParsed
    };

    const method = id ? 'PUT' : 'POST';
    const url = id ? `/api/patient/${id}` : '/api/patient';

    if (id) {
        patientDto.id = parseInt(id, 10);

        // При оновленні (PUT) ми не хочемо затирати ці поля,
        // оскільки вони керуються окремими ендпоінтами (AssignDoctor/AssignBed).
        // Але hospitalId ми ЗАЛИШАЄМО, бо ми ним керуємо з цієї форми.
        delete patientDto.assignedDoctorId;
        delete patientDto.bedId;
        // delete patientDto.hospitalId; // ✅ ВИПРАВЛЕНО: Цей рядок НЕ ПОТРІБЕН
    } else {
        // Для POST (створення) ми можемо відправити null
        patientDto.assignedDoctorId = null;
        patientDto.bedId = null;
    }

    try {
        showModalError(null);
        await apiFetch(url, { method: method, body: JSON.stringify(patientDto) });
        patientModal.hide();
        loadPatients();

    } catch (error) {
        console.error('Error saving patient:', error);
        showModalError(`Не вдалося зберегти: ${error.message}`);
    }
}

/**
 * Видалення пацієнта
 */
async function handleDelete(id) {
    if (!confirm('Ви впевнені, що хочете видалити цього пацієнта? Це також звільнить його ліжко.')) {
        return;
    }
    try {
        showError(null);
        await apiFetch(`/api/patient/${id}`, { method: 'DELETE' });
        loadPatients();
    } catch (error) {
        console.error('Error deleting patient:', error);
        showError(`Не вдалося видалити пацієнта: ${error.message}`);
    }
}

// --- ФУНКЦІЇ ДЛЯ КЕРУВАННЯ ЛІЖКАМИ ---

function openAssignBedModal(patientId, patientName) {
    showBedModalError(null);
    document.getElementById('assign-bed-form').reset();
    document.getElementById('assign-bed-patient-id').value = patientId;
    document.getElementById('assign-bed-patient-name').value = patientName;

    bedModal.show();
    loadAvailableBeds();
}

async function loadAvailableBeds() {
    const select = document.getElementById('assign-bed-select');
    select.innerHTML = '<option value="">Завантаження...</option>';

    try {
        // (Якщо 404, змініть на /api/bed/available)
        const availableBeds = await apiFetch('/api/hospital/bed/available');

        if (availableBeds.length === 0) {
            select.innerHTML = '<option value="">Вільних ліжок немає</option>';
            return;
        }

        select.innerHTML = '<option value="">Оберіть вільне ліжко...</option>';
        availableBeds.forEach(bed => {
            const location = `(Кім. ${bed.room.id}, ${bed.room.department.name})`;
            select.innerHTML += `<option value="${bed.id}">Ліжко #${bed.id} ${location}</option>`;
        });

    } catch (error) {
        showBedModalError(`Помилка завантаження ліжок: ${error.message}`);
    }
}

async function handleBedAssignSubmit(event) {
    event.preventDefault();
    showBedModalError(null);

    const bedId = parseInt(document.getElementById('assign-bed-select').value, 10);
    const patientId = parseInt(document.getElementById('assign-bed-patient-id').value, 10);

    if (!bedId || !patientId) {
        showBedModalError('Необхідно обрати пацієнта та ліжко.');
        return;
    }

    try {
        const url = `/api/patient/${patientId}/assign-bed/${bedId}`;

        await apiFetch(url, {
            method: 'POST'
        });

        bedModal.hide();
        loadPatients();

    } catch (error) {
        showBedModalError(`Не вдалося призначити ліжко: ${error.message}`);
    }
}

async function handleRemoveBed(patientId) {
    if (!confirm('Ви впевнені, що хочете звільнити це ліжко?')) {
        return;
    }

    try {
        await apiFetch(`/api/patient/${patientId}/unassign-bed`, {
            method: 'POST',
        });

        loadPatients();

    } catch (error) {
        showError(`Не вдалося звільнити ліжко: ${error.message}`);
    }
}

// --- ✅ ОНОВЛЕНІ ФУНКЦІЇ ДЛЯ КЕРУВАННЯ ЛІКАРЯМИ ---

/**
 * ✅ ОНОВЛЕНО: Знаходить пацієнта, фільтрує лікарів,
 * заповнює <select> і показує модальне вікно.
 */
function openAssignDoctorModal(patientId) {
    showDoctorModalError(null);
    document.getElementById('assign-doctor-form').reset();

    // 1. Знайти пацієнта з кешу
    const patient = allPatients.find(p => p.id == patientId);
    if (!patient) {
        showError('Не вдалося знайти дані пацієнта (ID: ' + patientId + ').');
        return;
    }

    // 2. Встановити дані в модальному вікні
    document.getElementById('assign-doctor-patient-id').value = patient.id;
    document.getElementById('assign-doctor-patient-name').value = patient.fullName;

    // 3. Динамічно заповнити список лікарів
    const doctorSelect = document.getElementById('assign-doctor-select');

    // 3a. Отримуємо локації пацієнта
    const patientClinicId = patient.clinicId;
    // (patient.hospitalId завантажується вашим GetAllWithAssociationsAsync)
    const patientHospitalId = patient.hospitalId;

    // 3b. Фільтруємо 'allDoctors'
    const availableDoctors = allDoctors.filter(doctor => {
        if (!doctor.employments || doctor.employments.length === 0) {
            return false;
        }

        // Перевірка на клініку
        if (doctor.employments.some(emp => emp.clinicId === patientClinicId)) {
            return true;
        }

        // Перевірка на госпіталь (якщо пацієнт там є)
        if (patientHospitalId && doctor.employments.some(emp => emp.hospitalId === patientHospitalId)) {
            return true;
        }

        return false;
    });

    // 3c. Заповнюємо <select>
    if (availableDoctors.length === 0) {
        doctorSelect.innerHTML = '<option value="">Немає доступних лікарів для цієї локації</option>';
    } else {
        doctorSelect.innerHTML = '<option value="">Оберіть лікаря...</option>';
        availableDoctors.forEach(doctor => {
            doctorSelect.innerHTML += `<option value="${doctor.id}">${doctor.fullName} (${doctor.specialty})</option>`;
        });
    }

    // 4. Показати модальне вікно
    doctorModal.show();
}

/**
 * (Ця функція без змін, вона викликає /api/patient/{patientId}/assign-doctor/{doctorId})
 */
async function handleDoctorAssignSubmit(event) {
    event.preventDefault();
    showDoctorModalError(null);

    const doctorId = parseInt(document.getElementById('assign-doctor-select').value, 10);
    const patientId = parseInt(document.getElementById('assign-doctor-patient-id').value, 10);

    if (!doctorId || !patientId) {
        showDoctorModalError('Необхідно обрати пацієнта та лікаря.');
        return;
    }

    try {
        const url = `/api/patient/${patientId}/assign-doctor/${doctorId}`;

        await apiFetch(url, {
            method: 'POST'
        });

        doctorModal.hide();
        loadPatients();

    } catch (error) {
        // Показуємо помилку (напр. "Doctor and patient do not share...")
        showDoctorModalError(`Не вдалося призначити лікаря: ${error.message}`);
    }
}

/**
 * (Ця функція без змін, вона викликає /api/patient/{patientId}/remove-doctor)
 */
async function handleRemoveDoctor(patientId) {
    if (!confirm('Ви впевнені, що хочете відкріпити лікаря від цього пацієнта?')) {
        return;
    }

    try {
        const url = `/api/patient/${patientId}/remove-doctor`;

        await apiFetch(url, {
            method: 'POST'
        });

        loadPatients();

    } catch (error) {
        showError(`Не вдалося відкріпити лікаря: ${error.message}`);
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

function showBedModalError(message) {
    const errorEl = document.getElementById('assign-bed-error-alert');
    if (message) {
        errorEl.textContent = message;
        errorEl.style.display = 'block';
    } else {
        errorEl.style.display = 'none';
    }
}

function showDoctorModalError(message) {
    const errorEl = document.getElementById('assign-doctor-error-alert');
    if (message) {
        errorEl.textContent = message;
        errorEl.style.display = 'block';
    } else {
        errorEl.style.display = 'none';
    }
}