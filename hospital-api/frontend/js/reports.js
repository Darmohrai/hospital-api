// Глобальні змінні для кешування списків
let allHospitals = [];
let allClinics = [];
let allDoctors = [];

// Константи спеціалізацій (мають збігатися з бекендом)
const specialties = [
    "Surgeon", "Neurologist", "Ophthalmologist", "Dentist", "Radiologist", "Gynecologist", "Cardiologist"
];

const academicDegrees = {
    0: "Немає", 1: "Кандидат наук", 2: "Доктор наук"
};

const academicTitles = {
    0: "Немає", 1: "Доцент", 2: "Професор"
};

const supportRoles = {
    0: "Медсестра", 1: "Санітар", 2: "Прибиральник"
};

document.addEventListener('DOMContentLoaded', async () => {
    // Перевірка авторизації
    const role = getUserRole();
    if (!role) {
        window.location.href = 'login.html';
        return;
    }

    // Завантаження допоміжних даних (Dropdowns)
    await loadCommonData();

    // Налаштування слухачів подій для всіх форм
    setupEventListeners();
});

/**
 * Завантажує списки лікарень, клінік та лікарів для фільтрів
 */
async function loadCommonData() {
    try {
        const [hospitals, clinics, doctors] = await Promise.all([
            apiFetch('/api/hospital'),
            apiFetch('/api/clinic'),
            apiFetch('/api/staff/doctors')
        ]);
        allHospitals = hospitals || [];
        allClinics = clinics || [];
        allDoctors = doctors || [];

        populateDropdowns();
    } catch (error) {
        console.error("Помилка завантаження довідників:", error);
        showError("Не вдалося завантажити списки закладів. Звіти можуть працювати некоректно.");
    }
}

function populateDropdowns() {
    // Заповнення лікарень
    document.querySelectorAll('.select-hospital').forEach(select => {
        // Зберігаємо перший option (плейсхолдер)
        const placeholder = select.firstElementChild;
        select.innerHTML = '';
        select.appendChild(placeholder);
        allHospitals.forEach(h => select.innerHTML += `<option value="${h.id}">${h.name}</option>`);
    });

    // Заповнення клінік
    document.querySelectorAll('.select-clinic').forEach(select => {
        const placeholder = select.firstElementChild;
        select.innerHTML = '';
        select.appendChild(placeholder);
        allClinics.forEach(c => select.innerHTML += `<option value="${c.id}">${c.name}</option>`);
    });

    // Заповнення лікарів
    document.querySelectorAll('.select-doctor').forEach(select => {
        const placeholder = select.firstElementChild;
        select.innerHTML = '';
        select.appendChild(placeholder);
        allDoctors.forEach(d => select.innerHTML += `<option value="${d.id}">${d.fullName} (${d.specialty})</option>`);
    });

    // Заповнення спеціальностей
    document.querySelectorAll('.select-specialty').forEach(select => {
        const placeholder = select.firstElementChild;
        select.innerHTML = '';
        select.appendChild(placeholder);
        specialties.forEach(s => select.innerHTML += `<option value="${s}">${s}</option>`);
    });
}

function setupEventListeners() {
    // 1. Daily Appointments
    bindForm('form-daily-app', '/api/report/daily-appointments', renderDailyApp);

    // 2. Hospital Capacity
    bindForm('form-hospital-cap', (formData) => `/api/report/hospital-capacity/${formData.get('hospitalId')}`, renderHospitalCap, 'GET', true);

    // 3. Laboratory Report
    bindForm('form-lab', '/api/report/laboratory-report', renderLabReport);

    // 5. Patient Operations
    bindForm('form-pat-ops', '/api/report/patient-operations', renderPatientOps);

    // 6. Patients by Specialty
    bindForm('form-patients-spec', '/api/report/patients-by-clinic-specialty', renderPatientList);

    // 7. Inpatient Report
    bindForm('form-inpatient', '/api/report/inpatient-report', renderPatientList);

    // 8. Doctor Report (Extended)
    bindForm('form-doc-filter', '/api/report/doctor-report', renderDoctorReport);

    // 9. Doctor Operations Count
    bindForm('form-doc-ops', '/api/report/doctor-operation-report', renderDoctorReport);

    // 10. Support Staff
    bindForm('form-support-staff', '/api/report/support-staff-report', renderSupportStaff);

    // 11. Doctor Fatality Rate
    bindForm('form-doc-fatal', '/api/report/doctor-performance-report', renderDoctorPerformance);

    // 13. Department Occupancy (Cascade logic)
    const hospitalSelect = document.getElementById('trigger-dept-load');
    const deptSelect = document.getElementById('dept-select');

    hospitalSelect.addEventListener('change', async () => {
        deptSelect.innerHTML = '<option value="">Завантаження...</option>';
        deptSelect.disabled = true;
        const hospId = hospitalSelect.value;
        if (!hospId) return;

        try {
            // Отримуємо деталі лікарні, щоб дістати відділення
            const hospital = await apiFetch(`/api/hospital/${hospId}`);
            let departments = [];
            for (const building of hospital.buildings) {
                const result = await apiFetch(`/api/hospital/department/building/${building.id}/with-rooms`);
                departments = departments.concat(result);
            }
            deptSelect.innerHTML = '<option value="">Оберіть відділення</option>';
            if (departments && departments.length > 0) {
                departments.forEach(d => {
                    deptSelect.innerHTML += `<option value="${d.id}">${d.name}</option>`;
                });
                deptSelect.disabled = false;
            } else {
                deptSelect.innerHTML = '<option value="">Відділень немає</option>';
            }
        } catch (e) {
            console.error(e);
            deptSelect.innerHTML = '<option value="">Помилка завантаження</option>';
        }
    });

    bindForm('form-dept-occupancy', (formData) => `/api/report/department-occupancy/${formData.get('departmentId')}`, renderDeptOccupancy, 'GET', true);
}

/**
 * Універсальна функція для прив'язки форми до API
 * @param {string} formId ID форми
 * @param {string|function} endpoint URL або функція, що повертає URL
 * @param {function} renderFunc Функція для відображення результату
 * @param {string} method HTTP метод (зазвичай GET для звітів)
 * @param {boolean} isUrlDynamic Чи генерується URL динамічно на основі даних форми
 */
function bindForm(formId, endpoint, renderFunc, method = 'GET', isUrlDynamic = false) {
    const form = document.getElementById(formId);
    const resultDiv = document.getElementById(formId.replace('form-', 'res-')); // Convention: res-ID

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        resultDiv.style.display = 'none';
        resultDiv.innerHTML = '<div class="text-center text-muted">Завантаження...</div>';
        resultDiv.style.display = 'block';

        const formData = new FormData(form);
        let url = '';

        if (isUrlDynamic) {
            url = endpoint(formData);
        } else {
            // Будуємо Query String
            const params = new URLSearchParams();
            for (const pair of formData.entries()) {
                if (pair[1]) params.append(pair[0], pair[1]);
            }
            url = `${endpoint}?${params.toString()}`;
        }

        try {
            const data = await apiFetch(url);
            resultDiv.innerHTML = '';
            renderFunc(data, resultDiv);
        } catch (error) {
            resultDiv.innerHTML = `<div class="text-danger">Помилка: ${error.message}</div>`;
        }
    });
}

// === ФУНКЦІЇ ВІДОБРАЖЕННЯ (RENDERERS) ===

function renderDailyApp(data, container) {
    if (!data) return;
    let html = `<h6>${data.filterDescription}</h6>
                <p class="lead">Всього пацієнтів: <strong>${data.patientCount}</strong></p>`;

    if (data.countByDoctor && data.countByDoctor.length > 0) {
        html += `<table class="table table-sm table-striped"><thead><tr><th>Лікар</th><th>Пацієнтів</th></tr></thead><tbody>`;
        data.countByDoctor.forEach(d => {
            html += `<tr><td>${d.doctorFullName}</td><td>${d.patientCount}</td></tr>`;
        });
        html += `</tbody></table>`;
    }
    container.innerHTML = html;
}

function renderHospitalCap(data, container) {
    let html = `<h6>${data.hospitalName}</h6>
                <div class="d-flex justify-content-between mb-2">
                    <span>Палат: ${data.totalRoomCount}</span>
                    <span>Ліжок: ${data.totalBedCount}</span>
                </div>`;

    html += `<table class="table table-sm table-bordered">
                <thead class="table-light">
                    <tr><th>Відділення</th><th>Ліжок</th><th>Вільних</th><th>Вільних палат</th></tr>
                </thead><tbody>`;

    data.departments.forEach(dept => {
        const rowClass = dept.freeBedCount === 0 ? 'table-danger' : (dept.freeBedCount < 5 ? 'table-warning' : '');
        html += `<tr class="${rowClass}">
                    <td>${dept.departmentName}</td>
                    <td>${dept.bedCount}</td>
                    <td><strong>${dept.freeBedCount}</strong></td>
                    <td>${dept.fullyFreeRoomCount}</td>
                 </tr>`;
    });
    html += `</tbody></table>`;
    container.innerHTML = html;
}

function renderLabReport(data, container) {
    let html = `<p>Період: ${new Date(data.startDate).toLocaleDateString()} - ${new Date(data.endDate).toLocaleDateString()}</p>
                <p>Всього аналізів: <strong>${data.totalAnalysesConducted}</strong></p>
                <p>В середньому за день: <strong>${data.averageAnalysesPerDay.toFixed(1)}</strong></p>`;

    if (data.dailyCounts && data.dailyCounts.length > 0) {
        html += `<hr><small>Деталізація по днях:</small><ul class="list-group list-group-flush">`;
        data.dailyCounts.forEach(d => {
            html += `<li class="list-group-item d-flex justify-content-between py-1">
                        <span>${new Date(d.date).toLocaleDateString()}</span>
                        <span>${d.count}</span>
                     </li>`;
        });
        html += `</ul>`;
    }
    container.innerHTML = html;
}

function renderPatientOps(data, container) {
    if (!data || data.length === 0) {
        container.innerHTML = '<div class="text-muted">Операцій не знайдено за цей період.</div>';
        return;
    }
    let html = `<table class="table table-sm table-hover">
                <thead><tr><th>Дата</th><th>Пацієнт</th><th>Операція</th><th>Лікар</th><th>Місце</th></tr></thead><tbody>`;
    data.forEach(row => {
        html += `<tr>
                    <td>${new Date(row.operationDate).toLocaleDateString()}</td>
                    <td>${row.patientFullName}</td>
                    <td>${row.operationType}</td>
                    <td>${row.doctorName}</td>
                    <td><small>${row.locationName}</small></td>
                 </tr>`;
    });
    html += `</tbody></table>`;
    container.innerHTML = html;
}

function renderPatientList(data, container) {
    if (!data || data.length === 0) {
        container.innerHTML = '<div class="text-muted">Пацієнтів не знайдено.</div>';
        return;
    }
    let html = `<p>Знайдено: ${data.length}</p><ul class="list-group">`;
    data.forEach(p => {
        html += `<li class="list-group-item">
                    <strong>${p.fullName}</strong> <span class="text-muted">(ID: ${p.id})</span><br>
                    <small>Д.Н: ${new Date(p.dateOfBirth).toLocaleDateString()} | Стан: ${p.healthStatus}</small>
                 </li>`;
    });
    html += `</ul>`;
    container.innerHTML = html;
}

function renderDoctorReport(data, container) {
    let html = `<p>${data.filterDescription}</p><p>Знайдено лікарів: <strong>${data.totalCount}</strong></p>`;
    if (data.doctors && data.doctors.length > 0) {
        html += `<table class="table table-sm"><thead><tr><th>ПІБ</th><th>Спец.</th><th>Стаж</th><th>Звання</th></tr></thead><tbody>`;
        data.doctors.forEach(d => {
            const degree = academicDegrees[d.academicDegree] || '-';
            const title = academicTitles[d.academicTitle] || '-';
            html += `<tr>
                        <td>${d.fullName}</td>
                        <td>${d.specialty}</td>
                        <td>${d.workExperienceYears} р.</td>
                        <td><small>${degree} / ${title}</small></td>
                     </tr>`;
        });
        html += `</tbody></table>`;
    }
    container.innerHTML = html;
}

function renderSupportStaff(data, container) {
    let html = `<p>${data.filterDescription}</p><p>Кількість: <strong>${data.totalCount}</strong></p>`;
    if (data.staff && data.staff.length > 0) {
        html += `<ul class="list-group">`;
        data.staff.forEach(s => {
            html += `<li class="list-group-item d-flex justify-content-between">
                        <span>${s.fullName} (${supportRoles[s.role]})</span>
                        <span class="badge bg-secondary">${s.workExperienceYears} р. стажу</span>
                     </li>`;
        });
        html += `</ul>`;
    }
    container.innerHTML = html;
}

function renderDoctorPerformance(data, container) {
    if (!data || data.length === 0) {
        container.innerHTML = '<div class="text-muted">Дані відсутні.</div>';
        return;
    }
    // Сортуємо за % летальності (від більшого)
    data.sort((a, b) => b.fatalityRatePercent - a.fatalityRatePercent);

    let html = `<table class="table table-sm"><thead><tr><th>Лікар</th><th>Операцій</th><th>Летальних</th><th>%</th></tr></thead><tbody>`;
    data.forEach(d => {
        let color = d.fatalityRatePercent > 5 ? 'text-danger fw-bold' : 'text-success';
        html += `<tr>
                    <td>${d.doctorFullName}<br><small class="text-muted">${d.specialty}</small></td>
                    <td>${d.totalOperations}</td>
                    <td>${d.fatalOperations}</td>
                    <td class="${color}">${d.fatalityRatePercent.toFixed(1)}%</td>
                 </tr>`;
    });
    html += `</tbody></table>`;
    container.innerHTML = html;
}

function renderDeptOccupancy(data, container) {
    let html = `<h6>${data.departmentName}</h6>
                <div class="progress mb-2" style="height: 20px;">
                    <div class="progress-bar ${getOccupancyColor(data.overallOccupancyPercent)}" 
                         role="progressbar" 
                         style="width: ${data.overallOccupancyPercent}%">
                        ${data.overallOccupancyPercent.toFixed(1)}%
                    </div>
                </div>
                <p class="small">Зайнято ${data.totalOccupiedBeds} з ${data.totalCapacity} місць.</p>`;

    if (data.rooms && data.rooms.length > 0) {
        html += `<div class="row g-2">`;
        data.rooms.forEach(r => {
            const bgClass = r.occupancyPercent >= 100 ? 'bg-danger text-white' : (r.occupancyPercent > 50 ? 'bg-warning' : 'bg-success text-white');
            html += `<div class="col-4 col-md-3">
                        <div class="p-2 border text-center rounded ${bgClass}" style="font-size:0.8rem;">
                            <div>Кім. ${r.roomNumber}</div>
                            <div>${r.occupiedBeds}/${r.roomCapacity}</div>
                        </div>
                     </div>`;
        });
        html += `</div>`;
    }
    container.innerHTML = html;
}

function getOccupancyColor(percent) {
    if (percent < 50) return 'bg-success';
    if (percent < 85) return 'bg-warning';
    return 'bg-danger';
}

function showError(message) {
    const el = document.getElementById('error-alert');
    if(el) { el.textContent = message; el.style.display = 'block'; }
    else alert(message);
}