function showExcelPopup() {
    let month = document.querySelector('input[name="month"]').value;
    let year = document.querySelector('input[name="year"]').value;

    if (!month || !year) {
        alert("Vui lòng nhập tháng và năm để xem sổ vật tư!");
        return;
    }

    // Cập nhật tiêu đề modal
    document.getElementById("popupMonth").innerText = month;
    document.getElementById("popupYear").innerText = year;

    // Tạo URL cho file Excel (giả sử endpoint `/ExcelViewer?year=...&month=...` trả về file)
    let excelUrl = `/Reports/ViewExcelVatTu?year=${year}&month=${month}`;

    // Gán vào iframe
    console.log("Đang tải URL:", excelUrl);
    document.getElementById("excelFrame").src = excelUrl;

    // Hiển thị modal (sử dụng Bootstrap Modal)
    let modal = new bootstrap.Modal(document.getElementById("excelPopup"));
    modal.show();
}

function printExcel() {
    let iframe = document.getElementById("excelFrame");
    iframe.contentWindow.focus();
    iframe.contentWindow.print();
}
