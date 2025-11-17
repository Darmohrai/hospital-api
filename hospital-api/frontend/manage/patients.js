// Глобальні змінні
let currentUserRole = null;
let patientModal = null; // Екземпляр Bootstrap Modal для Пацієнта
let allClinics = []; // Кеш для списку клінік
let bedModal = null; // Екземпляр Bootstrap Modal для Ліжок
let doctorModal = null; // Модальне вікно для керування лікарями
let allDoctors = []; // Кеш для списку лікарів
let allPatients = []; // Кеш для списку пацієнтів
let allHospitals = [];

// Enum спеціалізацій (C# HospitalSpecialization)
const hospitalSpecializations = {
    0: "Хірург",
    1: "Невролог",
    2: "Окуліст",
    3: "Стоматолог",
    4: "Рентгенолог",
    5: "Гінеколог",
    6: "Кардіолог"
};

const specIdToKey = {
    0: "Surgeon",
    1: "Neurologist",
    2: "Ophthalmologist",
    3: "Dentist",
    4: "Radiologist",
    5: "Gynecologist",
    6: "Cardiologist"
};

document.addEventListener('DOMContentLoaded', () => {
    patientModal = new bootstrap.Modal(document.getElementById('patient-modal'));
    bedModal = new bootstrap.Modal(document.getElementById('assign-bed-modal'));
    doctorModal = new bootstrap.Modal(document.getElementById('assign-doctor-modal'));

    setupUIBasedOnRole();

    // 1. Додаємо поле "Спеціалізація" (тільки для фільтрації лікарень)
    injectSpecializationField();

    loadInitialData();

    // Обробники подій
    document.getElementById('create-patient-btn').addEventListener('click', openCreateModal);
    document.getElementById('patient-form').addEventListener('submit', handleFormSubmit);
    document.getElementById('patients-table-body').addEventListener('click', handleTableClick);
    document.getElementById('assign-bed-form').addEventListener('submit', handleBedAssignSubmit);

    // Каскадні слухачі
    document.getElementById('patient-clinic').addEventListener('change', updateHospitalOptions);
    document.getElementById('referral-specialization').addEventListener('change', updateHospitalOptions);
});

/**
 * Додає поле Спеціалізації у модальне вікно створення/редагування
 */
function injectSpecializationField() {
    const hospitalSelect = document.getElementById('patient-hospital');
    const container = hospitalSelect.closest('.mb-3');

    const specDiv = document.createElement('div');
    specDiv.className = 'mb-3';
    specDiv.innerHTML = `
        <label for="referral-specialization" class="form-label text-primary">Необхідна спеціалізація (для направлення)</label>
        <select id="referral-specialization" class="form-select">
            <option value="">-- Без фільтру / Амбулаторно --</option>
        </select>
        <small class="text-muted" style="font-size: 0.8em;">Оберіть, щоб знайти відповідну лікарню.</small>
    `;
    container.parentNode.insertBefore(specDiv, container);

    const specSelect = document.getElementById('referral-specialization');
    for (const [key, value] of Object.entries(hospitalSpecializations)) {
        specSelect.innerHTML += `<option value="${key}">${value}</option>`;
    }
}

function setupUIBasedOnRole() {
    currentUserRole = getUserRole();
    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
        document.getElementById('create-patient-btn').style.display = 'block';
        document.getElementById('actions-header').style.display = 'table-cell';
    }
}

async function loadInitialData() {
    try {
        const [clinics, doctors, hospitals] = await Promise.all([
            apiFetch('/api/clinic'),
            apiFetch('/api/staff/doctors'),
            apiFetch('/api/hospital')
        ]);

        allClinics = clinics;
        allHospitals = hospitals;
        allDoctors = doctors;

        // Заповнення списку клінік
        const clinicSelect = document.getElementById('patient-clinic');
        clinicSelect.innerHTML = '<option value="">Оберіть поліклініку...</option>';
        allClinics.forEach(clinic => {
            clinicSelect.innerHTML += `<option value="${clinic.id}">${clinic.name}</option>`;
        });

        // Ініціалізація лікарень
        updateHospitalOptions();

        await loadPatients();

    } catch (error) {
        showError(`Критична помилка завантаження даних: ${error.message}`);
    }
}

function updateHospitalOptions() {
    const clinicId = parseInt(document.getElementById('patient-clinic').value, 10);
    const specValue = document.getElementById('referral-specialization').value;
    const hospitalSelect = document.getElementById('patient-hospital');
    const currentHospitalId = hospitalSelect.value;

    hospitalSelect.innerHTML = '<option value="">Не госпіталізовано (Амбулаторно)</option>';

    const reqSpec = specValue !== "" ? parseInt(specValue, 10) : null;
    const selectedClinic = allClinics.find(c => c.id === clinicId);

    let allowedHospitals = [];

    if (reqSpec === null) {
        allowedHospitals = allHospitals;
    } else {
        if (selectedClinic && selectedClinic.hospitalId) {
            const linkedHospital = allHospitals.find(h => h.id === selectedClinic.hospitalId);

            if (linkedHospital && linkedHospital.specializations && linkedHospital.specializations.includes(reqSpec)) {
                allowedHospitals.push(linkedHospital);
            } else {
                allowedHospitals = allHospitals.filter(h => h.specializations && h.specializations.includes(reqSpec));
            }
        } else {
            allowedHospitals = allHospitals.filter(h => h.specializations && h.specializations.includes(reqSpec));
        }
    }

    allowedHospitals.forEach(h => {
        const isLinked = (selectedClinic && h.id === selectedClinic.hospitalId) ? " (За місцем проживання)" : "";
        hospitalSelect.innerHTML += `<option value="${h.id}">${h.name}${isLinked}</option>`;
    });

    if (currentHospitalId && [...hospitalSelect.options].some(o => o.value === currentHospitalId)) {
        hospitalSelect.value = currentHospitalId;
    } else {
        hospitalSelect.value = "";
    }
}

async function loadPatients() {
    try {
        showError(null);
        const patients = await apiFetch('/api/patient');
        allPatients = patients;
        // ✅ Очікуємо завершення рендерингу (тепер це async)
        await renderTable(patients);
    } catch (error) {
        showError(`Не вдалося завантажити пацієнтів: ${error.message}`);
    }
}

// ✅ ОНОВЛЕНО: Функція тепер асинхронна для завантаження лікарів поліклініки
async function renderTable(patients) {
    const tableBody = document.getElementById('patients-table-body');

    if (!patients || patients.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="10" class="text-center">Пацієнтів не знайдено.</td></tr>';
        return;
    }

    // Показуємо індикатор завантаження поки дані збираються
    tableBody.innerHTML = '<tr><td colspan="10" class="text-center text-muted">Завантаження списку лікарів...</td></tr>';

    // Використовуємо Promise.all для паралельного завантаження даних по кожному пацієнту
    const rowsHtml = await Promise.all(patients.map(async (patient) => {
        let actionsHtml = '';
        if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
            actionsHtml = `
                <button class="btn btn-primary btn-sm me-1" data-id="${patient.id}" data-action="edit" title="Редагувати">Ред.</button>
                <button class="btn btn-danger btn-sm" data-id="${patient.id}" data-action="delete" title="Видалити">Видал.</button>
            `;
        }

        const dobText = patient.dateOfBirth ? patient.dateOfBirth.split('T')[0] : 'Н/Д';
        const clinic = allClinics.find(c => c.id === patient.clinicId);
        const clinicText = clinic ? clinic.name : `ID: ${patient.clinicId}`;

        let hospitalText = '<span class="text-muted small">Н/Д</span>';
        if (patient.hospitalId) {
            const hospital = allHospitals.find(h => h.id === patient.hospitalId);
            hospitalText = hospital ? `<span class="badge bg-primary">${hospital.name}</span>` : `ID: ${patient.hospitalId}`;
        }

        // --- ФОРМУВАННЯ КОЛОНКИ "ЛІКАРІ" ---
        let doctorText = '';

        // 1. Лікар стаціонару (з об'єкта пацієнта)
        if (patient.assignedDoctor) {
            doctorText += `<div class="small mb-1 text-primary"><strong>Стац:</strong> ${patient.assignedDoctor.fullName}</div>`;
        }

        // 2. Лікарі поліклініки (завантажуємо окремо)
        try {
            const clinicAssignments = await apiFetch(`/api/clinic-assignment/by-patient/${patient.id}`);
            if (clinicAssignments && clinicAssignments.length > 0) {
                const clinicDocNames = clinicAssignments.map(a => {
                    const d = allDoctors.find(doc => doc.id === a.doctorId);
                    return d ? d.fullName : `ID ${a.doctorId}`;
                }).join(', ');

                doctorText += `<div class="small text-success"><strong>Амб:</strong> ${clinicDocNames}</div>`;
            }
        } catch (e) {
            console.warn(`Failed to load clinic doctors for patient ${patient.id}`, e);
        }

        // Кнопка керування
        doctorText += `
            <button class="btn btn-outline-secondary btn-sm mt-1 w-100" style="font-size: 0.75rem;" data-action="manage-doctors" data-id="${patient.id}">
                <i class="bi bi-pencil"></i> Змінити
            </button>
        `;

        let bedText = patient.bed ? `<span class="badge bg-success">Ліжко #${patient.bed.id}</span>` : '<span class="text-muted small">Немає</span>';
        if ((currentUserRole === 'Admin' || currentUserRole === 'Operator') && patient.hospitalId) {
            if(!patient.bed) bedText += `<button class="btn btn-info btn-sm ms-2 py-0" data-action="assign-bed" data-id="${patient.id}">+</button>`;
            else bedText += `<button class="btn btn-warning btn-sm ms-2 py-0" data-action="remove-bed" data-id="${patient.id}">&times;</button>`;
        }

        return `
            <tr>
                <td>${patient.id}</td>
                <td>${patient.fullName}</td>
                <td>${dobText}</td>
                <td>${patient.healthStatus || '-'}</td>
                <td>${patient.temperature}°C</td>
                <td>${clinicText}</td>
                <td>${hospitalText}</td>
                <td>${doctorText}</td>
                <td>${bedText}</td>
                <td>${actionsHtml}</td>
            </tr>
        `;
    }));

    tableBody.innerHTML = rowsHtml.join('');
}

function handleTableClick(event) {
    const target = event.target.closest('button');
    if (!target) return;
    const action = target.dataset.action;
    const patientId = target.dataset.id;
    if (!patientId) return;

    if (action === 'edit') handleEdit(patientId);
    else if (action === 'delete') handleDelete(patientId);
    else if (action === 'assign-bed') openAssignBedModal(patientId, "");
    else if (action === 'remove-bed') handleRemoveBed(patientId);
    else if (action === 'manage-doctors') openManageDoctorsModal(patientId);
}

// --- Створення/Редагування Пацієнта (БЕЗ ЛІКАРЯ) ---

function openCreateModal() {
    showModalError(null);
    document.getElementById('patient-form').reset();
    document.getElementById('patient-id').value = '';
    document.getElementById('modal-title').textContent = 'Створити пацієнта';

    document.getElementById('patient-clinic').value = "";
    document.getElementById('referral-specialization').value = "";
    updateHospitalOptions();

    document.getElementById('patient-temperature').value = "36.6";
    patientModal.show();
}

async function handleEdit(id) {
    try {
        showModalError(null);
        const patient = allPatients.find(p => p.id == id);
        if (!patient) { showError('Пацієнта не знайдено'); return; }

        document.getElementById('patient-id').value = patient.id;
        document.getElementById('modal-title').textContent = `Редагувати: ${patient.fullName}`;

        document.getElementById('patient-clinic').value = patient.clinicId;
        document.getElementById('referral-specialization').value = "";
        updateHospitalOptions();
        document.getElementById('patient-hospital').value = patient.hospitalId || "";

        document.getElementById('patient-fullname').value = patient.fullName;
        document.getElementById('patient-dob').value = patient.dateOfBirth.split('T')[0];
        document.getElementById('patient-healthstatus').value = patient.healthStatus;
        document.getElementById('patient-temperature').value = patient.temperature;

        patientModal.show();
    } catch (error) {
        showError(error.message);
    }
}

async function handleFormSubmit(event) {
    event.preventDefault();
    const id = document.getElementById('patient-id').value;
    const hospitalIdVal = document.getElementById('patient-hospital').value;

    const patientDto = {
        clinicId: parseInt(document.getElementById('patient-clinic').value, 10),
        fullName: document.getElementById('patient-fullname').value,
        dateOfBirth: new Date(document.getElementById('patient-dob').value).toISOString(),
        healthStatus: document.getElementById('patient-healthstatus').value,
        temperature: parseFloat(document.getElementById('patient-temperature').value),
        hospitalId: hospitalIdVal ? parseInt(hospitalIdVal, 10) : null
    };

    if (id) patientDto.id = parseInt(id, 10);

    const method = id ? 'PUT' : 'POST';
    const url = id ? `/api/patient/${id}` : '/api/patient';

    try {
        showModalError(null);
        await apiFetch(url, { method: method, body: JSON.stringify(patientDto) });
        patientModal.hide();
        loadPatients();
    } catch (error) {
        showModalError(`Помилка збереження: ${error.message}`);
    }
}

async function handleDelete(id) {
    if (!confirm('Видалити пацієнта?')) return;
    try {
        await apiFetch(`/api/patient/${id}`, { method: 'DELETE' });
        loadPatients();
    } catch (error) {
        showError(error.message);
    }
}

// ==========================================================
// ✅ Модальне вікно керування лікарями
// ==========================================================

async function openManageDoctorsModal(patientId) {
    showDoctorModalError(null);

    const patient = allPatients.find(p => p.id == patientId);
    if (!patient) return;

    let clinicAssignments = [];
    try {
        clinicAssignments = await apiFetch(`/api/clinic-assignment/by-patient/${patientId}`);
    } catch (e) {
        console.warn("Помилка завантаження призначень:", e);
        clinicAssignments = [];
    }

    renderManageDoctorsModal(patient, clinicAssignments);
    doctorModal.show();
}

function renderManageDoctorsModal(patient, clinicAssignments) {
    const modalBody = document.querySelector('#assign-doctor-modal .modal-body');

    modalBody.innerHTML = `
        <div id="assign-doctor-error-alert" class="alert alert-danger" style="display:none;"></div>
        <h5 class="text-center mb-3 text-primary">${patient.fullName}</h5>
    `;

    // --- Секція 1: Лікарня (Стаціонар) ---
    const hospitalSection = document.createElement('div');
    hospitalSection.className = 'card mb-3 border-primary';

    let hospitalContent = `
        <div class="card-header bg-primary text-white">Лікар стаціонару (Лікарня)</div>
        <div class="card-body">
    `;

    if (patient.hospitalId) {
        const currentHospital = allHospitals.find(h => h.id === patient.hospitalId);
        const docName = patient.assignedDoctor ? patient.assignedDoctor.fullName : "Не призначено";

        hospitalContent += `
            <p class="mb-2"><strong>Заклад:</strong> ${currentHospital ? currentHospital.name : 'ID ' + patient.hospitalId}</p>
            <p class="mb-3"><strong>Поточний лікар:</strong> ${docName}</p>
            
            <form id="hospital-doc-form" class="row g-2">
                <div class="col-8">
                    <select id="hospital-doc-select" class="form-select form-select-sm">
                        <option value="">Оберіть лікаря...</option>
                    </select>
                </div>
                <div class="col-4">
                    <button type="submit" class="btn btn-success btn-sm w-100">Призначити</button>
                </div>
            </form>
            ${patient.assignedDoctor ? `<button class="btn btn-outline-danger btn-sm w-100 mt-2" onclick="handleRemoveHospitalDoc(${patient.id})">Відкріпити поточного</button>` : ''}
        `;
    } else {
        hospitalContent += `<p class="text-muted text-center my-2">Пацієнт не госпіталізований.<br>Призначення недоступне.</p>`;
    }
    hospitalContent += `</div>`;
    hospitalSection.innerHTML = hospitalContent;
    modalBody.appendChild(hospitalSection);


    // --- Секція 2: Поліклініка - Багато лікарів ---
    const clinicSection = document.createElement('div');
    clinicSection.className = 'card border-success';

    let clinicContent = `
        <div class="card-header bg-success text-white">Лікарі поліклініки (Амбулаторно)</div>
        <div class="card-body">
    `;

    const currentClinic = allClinics.find(c => c.id === patient.clinicId);
    clinicContent += `<p class="mb-2"><strong>Заклад:</strong> ${currentClinic ? currentClinic.name : 'ID ' + patient.clinicId}</p>`;

    if (clinicAssignments && clinicAssignments.length > 0) {
        clinicContent += `<ul class="list-group list-group-flush mb-3">`;
        clinicAssignments.forEach(assign => {
            const doc = allDoctors.find(d => d.id === assign.doctorId);
            const docName = doc ? `${doc.fullName} (${doc.specialty})` : `Лікар ID: ${assign.doctorId}`;

            clinicContent += `
                <li class="list-group-item d-flex justify-content-between align-items-center p-1">
                    <small>${docName}</small>
                    <button class="btn btn-sm btn-danger py-0" onclick="handleRemoveClinicDoc(${assign.doctorId}, ${patient.id}, ${patient.clinicId})">&times;</button>
                </li>
            `;
        });
        clinicContent += `</ul>`;
    } else {
        clinicContent += `<p class="text-muted small">Немає призначених лікарів.</p>`;
    }

    clinicContent += `
        <form id="clinic-doc-form" class="row g-2 mt-2 border-top pt-2">
            <div class="col-8">
                <select id="clinic-doc-select" class="form-select form-select-sm">
                    <option value="">Додати лікаря...</option>
                </select>
            </div>
            <div class="col-4">
                <button type="submit" class="btn btn-outline-success btn-sm w-100">Додати</button>
            </div>
        </form>
    </div>`;
    clinicSection.innerHTML = clinicContent;
    modalBody.appendChild(clinicSection);


    // --- Заповнюємо селекти ---

    // 1. Для лікарні
    if (patient.hospitalId) {
        const hSelect = document.getElementById('hospital-doc-select');
        const hDocs = allDoctors.filter(d => d.employments.some(e => e.hospitalId === patient.hospitalId));
        hDocs.forEach(d => hSelect.innerHTML += `<option value="${d.id}">${d.fullName} (${d.specialty})</option>`);

        document.getElementById('hospital-doc-form').onsubmit = (e) => {
            e.preventDefault();
            handleHospitalDocSubmit(patient.id, hSelect.value);
        };
    }

    // 2. Для поліклініки
    const cSelect = document.getElementById('clinic-doc-select');
    const assignedIds = clinicAssignments.map(a => a.doctorId);
    const cDocs = allDoctors.filter(d =>
        d.employments.some(e => e.clinicId === patient.clinicId) &&
        !assignedIds.includes(d.id)
    );

    if (cDocs.length > 0) {
        cDocs.forEach(d => cSelect.innerHTML += `<option value="${d.id}">${d.fullName} (${d.specialty})</option>`);
    } else {
        cSelect.innerHTML = '<option value="">Всі доступні лікарі вже призначені</option>';
    }

    document.getElementById('clinic-doc-form').onsubmit = (e) => {
        e.preventDefault();
        handleClinicDocSubmit(patient.id, cSelect.value, patient.clinicId);
    };
}

// --- Логіка збереження лікарів ---

async function handleHospitalDocSubmit(patientId, doctorId) {
    showDoctorModalError(null);
    if (!doctorId) return;
    try {
        await apiFetch(`/api/patient/${patientId}/assign-doctor/${doctorId}`, { method: 'POST' });
        await loadPatients();
        openManageDoctorsModal(patientId);
    } catch (e) { showDoctorModalError(e.message); }
}

window.handleRemoveHospitalDoc = async function(patientId) {
    if(!confirm('Відкріпити лікаря стаціонару?')) return;
    try {
        await apiFetch(`/api/patient/${patientId}/remove-doctor`, { method: 'POST' });
        await loadPatients();
        openManageDoctorsModal(patientId);
    } catch(e) { showDoctorModalError(e.message); }
};

async function handleClinicDocSubmit(patientId, doctorId, clinicId) {
    showDoctorModalError(null);
    if (!doctorId) return;
    try {
        await apiFetch('/api/clinic-assignment', {
            method: 'POST',
            body: JSON.stringify({
                patientId: parseInt(patientId),
                doctorId: parseInt(doctorId),
                clinicId: parseInt(clinicId)
            })
        });
        openManageDoctorsModal(patientId);
        loadPatients(); // Оновлюємо головну таблицю
    } catch (e) { showDoctorModalError("Не вдалося додати лікаря: " + e.message); }
}

window.handleRemoveClinicDoc = async function(doctorId, patientId, clinicId) {
    if(!confirm('Видалити цього лікаря зі списку?')) return;
    try {
        const params = new URLSearchParams({ patientId, doctorId, clinicId });
        await apiFetch(`/api/clinic-assignment?${params.toString()}`, { method: 'DELETE' });
        openManageDoctorsModal(patientId);
        loadPatients(); // Оновлюємо головну таблицю
    } catch(e) { showDoctorModalError(e.message); }
};


// --- Bed Logic (Без змін) ---
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
        const availableBeds = await apiFetch('/api/hospital/bed/available');
        if (!availableBeds.length) { select.innerHTML = '<option value="">Вільних ліжок немає</option>'; return; }
        select.innerHTML = '<option value="">Оберіть ліжко...</option>';
        availableBeds.forEach(bed => {
            select.innerHTML += `<option value="${bed.id}">#${bed.id} (Кім. ${bed.room.number})</option>`;
        });
    } catch (error) { showBedModalError(error.message); }
}

async function handleBedAssignSubmit(event) {
    event.preventDefault();
    const bedId = document.getElementById('assign-bed-select').value;
    const patientId = document.getElementById('assign-bed-patient-id').value;
    try {
        await apiFetch(`/api/patient/${patientId}/assign-bed/${bedId}`, { method: 'POST' });
        bedModal.hide();
        loadPatients();
    } catch (e) { showBedModalError(e.message); }
}

async function handleRemoveBed(patientId) {
    if(!confirm('Звільнити ліжко?')) return;
    try { await apiFetch(`/api/patient/${patientId}/unassign-bed`, { method: 'POST' }); loadPatients(); }
    catch(e) { showError(e.message); }
}

// Error helpers
function showError(msg) { const e = document.getElementById('error-alert'); e.innerText = msg || ''; e.style.display = msg ? 'block' : 'none'; }
function showModalError(msg) { const e = document.getElementById('modal-error-alert'); e.innerText = msg || ''; e.style.display = msg ? 'block' : 'none'; }
function showBedModalError(msg) { const e = document.getElementById('assign-bed-error-alert'); e.innerText = msg || ''; e.style.display = msg ? 'block' : 'none'; }
function showDoctorModalError(msg) { const e = document.getElementById('assign-doctor-error-alert'); e.innerText = msg || ''; e.style.display = msg ? 'block' : 'none'; }