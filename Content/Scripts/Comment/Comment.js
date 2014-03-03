function ShowOrNot(id, isshow) {
    $.ajax({
        url: '/Comment/ShowOrNot',
        type: 'post',
        data: { id: id, isshow: isshow },
        success:function(responsetext) {
            if (responsetext.success != null && responsetext.success) {
                var $text = isshow ? "前台显示" : "前台隐藏";
                var $opText = isshow ? "隐藏" : "显示";
                $("#Show" + id).html($text);
                $("#ShowOrNot" + id).attr("onclick", "javascript:ShowOrNot("+id+","+!isshow+");").html($opText);
            } else {
                $.error(responsetext.msg);
            }
        }
    });
}