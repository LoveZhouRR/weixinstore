function HoverIn() {
    $(this).addClass("cur");
}
function HoverOut() {
    $(this).removeClass("cur");
}

function Download(path, type, name) {
    window.location.href = "/Resource/Download?path="+path+"&type="+type+"&name="+name;
}

$(document).ready(function() {
    $("tr").hover(HoverIn, HoverOut);
});
