
//将jquery表单生成的数组转换为相应的JSON格式
function toJson(formArray) {
    if (!formArray || !formArray.length) return {};
    var retObj = {};
    for (var index = 0; index < formArray.length; index++) {
        var obj = formArray[index];
        if (!obj.name) continue;
        retObj[obj.name] = obj.value;
    }
    return retObj;
}

function btnSetDisabled(selector, disable) {
    if (disable) {
        $(selector).attr('disabled', 'disabled');
        var attr = $(selector).attr('data-loading-text');
        if (attr) {
            var val = $(selector).text();
            $(selector).attr('data-origin-text', val);
            $(selector).text(attr);

        }
    } else {
        $(selector).removeAttr('disabled');
        attr = $(selector).attr('data-origin-text');
        if (attr) {
            $(selector).text(attr);
        }
    }

}

(function ($) {
    var divId = "__MsgDiv";
    $.msg = function (content, timeout) {
        if (!content) return;
        var msgDiv = $('#__MsgDiv');
        msgDiv.show();
        $('#__MsgDiv div').html(content);
        $('#__MsgDiv').removeClass().addClass('msg_tips success');
        if (!timeout) timeout = 4000;
        setTimeout('$("#__MsgDiv").fadeOut()', timeout);
    };
    $.error = function (content, timeout) {
        if (!content) return;
        $('#__MsgDiv').show();
        $('#__MsgDiv div').html(content);
        $('#__MsgDiv').removeClass().addClass('msg_tips error');
        if (!timeout) timeout = 5000;
        setTimeout('$("#__MsgDiv").fadeOut()', timeout);
    };
    //获取一个表单的数据
    $.getValues = function (selector) {
        var targetForm = $(selector);
        if (!targetForm || !targetForm.length) return {};
        var formArray = targetForm.serializeArray();
        var retObj = {}, index;
        for (index = 0; index < formArray.length; index++) {
            var obj = formArray[index];
            if (!obj.name) continue;
            retObj[obj.name] = obj.value;
        }
        $(selector + ' input[type=checkbox]:not(:checked)').each(function (i, tar) {
            if (tar.hasAttribute('uncheckedValue') && tar.name) {
                retObj[tar.name] = $(tar).attr('uncheckedValue');
            }
        });
        return retObj;
    };
    $.fn.mask = function(msg,maskDivClass){
        this.unmask();
        // 参数
        var op = {
            opacity: 0.8,
            z: 10000,
            bgcolor: '#ccc'
        };
        var original=$(document.body);
        var position={top:0,left:0};
        if(this[0] && this[0]!==window.document){
            original=this;
            position=original.position();
        }
        // 创建一个 Mask 层，追加到对象中
        var maskDiv=$('<div class="maskdivgen">&nbsp;</div>');
        maskDiv.appendTo(original);
        var maskWidth=original.outerWidth();
        if(!maskWidth){
            maskWidth=original.width();
        }
        var maskHeight=original.outerHeight();
        if(!maskHeight){
            maskHeight=original.height();
        }
        maskDiv.css({
            position: 'absolute',
            top: position.top,
            left: position.left,
            'z-index': op.z,
            width: maskWidth,
            height:maskHeight,
            'background-color': op.bgcolor,
            opacity: 0
        });
        if(maskDivClass){
            maskDiv.addClass(maskDivClass);
        }
        if(msg){
            var msgDiv=$('<div style="position:absolute;border:#6593cf 1px solid; padding:2px;background:#ccca"><div style="line-height:24px;border:#a3bad9 1px solid;background:white;padding:2px 10px 2px 10px">'+msg+'</div></div>');
            msgDiv.appendTo(maskDiv);
            var widthspace=(maskDiv.width()-msgDiv.width());
            var heightspace=(maskDiv.height()-msgDiv.height());
            msgDiv.css({
                cursor:'wait',
                top:(heightspace/2-2),
                left:(widthspace/2-2)
            });
        }
        maskDiv.show('fast', function(){
            // 淡入淡出效果
            $(this).fadeTo('normal', op.opacity);
        })
        return maskDiv;
    }
    $.fn.unmask = function(){
        var original=$(document.body);
        if(this[0] && this[0]!==window.document){
            original=$(this[0]);
        }
        original.find("> div.maskdivgen").remove();
    }
})(jQuery)
