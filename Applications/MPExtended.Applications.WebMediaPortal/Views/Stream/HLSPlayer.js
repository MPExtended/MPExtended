$(document).ready(function () {
    var url = $("#hlsplayer").data("url");
    $.getJSON(url, function (data, textStatus) {
        if (textStatus == "success" && data.Success) {
            // If we use the standard video tag without controls='controls' and autoplay='autoplay' and a <source> tag, the iPad won't play this.
            var html = "<video src='" + data.URL + "' width='" + $("#hlsplayer").width() + "' height='" + $("#hlsplayer").height() + "' controls='controls' autoplay='autoplay'>" +
                            "Your browser does not support playing this video." +
                        "</video>";
            $("#hlsplayer").html(html);
        } else {
            $("#hlsplayer").hide();
            $("#error").show();
        }
    });
});