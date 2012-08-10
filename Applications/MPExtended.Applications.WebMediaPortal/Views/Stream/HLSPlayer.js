$(document).ready(function () {
    var url = $("#hlsplayer").data("url");
    $.getJSON(url, function (data, textStatus) {
        if (textStatus == "success" && data.Success) {
            var html = "<video width='" + $("#hlsplayer").width() + "' height='" + $("#hlsplayer").height() + "'>" +
                            "<source type='application/vnd.apple.mpegurl' src='" + data.URL + "' />" +
                        "</video>";
            $("#hlsplayer").html(html);
        } else {
            $("#hlsplayer").hide();
            $("#error").show();
        }
    });
});