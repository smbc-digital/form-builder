function disableBtn(buttonId) {
    var disabledButton = document.getElementById(buttonId);
    if (!disabledButton.className.includes("is-loading")) {
        disabledButton.className += " button-loading is-loading";
        return;
    }
    disabledButton.setAttribute("type", "button");
    return;
}
