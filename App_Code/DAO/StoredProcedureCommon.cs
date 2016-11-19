
/*
 * 存储过程 执行结果及参数的处理
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;

/// <summary>
/// 数据库执行返回值。包括存储过程的返回值和Output参数信息。
/// </summary>
public class ReturnData<T>
{
    /// <summary>
    /// 设置或获取存储过程可向执行调用的过程或应用程序返回的一个整数值。
    /// </summary>
    public Int32? ReturnValue;
    /// <summary>
    /// 设置或获取存储过程的Output参数值 
    /// </summary>
    public Dictionary<string, object> OutParameters;
    /// <summary>
    /// 数据（集）。如DataTable、DataSet、SqlDataReader、int 等。
    /// </summary>
    public T Data;
    
}

/// <summary>
/// 存储过程参数
/// </summary>
public class StoredProcedureParameter
{
    /// <summary>
    /// 名称
    /// </summary>
    public string name;
    /// <summary>
    /// 类型
    /// </summary>
    public string type;
    /// <summary>
    /// 大小
    /// </summary>
    public int length;
    /// <summary>
    /// 是否输出参数
    /// </summary>
    public bool isoutparam;
    /// <summary>
    /// 是否可以为空
    /// </summary>
    public bool isnullable;

}

/// <summary>
/// 存储过程参数工厂
/// </summary>
public class ParameterFactory
{
    #region 参数工厂本身的变量和方法
    /// <summary>
    /// 参数池
    /// </summary>
    private static Dictionary<string, Dictionary<string,StoredProcedureParameter>> StoredProcedureParameterPool = new Dictionary<string, Dictionary<string,StoredProcedureParameter>>();
    /// <summary>
    /// 写参数记录到文件
    /// </summary>
    /// <param name="message">消息</param>
    private static void WriteLog(string message)
    {
        string logFile =HttpContext.Current.Server.MapPath("~/")+"StoredProcedureParameter.log";
        FileStream fileStream = null;
        StreamWriter writer = null;
        try
        {
            FileInfo fileInfo = new FileInfo(logFile);
            if (!fileInfo.Exists)
            {
                fileStream = fileInfo.Create();
                writer = new StreamWriter(fileStream);
            }
            else
            {
                fileStream = fileInfo.Open(FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(fileStream);
            }
            writer.WriteLine(DateTime.Now + ": " + message);
        }
        finally
        {
            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
                fileStream.Close();
                fileStream.Dispose();
            }
        }
    }
    /// <summary>
    /// 清空参数池
    /// </summary>
    /// <returns></returns>
    public static bool Clear()
    {
        try
        {
            StoredProcedureParameterPool.Clear();
            return StoredProcedureParameterPool.Count==0;
        }
        catch { return false; }
    }
    /// <summary>
    /// 获取某存储过程的参数
    /// </summary>
    /// <param name="ProcedureName">存储过程名称</param>
    /// <returns></returns>
    public static Dictionary<string, StoredProcedureParameter> Get(string ProcedureName)
    {
        if (StoredProcedureParameterPool.ContainsKey(ProcedureName))
            {
                return StoredProcedureParameterPool[ProcedureName];
            }
        else { return null; }
    }
    /// <summary>
    /// 获取某存储过程的某参数
    /// </summary>
    /// <param name="ProcedureName">存储过程名称</param>
    /// <param name="ParameterName">参数名称</param>
    /// <returns></returns>
    public static StoredProcedureParameter Get(string ProcedureName, string ParameterName)
    {
        if (StoredProcedureParameterPool.ContainsKey(ProcedureName))
        {
            Dictionary<string, StoredProcedureParameter> ParameterDictionary = StoredProcedureParameterPool[ProcedureName];
            if (ParameterDictionary.ContainsKey(ParameterName)) {
                return ParameterDictionary[ParameterName];
            }
            return null; 
        }
        else { return null; }
    }
    /// <summary>
    /// 设置存储过程参数
    /// </summary>
    /// <param name="ProcedureName">存储过程名称</param>
    /// <param name="ParameterList">参数</param>
    /// <returns></returns>
    public static bool Set(string ProcedureName, Dictionary<string, StoredProcedureParameter> ParameterDictionary)
    {
        if (StoredProcedureParameterPool.ContainsKey(ProcedureName))
        {
            StoredProcedureParameterPool[ProcedureName] = ParameterDictionary;
        }
        else
        {
            StoredProcedureParameterPool.Add(ProcedureName, ParameterDictionary);
        }
        return StoredProcedureParameterPool[ProcedureName] != null;
    }
    /// <summary>
    /// 设置存储过程参数
    /// </summary>
    /// <param name="ProcedureName">存储过程名称</param>
    /// <param name="ParameterName">参数名称</param>
    /// <param name="Parameter">参数值</param>
    /// <returns></returns>
    public static bool Set(string ProcedureName, string ParameterName, StoredProcedureParameter Parameter)
    {
        if (HasParameter(ProcedureName, ParameterName)) {
            StoredProcedureParameterPool[ProcedureName][ParameterName] = Parameter;
        }
        else if (HasParameter(ProcedureName))
        {
            StoredProcedureParameterPool[ProcedureName].Add(ParameterName, Parameter);
        }
        else
        { //不存在 存储过程ProcedureName 的参数
            Dictionary<string, StoredProcedureParameter> ParameterDictionary = new Dictionary<string, StoredProcedureParameter>();
            ParameterDictionary.Add(ParameterName,Parameter);
            StoredProcedureParameterPool.Add(ProcedureName, ParameterDictionary);
        }
        return Get(ProcedureName,ParameterName) != null;
    }
    /// <summary>
    /// 是否存在某存储过程的参数
    /// </summary>
    /// <param name="ProcedureName">存储过程名称</param>
    /// <returns></returns>
    public static bool HasParameter(string ProcedureName)
    {
        return StoredProcedureParameterPool.ContainsKey(ProcedureName);
    }
    /// <summary>
    /// 是否存在某存储过程的某参数
    /// </summary>
    /// <param name="ProcedureName">存储过程名称</param>
    /// <param name="ParameterName">参数名称</param>
    /// <returns></returns>
    public static bool HasParameter(string ProcedureName, string ParameterName)
    {
        if (StoredProcedureParameterPool.ContainsKey(ProcedureName)) {
            return StoredProcedureParameterPool[ProcedureName].ContainsKey(ParameterName);
        } 
        return false;
    }

    #endregion

    #region SQLServerParameter

    /// <summary>
    /// 获取存储过程参数
    /// </summary>
    /// <param name="ProcedureName"></param>
    /// <returns></returns>
    private static StoredProcedureParameter GetSQLServerParameter(string ProcedureName, string ParameterName)
    {
        try
        {
            if (Get(ProcedureName, ParameterName) != null)
            {   
                /*
                 * 取不到参数，则刷新参数池。
                 */
                string sql = @"SELECT A.name as procname, B.[name], C.[name] AS [type], B.length, B.isoutparam, B.isnullable
                    FROM sysobjects AS A INNER JOIN
                    syscolumns AS B ON A.id = B.id AND A.xtype = 'P' INNER JOIN
                    systypes C ON B.xtype = C.xtype AND C.[name] <> 'sysname'
                    where A.name='" + ProcedureName + "'ORDER BY A.name, B.isoutparam";

                using (DataTable dt = SQLServerDAO.ExecuteDataTable(sql, null, CommandType.Text).Data)
                {
                    Dictionary<string, StoredProcedureParameter> ParameterDictionary = new Dictionary<string, StoredProcedureParameter>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        StoredProcedureParameter spp = new StoredProcedureParameter();
                        spp.name = dr["name"].ToString();
                        spp.type = dr["type"].ToString();
                        spp.length = Convert.ToInt32(dr["length"]);
                        spp.isoutparam = dr["isoutparam"].ToString() == "1";
                        spp.isnullable = dr["isnullable"].ToString() == "1";
                        ParameterDictionary.Add(spp.name, spp);

                    }
                    if (ParameterDictionary.Count > 0)
                    {
                        Set(ProcedureName, ParameterDictionary);
                        return Get(ProcedureName, ParameterName);
                    }
                    else { return null; }
                }
            }
            else { return Get(ProcedureName, ParameterName); }
        }
        catch { return null; }

    }
    /// <summary>
    /// 返回SqlDbType类型
    /// </summary>
    /// <param name="TypeName">类型名</param>
    /// <returns></returns>
    private static SqlDbType GetSqlDbType(String TypeName)
    {
        switch (TypeName)
        {
            case "image":
                return SqlDbType.Image;
            case "text":
                return SqlDbType.Text;
            case "uniqueidentifier":
                return SqlDbType.UniqueIdentifier;
            case "tinyint":
                return SqlDbType.TinyInt;
            case "smallint":
                return SqlDbType.SmallInt;
            case "int":
                return SqlDbType.Int;
            case "smalldatetime":
                return SqlDbType.SmallDateTime;
            case "real":
                return SqlDbType.Real;
            case "money":
                return SqlDbType.Money;
            case "datetime":
                return SqlDbType.DateTime;
            case "float":
                return SqlDbType.Float;
            case "sql_variant":
                return SqlDbType.Variant;
            case "ntext":
                return SqlDbType.NText;
            case "bit":
                return SqlDbType.Bit;
            case "decimal":
                return SqlDbType.Decimal;
            case "numeric":
                return SqlDbType.Decimal;
            case "smallmoney":
                return SqlDbType.SmallMoney;
            case "bigint":
                return SqlDbType.BigInt;
            case "varbinary":
                return SqlDbType.VarBinary;
            case "varchar":
                return SqlDbType.VarChar;
            case "binary":
                return SqlDbType.Binary;
            case "char":
                return SqlDbType.Char;
            case "timestamp":
                return SqlDbType.Timestamp;
            case "nvarchar":
                return SqlDbType.NVarChar;
            case "nchar":
                return SqlDbType.NChar;
            case "xml":
                return SqlDbType.Xml;
            default:
                return SqlDbType.Variant;
        }
    }
    /// <summary>
    /// 获取存储过程参数
    /// </summary>
    /// <param name="ProcedureName"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static SqlParameter[] GetSQLServerParameters(string ProcedureName, Dictionary<string, object> parameter)
    {
        List<SqlParameter> res = new List<SqlParameter>();
        //记录已经绑定的参数
        Dictionary<string, StoredProcedureParameter> AddedParameterRecord = new Dictionary<string, StoredProcedureParameter>();
        // 绑定input参数
        foreach (KeyValuePair<string, object> kv in parameter)
        {
            string key = (kv.Key.StartsWith("@") ? kv.Key : "@" + kv.Key);//支持参数名不带 @ 。
            StoredProcedureParameter spp2 = GetSQLServerParameter(ProcedureName, key);
            if (spp2 == null) {
                continue; 
            }

            SqlParameter p = new SqlParameter();
            p.ParameterName = spp2.name;
            p.SqlDbType = GetSqlDbType(spp2.type);
            p.Size = spp2.length;
            p.Value = kv.Value;
            p.Direction = ParameterDirection.Input;
            res.Add(p);

            //记录已绑定
            AddedParameterRecord.Add(spp2.name, spp2);

        }

        //绑定output参数及处理未传参数的input参数
        foreach (KeyValuePair<string, StoredProcedureParameter> kvp in Get(ProcedureName))
        {
            SqlParameter p = new SqlParameter();
            StoredProcedureParameter spp = kvp.Value;
            if (spp.isoutparam) { //输出参数
                // 绑定output参数，并赋值
                p.ParameterName = spp.name;
                p.SqlDbType = GetSqlDbType(spp.type);
                p.Size = spp.length;
                p.Direction = ParameterDirection.Output;

            } else if (!AddedParameterRecord.ContainsKey(spp.name))
            {
                // 处理未传参数的input参数
                p.ParameterName = spp.name;
                p.SqlDbType = GetSqlDbType(spp.type);
                p.Size = spp.length;
                p.Direction = ParameterDirection.Input;
                p.Value = DBNull.Value;
                
            }
            res.Add(p);
        }
        
        // 绑定返回值
        SqlParameter ReturnValue = new SqlParameter("ReturnValue", SqlDbType.Variant);
        ReturnValue.Direction = ParameterDirection.ReturnValue;
        res.Add(ReturnValue);

        return res.ToArray();
    }

    #endregion

    #region MySQLParameter

    /// <summary>
    /// 获取存储过程参数
    /// </summary>
    /// <param name="ProcedureName"></param>
    /// <param name="ParameterName"></param>
    /// <returns></returns>
    public static StoredProcedureParameter GetMySqlParameter(string ProcedureName, string ParameterName)
    {
        try
        {
            if (Get(ProcedureName, ParameterName) != null)
            {
                /*
                 * 取不到参数，则刷新参数池。
                 */
                string sql = @"SELECT A.name as procname, B.[name], C.[name] AS [type], B.length, B.isoutparam, B.isnullable
                    FROM sysobjects AS A INNER JOIN
                    syscolumns AS B ON A.id = B.id AND A.xtype = 'P' INNER JOIN
                    systypes C ON B.xtype = C.xtype AND C.[name] <> 'sysname'
                    where A.name='" + ProcedureName + "'ORDER BY A.name, B.isoutparam";

                using (DataTable dt = SQLServerDAO.ExecuteDataTable(sql, null, CommandType.Text).Data)
                {
                    Dictionary<string, StoredProcedureParameter> ParameterDictionary = new Dictionary<string, StoredProcedureParameter>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        StoredProcedureParameter spp = new StoredProcedureParameter();
                        spp.name = dr["name"].ToString();
                        spp.type = dr["type"].ToString();
                        spp.length = Convert.ToInt32(dr["length"]);
                        spp.isoutparam = dr["isoutparam"].ToString() == "1";
                        spp.isnullable = dr["isnullable"].ToString() == "1";
                        ParameterDictionary.Add(spp.name, spp);

                    }
                    if (ParameterDictionary.Count > 0)
                    {
                        Set(ProcedureName, ParameterDictionary);
                        return Get(ProcedureName, ParameterName);
                    }
                    else { return null; }
                }
            }
            else { return Get(ProcedureName, ParameterName); }
        }
        catch { return null; }

    }
    /// <summary>
    /// 获取存储过程参数
    /// </summary>
    /// <param name="ProcedureName"></param>
    /// <param name="ParameterName"></param>
    /// <returns></returns>
    public static MySqlParameter[] GetMySqlParameters(string ProcedureName,Dictionary<string,object> parameter)
    {
        return new MySqlParameter[] { };
    }


    #endregion





}