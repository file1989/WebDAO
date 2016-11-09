using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// JSON 帮助类
/// </summary>
public class JSON
{
	public JSON(){}
    /// <summary>
    /// 将指定的 JSON 字符串转换为 T 类型的对象。
    /// </summary>
    /// <typeparam name="T">要转换目标对象的类型</typeparam>
    /// <param name="jsonString">JSON 字符串</param>
    /// <returns></returns>
    public static T ToObject<T>(string jsonString)
    {
        try
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Deserialize<T>(jsonString);
        }
        catch (Exception ex) {
            throw new Exception("JSON字符串转换为对象出错：" + ex.Message);
        }
    }
    /// <summary>
    /// 将对象转换为JSON字符串
    /// </summary>
    /// <param name="obj">要序列化的对象。</param>
    /// <returns></returns>
    public static string ToJSON(object obj) {
        try
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Serialize(obj);
        }
        catch (Exception ex)
        {
            throw new Exception("对象转换为JSON字符串出错：" + ex.Message);
        }
    }

    /// <summary>
    /// 输出JSON值
    /// </summary>
    /// <param name="data"></param>
    public static void Write(object data)
    {
        HttpContext.Current.Response.Write(ToJSON(data));
    }
    /// <summary>
    /// 输出Ajax HandleJSON值
    /// </summary>
    /// <param name="success">成功</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    public static void Write(bool success, string message, object data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder("{");
        sb.AppendFormat("\"success\":{0}", (success == null ? "false" : success.ToString().ToLower()));
        if (message == null)
        {
            sb.Append(",\"message\":null");
        }
        else
        {
            sb.AppendFormat(",\"message\":\"{0}\"", message);
        }
        if (data == null) { sb.Append(",\"data\":null"); }
        else if (data is DateTime)
        {
            sb.AppendFormat(",\"data\":\"{0}\"", ((DateTime)data).ToString("yyyy-MM-dd"));
        }
        else
        {
            sb.AppendFormat(",\"data\":{0}", ToJSON(data));
        }
        sb.Append("}");
        HttpContext.Current.Response.Write(sb.ToString());
    }











}