using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data;
using System.Data.SqlClient;
using System.Text;

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
    /// 数据表转JSON
    /// </summary>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    public static string DataTableToJson(DataTable dataTable)
    {
        StringBuilder table = new StringBuilder();
        foreach (DataRow dr in dataTable.Rows)
        {
            StringBuilder row = new StringBuilder();
            foreach (DataColumn dc in dataTable.Columns)
            {
                if (row.Length > 0)
                {
                    row.Append(",");
                }
                row.AppendFormat("\"{0}\":{1}", dc.ColumnName, GetJsonValue(dr[dc.ColumnName]));
                
            }
            if (table.Length > 0)
            {
                table.Append(",");
            }
            table.Append("{"+row.ToString()+"}");
        }
        return "[" + table.ToString() + "]";
    }
    /// <summary>
    /// 数据表集转JSON
    /// </summary>
    /// <param name="dataSet"></param>
    /// <returns></returns>
    public static string DataSetToJson(DataSet dataSet)
    {
        StringBuilder sb = new StringBuilder();
        foreach (DataTable dt in dataSet.Tables)
        {
            if (sb.Length > 0)
            {
                sb.Append(",");
            }
            sb.Append(DataTableToJson(dt));
        }
        return "[" + sb.ToString() + "]";
    }
    
    public static string SqlDataReaderToJson(SqlDataReader reader){
        System.Text.StringBuilder r = new System.Text.StringBuilder();
        while (reader.Read())
        {
            System.Text.StringBuilder row = new System.Text.StringBuilder();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (row.Length > 0)
                {
                    row.Append(",");
                }
                row.Append("\"" + reader.GetName(i) + "\":");
                row.Append(GetJsonValue(reader[i]));
            }

            if (r.Length > 0)
            {
                r.Append(",");
            }
            r.Append("{" + row.ToString() + "}");

        }
        return "[" + r.ToString() + "]";

    }
    /// <summary>
    /// 获取JSON值
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetJsonValue(object obj) {
        if (obj == null||obj is DBNull) {
            return "null";
        }
        else if (obj is bool)
        {
            return Convert.ToBoolean(obj) == true ? "true" : "false";
        }
        else if (
                obj is sbyte || obj is short || obj is int || obj is long
                || obj is byte || obj is ushort || obj is uint || obj is ulong
                || obj is float || obj is double || obj is decimal)
        {
            return obj.ToString();
        }
        else if (obj is char || obj is string || obj is StringBuilder)
        {
            return "\"" + obj.ToString() + "\"";
        }
        else if (obj is DateTime)
        {
            return "\"" + Convert.ToDateTime(obj).ToString("yyyy-MM-dd") + "\"";
        }
        else if (obj is DataTable)
        {
            return DataTableToJson((DataTable)obj);
        }
        else if (obj is DataSet)
        {
            return DataSetToJson((DataSet)obj);
        }
        else if (obj is SqlDataReader) {
            return SqlDataReaderToJson((SqlDataReader)obj);
        }
        else
        {
            return ToJSON(obj);
        }


    }

    /// <summary>
    /// 输出JSON值
    /// </summary>
    /// <param name="data"></param>
    public static void Write(object data)
    {
        string json = string.Empty;
        if (data is DataTable || data is DataSet || data is SqlDataReader)
        {
            json = GetJsonValue(data);
        }
        else {
            json = ToJSON(data);
        }
        HttpContext.Current.Response.Write(json);
    }
    /// <summary>
    /// 输出Ajax HandleJSON值
    /// </summary>
    /// <param name="success">成功</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    public static void Write(bool success, string message, object data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{");
        sb.AppendFormat("\"success\":{0}", GetJsonValue(success));
        sb.AppendFormat(",\"message\":{0}", GetJsonValue(message));
        sb.AppendFormat(",\"data\":{0}", GetJsonValue(data));
        sb.Append("}");
        HttpContext.Current.Response.Write(sb.ToString());
    }
    








}