
function LoadAllImages()
{
    LoadImageAsync($($('.async-image-display').get(0)));
}

function LoadImageAsync(imageContainer)
{
    var img = new Image();

    // Set the new image up
    $(img)
        // Do this before attaching to the load event
        .css('height', imageContainer.css('height'))
        .css('width', imageContainer.css('width'))
        .attr('alt', imageContainer.attr('data-alt'))
        // once the image has loaded, execute this code
        .load(function () {
            $(this).hide();
            // remove the loading class (so no background spinner), 
            imageContainer.removeClass('async-image-display')
            // then insert our image
            imageContainer.append(this);

            $(this).fadeIn();

            if ($('.async-image-display').get(0) != null)
                LoadImageAsync($($('.async-image-display').get(0)));
        })
        // Set the src attribute, which will fire load when complete
        .attr('src', imageContainer.attr('data-src'));
}