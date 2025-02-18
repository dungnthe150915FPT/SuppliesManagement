// Vẽ biểu đồ tài khoản
const accountCtx = document.getElementById('accountChart').getContext('2d');
new Chart(accountCtx, {
    type: 'pie',
    data: accountData,
    options: {
        responsive: true,
        plugins: {
            legend: { position: 'top' },
            tooltip: {
                callbacks: { label: (context) => `${context.label}: ${context.raw}%` }
            }
        }
    }
});

// Vẽ biểu đồ hàng hóa
const hangHoaCtx = document.getElementById('hangHoaChart').getContext('2d');
new Chart(hangHoaCtx, {
    type: 'pie',
    data: hangHoaData,
    options: {
        responsive: true,
        plugins: {
            legend: { position: 'top' },
            tooltip: {
                callbacks: { label: (context) => `${context.label}: ${context.raw}%` }
            }
        }
    }
});

// Vẽ biểu đồ hóa đơn
const hoaDonCtx = document.getElementById('hoaDonChart').getContext('2d');
new Chart(hoaDonCtx, {
    type: 'pie',
    data: hoaDonData,
    options: {
        responsive: true,
        plugins: {
            legend: { position: 'top' },
            tooltip: {
                callbacks: { label: (context) => `${context.label}: ${context.raw}%` }
            }
        }
    }
});

// Vẽ biểu đồ số lượng hàng hóa
const soLuongCtx = document.getElementById('soLuongChart').getContext('2d');
new Chart(soLuongCtx, {
    type: 'pie',
    data: soLuongData,
    options: {
        responsive: true,
        plugins: {
            legend: { position: 'top' },
            tooltip: {
                callbacks: { label: (context) => `${context.label}: ${context.raw}%` }
            }
        }
    }
});