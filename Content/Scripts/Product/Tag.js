function DeleteTag(productid,tagid) {
    $.ajax({
        url: '/Tag/Delete',
        type: 'post',
        data: { productid: productid, tagid: tagid },
        success: function (responsetext){
            if (responsetext.success) {
                $("#Tag" + tagid).remove();
            } else {
                $.error("标签删除失败");
            }
        }
    });
}


$(function () {
    $("#PasteTag").bind("click", function() {
        var $productid = $("input[name=TagName]").attr("productID");
        var $tagName = $("input[name=TagName]").val();
        $.ajax({
            url: '/Tag/Add',
            type: 'post',
            data: { name: $tagName, id: $productid },
            success: function (responsetext) {
                if (responsetext.success) {
                    var $deleteMe = $("<i></i>").html("x").attr("onclick", "javascript:DeleteTag(" + $productid + ", " + responsetext.tagID + ");");
                    var $item = $("<a></a>").attr("id", "Tag" + responsetext.tagID).append($("<em></em>").html($tagName)).append($deleteMe);
                    $("#TagList").append($item);
                } else {
                    $.error("标签添加失败");
                }
            }
        });
    } );
});