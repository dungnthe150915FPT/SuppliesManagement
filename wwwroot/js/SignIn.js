const passField = document.getElementById("password");
const showBtn = document.getElementById("togglePassword");

showBtn.onclick = function () {
    const type = passField.getAttribute("type") === "password" ? "text" : "password";
    passField.setAttribute("type", type);
    this.classList.toggle("fa-eye");
    this.classList.toggle("fa-eye-slash");
};

document.addEventListener("DOMContentLoaded", function () {
    const errorAlert = document.getElementById("errorAlert");
    if (errorAlert) {
        setTimeout(() => {
            errorAlert.style.display = "none";
        }, 5000);
    }
});