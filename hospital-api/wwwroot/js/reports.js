const doctorSpecialties = {
    "Surgeon": "Хірург",
    "Neurologist": "Невролог",
    "Ophthalmologist": "Окуліст",
    "Dentist": "Стоматолог",
    "Radiologist": "Рентгенолог",
    "Gynecologist": "Гінеколог",
    "Cardiologist": "Кардіолог"
};

function getSpecialtyTranslation(specialtyEn) {
    if (!specialtyEn) return "";

    if (doctorSpecialties[specialtyEn]) {
        return doctorSpecialties[specialtyEn];
    }

    const key = Object.keys(doctorSpecialties).find(k => k.toLowerCase() === specialtyEn.toLowerCase());
    if (key) {
        return doctorSpecialties[key];
    }

    return specialtyEn;
}

const specialties = [
    "Surgeon", "Neurologist", "Ophthalmologist", "Dentist", "Radiologist", "Gynecologist", "Cardiologist"
];

let allHospitals = [];
let allClinics = [];
let allDoctors = [];

const academicDegrees = {
    "None": "Немає", "Candidate": "Кандидат наук", "Doctor": "Доктор наук"
};
const academicTitles = {
    "None": "Немає", "AssociateProfessor": "Доцент", "Professor": "Професор"
};
const supportRoles = {
    0: "Медсестра", 1: "Санітар", 2: "Прибиральник"
};

document.addEventListener('DOMContentLoaded', async () => {
    const role = getUserRole();
    if (!role) {
        window.location.href = 'login.html';
        return;
    }

    await loadCommonData();
    setupEventListeners();
});

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
    document.querySelectorAll('.select-hospital').forEach(select => {
        const placeholder = select.firstElementChild;
        select.innerHTML = '';
        select.appendChild(placeholder);
        allHospitals.forEach(h => select.innerHTML += `<option value="${h.id}">${h.name}</option>`);
    });

    document.querySelectorAll('.select-clinic').forEach(select => {
        const placeholder = select.firstElementChild;
        select.innerHTML = '';
        select.appendChild(placeholder);
        allClinics.forEach(c => select.innerHTML += `<option value="${c.id}">${c.name}</option>`);
    });

    document.querySelectorAll('.select-doctor').forEach(select => {
        const placeholder = select.firstElementChild;
        select.innerHTML = '';
        select.appendChild(placeholder);
        allDoctors.forEach(d => {
            const specUA = getSpecialtyTranslation(d.specialty);
            select.innerHTML += `<option value="${d.id}">${d.fullName} (${specUA})</option>`;
        });
    });

    document.querySelectorAll('.select-specialty').forEach(select => {
        const placeholder = select.firstElementChild;
        select.innerHTML = '';
        select.appendChild(placeholder);
        specialties.forEach(s => {
            const specUA = getSpecialtyTranslation(s);
            select.innerHTML += `<option value="${s}">${specUA}</option>`;
        });
    });
}

function setupEventListeners() {
    bindForm('form-daily-app', '/api/report/daily-appointments', renderDailyApp);
    bindForm('form-hospital-cap', (formData) => `/api/report/hospital-capacity/${formData.get('hospitalId')}`, renderHospitalCap, 'GET', true);
    bindForm('form-lab', '/api/report/laboratory-report', renderLabReport);
    bindForm('form-pat-ops', '/api/report/patient-operations', renderPatientOps);
    bindForm('form-patients-spec', '/api/report/patients-by-clinic-specialty', renderPatientList);
    bindForm('form-inpatient', '/api/report/inpatient-report', renderPatientList);
    bindForm('form-doc-filter', '/api/report/doctor-report', renderDoctorReport);
    bindForm('form-doc-ops', '/api/report/doctor-operation-report', renderDoctorReport);
    bindForm('form-support-staff', '/api/report/support-staff-report', renderSupportStaff);
    bindForm('form-doc-fatal', '/api/report/doctor-performance-report', renderDoctorPerformance);

    const hospitalSelect = document.getElementById('trigger-dept-load');
    const deptSelect = document.getElementById('dept-select');

    if (hospitalSelect) {
        hospitalSelect.addEventListener('change', async () => {
            deptSelect.innerHTML = '<option value="">Завантаження...</option>';
            deptSelect.disabled = true;

            const hospId = hospitalSelect.value;
            if (!hospId) return;

            try {
                const hospital = await apiFetch(`/api/hospital/${hospId}`);
                let departments = [];

                if (hospital.buildings) {
                    for (const building of hospital.buildings) {
                        const result = await apiFetch(`/api/hospital/department/building/${building.id}/with-rooms`);
                        departments = departments.concat(result);
                    }
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
    }

    bindForm('form-dept-occupancy', (formData) => `/api/report/department-occupancy/${formData.get('departmentId')}`, renderDeptOccupancy, 'GET', true);
}

function bindForm(formId, endpoint, renderFunc, method = 'GET', isUrlDynamic = false) {
    const form = document.getElementById(formId);
    const resultDiv = document.getElementById(formId.replace('form-', 'res-'));

    if (!form) return;

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

function renderDailyApp(data, container) {
    if (!data) return;

    let desc = data.filterDescription;
    for (const [en, ua] of Object.entries(doctorSpecialties)) {
        desc = desc.replace(en, ua);
    }

    let html = `<h6>${desc}</h6>
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
    // Переклад опису фільтру
    let desc = data.filterDescription;
    for (const [en, ua] of Object.entries(doctorSpecialties)) {
        desc = desc.replace(en, ua);
    }

    let html = `<p>${desc}</p><p>Знайдено лікарів: <strong>${data.totalCount}</strong></p>`;

    if (data.doctors && data.doctors.length > 0) {
        html += `<table class="table table-sm"><thead><tr><th>ПІБ</th><th>Спец.</th><th>Стаж</th><th>Звання</th></tr></thead><tbody>`;
        data.doctors.forEach(d => {
            const degree = academicDegrees[d.academicDegree] || '-';
            const title = academicTitles[d.academicTitle] || '-';
            // Переклад спеціальності в таблиці
            const specUA = getSpecialtyTranslation(d.specialty);

            html += `<tr>
                        <td>${d.fullName}</td>
                        <td>${specUA}</td>
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
    data.sort((a, b) => b.fatalityRatePercent - a.fatalityRatePercent);
    let html = `<table class="table table-sm"><thead><tr><th>Лікар</th><th>Операцій</th><th>Летальних</th><th>%</th></tr></thead><tbody>`;
    data.forEach(d => {
        let color = d.fatalityRatePercent > 5 ? 'text-danger fw-bold' : 'text-success';
        const specUA = getSpecialtyTranslation(d.specialty);

        html += `<tr>
                    <td>${d.doctorFullName}<br><small class="text-muted">${specUA}</small></td>
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