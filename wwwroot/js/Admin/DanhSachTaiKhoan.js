document.getElementById('clearSearch').addEventListener('click', function () {
    document.getElementById('searchInput').value = '';
    document.getElementById('phoneInput').value = '';
    document.getElementById('fullNameInput').value = '';
    document.getElementById('emailInput').value = '';
    document.getElementById('genderSelect').value = '';
    document.getElementById('roleSelect').value = '';
    document.getElementById('startDate').value = '';
    document.getElementById('endDate').value = '';
    document.getElementById('searchForm').submit();
});