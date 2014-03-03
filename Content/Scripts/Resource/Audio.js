
function deleteMe(id) {
    $.ajax({
        type: "post",
        url: "/Resource/DeleteAudio",
        data: { id: id },
        success: function (response) {
            if (response.success) {
                $("#tr" + id).remove();
                $.msg("删除成功");
            } else {
                $.error(response.msg);
            }
        }
    });
}
$(function () {
    $("#audioResource").change(function () {
        if ($("#audioResource").val() != '') {
            $("#filePostForm").ajaxSubmit({
                success: function (response) {
                    if (response.success) {
                        var $endcodepath = encodeURIComponent(response.path);
                        var $tr = $("<tr></tr>").attr("id", "tr" + response.resourceID).hover(HoverIn, HoverOut);
                        var $audio = $("<audio></audio>").attr("controls", "controls").append($("<source></source>").attr("path", response.path).attr("type", "audio/mpeg")).html(" 您的浏览器不支持html5播放 ,推荐下载Chrome");
                        var $pictd = $("<td></td>").addClass("pl_title").append($("<b></b>").html(response.name))
                            .append($("<div></div>").addClass("pl_sound").append($audio));
                        var $sizetd = $("<td></td>").addClass("pl_size").html(response.size + "KB");
                        var $span = $("<span></span>").addClass("ps_opera")
                            .append($("<i></i>").addClass("ps_up").attr("onclick", "javascript:Download('" + $endcodepath + "','audio/mpeg','" + response.name + "');"))
                            .append($("<i></i>").addClass("ps_xg"))
                            .append($("<i></i>").addClass("ps_sc").attr("onclick", "javascript:deleteMe(" + response.resourceID + ");"));
                        var $deletetd = $("<td></td>").addClass("pl_opera").append($span);
                        $("#audioTable").prepend($tr.append($pictd).append($sizetd).append($deletetd));
                        $.msg("上传成功");
                    } else {
                        $.error(response.msg);
                    }
                }
            });
        }
    });
})