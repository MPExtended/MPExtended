$(document).ready(function () {
    $("#group").change(function () {
        this.form.submit();
    });

    $("input[name='date']").datepicker();
    $("input[name='date']").change(function () {
        this.form.submit();
    });
});