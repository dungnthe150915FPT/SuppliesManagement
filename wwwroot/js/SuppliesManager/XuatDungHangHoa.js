function fillHangHoaDetails(selectElement) {
    const selectedOption = selectElement.options[selectElement.selectedIndex];
    const nhomHang = selectedOption.getAttribute("data-nhom-hang");
    const donViTinh = selectedOption.getAttribute("data-don-vi-tinh");
    const giaTruocThue = parseFloat(selectedOption.getAttribute("data-gia-truoc-thue"));
    const vat = parseFloat(selectedOption.getAttribute("data-vat"));

    const row = selectElement.closest("tr");

    row.querySelector('input[name="NhomHang[]"]').value = nhomHang || "";
    row.querySelector('input[name="DonViTinh[]"]').value = donViTinh || "";
    row.querySelector('input[name="GiaTriTruocThue[]"]').value = giaTruocThue.toFixed(0);
    row.querySelector('input[name="VAT[]"]').value = vat.toFixed(0);

    calculateRowTotals(row);
}

function calculateRowTotals(row) {
    const soLuong = parseFloat(row.querySelector('input[name="SoLuongs[]"]').value) || 0;
    const giaTruocThue = parseFloat(row.querySelector('input[name="GiaTriTruocThue[]"]').value) || 0;
    const vat = parseFloat(row.querySelector('input[name="VAT[]"]').value) || 0;

    const giaSauThue = giaTruocThue * (1 + vat / 100);
    const tienHang = soLuong * giaTruocThue;
    const tongTien = soLuong * giaSauThue;

    row.querySelector('input[name="GiaTriSauThue[]"]').value = giaSauThue.toFixed(0);
    row.querySelector('input[name="TienHang[]"]').value = tienHang.toFixed(0);
    row.querySelector('input[name="TongTien[]"]').value = tongTien.toFixed(0);

    updateTotalThanhTien();
    updateTotalTongTien();
}

function updateTotalThanhTien() {
    const rows = document.querySelectorAll('#hangHoaTableBody tr');
    let totalThanhTien = 0;

    rows.forEach(row => {
        const tienHang = parseFloat(row.querySelector('input[name="TienHang[]"]').value) || 0;
        totalThanhTien += tienHang;
    });

    document.getElementById('ThanhTien').value = totalThanhTien.toFixed(0);
}
function updateTotalTongTien() {
    const rows = document.querySelectorAll('#hangHoaTableBody tr');
    let totalTongTien = 0;

    rows.forEach(row => {
        const tienHang = parseFloat(row.querySelector('input[name="TongTien[]"]').value) || 0;
        totalTongTien += tienHang;
    });

    document.getElementById('TongTien').value = totalTongTien.toFixed(0);
}
document.addEventListener("DOMContentLoaded", function () {
    const successAlert = document.getElementById("successAlert");
    if (successAlert) {
        setTimeout(() => {
            successAlert.style.display = "none";
        }, 10000);
    }
});
document.addEventListener("DOMContentLoaded", function () {
    const errorAlert = document.getElementById("errorAlert");
    if (errorAlert) {
        setTimeout(() => {
            errorAlert.style.display = "none";
        }, 10000);
    }
});

document.getElementById('xuatHangHoaForm').addEventListener('submit', function(e) {
    var inputs = document.querySelectorAll('input[name^="SoLuongs["]');
    var hasSelectedItems = Array.from(inputs).some(input => input.value && parseInt(input.value) > 0);
    
    if (!hasSelectedItems) {
        e.preventDefault();
        alert('Vui lòng chọn ít nhất một hàng hóa để xuất.');
    }
});