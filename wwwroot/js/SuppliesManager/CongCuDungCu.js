function showExcelPopup() {
    let year = document.querySelector('input[name="year"]').value;

    if (!month || !year) {
        alert("Vui lòng nhập năm để xem sổ Công Cụ Dụng Cụ!");
        return;
    }

    // Cập nhật tiêu đề modal
    document.getElementById("popupYear").innerText = year;

    // Tạo URL cho file Excel (giả sử endpoint `/ExcelViewer?year=...&month=...` trả về file)
    let excelUrl = `/Reports/ViewExcelCongCuDungCu?year=${year}`;

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
