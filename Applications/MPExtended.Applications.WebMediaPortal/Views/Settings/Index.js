function attachUpdateChecker(linkElement, url, errorText, noUpdateText, updateText) {
    $(linkElement).click(function () {
        $.getJSON(url, null, function (data) {
            if(data['Error']) {
                $("a#update_checker").replaceWith(errorText);
            } else if (!data['UpdateAvailable']) {
                $("a#update_checker").replaceWith(noUpdateText);
            } else {
                $("a#update_checker").replaceWith(updateText.replace("{0}", data['NewVersion']));
            }
        });
    });
}