var totalPages = 0;
var pageSelectorPrefix = ".page";

function InitSlider(pages, sliderSelector, labelText)
{
    totalPages = pages;

    if (pages <= 1) return;
   
    var sliderLabel = $('<div class="pager-label"><h3>' + labelText + '</h3></div>');
    var sliderContent = $('<div class="pager"></div>');
    $(sliderSelector).append(sliderLabel);
    $(sliderSelector).append(sliderContent);

    $(sliderContent).rangeSlider({ bounds: { min: 1, max: totalPages } });
    $(sliderContent).bind("valuesChanged", SliderChanged);
    $(sliderContent).rangeSlider("values", 1, 1);
}

function SliderChanged(e, data)
{
    for(i = 1; i <= totalPages; i++)
    { 
        if (i >= Math.round(data.values.min) && i <= Math.round(data.values.max))
            $(pageSelectorPrefix + i).show();
        else
            $(pageSelectorPrefix + i).hide();
    }
}