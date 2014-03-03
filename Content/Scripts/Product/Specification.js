$(function () {
    var count = parseInt($("#RowsCount").val());

    var deleteFun = function () {
        var index = parseInt($(this).attr('index'));
        $('#specificationRow' + index).remove();
        for (var i = index + 1; i < count + 1; i++) {
            var pre = i - 1;
            $("#specificationRow" + i).attr("id", "specificationRow" + pre);
            $("[name=isShow" + i + "]").attr("name", "isShow" + pre);
            $("[name=name" + i + "]").attr("name", "name" + pre);
            $("[name=price" + i + "]").attr("name", "price" + pre);
            $("[name=stock" + i + "]").attr("name", "stock" + pre);
            $("[index=" + i + "]").attr("index", index).attr("id", "deleteMe" + pre);
        }
        count--;
        $('#RowsCount').attr("value", count);
        if (count == 0) {
            $(".ap_addnorms").css("display", "none");
        }
    };

    var checkShow = function () {
        var index = parseInt($(this).attr('index'));
        $("input[name=isShow" + index + "]").attr("value", $("#isShow" + index).is(':checked'));
    };

    for (var i = 1; i < count + 1; i++) {
        $('#isShow' + i).bind("click", checkShow);
    }

    $('#AddSpecification').bind("click", (function () {
        $(".ap_addnorms").css("display", "");
        if (count >= 5) {
            alert("规格不能超过5个");
            return;
        }
        count++;
        var id = "<td style='display: none'><input type='text' name='id" + count + "' value=0 /></td>";
        var op = "<td class='ap_operate'><input index='" + count + "' type='checkbox' id='isShow" + count + "' /></td>";
        var hidden = "<input name='isShow" + count + "'  type='hidden' value='false'>";
        var name = "<td class='ap_norms'><input name='name" + count + "' type='text'  /></td>";
        var price = "<td class='ap_price'><input name='price" + count + "' type='text'  /></td>";
        var stock = "<td class='ap_invent'><input name='stock" + count + "' type='text'  /></td>";
        var deleteOp = "<td class='ap_operate'><a id='deleteMe" + count + "' index='" + count + "' >删除</a></td>";
        var specificationRow = "<tr id='specificationRow" + count + "'>" + id + op + hidden + name + price + stock + deleteOp + "</tr>";
        $('#SpecificationBody').append(specificationRow);
        $('#deleteMe' + count).bind("click", deleteFun);
        $('#isShow' + count).bind("click", checkShow);
        $('#RowsCount').attr("value", count);
    }));
});

//验证待保存的信息
function VerifyData() {
    var _Name = $('#Name').val();
    var _Description = $('#Description').val();
    var validString = /<[A-Za-z]+/;
    if (_Name == "" || _Name.length >= 100) {
        alert("商品名称长度为1~100，请查证后重新提交");
        return false;
    }
    if (_Description.length >= 256) {
        alert("商品描述长度过大");
        return false;
    }
    if (validString.test(_Description)) {
        alert("商品描述含有非法字符");
        return false;
    }

    //检测商品重量是否为数字
    var _Weight = $("#Weight").val();
    if (_Weight != "") {
        if (isNaN(_Weight)) {
            alert("商品重量如果不为空，则必须为数字类型");
            return false;
        }
    }

    //检测规格系列参数是否为数字
    //默认
    var _defaultPrice = $("[name=price0]").val();
    var _defaultStock = $("[name=stock0]").val();
    if (_defaultPrice != "") {
        if (isNaN(_defaultPrice)) {
            alert("默认价格如果不为空，则必须为数字类型");
            return false;
        }
    }
    if (_defaultStock != "") {
        if (!isInteger(_defaultStock)) {
            alert("默认库存如果不为空，则必须为整数");
            return false;
        }
    }
    //其他规格
    var _Count = parseInt($("#RowsCount").val());
    var _SpecificationName = "";
    for (var i = 1; i <= _Count; i++) {
        _SpecificationName = $("[name=name" + i + "]").val();
        _defaultPrice = $("[name=price" + i + "]").val();
        _defaultStock = $("[name=stock" + i + "]").val();
        if (_SpecificationName == "") {
            alert("规格" + i + "名称不能为空");
            return false;
        }
        if (_defaultPrice != "") {
            if (isNaN(_defaultPrice)) {
                alert("规格" + i + "价格如果不为空，则必须为数字类型");
                return false;
            }
        }
        if (_defaultStock != "") {
            if (!isInteger(_defaultStock)) {
                alert("规格" + i + "库存如果不为空，则必须为整数");
                return false;
            }
        }
    }

    return true;
}

//验证是否为整数
function isInteger(str) {
    var ex = /^\d+$/;
    return ex.test(str);
}

//提交保存信息
function SumbitData() {
    if (VerifyData())
        document.forms[1].submit();
}
