function changestate(ids, state, originstate, isCOD) {
    /// <summary>
    /// 待付款
    /// </summary>
    var ToBePaid = 2;
    /// <summary>
    /// 待发货
    /// </summary>
    var ToBeShipped = 3;
    /// <summary>
    /// 待退款
    /// </summary>
    var ToBeReturn = 4;
    /// <summary>
    /// 已完成
    /// </summary>
    var Finished = 5;
    /// <summary>
    /// 已作废
    /// </summary>
    var Canceled = 6;
    /// <summary>
    /// 待收货
    /// </summary>
    var ToBeAccept = 7;


    if (ids == null || ids.length == 0) return;
    var sure = confirm("是否确认操作");
    if (!sure) {
        return;
    }
    $.ajax({
        url: '/SaleOrder/ChangeState',
        type: 'post',
        dataType: "json",
        data: { ids: ids, state: state, originstate: originstate, isCOD: isCOD },
        success: function (responseText) {
            if (!responseText.success) {
                alert(responseText.message);
                return;
            }

            PartlyQuery();
            //var resultIDs = responseText.IDs;
            //var resultState = responseText.StateView;
            //for (var i = 0; i < resultIDs.length; i++) {
            //    $("#status" + resultIDs[i]).html(resultState);
            //    $("#state" + resultIDs[i]).attr("value", state);

            //    var opt = "";
            //    if (state == ToBePaid) {
            //        opt = "<a href='javascript:changestate(" + resultIDs[i] + "," + Canceled + "," + state + ",'" + isCOD + "');' class='cur'>作废</a>";
            //    }
            //    else if (state == ToBeAccept) {
            //        opt = "<a href='javascript:changestate(" + resultIDs[i] + "," + Canceled + "," + state + ",'" + isCOD + "');' class='cur'>作废</a>";
            //    }
            //    else if (state == ToBeShipped) {
            //        opt = "<a href='javascript:changestate(" + resultIDs[i] + "," + ToBeAccept + "," + state + ",'" + isCOD + "');' class='cur'>发货</a>";
            //        opt += "<a href='javascript:changestate(" + resultIDs[i] + "," + Canceled + "," + state + ",'" + isCOD + "');' class='cur'>作废</a>";
            //    }

            //    $('#orlt_operCell').html(opt);
            //}
        }
    });
}

//排序事件
var SortEventHandler;

SortEventHandler = function () {
    var _OrderDirection = $("#OrderDirection").val();
    var _OrderField = $("#OrderField").val();

    var fields = $('[action=Sort]');

    //遍历集合根据需要变更箭头方向
    $.each(fields, function () {
        if (_OrderDirection == "Asc") {
            if (_OrderField == $(this).attr('orderField')) {
                $(this).css("background-position", "-122px 0");
                return false;//跳出循环
            }
        } else {
            return false;//跳出循环
        }
    })

    //触发点击排序
    fields.bind('click', function (event) {
        var field = $(event.target);
        $("#OrderField").val(field.attr('orderField'));
        if (_OrderDirection == "Asc") {
            $("input[name=OrderDirection]").attr("value", "Desc");
        } else {
            $("input[name=OrderDirection]").attr("value", "Asc");
        }

        $("input[name=Skip]").attr("value", 0);
        PartlyQuery();
    });
}

function QuerySuccess() {
    SortEventHandler();
}

$(document).ready(function () {
    SortEventHandler();
});

function VerifyData() {
    var _ContactMan = $("#Name").val();
    var _ContactMobile = $("#Mobile").val();
    var _ContactAddress = $("#Address").val();

    if (_ContactMan == "") {
        alert("收货人姓名不能为空");
        return false;
    }
    if (_ContactMobile == "") {
        alert("收货人联系电话不能为空");
        return false;
    }
    else {
        if (!IsLegalContactNumber(_ContactMobile)) {
            alert("联系电话格式不正确，请查证后重试");
            return false;
        }
    }
    if (_ContactAddress == "") {
        alert("收货人详细地址不能为空");
        return false;
    }

    return true;
}

//判断号码是否为合法有效的联系号码
function IsLegalContactNumber(phoneStr) {
    var isMobile = /^(?:13\d|14\d|15\d|18\d)\d{5}(\d{3}|\*{3})$/;
    var isPhone = /^((0\d{2,3})-)?(\d{7,8})(-(\d{3,}))?$/;

    if (!isMobile.test(phoneStr) && !isPhone.test(phoneStr))
        return false;

    return true;
}

function SubmitData() {
    if (VerifyData())
        document.forms[0].submit();
}

function FilterSaleOrderStatus()
{
    var _res = $("input[name=isShowFinished]").val();
    var res = "false";
    if (_res == "false")
        res = "true";
    $("input[name=isShowFinished]").val(res)

    PartlyQuery();
}