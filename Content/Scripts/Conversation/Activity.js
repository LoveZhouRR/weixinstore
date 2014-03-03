function AddSuccess(responseText) {
    var $error = $("#AddError").val();
    if ($error != null && $error != "") {
        alert($error);
    } else {
        alert("添加成功");
        $('#myModal').modal('hide');
    }
}
function EditSuccess(responseText) {
    var $error = $("#Error").val();
    if ($error != null && $error != "") {
        alert($error);
        return;
    }
    if (responseText.success != null && !responseText.success) {
        alert(responseText.msg);
        return;
    }
    alert("保存成功");

}

function ChangeState(id, state) {
    var $stop = $("#Stop");
    var $start = $("#Start");
    var $target = state == $start.val() ? $stop : $start;
    var $me = $("#Change" + id);
    if (confirm("是否确认修改")) {
        $.ajax({
            url: '/Activity/ChangeState',
            type: 'post',
            data: { id: id, state: state },
            success: function (responsetext) {
                if (!responsetext.success) {
                    $.error(responsetext.msg);
                } else {
                    $("#State" + id).html($target.attr("des"));
                    $me.attr("onclick", "javascript:ChangeState(" + id + "," + $target.val() + ")").html($target.attr("action"));
                }
            }
        });
    }
}



function VerifyActivity() {
    //活动名称
    var aName = $("input[name=Name]").val();
    if (aName == "") {
        alert("活动名称不能为空");
        return false;
    }
    var validString = /<[A-Za-z]+/;
    if (validString.test(aName)) {
        alert("活动名称含有非法字符");
        return false;
    }

    //获取活动条件类型
    var ruleType = $("#RuleTypeSelect option:selected").val();
    //限制次数
    if (ruleType == "1" || ruleType=="3") {
        var aTimes = $("input[name=Times]").val();
        if (aTimes == "") {
            alert("请设定活动限制次数");
            return false;
        } else if (!isInteger(aTimes)) {
            alert("活动限制次数必须为整数");
            return false;
        }
    } else {//使用积分规则
        var aCredits = $("input[name=RequireCredits]").val();
        if (aCredits == "") {
            alert("请设定活动消耗积分");
            return false;
        } else if (!isInteger(aCredits)) {
            alert("活动消耗积分必须为整数");
            return false;
        }
    }

    //截至日期
    var aTime = $("input[name=DeadLineTime]").val();
    if (aTime == "") {
        alert("截至日期不能为空");
        return false;
    }

    //未中奖提示
    var aReply = $("input[name=DefaultReply]").val();
    if (validString.test(aReply)) {
        alert("未中奖提示含有非法字符");
        return false;
    }

    //奖项数据检查
    var awardTypeCounts = parseInt($("#AwardTypeCount").val());

    for (var i = 0; i < awardTypeCounts; i++) {
        var typeName = $("input[name=TypeName" + i + "]").val();
        var awardName = $("input[name=Name" + i + "]").val();
        var awardCount = $("input[name=Count" + i + "]").val();
        var awardProbality = $("input[name=Probality" + i + "]").val();
        var awardReply = $("input[name=Reply" + i + "]").val();

        if (typeName == "") {
            alert("奖项:" + typeName + "奖项名称不能为空");
            return false;
        } else if (validString.test(typeName)) {
            alert("奖项:" + typeName + "奖项名称含有非法字符");
            return false;
        } else if (awardName == "") {
            alert("奖项" + typeName + "奖品名称不能为空");
            return false;
        } else if (validString.test(awardName)) {
            alert("奖项" + typeName + "奖品名称含有非法字符");
            return false;
        } else if (awardCount == "" || !isInteger(awardCount)) {
            alert("奖项" + typeName + "奖品数量请输入正确的整数");
            return false;
        } else if (awardProbality != "" && isNaN(awardProbality)) {
            alert("奖项" + typeName + "中奖概率请输入正确的小数");
            return false;
        } else if (parseFloat(awardProbality) > 100) {
            alert("奖项" + typeName + "中奖概率超过100%");
            return false;
        }
    }

    return true;
}

function SummitActive() {
    if (VerifyActivity())
        $('#AddForm').submit();
}

function SaveEditActive() {
    if (VerifyActivity())
        $('#EditForm').submit();
}

//Format:yyyy-MM-dd(hh:mm:ss)?
function isDateString(dStr) {
    var dateRule = /^(?:(?:1[6-9]|[2-9][0-9])[0-9]{2}([-/.]?)(?:(?:0?[1-9]|1[0-2])\1(?:0?[1-9]|1[0-9]|2[0-8])|(?:0?[13-9]|1[0-2])\1(?:29|30)|(?:0?[13578]|1[02])\1(?:31))|(?:(?:1[6-9]|[2-9][0-9])(?:0[48]|[2468][048]|[13579][26])|(?:16|[2468][048]|[3579][26])00)([-/.]?)0?2\2(?:29))(((T)|(\s))((([0-1]\d)|([2][0-3]))([:][0-5][0-9]){2})\s*)?$/;

    if (dateRule.test(dStr))
        return true;

    return false;
}

//验证是否为整数
function isInteger(str) {
    var ex = /^-?[1-9]\d{0,9}$/;
    return ex.test(str);
}
