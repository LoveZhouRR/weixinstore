
$.extend($.validator.messages, {
    required: "该字段必填",
    remote: "Please fix this field.",
    email: "请输入有效的Email帐号，如hello@dbcsoft.com",
    url: "请输入有效的URL",
    date: "请输入有效的日期",
    dateISO: "请输入有效的日期格式 (ISO).",
    number: "请输入有效的数字",
    digits: "请输入有效的数字",
    creditcard: "Please enter a valid credit card number.",
    equalTo: "两次输入不一致",
    accept: "请输入指定的文件",
    maxlength: jQuery.validator.format("不能超过{0}个字符"),
    minlength: jQuery.validator.format("请至少输入{0}个字符"),
    rangelength: jQuery.validator.format("字符长度必须介于{0}和{1}之间"),
    range: jQuery.validator.format("请输入{0}到{1}之间的数值"),
    max: jQuery.validator.format("请输入不大于{0}的值"),
    min: jQuery.validator.format("请输入不小于{0}的值")
});

$.validator.addMethod("mobile", function (value, element) {
    var length = value.length;
    var mobile = /^(13[0-9]|15[012356789]|18[0123456789]|14[57])[0-9]{8}$/i;
    return this.optional(element) || (length == 11 && mobile.test(value));
}, "手机号码格式错误");

$.validator.addMethod("telephone", function (value, element) {
    var telephone = /^((0\d{2,3})-)?(\d{7,8})(-(\d{3,}))?$/;
    return this.optional(element) || telephone.test(value);
}, "固定电话格式错误");

$.validator.addMethod("realdate", function (value, element) {
    var realdate = /^(?:(?:1[6-9]|[2-9][0-9])[0-9]{2}([-/.]?)(?:(?:0?[1-9]|1[0-2])\1(?:0?[1-9]|1[0-9]|2[0-8])|(?:0?[13-9]|1[0-2])\1(?:29|30)|(?:0?[13578]|1[02])\1(?:31))|(?:(?:1[6-9]|[2-9][0-9])(?:0[48]|[2468][048]|[13579][26])|(?:16|[2468][048]|[3579][26])00)([-/.]?)0?2\2(?:29))(((T)|(\s))((([0-1]\d)|([2][0-3]))([:][0-5][0-9]){2})\s*)?$/;
    return this.optional(element) || realdate.test(value);
}, "日期格式错误");

$.validator.addMethod("commonContact", function (value, element) {
    var servicePhone = /^400-?([0-9]){1}(-?[0-9]{5}|[0-9]{1}-?[0-9]{4}|[0-9]{2}-?[0-9]{3}|[0-9]{3}-?[0-9]{2}|[0-9]{4}-?[0-9]{1}|[0-9]{5}-?)([0-9]){1}$/;
    var res1 = servicePhone.test(value);

    var length = value.length;
    var mobile = /^(13[0-9]|15[012356789]|18[0123456789]|14[57])[0-9]{8}$/i;
    var res2 = mobile.test(value) && length == 11;

    var telephone = /^((0\d{2,3})-)?(\d{7,8})(-(\d{3,}))?$/;
    var res3 = telephone.test(value);

    return this.optional(element) || res1 || res2 || res3;
}, "联系方式格式错误");

$.validator.addMethod("validString", function (value, element) {
    var validString = /<[A-Za-z]+/;
    return this.optional(element) || !validString.test(value);
}, "输入含非法字符");
