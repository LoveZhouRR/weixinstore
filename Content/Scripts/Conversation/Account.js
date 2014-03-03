var loadEvent = function (Actions, el) {
    if (Actions.events) {
        for (var a in Actions.events) {
            (function (p) {
                var e = p.split(' ');
                var fun = Actions.events[p];
                el.delegate(e[0], e[1], function (e) {
                    (function (t, ev) {
                        Actions[fun](t, (ev || window.event))
                    })(this, e)
                })
            })(a);
        }
    }
}
$(document).ready(function () {
    var AccountAction = function () { }
    AccountAction.prototype = {
        events: {
            '#BindBtn click': 'BindAccount',
            '#AccountType change': 'TypeSelect',
            'a:contains("复制url") click': 'CopyURL',
            'a:contains("复制token") click': 'CopyToken',
        },
        BindAccount: function () {
            var Name = $('#AccountName').val();
            var OriginalID = $('#OriginalID').val();
            if ($.trim(Name) == '') {
                alert('公众号名称不能为空');
                return;
            }
            if ($.trim(OriginalID) == '') {
                alert('公众号原始ID不能为空');
                return;
            }
            $.post('/Conversation/Account/BindAccount', { Name: Name, OriginalID: OriginalID }, function (data) {
                if (data.status == 200) {
                    $('#AccountUrl').val(data.Account.Url);
                    $('#AccountToken').val(data.Account.Token);
                    $('.ab_mb').show();
                    $('.ab_mc').show();
                    $('.ab_as').hide();
                }else if (data.status == -1) {
                    alert(data.msg);
                }
            });

        },
        TypeSelect: function () {
            if ($('#AccountType').val() == '订阅号') {
                $('#AppInfo').hide();
                $('p:contains("自定义菜单编辑")').hide();
            } else {
                $('#AppInfo').show();
                $('p:contains("自定义菜单编辑")').show();
            }
        },
        CopyURL: function () {
            var msg = document.getElementById("AccountUrl");
            document.getElementById("AccountUrl").select();
            //window.clipboardData.setData("Text", "asdfasdfasdfasdf");
        },
        CopyToken: function () {
            document.getElementById("AccountToken").select();
            
        }

    }
    loadEvent(new AccountAction(), $('.accounts_bind'));
});