function ValidateSize(file) {
    var FileSize = file.files[0].size / 1024 / 1024; // in MB
    var sizeValidation = document.getElementById("fileSizeError");
    var next = document.getElementById("submit");
    var input = document.getElementById("fileUpload");
    var validation = document.getElementById("fileUpload-error");
    if (FileSize > 23) {
        console.log(validation);
        if (validation !== null) {
            validation.remove();
        }
        sizeValidation.style.display = "block";
        next.disabled = true;
        input.setAttribute("aria-describedby", "fileSizeError");
    } else {
        sizeValidation.style.display = "none";
        next.disabled = false;
        input.removeAttribute("aria-describedby");
    }
}