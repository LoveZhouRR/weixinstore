

//添加右侧编辑界面
function AddNews(index) {
    var $top = 0;
    if (index != 0) {
        $top = 160 + (index - 1) * 80;
    }
    var $newsdiv = $("<div></div>").attr("id", "Temp" + index).css({
        "display": "none",
        "margin-top": $top + "px"
    })
        .append($("<i></i>").addClass("tm_rtriangle"))
        .append($("<p></p>").addClass("tm_rt").html("标题"))
        .append($("<div></div>").addClass("tm_rtitle").append($("<input></input>").attr("type", "text").attr("name", "Title" + index)))
        .append($("<p></p>").addClass("tm_rt").addClass("tm_rtop").html("封面").append($("<span></span>")))
        .append($("<div></div>").addClass("tm_rimg")
            .append($("<span></span>")
                .append($("<input></input>").attr("type", "file").attr("name", "Pic" + index))
                .append($("<i></i>")))
            .append($("<img></img>").attr("id", "PreViewCover" + index))
            .append($("<a></a>").html("删除")))
        .append($("<p></p>").addClass("tm_rt").append($("<input></input>").attr("type", "checkbox")).html(" 封面图片显示在正文中"))
        .append($("<p></p>").addClass("tm_rt").html("正文"))
        .append($("<div></div>").addClass("tm_rb").append($("<script></script>").attr("name", "NewsContent" + index)
            .attr("id", "NewsContent" + index).css({
            "height": '280px',
            "width":'100%',
    })));
    $("#EditContent").append($newsdiv);
    $newsdiv.find("input[name=Pic" + index + "]").change(function() {
        UploadPic(index);
    });
    $newsdiv.find("input[name=Title" + index + "]").change(function() {
        $("#PreViewTitle" + index).html($(this).val());
    });
    var editor = new UE.ui.Editor();
    textarea: 'NewsContent'+index; //与textarea的name值保持一致  
    editor.render('NewsContent'+index);
}

//添加左侧预览
function AddPreview(index) {
    var $preview = $("<dd></dd>").attr("id", "dd" + index).hover(HoverIn, HoverOut)
        .append($("<b><.b>").attr("id", "PreViewTitle"+index).html("标题"))
        .append($("<img></img>").attr("id", "previewPic" + index))
        .append($("<i></i>").addClass("tm_lm_edit icon").attr("onclick", "javascript:ShowEditDiv(" + index + ");"))
        .append($("<i></i>").addClass("tm_lm_delete icon"));
    $("#PreviewList").append($preview);
}


function AddNewsContent() {
    var $countinput = $("input[name=Count]");
    var $count = parseInt($countinput.val());
    $countinput.val($count + 1);
    AddPreview($count);
    AddNews($count);
}

function ShowEditDiv(index) {
    var $countinput = parseInt($("input[name=Count]").val());
    for (var i = 0; i < $countinput; i++) {
        $("#Temp" + i).css("display", "none");
    }
    $("#Temp" + index).css("display", "");

}

function CreateAll() {
    var $count = parseInt($("input[name=Count]").val());
    for (var i = 0; i < $count; i++) {
        if ($("input[name=Pic" + i + "]").val() == '') {
            $.error("第" + i + "条消息的图片为空！");
            return;
        }
    }
    $("#PostForm").attr("action", "/Resource/CreateResult");
    $("#PostForm").submit();
}

function Edit() {
    var $count = parseInt($("input[name=Count]").val());
    var $origincount = parseInt($("#OriginCount").val());
    for (var i = $origincount; i < $count; i++) {
        if ($("input[name=Pic" + i + "]").val() == '') {
            $.error("第" + i + "条消息的图片为空！");
            return;
        }
    }
    $("#PostForm").attr("action", "/Resource/EditNews");
    $("#PostForm").submit();
}


function DeleteMe(id) {
    $.ajax({
        url: "/Resource/DeleteNews",
        type: 'post',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                window.location.href = "/Resource/News";
                $.msg("删除成功");
            } else {
                $.error(response.msg);
            }
        }
    });
}


function UploadPic(index) {
    var $inputfile = $("input[name=Pic" + index + "]");
    if ($inputfile.val() == null || $inputfile.val() == "") return;
    $("#PostForm").attr("action", "/Resource/UploadPic");
    $("#PostForm").ajaxSubmit({
        data: { index: index },
        success: function (response) {
            $("#previewPic" + index).attr("src", response.path);
            $("#PreViewCover" + index).attr("src", response.path).css({
                "width": "100px",
                "height": "50px"
            });
        }
    });
}




