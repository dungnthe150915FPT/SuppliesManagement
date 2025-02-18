document.getElementById('clearSearch').addEventListener('click', function () {
    document.getElementById('searchInput').value = '';
    document.getElementById('yearInput').value = '';
    document.getElementById('nhomHangHoaSelect').value = '';
    document.getElementById('startDate').value = '';
    document.getElementById('endDate').value = '';
    document.getElementById('searchForm').submit();
});