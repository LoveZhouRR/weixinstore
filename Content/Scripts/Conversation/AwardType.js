function AddAwardTypeLine(index) {
    var $typeName = $("<input></input>").attr("type", "text").attr("name", "TypeName" + index);
    var $typeSpan = $("<td></td>").addClass("sc_ctname").append($typeName);
    var $name = $("<input></input>").attr("type", "text").attr("name", "Name" + index);
    var $nameSpan = $("<td></td>").addClass("sc_ctname").append($name);
    var $count = $("<input></input>").attr("type", "text").attr("name", "Count" + index);
    var $countSpan = $("<td></td>").addClass("sc_ctnums").append($count);
    var $probality = $("<input></input>").attr("type", "text").attr("name", "Probality" + index);
    var $probalitySpan = $("<td></td>").addClass("sc_ctodds").append($probality);
    var $reply = $("<input></input>").attr("type", "text").attr("name", "Reply" + index);
    var $replySpan = $("<td></td>").addClass("sc_ctodds").append($reply);
    var $deleteMe = $("<a></a>").html("删除").attr("id", "Delete" + index).attr("index", index);
    var $deleteSpan = $("<td></td>").addClass("sc_ctopea").append($deleteMe);
    var $row = $("<tr></tr>").attr("id", "li" + index).append($typeSpan).append($nameSpan).append($countSpan).append($probalitySpan).append($replySpan).append($deleteSpan);
    $("#AwardList").append($row);
}

function DeleteMe() {
    var $index = parseInt($(this).attr('index'));
    var $count = parseInt($("#AwardTypeCount").val());
    if ($count == 1) {
        $.error("奖品不能为空");
        return;
    }
    if ($(this).attr("action") == "DoAjax") {
        $.ajax({
            url: '/Activity/DeleteAward',
            type: 'post',
            data: { activityid: $("input[name=ID]").val(), id: $("input[name=ID" + $index + "]").val() },
            success: function (responstext) {
                if (responstext.success) {
                    $.msg("删除成功");
                } else {
                    $.error(responstext.msg);
                }
            }
        });
    }
    $("#li" + $index).remove();
    for (var i = $index + 1; i < $count; i++) {
        $("#li" + i).attr("id", "li" + (i - 1));
        $("input[name=TypeName" + i + "]").attr("name", "TypeName" + (i - 1));
        $("input[name=Name" + i + "]").attr("name", "Name" + (i - 1));
        $("input[name=Count" + i + "]").attr("name", "Count" + (i - 1));
        $("input[name=Probality" + i + "]").attr("name", "Probality" + (i - 1));
        $("input[name=Reply" + i + "]").attr("name", "Reply" + (i - 1));
        $("input[name=ID" + i + "]").attr("name", "ID" + (i - 1));
        $("#Delete" + i).attr("id", "Delete" + (i - 1)).attr("index", i - 1);
    }
    $("#AwardTypeCount").attr("value", $count - 1);
}


$(function () {
    $("a[action=DoAjax]").bind("click", DeleteMe);
    $("#AddAwardType").on("click", function () {
        var $count = parseInt($("#AwardTypeCount").val());
        AddAwardTypeLine($count);
        $("#AwardTypeCount").attr("value", $count + 1);
        $("#Delete" + $count).bind("click", DeleteMe);
    });
    
    //$("#AddForm").validate({
    //    debug: true,
    //    rules: {
    //        Name: {
    //            required: true
    //        },
    //        RequireCredits: {
    //            digits: true,
    //            required: function (element) {
    //                return $("#RuleTypeSelect").val() == "2";
    //            }
    //        },
    //        Times: {
    //            digits: true,
    //            required: function (element) {
    //                return $("#RuleTypeSelect").val() == "1";
    //            }
    //        },
    //        DefaultReply: {
    //            required: true
    //        },
    //        DeadLineTime: {
    //            required: true
    //        }
    //    }
    //})
});
