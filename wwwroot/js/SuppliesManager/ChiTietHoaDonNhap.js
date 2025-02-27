document.addEventListener('DOMContentLoaded', function () {
    const excelButtons = document.querySelectorAll('button[onclick*="viewExcelFile"]');
    const modal = document.getElementById('excelPreviewModal');
    const excelPreviewContent = document.getElementById('excelPreviewContent');
    const closeModalButton = modal.querySelector('.btn-close');
    const printButton = modal.querySelector('.btn-print');

    excelButtons.forEach(button => {
        button.addEventListener('click', function () {
            const excelFileId = this.getAttribute('onclick').match(/'([^']+)'/)[1];
            fetch(`/SuppliesManager/ChiTietHoaDonNhap/ViewExcel?id=${excelFileId}`)
                .then(response => response.text())
                .then(data => {
                    const base64Data = atob(data);
                    const binaryData = new Uint8Array(base64Data.length);
                    for (let i = 0; i < base64Data.length; i++) {
                        binaryData[i] = base64Data.charCodeAt(i);
                    }
                    const blob = new Blob([binaryData], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                    const url = URL.createObjectURL(blob);
                    excelPreviewContent.innerHTML = `<iframe src="${url}" style="width: 100%; height: 100%;"></iframe>`;
                    const modalInstance = new bootstrap.Modal(modal);
                    modalInstance.show();
                })
                .catch(error => console.error('Error fetching Excel file:', error));
        });
    });

    closeModalButton.addEventListener('click', function () {
        const modalInstance = bootstrap.Modal.getInstance(modal);
        modalInstance.hide();
    });

    printButton.addEventListener('click', function () {
        const printWindow = window.open('', '_blank');
        printWindow.document.write(`
            <html>
                <head>
                    <title>Print Import Receipt</title>
                    <style>
                        @media print {
                            @page {
                                size: A4;
                                margin: 0;
                            }
                            body {
                                margin: 20mm;
                            }
                        }
                    </style>
                </head>
                <body>
                    ${excelPreviewContent.innerHTML}
                </body>
            </html>
        `);
        printWindow.document.close();
        printWindow.print();
    });
});

document.addEventListener("DOMContentLoaded", function () {
    function calculateTotals() {
        let totalAmount = 0;
        document.querySelectorAll('tbody tr').forEach(row => {
            let soLuong = parseFloat(row.querySelector('input[name*="SoLuong"]').value) || 0;
            let donGiaTruocThue = parseFloat(row.querySelector('input[name*="DonGiaTruocThue"]').value) || 0;
            let vat = parseFloat(row.querySelector('input[name*="VAT"]').value) || 0;

            // Tính đơn giá sau thuế
            let donGiaSauThue = donGiaTruocThue * (1 + vat / 100);
            let tongGiaTruocThue = donGiaTruocThue * soLuong;
            let tongGiaSauThue = donGiaSauThue * soLuong;

            // Cập nhật giá trị vào các input readonly
            row.querySelector('input[name*="DonGiaSauThue"]').value = donGiaSauThue.toFixed(0);
            row.querySelector('input[name*="TongGiaTruocThue"]').value = tongGiaTruocThue.toFixed(0);
            row.querySelector('input[name*="TongGiaSauThue"]').value = tongGiaSauThue.toFixed(0);

            totalAmount += tongGiaSauThue;
        });

        // Cập nhật tổng tiền
        document.getElementById('thanhTien').value = totalAmount.toFixed(0);
    }

    // Gán sự kiện input để cập nhật ngay lập tức khi thay đổi số lượng, đơn giá trước thuế hoặc VAT
    document.querySelectorAll('input[name*="SoLuong"], input[name*="DonGiaTruocThue"], input[name*="VAT"]').forEach(input => {
        input.addEventListener('input', calculateTotals);
    });

    // Chạy tính toán ngay khi trang tải
    calculateTotals();
});


document.addEventListener("DOMContentLoaded", function () {
    const successAlert = document.getElementById("successAlert");
    if (successAlert) {
        setTimeout(() => {
            successAlert.style.display = "none";
        }, 5000);
    }
});

document.addEventListener("DOMContentLoaded", function () {
    const errorAlert = document.getElementById("errorAlert");
    if (errorAlert) {
        setTimeout(() => {
            errorAlert.style.display = "none";
        }, 5000);
    }
});