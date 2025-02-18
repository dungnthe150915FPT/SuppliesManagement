document.getElementById('searchInput').addEventListener('keyup', function () {
    var searchTerm = this.value.toLowerCase();
    filterTable(searchTerm);
});

document.getElementById('filterDateBtn').addEventListener('click', function () {
    var startDate = document.getElementById('startDate').value;
    var endDate = document.getElementById('endDate').value;

    fetch(`?startDate=${startDate}&endDate=${endDate}`)
        .then(response => response.text())
        .then(html => {
            var tableBody = document.getElementById('invoiceTableBody');
            var parser = new DOMParser();
            var doc = parser.parseFromString(html, 'text/html');
            var newRows = doc.getElementById('invoiceTableBody').innerHTML;
            tableBody.innerHTML = newRows;
        });
});