function deleteMe(id) {
    $.ajax({
        type: "post",
        url: "/Resource/DeleteVideo",
        data: { id: id },
        success: function (response) {
            if (response.success) {
                window.location.href = "/Resource/Video";
                $.msg("删除成功");
            } else {
                $.error(response.msg);
            }
        }
    });

}

function Upload () {
    if ($("#videoResource").val() != '') {
        $("#filePostForm").ajaxSubmit({
            success: function (response) {
                if (response.success) {
                    $.msg("上传成功");
                } else {
                    $.error(response.msg);
                }
            }
        });
    }
}
