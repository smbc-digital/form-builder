function ValidateSize(file) {
    var FileSize = file.files[0].size / 1024 / 1024; // in MB
    var sizeValidation = document.getElementById(`${file.id}-fileSizeError`);
    var next = document.getElementById("submit");
    var input = document.getElementById(file.id);
    var validation = document.getElementById(`${file.id}-error`);
    if (FileSize > 23) {
        if (validation !== null) {
            validation.remove();
        }
        sizeValidation.style.display = "block";
        next.disabled = true;
        input.setAttribute("aria-describedby", `${file.id}-fileSizeError`);
    } else {
        sizeValidation.style.display = "none";
        next.disabled = false;
        input.removeAttribute("aria-describedby");
    }
}