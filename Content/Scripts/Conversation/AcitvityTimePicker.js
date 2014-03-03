$(function () {
    $("input.datepicker").datetimepicker({
        changeYear: true,
        showSecond: true,
        timeFormat: 'HH:mm:ss',
        stepHour: 1,
        stepMinute: 1,
        stepSecond: 1,
    });
    $("#TypeSelect").on("change", function () {
        $("input[name=Type]").attr("value", $(this).find("option:selected").val());
    });
    $("#RuleTypeSelect").on("change", function () {
        var $ruletype = $(this).find("option:selected").val();
        $("input[name=RuleType]").attr("value", $ruletype);
        if ($ruletype == 1 || $ruletype == 3) {
            $("#Times").css("display", "");
            $("#RequireCredits").css("display", "none");
        } else if ($ruletype == 2) {
            $("#RequireCredits").css("display", "");
            $("#Times").css("display", "none");
        }

    });
});