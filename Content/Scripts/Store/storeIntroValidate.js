/*
验证表单信息
*/
$().ready(function () {
    $("#IntroForm").validate({
        debug: true,
        rules: {
            Phone: {
                required: true,
                commonContact: true
            },
            Email: {
                required: true,
                email: true
            },
            Address: {
                required: true
            },
            Description: {
                required: true,
                validString: true
            }
        }
    })
})

//提交数据
//验证通过-----提交
function SaveStoreInfo() {
    if ($("#IntroForm").valid()) {
        submitIntro();
    }
}

function submitIntro() {
    var _Phone = $("#Phone").val();
    var _Email = $("#Email").val();
    var _Address = $("#Address").val();
    var _Description = $("#Description").val();

    $.ajax({
        url: '/Store/AjaxIntro',
        type: 'post',
        dataType: "json",
        data: { Phone: _Phone, Email: _Email, Address: _Address, Description: _Description },
        success: function (responsetext) {
            if (responsetext.success) {
                $.msg("修改成功");
            }
            else {
                $.error(responsetext.message);
                return false;
            }
        }
    });
}