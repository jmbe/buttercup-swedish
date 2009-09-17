$(document).ready(function() {
    $("#switchBlue").click(function() {
        $("body").removeClass("white").removeClass("black").addClass("blue");
    });
    $("#switchBlack").click(function() {
        $("body").removeClass("white").removeClass("blue").addClass("black");
    });
    $("#switchWhite").click(function() {
        $("body").removeClass("blue").removeClass("black").addClass("white");
    });
});