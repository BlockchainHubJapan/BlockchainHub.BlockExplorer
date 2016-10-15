$(function () {
    function mobileVersion() {
        var ret = true;
        if ($("#mobile-version").css("display") == "none")
            ret = false;
        return ret;
    }

    $(".search-quit").click(function () {
        $(".header-search-view").hide();
        $(".header-default-view").show();
    });

    $(".search-show").click(function () {
        if (!mobileVersion()) {
            $(".header-default-view").hide();
            $(".header-search-view").show();
        }
    });

    $(".accordion-custom .panel-heading").click(function () {
        var parent = $(this).parent();
        $(".custom-collapse-active").not(parent).removeClass("custom-collapse-active");
        $(this).parent().toggleClass("custom-collapse-active");
    });


    $(".panel-heading a").click(function () {
        event.stopPropagation();
        return true;
    });

});