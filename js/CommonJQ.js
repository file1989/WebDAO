

/*
 *
 * create by lzp 20150920
 */

/*引入智能提示文档*/
/// <reference path="jquery-1.9.1-vsdoc.js" />
/// <reference path="CommonJQ-vsdoc.js" />



//如果不支持html5的 placeholder，则JS实现
; $(function () {
    if (!$.support.placeholder) {
        $("input[placeholder]").each(function () {
            $(this).css("color", "#CCC").val($(this).attr("placeholder"));
        });
        $("input[placeholder]").blur(function () {
            if ($(this).val() == "") $(this).css("color", "#CCC").val($(this).attr("placeholder"))
        }).focus(function () {
            $(this).css("color", "#000").val("");
        });
    }
});
























