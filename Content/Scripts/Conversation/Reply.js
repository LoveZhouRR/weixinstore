function SaveTextItem(t) {
    var Content = $('#TextContent').val();
    var KeyWordGroupID = $('.hcur').attr('data-GroupID');
    if ($.trim(Content) == '') {
        alert('回复不能为空');
        return;
    }
    if ($.trim(KeyWordGroupID) == '') {
        alert('请选择要回复的关键词组');
        return;
    }
    $.post("/Conversation/Reply/CreateOrUpdateTextReply", { Content: Content, KeyWordGroupID: KeyWordGroupID }, function (data) {
        if (data.ack == "ok") {
            alert('保存成功');
        } else if (data.ack == 'group not exist') {
            alert('请选择要回复的关键词组');
        } else if (data.ack == "server error") {
            alert('服务器错误');
        }
    });
}

function SaveReplyItem(t, IsFirst) {
    var ID = $(t).attr('data-ItemID');
    var Title = '';
    var Description = '';
    var Url = '';
    var PicID = '';
    var OrderIndex = '';
    if (IsFirst) {
        Title = $('#bigtitle').val();
        Url = $('#bigurl').val();
        Description = $('#bigdesc').val();
        PicID = $('.FirstReplyBtn').attr('data-PicID');
        OrderIndex = 0;
        $('.sar_bl').hide();
    } else {
        var par = $(t).parents('.sar_sl');
        Title = par.find('.sar_sl_txt input').val();
        Url = par.find('.sar_sl_url input').val();
        Description = par.find('.sar_sl_text textarea').val();
        PicID = par.find('.ReplyBtn').attr('data-PicID');
        $('.sar_sl').hide();
    }
    if ($.trim(Title) == '') {
        alert('标题不能为空');
        return;
    }
    if (!checkURL(Url)) {
        alert('请输入正确的链接地址');
        return;
    }
    var KeyWordGroupID = $('.hcur').attr('data-GroupID');

    if ($.trim(KeyWordGroupID) == '') {
        alert('未选择要回复的关键词组');
        return;
    }
    $.post("/Conversation/Reply/CreateOrUpdateNewsReply", { ID: ID, IsFirst: IsFirst, Title: Title, Description: Description, PicID: PicID, Url: Url, KeyWordGroupID: KeyWordGroupID, OrderIndex: OrderIndex }, function (data) {
        if (data.ack == "ok") {
            if (IsFirst) {
                var par = $(t).parents('.sar_big');
                var PicSrc = par.find('#FirstReplyImage').attr('src');
                par.find('.sar_big_txt').html(Title);
                par.find('.sar_big_img').attr('src', PicSrc);
                console.log(PicSrc + '  ' + Title + ' ' + par);
            } else {
                var par = $(t).parents('.sar_smain');
                var PicSrc = par.find('.ReplyImage').attr('src');
                par.find('.sar_sm_img').attr('src', PicSrc);
                par.find('.sar_sm_txt').html(Title);
                console.log(PicSrc + '  ' + Title);
            }
        } else if (data.ack == 'group not exist') {
            alert('请选择回复的关键词组');
        } else if (data.ack == "server error") {
            alert('服务器错误');
        }
    });
}

//被关注自动回复
function SaveFollowedReply(id) {
    var ReplyContent = $('#Content').val();

    var validString = /<[A-Za-z]+/;

    if (ReplyContent == '') {
        alert('被关注自动回复不能为空');
    }
    else if (validString.test(ReplyContent)) {
        alert("被关注自动回复含有非法字符");
    }
    else {
        $.ajax({
            url: '/Conversation/UpdateFollowedReply',
            type: 'post',
            dataType: "json",
            data: { id: id, content: ReplyContent },
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
}

//清空页面文本框
function ClearFollowedReply() {
    $('#Content').val('');
}

$(document).ready(function () {
    $('.cancel').bind("click", ClearFollowedReply);
})