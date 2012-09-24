function discoverServices(url, imageLink, configureText, mediaText, tvText, noneText) {
    $.getJSON(url, null, function (data) {
        $("div#loader").hide();
        if (data.length == 0) {
            $("div.search").html("<em>" + noneText + "</em>");
            return;
        }

        $("div#loader").hide();
        for (var i = 0; i < data.length; i++) {
            var configureLink = $("<a href='#'>" + configureText + "</a>");
            var resultItem = $("<div class='result'></div>");
            resultItem.append('<img src="' + imageLink + '" /><br />');
            if (data[i].MAS != null)
                resultItem.append(mediaText + ': ' + data[i].MASHost + '<br />');
            configureLink.data("mas", data[i].MAS == null ? "" : data[i].MAS);
            if (data[i].TAS != null)
                resultItem.append(tvText + ': ' + data[i].TASHost + '<br />');
            configureLink.data("tas", data[i].TAS == null ? "" : data[i].TAS);
            resultItem.append(configureLink);
            $("div.search").append(resultItem);
        }

        $("div.search .result a").click(function () {
            $("#servicesForm input[name='MAS']").val($(this).data("mas"));
            $("#servicesForm input[name='TAS']").val($(this).data("tas"));
            $("#servicesForm").submit();
            return false;
        });
    });
}