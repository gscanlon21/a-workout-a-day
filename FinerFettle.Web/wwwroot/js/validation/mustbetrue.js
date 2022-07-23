console.log('Addiong Validation');
$.validator.addMethod("mustbetrue", function (value, element, param) {
    return element.checked;
});
$.validator.unobtrusive.adapters.addBool("mustbetrue");