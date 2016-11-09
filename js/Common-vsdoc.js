
/*
*lzp create by 2015-07-10
*/

/*
vs2010的javascript 智能提示文档书写
    所有注释都要写在函数内部，<summary>是函数的描述，<param>是参数的描述,<returns>是返回值的描述,
js文件按 xxx-vsdoc.js保存。然后在vs2010里就可以看到智能提示了。
xxx-vsdoc.js 文件必须与要提示的文件在同一目录。
例如
    codeex.fun3 = function (name) {
        /// <summary>
        /// 输出HelloWorld。
        /// </summary>
        /// <param name="name" type="String">
        ///  1: name - helloworld的对象。
        /// </param>
        ///<returns type="String" />

    }
用法：
    可以通过在js文件开始的地方加入（js文件与xxx-vsdoc.js必须在同一目录）
/// <reference path="xxx-vsdoc.js" />
来开启智能提示
*/

function getRootPath() {
/// <summary>获得网站的根目录或虚拟目录的根地址</summary>
/// <returns type="String" />
}

function dragInit(mover, moveDiv) {
/// <summary>窗口拖拽事件绑定</summary>
/// <param name="mover" type="String">要触发移动的元素ID</param>
/// <param name="moveDiv" type="String">要移动的块ID</param>
}

function getExplorer() {
    /// <summary>获取浏览器类型</summary>
    /// <returns type="String" />
}

Date.prototype.toFormatString = function (format) {
    /// <summary>将 Date 转化为指定格式的String </summary>
    /// <param name="format" type="String">指定的格式:月(M)、日(d)、12小时(h)、24小时(H)、分(m)、秒(s)、周(E)、季度(q) 可以用 1-2 个占位符;年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符。</param>
    /// <returns type="String" />
}
//用法：window.alert((new Date()).toFormatString("yyyy 年 MM 月 dd 日 hh 时 mm 分 ss 秒")); 

function getQueryString(key) {
    /// <summary>获取来自地址栏（URL）中的某个参数值</summary>
    /// <param name="key" type="String">参数名称</param>
    /// <returns type="String" />
}

function getQueryStrings() {
    /// <summary>获取来自地址栏（URL）中的所有参数值</summary>
    /// <returns type="Json" />
}

String.prototype.format = function (arguments) {
    /// <summary>字符串格式化</summary>
    /// <param name="arguments" type="">数据参数。使用方法："你是{0}, 他是{1}".format("张", "王"));或者 'name={name},sex={sex}'.format({name:'xxx',sex:1});</param>
    /// <returns type="String" />
}


var JsonObject = {
    toJSONString: function toJSONString(value, replacer, space) {
        /// <summary>将JSON对象转换为JSON字符串</summary>
        /// <param name="value" type="Json">JSON对象</param>
        /// <param name="replacer" type="Function/Array">一个可选参数。它可以是一个函数【function (key, value) {处理代码 return value;}】或一个字符串数组。</param>
        /// <param name="space" type="String/Number">一个可选参数,指明的缩进的嵌套结构。如果省略,将文本装没有额外的空格。如果它是一个数字,将指定数量的空间在每个缩进的水平。如果它是一个字符串(如“\ t”或''),它包含用于缩进的角色在每个级别。</param>
        /// <returns type="String" />

    }
    , parseJSON: function parseJSON(text, reviver) {
        /// <summary>将JSON字符串解析为JSON对象</summary>
        /// <param name="text" type="String">JSON字符串</param>
        /// <param name="reviver" type="Function">一个可选参数,用于处理键值。它接收每个键和值【function (key, value) {处理代码 return value;}】,它的返回值是用来代替原来的值。</param>
        /// <returns type="Json" />
    }

};










