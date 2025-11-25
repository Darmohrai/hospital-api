document.addEventListener('DOMContentLoaded', () => {
    const role = getUserRole();
    if (role !== 'Admin') {
        alert('У вас немає доступу до цього ресурсу.');
        window.location.href = 'index.html';
        return;
    }

    loadUpgradeRequests();

    document.getElementById('requests-table-body').addEventListener('click', handleRequestAction);
});

async function loadUpgradeRequests() {
    const tableBody = document.getElementById('requests-table-body');
    tableBody.innerHTML = '<tr><td colspan="5">Завантаження...</td></tr>';

    try {
        const requests = await apiFetch('/api/auth/pending-requests');

        tableBody.innerHTML = '';

        if (requests.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="5">Нових запитів на підвищення немає.</td></tr>';
            return;
        }

        requests.forEach(req => {
            console.log(req);
            const row = document.createElement('tr');
            console.log(req.requestedRole)
            row.innerHTML = `
                <td>${req.requestId}</td>
                <td>${req.userName}</td>
                <td>${req.role}</td>
                <td>${new Date(req.requestDate).toLocaleString('uk-UA')}</td>
                <td>
                    <button class="btn btn-success btn-sm btn-approve" data-id="${req.userId}" data-role="${req.role}">
                        Схвалити
                    </button>
                    <button class="btn btn-danger btn-sm btn-reject" data-id="${req.userId}">
                        Відхилити
                    </button>
                </td>
            `;
            tableBody.appendChild(row);
        });

    } catch (error) {
        tableBody.innerHTML = `<tr><td colspan="5" class="text-danger">Помилка завантаження: ${error.message}</td></tr>`;
    }
}

async function handleRequestAction(event) {
    const target = event.target;
    const id = target.dataset.id;
    const targetRole = target.dataset.role;

    console.log("targetRole =", targetRole);


    if (!id) return;

    let endpoint = '';
    let actionName = '';

    if (target.classList.contains('btn-approve')) {
        endpoint = `/api/auth/approve-guest/${id}`;
        actionName = 'схвалити';
    } else if (target.classList.contains('btn-reject')) {
        endpoint = `/api/auth/reject-guest/${id}`;
        actionName = 'відхилити';
    } else {
        return;
    }
    
    target.disabled = true;

    try {
        await apiFetch(endpoint,
            {
                method: 'POST',
                body: JSON.stringify({
                    targetRole: targetRole 
                })
            });

        alert(`Запит ${id} успішно ${actionName}.`);
        loadUpgradeRequests();

    } catch (error) {
        alert(`Помилка: ${error.message}`);
        target.disabled = false;
    }
}