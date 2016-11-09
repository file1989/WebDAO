


/*
 *纯JS 公共方法
 *lzp create by 2015-07-10
 */

//引入智能提示文档
/// <reference path="CommonJS-vsdoc.js" />


//获得网站的根目录或虚拟目录的根地址
function getRootPath() {/// <summary>获得网站的根目录或虚拟目录的根地址</summary>
    var strFullPath = window.document.location.href;
    var strPath = window.document.location.pathname;
    var pos = strFullPath.indexOf(strPath);
    var prePath = strFullPath.substring(0, pos);
    var postPath = strPath.substring(0, strPath.substr(1).indexOf('/') + 1);
    return (prePath + postPath + "/");
}


//窗口拖拽事件绑定
function dragInit(mover, moveDiv) {
    /// <summary>窗口拖拽事件绑定</summary>
    /// <param name="mover" type="String">要触发移动的元素ID</param>
    /// <param name="moveDiv" type="String">要移动的块ID</param>
    mover = document.getElementById(mover);
    moveDiv = document.getElementById(moveDiv);
    if (mover == null || moveDiv == null) return;

    mover.onmousedown = function (event) {

        mover.style.cursor="move";
        //moveDiv.setAttribute("data-position",moveDiv.style.position);

        //设置鼠标捕获范围
        if (this.setCapture) {
            this.setCapture();
        } else if (window.captureEvents) {
            window.captureEvents(Event.MOUSEMOVE | Event.MOUSEUP);
        }

        var event = event || window.event;
        var posX = posY = 0;
        var disX = event.clientX - moveDiv.offsetLeft;
        var disY = event.clientY - moveDiv.offsetTop;
        //鼠标移动，窗口随之移动        
        document.onmousemove = function (event) {
            var event = event || window.event;
            maxW = document.documentElement.clientWidth - moveDiv.offsetWidth;
            maxH = document.documentElement.clientHeight - moveDiv.offsetHeight;
            posX = event.clientX - disX;
            posY = event.clientY - disY;
            if (posX < 0) {
                posX = 0;
            } else if (posX > maxW) {
                posX = maxW;
            }
            if (posY < 0) {
                posY = 0;
            } else if (posY > maxH) {
                posY = maxH;
            }
            
            moveDiv.style.position="absolute";
            moveDiv.style.left = posX + 'px';
            moveDiv.style.top = posY + 'px';
            
        }
        //鼠标松开，窗口将不再移动
        document.onmouseup = function () {
            //取消鼠标捕获范围设置
            if (moveDiv.releaseCapture) {
                moveDiv.releaseCapture();
            } else if (window.captureEvents) {
                window.captureEvents(Event.MOUSEMOVE | Event.MOUSEUP);
            }
            mover.style.cursor="default";
            //moveDiv.style.position=moveDiv.getAttribute("data-position");moveDiv.removeAttribute("data-position");
            document.onmousemove = null;
            document.onmouseup = null;
        }
    }
}

//获取浏览器类型

function getExplorer() {/// <summary>获取浏览器类型</summary>
    var explorer = window.navigator.userAgent;
    //ie
    if (explorer.indexOf("MSIE") >= 0) {
        return 'ie';
    }
    //firefox 
    else if (explorer.indexOf("Firefox") >= 0) {
        return 'Firefox';
    }
    //Chrome
    else if (explorer.indexOf("Chrome") >= 0) {
        return 'Chrome';
    }
    //Opera
    else if (explorer.indexOf("Opera") >= 0) {
        return 'Opera';
    }
    //Safari
    else if (explorer.indexOf("Safari") >= 0) {
        return 'Safari';
    } else {
        return '';
    }
}

/**       
* 对Date的扩展，将 Date 转化为指定格式的String       
* 月(M)、日(d)、12小时(h)、24小时(H)、分(m)、秒(s)、周(E)、季度(q) 可以用 1-2 个占位符       
* 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字)       
* eg:       
* (new Date()).toFormatString("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423       
* (new Date()).toFormatString("yyyy-MM-dd E HH:mm:ss") ==> 2009-03-10 二 20:09:04       
* (new Date()).toFormatString("yyyy-MM-dd EE hh:mm:ss") ==> 2009-03-10 周二 08:09:04       
* (new Date()).toFormatString("yyyy-MM-dd EEE hh:mm:ss") ==> 2009-03-10 星期二 08:09:04       
* (new Date()).toFormatString("yyyy-M-d h:m:s.S") ==> 2006-7-2 8:9:4.18       
*/
Date.prototype.toFormatString = function (format) {
    var o = {
        "M+": this.getMonth() + 1, //月份           
        "d+": this.getDate(), //日           
        "h+": this.getHours() % 12 == 0 ? 12 : this.getHours() % 12, //小时           
        "H+": this.getHours(), //小时           
        "m+": this.getMinutes(), //分           
        "s+": this.getSeconds(), //秒           
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度           
        "S": this.getMilliseconds() //毫秒           
    };
    var week = {
        "0": "\u65e5",
        "1": "\u4e00",
        "2": "\u4e8c",
        "3": "\u4e09",
        "4": "\u56db",
        "5": "\u4e94",
        "6": "\u516d"
    };
    if (/(y+)/.test(format)) {
        format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    if (/(E+)/.test(format)) {
        format = format.replace(RegExp.$1, ((RegExp.$1.length > 1) ? (RegExp.$1.length > 2 ? "\u661f\u671f" : "\u5468") : "") + week[this.getDay() + ""]);
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(format)) {
            format = format.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        }
    }
    return format;
}
//window.alert((new Date()).toFormatString("yyyy 年 MM 月 dd 日 hh 时 mm 分 ss 秒")); 

//获取来自地址栏（URL）中的一个参数值

function getQueryString(key) {
    var regExp = new RegExp("[\?|\&]" + key + "=([^&]+)"); //  new RegExp("[\?\&]" + key + "=([^\&]+)(&|$)");
    var matchStrings = window.location.search.match(regExp);
    return matchStrings && decodeURIComponent(matchStrings[1]);
}

//获取来自地址栏（URL）中的所有参数值

function getQueryStrings() {
    var matchStrings = window.location.search.match(/[\?\&]\w+=([^&]*)/g);
    var retJson = {}
    for (var i = 0; i < matchStrings.length; i++) {
        retJson[matchStrings[i].match(/\w+/)[0]] = decodeURIComponent(matchStrings[i].split('=')[1]);
    }
    return retJson;
}

/*
 *字符串的format 调用如：
 * "你是{0}, 他是{1}, 我不是{0}，也不是{1}。".format("张三", "王五"));
 *或者 'name={name},sex={sex}'.format({name:'xxx',sex:1});
*/
String.prototype.format = function () {
    var args = arguments;
    //如果第一个参数是对象
    var isObject = typeof args[0] === 'object';
    var re = isObject ? /\{(\w+)\}/g : /\{(\d+)\}/g;
    return this.replace(re, function (m, key) {
        return isObject ? args[0][key] : args[key];
    });
}


//函数节流
function throttle(method, context) {
    clearTimeout(method.tId);
    method.tId = setTimeout(function () {
        method.call(context);
    }, 100);
}


var JsonObject = (function () {
    /// <summary>Description</summary>

    //转换为JSON字符串
    function toJSONString(value, replacer, space) {
        /// <summary>将JSON对象转换为JSON字符串</summary>
        /// <param name="value" type="Json">JSON对象</param>
        /// <param name="replacer" type="Function/Array">一个可选参数。它可以是一个函数【function (key, value) {处理代码 return value;}】或一个字符串数组。</param>
        /// <param name="space" type="String/Number">一个可选参数,指明的缩进的嵌套结构。如果省略,将文本装没有额外的空格。如果它是一个数字,将指定数量的空间在每个缩进的水平。如果它是一个字符串(如“\ t”或''),它包含用于缩进的角色在每个级别。</param>
        /// <returns type="String" />

        // The stringify method takes a value and an optional replacer, and an optional
        // space parameter, and returns a JSON text. The replacer can be a function
        // that can replace values, or an array of strings that will select the keys.
        // A default replacer method can be provided. Use of the space parameter can
        // produce text that is more easily readable.
        meta = {    // table of character substitutions
            '\b': '\\b',
            '\t': '\\t',
            '\n': '\\n',
            '\f': '\\f',
            '\r': '\\r',
            '"': '\\"',
            '\\': '\\\\'
        };
        var rx_one = /^[\],:{}\s]*$/,
    rx_two = /\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g,
    rx_three = /"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g,
    rx_four = /(?:^|:|,)(?:\s*\[)+/g,
    rx_escapable = /[\\\"\u0000-\u001f\u007f-\u009f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
    rx_dangerous = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g;

        function f(n) {
            // Format integers to have at least two digits.
            return n < 10
        ? '0' + n
        : n;
        }

        function this_value() {
            return this.valueOf();
        }

        if (typeof Date.prototype.toJSON !== 'function') {

            Date.prototype.toJSON = function () {

                return isFinite(this.valueOf())
            ? this.getUTCFullYear() + '-' +
                    f(this.getUTCMonth() + 1) + '-' +
                    f(this.getUTCDate()) + 'T' +
                    f(this.getUTCHours()) + ':' +
                    f(this.getUTCMinutes()) + ':' +
                    f(this.getUTCSeconds()) + 'Z'
            : null;
            };

            Boolean.prototype.toJSON = this_value;
            Number.prototype.toJSON = this_value;
            String.prototype.toJSON = this_value;
        }

        var gap,
    indent,
    meta,
    rep;


        function quote(string) {

            // If the string contains no control characters, no quote characters, and no
            // backslash characters, then we can safely slap some quotes around it.
            // Otherwise we must also replace the offending characters with safe escape
            // sequences.

            rx_escapable.lastIndex = 0;
            return rx_escapable.test(string)
        ? '"' + string.replace(rx_escapable, function (a) {
            var c = meta[a];
            return typeof c === 'string'
                ? c
                : '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4);
        }) + '"'
        : '"' + string + '"';
        }


        function str(key, holder) {

            // Produce a string from holder[key].

            var i,          // The loop counter.
        k,          // The member key.
        v,          // The member value.
        length,
        mind = gap,
        partial,
        value = holder[key];

            // If the value has a toJSON method, call it to obtain a replacement value.

            if (value && typeof value === 'object' &&
            typeof value.toJSON === 'function') {
                value = value.toJSON(key);
            }

            // If we were called with a replacer function, then call the replacer to
            // obtain a replacement value.

            if (typeof rep === 'function') {
                value = rep.call(holder, key, value);
            }

            // What happens next depends on the value's type.

            switch (typeof value) {
                case 'string':
                    return quote(value);

                case 'number':

                    // JSON numbers must be finite. Encode non-finite numbers as null.

                    return isFinite(value)
            ? String(value)
            : 'null';

                case 'boolean':
                case 'null':

                    // If the value is a boolean or null, convert it to a string. Note:
                    // typeof null does not produce 'null'. The case is included here in
                    // the remote chance that this gets fixed someday.

                    return String(value);

                    // If the type is 'object', we might be dealing with an object or an array or
                    // null.

                case 'object':

                    // Due to a specification blunder in ECMAScript, typeof null is 'object',
                    // so watch out for that case.

                    if (!value) {
                        return 'null';
                    }

                    // Make an array to hold the partial results of stringifying this object value.

                    gap += indent;
                    partial = [];

                    // Is the value an array?

                    if (Object.prototype.toString.apply(value) === '[object Array]') {

                        // The value is an array. Stringify every element. Use null as a placeholder
                        // for non-JSON values.

                        length = value.length;
                        for (i = 0; i < length; i += 1) {
                            partial[i] = str(i, value) || 'null';
                        }

                        // Join all of the elements together, separated with commas, and wrap them in
                        // brackets.

                        v = partial.length === 0
                ? '[]'
                : gap
                    ? '[\n' + gap + partial.join(',\n' + gap) + '\n' + mind + ']'
                    : '[' + partial.join(',') + ']';
                        gap = mind;
                        return v;
                    }

                    // If the replacer is an array, use it to select the members to be stringified.

                    if (rep && typeof rep === 'object') {
                        length = rep.length;
                        for (i = 0; i < length; i += 1) {
                            if (typeof rep[i] === 'string') {
                                k = rep[i];
                                v = str(k, value);
                                if (v) {
                                    partial.push(quote(k) + (
                            gap
                                ? ': '
                                : ':'
                        ) + v);
                                }
                            }
                        }
                    } else {

                        // Otherwise, iterate through all of the keys in the object.

                        for (k in value) {
                            if (Object.prototype.hasOwnProperty.call(value, k)) {
                                v = str(k, value);
                                if (v) {
                                    partial.push(quote(k) + (
                            gap
                                ? ': '
                                : ':'
                        ) + v);
                                }
                            }
                        }
                    }

                    // Join all of the member texts together, separated with commas,
                    // and wrap them in braces.

                    v = partial.length === 0
            ? '{}'
            : gap
                ? '{\n' + gap + partial.join(',\n' + gap) + '\n' + mind + '}'
                : '{' + partial.join(',') + '}';
                    gap = mind;
                    return v;
            }
        }


        var i;
        gap = '';
        indent = '';

        // If the space parameter is a number, make an indent string containing that
        // many spaces.

        if (typeof space === 'number') {
            for (i = 0; i < space; i += 1) {
                indent += ' ';
            }

            // If the space parameter is a string, it will be used as the indent string.

        } else if (typeof space === 'string') {
            indent = space;
        }

        // If there is a replacer, it must be a function or an array.
        // Otherwise, throw an error.

        rep = replacer;
        if (replacer && typeof replacer !== 'function' &&
            (typeof replacer !== 'object' ||
            typeof replacer.length !== 'number')) {
            throw new Error('toJSONString');
        }

        // Make a fake root object containing our value under the key of ''.
        // Return the result of stringifying the value.

        return str('', { '': value });
    }

    //将JSON字符串解析为JSON对象
    function parseJSON(text, reviver) {
        /// <summary>将JSON字符串解析为JSON对象</summary>
        /// <param name="text" type="String">JSON字符串</param>
        /// <param name="reviver" type="Function">一个可选参数,用于处理键值。它接收每个键和值【function (key, value) {处理代码 return value;}】,它的返回值是用来代替原来的值。</param>
        /// <returns type="Json" />

        var rx_one = /^[\],:{}\s]*$/,
            rx_two = /\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g,
            rx_three = /"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g,
            rx_four = /(?:^|:|,)(?:\s*\[)+/g,
            rx_dangerous = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g;


        // The parse method takes a text and an optional reviver function, and returns
        // a JavaScript value if the text is a valid JSON text.

        var j;

        function walk(holder, key) {

            // The walk method is used to recursively walk the resulting structure so
            // that modifications can be made.

            var k, v, value = holder[key];
            if (value && typeof value === 'object') {
                for (k in value) {
                    if (Object.prototype.hasOwnProperty.call(value, k)) {
                        v = walk(value, k);
                        if (v !== undefined) {
                            value[k] = v;
                        } else {
                            delete value[k];
                        }
                    }
                }
            }
            return reviver.call(holder, key, value);
        }


        // Parsing happens in four stages. In the first stage, we replace certain
        // Unicode characters with escape sequences. JavaScript handles many characters
        // incorrectly, either silently deleting them, or treating them as line endings.

        text = String(text);
        rx_dangerous.lastIndex = 0;
        if (rx_dangerous.test(text)) {
            text = text.replace(rx_dangerous, function (a) {
                return '\\u' +
                        ('0000' + a.charCodeAt(0).toString(16)).slice(-4);
            });
        }

        // In the second stage, we run the text against regular expressions that look
        // for non-JSON patterns. We are especially concerned with '()' and 'new'
        // because they can cause invocation, and '=' because it can cause mutation.
        // But just to be safe, we want to reject all unexpected forms.

        // We split the second stage into 4 regexp operations in order to work around
        // crippling inefficiencies in IE's and Safari's regexp engines. First we
        // replace the JSON backslash pairs with '@' (a non-JSON character). Second, we
        // replace all simple value tokens with ']' characters. Third, we delete all
        // open brackets that follow a colon or comma or that begin the text. Finally,
        // we look to see that the remaining characters are only whitespace or ']' or
        // ',' or ':' or '{' or '}'. If that is so, then the text is safe for eval.

        if (
            rx_one.test(
                text
                    .replace(rx_two, '@')
                    .replace(rx_three, ']')
                    .replace(rx_four, '')
            )
        ) {

            // In the third stage we use the eval function to compile the text into a
            // JavaScript structure. The '{' operator is subject to a syntactic ambiguity
            // in JavaScript: it can begin a block or an object literal. We wrap the text
            // in parens to eliminate the ambiguity.

            j = eval('(' + text + ')');

            // In the optional fourth stage, we recursively walk the new structure, passing
            // each name/value pair to a reviver function for possible transformation.

            return typeof reviver === 'function'
                ? walk({ '': j }, '')
                : j;
        }

        // If the text is not JSON parseable, then a SyntaxError is thrown.

        throw new SyntaxError('parseJSON');
    }






    // 通过返回命名空间对象将公共API导出
    return {
        toJSONString: toJSONString
        , parseJSON: parseJSON

    };
})();














































