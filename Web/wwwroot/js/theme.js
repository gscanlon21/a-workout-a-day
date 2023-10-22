// Dark theme for bootstrap.
if (document.querySelector("html").getAttribute("data-bs-theme") === 'auto') {
    const updateDarkTheme = () => {
        document.querySelector("html").setAttribute("data-bs-theme",
            window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light")
    };
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', updateDarkTheme);
    updateDarkTheme();
}