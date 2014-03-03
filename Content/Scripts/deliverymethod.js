function dm_init(isEdit) {
    var edit = isEdit;
    $('form').validate({
        rules: {
            Name: {
                required: true,
                validString: true
            },
            DefaultFirstPrice: {
                number: true,
                required: true,
                min: 0
            },
            DefaultAddPrice: {
                number: true,
                required: true,
                min: 0
            },
            AssignFirstPrice: {
                number: true,
                min: 0
            },
            FreeBase: {
                number:true,
                min:0,
                max: 999999,
                required:true
            },
            AssignAddPrice: {
                number: true,
                min: 0
            }
        },
        messages: {
            FreeBase: {
                min: "最小值为0",
                required: '该字段必填(为0表示不会免运费)'
            }
        },
        submitHandler: function () {
            var retObj = $.getValues('form');
            $.ajax({
                data: retObj,
                type: 'POST',
                url: edit?'/deliverymethod/edit':'/deliverymethod/add',
                success: function (msg) {
                    if (msg.success) {
                        window.location.href = '/deliverymethod';
                    } else {
                        $('#errMsg').show(120).html(msg.msg);
                    }
                }
            });
        }
    });

    $('[name=HasAssignArea]').bind('click', function () {
        var element = this;
        if (element.value == "false") {
            $('.dm_set_part').hide();
        } else {
            $('.dm_set_part').show();
        }
    });
    if ($('[name=HasAssignArea]')[1].checked) {
        $('.dm_set_part').show();
    }
}

function doSubmit(parameters) {
    $('#errMsg').hide(100);
    $('form').submit();
}