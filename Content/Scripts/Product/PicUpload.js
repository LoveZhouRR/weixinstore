
$(document).ready(function () {
    var picCount=parseInt($("#picCount").val());
    var options = {
        success: showResponse
    };
    $("#picFile").change(function () {
        $("#picPreView").attr("src","/Content/images/pic/ing.gif");
        if ($("#picFile").val() != '') $("#filePostForm").ajaxSubmit(options);
    });
    $('#pics').change(SelectChanged);
    $('#setFirst').click(SetFirst);
    $('#picDelete').click(PicDelete);
    $('#picAddDelete').click(PicViewDelete);
    
    function showResponse(responseText, statusText) {
        responseText = $.parseJSON(responseText);
        if (responseText.success) {
            picCount++;
            $("#picCount").attr("value", picCount);
            var path = responseText.MallName;
            $("#picPreView").attr("src", path);
            var option = $("<option></option>").attr("value", responseText.Id)
                .attr("path", path).text(responseText.OriginName).attr("index", picCount - 1).attr("selected",true);
            option.appendTo($("#pics"));
            $("#pics").change();
            var hidId = $("<input></input>").attr("name", "picID" + (picCount-1)).attr("value",responseText.Id);
            var hidName = $("<input></input>").attr("name", "picName" + (picCount-1)).attr("value",responseText.OriginName);
            var hidPath = $("<input></input>").attr("name", "picPath" + (picCount-1)).attr("value",responseText.Path);
            var hidIsFirst = $("<input></input>").attr("name", "picIsFirst" + (picCount-1)).attr("value", responseText.IsFirst).attr("classname","isfirst");
            var tr = $("<tr id='tr"+(picCount-1)+"'></tr>").append($("<td></td>").append(hidId).append(hidName).append(hidPath).append(hidIsFirst));
            $("#hidPic").append(tr);
        } else {
            alert(responseText.message);
            $("#picPreView").attr("src", "/Content/images/pic/error.jpg");
        }
    }
    
    function SelectChanged() {
        var selectedID = $(this).val();
        var selectedPath = $(this).find('option:selected').attr("path");
        $("#setFirst").attr("picID", selectedID);
        $("#picDelete").attr("picID", selectedID); 
        $("#picPreView").attr("src", selectedPath);
    }

    function SetFirst() {
        var productID = $("#setFirst").attr("productID");
        var picID = $("#setFirst").attr("picID");
        var index = parseInt($("#pics option:selected").attr("index"));
        if (picID == 0||picID==null) {
            $("input[classname='isfirst']").attr("value", false);
            $("[name=picIsFirst" + index + "]").attr("value", true);
            alert("设置成功");
            return;
        }
        $.ajax({
            url: '/Product/SetFirst',
            type: 'post',
            dataType: "json",
            data: { productID: productID, firstID: picID },
            success: function (responsetext) {
                if (responsetext.Success) {
                    alert("设置成功");
                }
            }
        });
    }

    function PicDelete() {
        var productID = $("#picDelete").attr("productID");
        var picID = $("#picDelete").attr("picID");
        if (picID == 0) {
            PicViewDelete();
            return;
        } 
        $.ajax({
            url: '/Product/PicDelete',
            type: 'post',
            dataType: "json",
            data: { productID: productID, picID: picID },
            success: function () {
                var index = $("#pics option:selected").attr("index");
                $("#tr" + index).remove();
                $("#pics option[value='" + picID + "']").remove(); 
                $("#pics").change();
                alert("删除成功");
                picCount--;
            }
        });
    }

    function PicViewDelete() {
        var index = parseInt($("#pics option:selected").attr("index"));
        $("#tr" + index).remove();
        for (var i = index + 1; i < picCount + 1; i++) {
            var pre = i - 1;
            $("#tr" + i).attr("id", "tr" + pre);
            $("[name=picID" + i + "]").attr("name", "picID" + pre);
            $("[name=picName" + i + "]").attr("name", "picName" + pre);
            $("[name=picPath" + i + "]").attr("name", "picPath" + pre);
            $("[name=picIsFirst" + i + "]").attr("name", "picIsFirst" + pre);
            $("[index=" + i + "]").attr("index", index).attr("id", "deleteMe" + pre);
            $("#pics").find('option').filter("[index=" + i + "]").attr("index", pre);
        }
        $("#pics option:selected").remove();
        var first = $("#pics option:first");
        if (first != null) {
            first.attr("selected", true);
            $("#pics").change();
        }
        picCount--;
    }


});



