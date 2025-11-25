document.addEventListener('DOMContentLoaded', () => {

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

    const clinicsTableBody = document.getElementById('clinics-table-body');
    const addClinicBtn = document.getElementById('add-clinic-btn');
    const pageError = document.getElementById('page-error');

    const mainModalEl = document.getElementById('clinic-manage-modal');
    const mainModal = new bootstrap.Modal(mainModalEl);
    const mainModalTitle = document.getElementById('clinic-modal-title');
    const mainModalError = document.getElementById('modal-error');
    const clinicForm = document.getElementById('clinic-form');
    const clinicIdInput = document.getElementById('clinic-id');
    const clinicNameInput = document.getElementById('clinic-name');
    const clinicAddressInput = document.getElementById('clinic-address');
    const clinicHospitalIdSelect = document.getElementById('clinic-hospital-id');
    const saveClinicBtn = document.getElementById('save-clinic-btn');

    let currentMode = 'create';
    let currentUserRole = getUserRole();

    function showPageError(message) {
        pageError.textContent = message;
        pageError.style.display = message ? 'block' : 'none';
    }

    function showMainModalError(message) {
        mainModalError.textContent = message;
        mainModalError.style.display = message ? 'block' : 'none';
    }

    async function loadHospitalsForSelect(selectedId = null) {
        try {
            const hospitals = await apiFetch('/api/hospital');

            clinicHospitalIdSelect.innerHTML = '<option value="">Не прив\'язано</option>';

            if (hospitals && hospitals.length > 0) {
                hospitals.forEach(hospital => {
                    const isSelected = hospital.id == selectedId;
                    clinicHospitalIdSelect.innerHTML += `
                        <option value="${hospital.id}" ${isSelected ? 'selected' : ''}>
                            ID: ${hospital.id} - ${hospital.name}
                        </option>
                    `;
                });
            }
        } catch (error) {
            console.error("Не вдалося завантажити лікарні для select:", error);
            showMainModalError("Не вдалося завантажити список лікарень.");
        }
    }

    async function loadClinics() {
        try {
            showPageError(null);
            const clinics = await apiFetch('/api/clinic');
            clinicsTableBody.innerHTML = '';

            console.log(clinics);

            if (clinics && clinics.length > 0) {
                for (const clinic of clinics) {
                    const row = document.createElement('tr');

                    let actionsHtml = '';
                    if (currentUserRole === 'Admin' || currentUserRole === 'Operator') {
                        actionsHtml = `
                            <button class="btn btn-sm btn-primary" data-action="edit" data-id="${clinic.id}" title="Редагувати">
                                <i class="bi bi-pencil-square"></i>
                            </button>
                        `;
                    }
                    if (currentUserRole === 'Admin') {
                        actionsHtml += `
                            <button class="btn btn-sm btn-danger ms-1" data-action="delete" data-id="${clinic.id}" title="Видалити">
                                <i class="bi bi-trash"></i>
                            </button>
                        `;
                    }

                    let hospital;

                    if (clinic.hospitalId) {
                        hospital = await apiFetch(`/api/hospital/${clinic.hospitalId}`);
                    }

                    let hospitalName = (hospital && hospital.name) ? hospital.name : 'Не прив\'язано';

                    row.innerHTML = `
                        <td>${clinic.id}</td>
                        <td>${clinic.name}</td>
                        <td>${clinic.address}</td>
                        <td>${hospitalName}</td>
                        <td>${actionsHtml || 'Недоступно'}</td>
                    `;

                    clinicsTableBody.appendChild(row);
                }
            } else {
                clinicsTableBody.innerHTML = '<tr><td colspan="4" class="text-center">Поліклініки не знайдено.</td></tr>';
            }
        } catch (error) {
            showPageError(`Не вдалося завантажити поліклініки: ${error.message}`);
        }
    }

    function handleTableClick(event) {
        const target = event.target.closest('button');
        if (!target) return;

        const action = target.dataset.action;
        const id = target.dataset.id;

        if (action === 'edit') {
            handleEditClinic(id);
        } else if (action === 'delete') {
            handleDeleteClinic(id);
        }
    }

    async function handleCreateClinic() {
        currentMode = 'create';
        showMainModalError(null);
        clinicForm.reset();
        clinicIdInput.value = '';

        mainModalTitle.textContent = 'Створити нову поліклініку';
        saveClinicBtn.textContent = 'Створити';

        await loadHospitalsForSelect(null);

        mainModal.show();
    }

    async function handleEditClinic(id) {
        currentMode = 'edit';
        showMainModalError(null);
        clinicForm.reset();

        mainModalTitle.textContent = 'Завантаження даних...';
        saveClinicBtn.textContent = 'Зберегти зміни';
        mainModal.show();

        try {
            const clinic = await apiFetch(`/api/clinic/${id}`);

            if (clinic) {
                clinicIdInput.value = clinic.id;
                clinicNameInput.value = clinic.name;
                clinicAddressInput.value = clinic.address;

                await loadHospitalsForSelect(clinic.hospitalId);

                mainModalTitle.textContent = `Редагування: ${clinic.name}`;
            } else {
                showMainModalError(`Поліклініку з ID ${id} не знайдено.`);
            }

        } catch (error) {
            showMainModalError(`Не вдалося завантажити дані поліклініки: ${error.message}`);
            mainModal.hide();
        }
    }

    async function handleClinicFormSubmit(event) {
        event.preventDefault();
        showMainModalError(null);

        const name = clinicNameInput.value;
        const address = clinicAddressInput.value;
        const hospitalId = clinicHospitalIdSelect.value ? parseInt(clinicHospitalIdSelect.value, 10) : null;

        try {
            if (currentMode === 'create') {
                const createDto = {name, address, hospitalId};

                await apiFetch('/api/clinic', {
                    method: 'POST',
                    body: JSON.stringify(createDto)
                });

            } else {
                const id = parseInt(clinicIdInput.value, 10);
                const updateModel = {id, name, address, hospitalId};

                await apiFetch(`/api/clinic/${id}`, {
                    method: 'PUT',
                    body: JSON.stringify(updateModel)
                });
            }

            mainModal.hide();
            loadClinics();

        } catch (error) {
            showMainModalError(`Не вдалося зберегти поліклініку: ${error.message}`);
        }
    }

    async function handleDeleteClinic(id) {
        if (!confirm(`Ви впевнені, що хочете видалити поліклініку з ID: ${id}?`)) {
            return;
        }

        try {
            await apiFetch(`/api/clinic/${id}`, {method: 'DELETE'});
            loadClinics();
        } catch (error) {
            showPageError(`Помилка видалення: ${error.message}`);
        }
    }

    clinicsTableBody.addEventListener('click', handleTableClick);

    addClinicBtn.addEventListener('click', handleCreateClinic);
    clinicForm.addEventListener('submit', handleClinicFormSubmit);

    loadClinics();
});