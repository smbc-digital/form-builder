function ValidateSize(file) {
    var FileSize = file.files[0].size / 1024 / 1024; // in MB
    var validation = document.getElementById("fileSizeError");
    var next = document.getElementById("submit");
    if (FileSize > 23) {
        validation.style.visibility = "visible";
        next.disabled = true;
    } else {
        validation.style.visibility = "hidden";
        next.disabled = false;
    }
}