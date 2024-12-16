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