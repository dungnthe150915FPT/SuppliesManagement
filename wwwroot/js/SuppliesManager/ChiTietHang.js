function calculateTotals() {
    const donGiaTruocThue = parseFloat(document.getElementById('DonGiaTruocThue')?.value) || 0;
    const vat = parseFloat(document.getElementById('Vat')?.value) || 0;
    const soLuong = parseFloat(document.getElementById('SoLuong')?.value) || 0;

    if (isNaN(donGiaTruocThue) || isNaN(vat) || isNaN(soLuong)) {
        console.warn("Giá trị nhập không hợp lệ");
        return;
    }

    const donGiaSauThue = donGiaTruocThue * (1 + vat / 100);
    const tongGiaTruocThue = donGiaTruocThue * soLuong;
    const tongGiaSauThue = donGiaSauThue * soLuong;

    document.getElementById('DonGiaSauThue').value = formatNumber(donGiaSauThue);
    document.getElementById('TongGiaTruocThue').value = formatNumber(tongGiaTruocThue);
    document.getElementById('TongGiaSauThue').value = formatNumber(tongGiaSauThue);
}
function formatNumber(value) {
    return Math.round(value).toString(); // Làm tròn và chuyển thành chuỗi
}

function toggleEdit() {
    const inputs = document.querySelectorAll('input.form-control, select.form-select');
    const editButton = document.getElementById('editButton');
    const imageUploadSection = document.getElementById('imageUploadSection');
    const saveChangesSection = document.getElementById('saveChangesSection');

    inputs.forEach(input => {
        if (input.tagName === 'SELECT') {
            input.disabled = !input.disabled;
        } else {
            input.readOnly = !input.readOnly;
        }
    });

    document.getElementById('DonGiaSauThue').readOnly = true;
    document.getElementById('TongGiaTruocThue').readOnly = true;
    document.getElementById('TongGiaSauThue').readOnly = true;
    document.getElementById('SoLuongConLai').readOnly = true;
    document.getElementById('SoLuongDaXuat').readOnly = true;
    document.getElementById('Id').readOnly = true;

    if (editButton.textContent === 'Chỉnh sửa') {
        editButton.textContent = 'Hủy chỉnh sửa';
        editButton.classList.remove('btn-light');
        editButton.classList.add('btn-danger');
        imageUploadSection.classList.remove('d-none');
        saveChangesSection.classList.remove('d-none');
        // Cập nhật lại giá trị sau khi bật chế độ chỉnh sửa
        document.getElementById('DonGiaSauThue').value = formatNumber(parseFloat(document.getElementById('DonGiaSauThue').value) || 0);
        document.getElementById('TongGiaTruocThue').value = formatNumber(parseFloat(document.getElementById('TongGiaTruocThue').value) || 0);
        document.getElementById('TongGiaSauThue').value = formatNumber(parseFloat(document.getElementById('TongGiaSauThue').value) || 0);
        document.getElementById('DonGiaTruocThue').value = formatNumber(parseFloat(document.getElementById('DonGiaTruocThue').value) || 0);
    } else {
        editButton.textContent = 'Chỉnh sửa';
        editButton.classList.remove('btn-danger');
        editButton.classList.add('btn-light');
        imageUploadSection.classList.add('d-none');
        saveChangesSection.classList.add('d-none');
    }
}

function openImageModal(imageSrc, index) {
    const modal = new bootstrap.Modal(document.getElementById('imageModal'));
    const modalImage = document.getElementById('modalImage');
    modalImage.src = imageSrc;
    modal.show();
}

document.addEventListener("DOMContentLoaded", function () {
    const successAlert = document.getElementById("successAlert");
    if (successAlert) {
        setTimeout(() => {
            successAlert.style.display = "none";
        }, 10000);
    }
});
function goBack() {
    window.history.back();
}