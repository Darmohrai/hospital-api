document.addEventListener('DOMContentLoaded', () => {

    const queryForm = document.getElementById('query-form');
    const queryInput = document.getElementById('sql-query');
    const executeBtn = document.getElementById('execute-query-btn');

    const queryError = document.getElementById('query-error');
    const queryLoading = document.getElementById('query-loading');

    const resultContainer = document.getElementById('query-result-container');
    const queryMessage = document.getElementById('query-message');
    const tableHead = document.getElementById('result-table-head');
    const tableBody = document.getElementById('result-table-body');

    async function handleQuerySubmit(event) {
        event.preventDefault();
        clearResults();
        queryLoading.style.display = 'block';
        executeBtn.disabled = true;

        const query = queryInput.value.trim();

        if (!query) {
            showError('Запит не може бути порожнім.');
            queryLoading.style.display = 'none';
            executeBtn.disabled = false;
            return;
        }

        try {
            const result = await apiFetch('/api/admin-query/execute', {
                method: 'POST',
                body: JSON.stringify({ SqlQuery: query })
            });

            displayResults(result);

        } catch (error) {
            showError(`Помилка виконання запиту: ${error.message}`);
        } finally {
            queryLoading.style.display = 'none';
            executeBtn.disabled = false;
        }
    }

    function displayResults(data) {
        resultContainer.style.display = 'block';

        if (typeof data === 'number') {
            queryMessage.textContent = `Запит успішно виконано. Кількість змінених рядків: ${data}`;
            tableHead.innerHTML = '';
            tableBody.innerHTML = '';
            return;
        }

        if (!data || data.length === 0) {
            queryMessage.textContent = 'Запит виконано, але не повернув жодного результату.';
            tableHead.innerHTML = '';
            tableBody.innerHTML = '';
            return;
        }

        queryMessage.textContent = `Знайдено рядків: ${data.length}`;

        const headers = Object.keys(data[0]);
        tableHead.innerHTML = `<tr>${headers.map(h => `<th>${escapeHTML(h)}</th>`).join('')}</tr>`;

        let rowsHtml = '';
        for (const row of data) {
            rowsHtml += '<tr>';
            for (const header of headers) {
                rowsHtml += `<td>${escapeHTML(row[header])}</td>`;
            }
            rowsHtml += '</tr>';
        }
        tableBody.innerHTML = rowsHtml;
    }

    function showError(message) {
        queryError.textContent = message;
        queryError.style.display = 'block';
    }

    function clearResults() {
        queryError.style.display = 'none';
        resultContainer.style.display = 'none';
        queryMessage.textContent = '';
        tableHead.innerHTML = '';
        tableBody.innerHTML = '';
    }

    function escapeHTML(str) {
        if (str === null || str === undefined) {
            return '<i class="text-muted">NULL</i>';
        }
        const p = document.createElement("p");
        p.textContent = str;
        return p.innerHTML;
    }

    queryForm.addEventListener('submit', handleQuerySubmit);
});