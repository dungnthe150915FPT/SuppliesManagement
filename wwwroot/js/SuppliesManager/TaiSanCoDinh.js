function showExcelPopup() {
    let year = document.querySelector('input[name="year"]').value;

    if (!year) {
        alert("Vui lòng nhập năm để xem sổ Tài Sản Cố Định!");
        return;
    }

    // Cập nhật tiêu đề modal
    document.getElementById("popupYear").innerText = year;

    let excelUrl = `/Reports/ViewExcelTaiSanCoDinh?year=${year}`;

    // Kiểm tra đường dẫn trong console
    console.log("Tải dữ liệu từ URL:", excelUrl);

    // Gán vào iframe
    let iframe = document.getElementById("excelFrame");
    iframe.src = excelUrl;

    // Hiển thị modal (sử dụng Bootstrap Modal)
    let modal = new bootstrap.Modal(document.getElementById("excelPopup"));
    modal.show();
}

function printExcel() {
    let iframe = document.getElementById("excelFrame");
    iframe.contentWindow.focus();
    iframe.contentWindow.print();
}
