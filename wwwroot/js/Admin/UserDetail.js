document.addEventListener("DOMContentLoaded", function () {
    const successAlert = document.getElementById("successAlert");
    if (successAlert) {
        setTimeout(() => {
            successAlert.style.display = "none";
        }, 10000);
    }

    const passwordField = document.getElementById("password");
    const togglePasswordButton = document.getElementById("togglePassword");
    const togglePasswordIcon = document.getElementById("togglePasswordIcon");

    togglePasswordButton.addEventListener("click", function () {
        const type = passwordField.getAttribute("type") === "password" ? "text" : "password";
        passwordField.setAttribute("type", type);
        togglePasswordIcon.classList.toggle("fa-eye");
        togglePasswordIcon.classList.toggle("fa-eye-slash");
    });
});