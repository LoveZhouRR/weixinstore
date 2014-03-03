
function deleteMe(id) {
    $.ajax({
        type: "post",
        url: "/Resource/DeletePic",
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
    $("#picResource").change(function () {
        if ($("#picResource").val() != '') {
            $("#filePostForm").ajaxSubmit({
                success: function (response) {
                    if (response.success) {
                        var $endcodepath = encodeURIComponent(response.path);
                        var $tr = $("<tr></tr>").attr("id","tr"+response.resourceID).hover(HoverIn,HoverOut);
                        var $pictd = $("<td></td>").addClass("pl_title").append($("<img></img>").attr("src", response.path)).append($("<b></b>").html(response.name));
                        var $sizetd = $("<td></td>").addClass("pl_size").html(response.size + "KB");
                        var $span = $("<span></span>").addClass("ps_opera")
                            .append($("<i></i>").addClass("ps_up").attr("onclick", "javascript:Download('" + $endcodepath + "','audio/mpeg','" + response.name + "');"))
                            .append($("<i></i>").addClass("ps_xg"))
                            .append($("<i></i>").addClass("ps_sc").attr("onclick", "javascript:deleteMe(" + response.resourceID + ");"));
                        var $deletetd = $("<td></td>").addClass("pl_opera").append($span);
                        $("#picTable").prepend($tr.append($pictd).append($sizetd).append($deletetd));
                        $.msg("上传成功");
                    } else {
                        $.error(response.msg);
                    }
                }
            });
        }
    });

})