for (const elem of Array.from(document.getElementById('main').querySelectorAll('select:not(.allow-demo), a:not(.allow-demo), button:not(.allow-demo), input:not(.allow-demo)'))) {
    elem.setAttribute('disabled', 'disabled');
    elem.style.pointerEvents = 'none';
    if (elem.tagName.toLowerCase() === 'a') {
        elem.style.opacity = '.5';
    }
}
