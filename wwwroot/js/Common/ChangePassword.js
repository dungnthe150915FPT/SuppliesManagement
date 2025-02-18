function togglePasswordVisibility(inputId, iconId) {
    const passField = document.getElementById(inputId);
    const icon = document.getElementById(iconId);

    // Đổi kiểu hiển thị mật khẩu
    const type = passField.getAttribute("type") === "password" ? "text" : "password";
    passField.setAttribute("type", type);

    // Đổi icon
    icon.classList.toggle("fa-eye");
    icon.classList.toggle("fa-eye-slash");
}

// Gán sự kiện cho các nút toggle
document.getElementById("toggleCurrentPassword").onclick = () => togglePasswordVisibility("currentPassword", "toggleCurrentPassword");
document.getElementById("toggleNewPassword").onclick = () => togglePasswordVisibility("newPassword", "toggleNewPassword");
document.getElementById("toggleReNewPassword").onclick = () => togglePasswordVisibility("reNewPassword", "toggleReNewPassword");

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