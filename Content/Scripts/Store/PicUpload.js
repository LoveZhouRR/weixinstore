
$(document).ready(function () {
    var options = {
        success: showResponse
    };

    $("#bannerPic").change(function () {
        if ($("#bannerPic").val() != '')
            $("#filePostForm").ajaxSubmit(options);
    });

    $("#introPic").change(function () {
        if ($("#introPic").val() != '')
            $("#filePostForm").ajaxSubmit(options);
    });
    //展示操作结果
    function showResponse(responseText, statusText) {
        responseText = $.parseJSON(responseText);
        if (responseText.success) {
            if (responseText.picList != null) {
                $(responseText.picList).each(function () {
                    var bannerInnerHtml = "<div class='pic' id='pic" + this.ID + "'><a id='picAddDelete" + this.ID + "'";
                    bannerInnerHtml += " href='javascript:;' class='icon ap_delete'";
                    bannerInnerHtml += " onclick='PicDelete(" + this.ID + ")'></a>";
                    bannerInnerHtml += "<img src='" + this.Path + "' alt='" + this.Name + "'/></div>";
                    $('.picPanel').append(bannerInnerHtml);
                });
            }

        } else {
            alert(responseText.Message);
        }
    }
});

//删除banner图片
function PicDelete(picId) {
    $.ajax({
        url: '/Store/PicDelete',
        type: 'post',
        dataType: "json",
        data: { picID: picId, picType: 0 },
        success: function (ResponseText) {
            if (ResponseText.Success) {
                var divId = "pic" + picId;
                $('#' + divId).remove();
            }
            else {
                alert(ResponseText.Message);
            }
        }
    });
};

//删除店铺介绍图片
function IntroPicDelete(picId) {
    $.ajax({
        url: '/Store/PicDelete',
        type: 'post',
        dataType: "json",
        data: { picID: picId, picType: 1 },
        success: function (ResponseText) {
            if (ResponseText.Success) {
                var divId = "pic" + picId;
                $('#' + divId).remove();
            }
            else {
                alert(ResponseText.Message);
            }
        }
    })
};

//保存Announcement字段
function SaveAnnouncement() {
    var announcement = $('#Announcement').val();
    var storename = $("#StoreName").val();
    var validString = /<[A-Za-z]+/;
    if (storename == "") {
        alert("店铺名称不能为空");
        return false;
    }
    if (validString.test(storename)) {
        alert("店铺名称含有非法字符");
        return false;
    }
    if (validString.test(announcement)) {
        alert("商铺介绍含有非法字符");
        return false;
    }
    $.ajax({
        url: '/Store/SaveAnnouncement',
        type: 'post',
        dataType: 'json',
        data: { sname: storename, announcement: announcement },
        success: function (ResponseText) {
            if (ResponseText.Success) {
                alert("保存成功");
            }
            else {
                alert(ResponseText.Message);
            }
        }
    });
}



