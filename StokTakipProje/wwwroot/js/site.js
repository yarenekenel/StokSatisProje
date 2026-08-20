function enableLiveSearch(inputId, tableBodyId) {
    const input = document.getElementById(inputId);
    const tbody = document.getElementById(tableBodyId);
    if (!input || !tbody) return;

    input.addEventListener('input', function () {
        const term = input.value.trim().toLocaleLowerCase('tr-TR');
        const rows = tbody.querySelectorAll('tr[data-search-row]');
        let visibleCount = 0;

        rows.forEach(row => {
            const text = row.getAttribute('data-search-text') || '';
            const match = text.includes(term);
            row.style.display = match ? '' : 'none';
            if (match) visibleCount++;
        });

        const emptyRow = tbody.querySelector('.search-empty-row');
        if (emptyRow) {
            emptyRow.style.display = visibleCount === 0 ? '' : 'none';
        }
    });
}