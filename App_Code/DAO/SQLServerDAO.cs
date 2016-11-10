using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
/// <summary>
/// SQLServerDAO 的摘要说明
/// </summary>
public class SQLServerDAO
{
	public SQLServerDAO(){}

    /// <summary>
    /// 获取配置文件web.config的数据库连接字符串
    /// </summary>
    private static string ConnectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

    /// <summary>
    /// 存储过程可向执行调用的过程或应用程序返回一个整数值。
    /// </summary>
    /// <param name="cmd">数据库执行命令</param>
    /// <returns></returns>
    public static Int32? GetReturnValue(SqlCommand cmd)
    {

        if (CommandType.StoredProcedure == cmd.CommandType)
            return (Int32)cmd.Parameters["ReturnValue"].Value;
        else return null;
    }

    /// <summary>
    /// 获取存储过程的Output参数值
    /// </summary>
    /// <param name="cmd">数据库执行命令</param>
    /// <returns></returns>
    public static Dictionary<string, object> GetOutParameters(SqlCommand cmd)
    {
        Dictionary<string, object> p = new Dictionary<string, object>();
        foreach (SqlParameter sp in cmd.Parameters)
        {
            p.Add(sp.ParameterName, sp.Value);
        }
        if (0 == p.Count) { return null; }
        else { return p; }
    }

    /// <summary>
    /// 建立数据库连接，并打开连接。
    /// </summary>
    /// <returns></returns>
    public static SqlConnection getConnection()
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }
    /// <summary>
    /// 创建数据库命令
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameter"></param>
    /// <param name="commandType"></param>
    /// <returns></returns>
    public static SqlCommand createSqlCommand(string sql, Dictionary<string, object> parameter, CommandType commandType)
    {
        using (SqlCommand cmd = new SqlCommand(sql, getConnection()))
        {
            cmd.CommandType = commandType;
            cmd.CommandTimeout = 60;

            #region SQL语句的参数处理

            if (CommandType.Text == commandType)
            { //SQL语句的参数处理。

                MatchCollection ms = Regex.Matches(sql, @"@\w+");
                if (ms.Count > 0)
                {
                    foreach (Match m in ms)
                    {
                        string key = m.Value;
                        string key2 = m.Value.Substring(1);//去除 @
                        Object value;
                        if (parameter.ContainsKey(key2) || parameter.ContainsKey(key))
                        {
                            value = parameter.ContainsKey(key2) ? parameter[key2] : parameter[key];
                        }
                        else
                        {
                            value = DBNull.Value;
                        }
                        cmd.Parameters.Add(new SqlParameter(key, value));
                    }

                }
                cmd.CommandText = sql;
            }
            #endregion

            #region 存储过程的参数处理

            else if (CommandType.StoredProcedure == commandType)
            {//存储过程的参数处理。

                List<Syscolumns> cols = CacheHelper.getStoredProcedureParameters(sql);
                if (cols == null)
                {
                    throw new Exception("找不到存储过程 " + sql);
                }

                // 绑定input参数，并赋值
                if (parameter != null)
                {
                    foreach (KeyValuePair<string, object> kv in parameter)
                    {
                        Syscolumns col = CacheHelper.getColumn(sql, "P", "@" + kv.Key);
                        if (col != null)
                        {
                            cmd.Parameters.Add(col.name, GetSqlDbType(col.type), col.length).Value = kv.Value;
                            cols.Remove(col);
                        }
                    }

                }

                //处理未传参数的参数
                foreach (Syscolumns col in cols)
                {
                    if (col.isoutparam)
                    {
                        // 绑定output参数，并赋值
                        cmd.Parameters.Add(col.name, GetSqlDbType(col.type), col.length).Direction = ParameterDirection.Output;
                    }
                    else
                    {
                        //处理未传参数的参数
                        cmd.Parameters.Add(col.name, GetSqlDbType(col.type), col.length).Value = DBNull.Value;
                    }
                }

                // 绑定返回值
                cmd.Parameters.Add("ReturnValue", SqlDbType.Variant).Direction = ParameterDirection.ReturnValue;

            }
            #endregion

            return cmd;
        }

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

    #region 访问数据库

    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数</param>
    /// <param name="cmdType">执行类型</param>
    /// <returns></returns>
    public static DataTable ExecuteDataTable(string sql, Dictionary<string, object> parameter, CommandType cmdType)
    {
        DataTable dt = new DataTable();
        try
        {
            SqlDataAdapter sda = new SqlDataAdapter(createSqlCommand(sql, parameter, cmdType));
            sda.Fill(dt);
            sda.Dispose();
            return dt;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        finally
        {
            dt.Dispose();
        }
    }

    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数</param>
    /// <param name="cmdType">命令类型</param>
    /// <returns></returns>
    public static DataSet ExecuteDataSet(string sql, Dictionary<string, object> parameter, CommandType cmdType)
    {
        DataSet ds = new DataSet();
        try
        {
            SqlDataAdapter sda = new SqlDataAdapter(createSqlCommand(sql, parameter, cmdType));
            sda.Fill(ds);
            sda.Dispose();
            return ds;
        }
        catch (Exception ex) { throw new Exception(ex.Message); }
        finally { ds.Dispose(); }
    }

    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="StoredProcedureName">存储过程名称</param>
    /// <param name="parameter">参数</param>
    /// <param name="ReturnData">out参数</param>
    /// <returns></returns>
    public static List<DataTable> ExecuteStoredProcedure(string StoredProcedureName, Dictionary<string, object> parameter, out SqlReturnData ReturnData)
    {

        try
        {
            DataSet ds = new DataSet();
            List<DataTable> dataTables = new List<DataTable>();
            SqlCommand cmd = createSqlCommand(StoredProcedureName, parameter, CommandType.StoredProcedure);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(ds);
            sda.Dispose();
            SqlReturnData srd = new SqlReturnData();
            srd.ReturnValue = GetReturnValue(cmd);
            srd.OutParameters = GetOutParameters(cmd);
            ReturnData = srd;
            cmd.Dispose();

            foreach (DataTable dt in ds.Tables)
            {
                dataTables.Add(dt);
            }
            ds.Dispose();
            return dataTables;
        }
        catch (Exception ex) { throw new Exception(ex.Message); }

    }
    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="StoredProcedureName">存储过程名称</param>
    /// <param name="parameter">参数</param>
    /// <returns></returns>
    public static List<DataTable> ExecuteStoredProcedure(string StoredProcedureName, Dictionary<string, object> parameter)
    {
        DataSet ds = new DataSet();
        try
        {
            List<DataTable> dataTables = new List<DataTable>();
            SqlDataAdapter sda = new SqlDataAdapter(createSqlCommand(StoredProcedureName, parameter, CommandType.StoredProcedure));
            sda.Fill(ds);
            sda.Dispose();

            foreach (DataTable dt in ds.Tables)
            {
                dataTables.Add(dt);
            }
            return dataTables;
        }
        catch (Exception ex) { throw new Exception(ex.Message); }
        finally { ds.Dispose(); }
    }

    /// <summary>
    /// 执行查询，返回查询所返回的结果集中的第一行第一列
    /// </summary>
    /// <param name="sql">存储过程名称</param>
    /// <param name="parameter">参数</param>
    /// <param name="cmdType"></param>
    /// <returns></returns>
    public static Object ExecuteScalar(string sql, Dictionary<string, object> parameter, CommandType cmdType)
    {
        return createSqlCommand(sql, parameter, cmdType).ExecuteScalar();
    }

    /// <summary>
    /// 执行SQL语句，返回受影响的行数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameter">参数</param>
    /// <returns></returns>
    public static int ExecuteNonQuery(string sql, Dictionary<string, object> parameter)
    {
        return createSqlCommand(sql, parameter, CommandType.Text).ExecuteNonQuery();
    }


    #endregion

    #region 数据转换

    /// <summary>  
    /// 把DataTable转成 DictionaryList集合, 存每一行；集合中放的是键值对字典,存每一列 。
    /// </summary> 
    /// <param name="dt">数据表</param> 
    /// <returns></returns> 
    public static List<Dictionary<string, object>> ToDictionaryList(DataTable dt)
    {
        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

        foreach (DataRow dr in dt.Rows)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (DataColumn dc in dt.Columns)
            {
                dic.Add(dc.ColumnName, dr[dc.ColumnName]);
            }
            list.Add(dic);
        }
        return list;
    }

    /// <summary>
    /// 把DataTable转成数组，首行数据为表的标题行
    /// </summary>
    /// <param name="dt">数据表</param>
    /// <returns></returns>
    public static ArrayList ToArrayList(DataTable dt)
    {
        ArrayList als = new ArrayList();
        ArrayList h = new ArrayList();
        //表标题行
        foreach (DataColumn dc in dt.Columns)
        {
            h.Add(dc.ColumnName);
        }
        als.Add(h);
        //表数据

        foreach (DataRow dr in dt.Rows)
        {
            ArrayList al = new ArrayList();
            foreach (DataColumn dc in dt.Columns)
            {
                al.Add(dr[dc.ColumnName]);
            }
            als.Add(al);
        }
        return als;
    }

    /// <summary>
    /// DataTable转换为Dictionary
    /// </summary>
    /// <param name="dt">DataTable数据</param>
    /// <returns></returns>
    public static Dictionary<string, Dictionary<string, object>> ToDictionary(DataTable dt)
    {
        Dictionary<string, Dictionary<string, object>> dic = new Dictionary<string, Dictionary<string, object>>();
        for (Int32 i = 0; i < dt.Rows.Count; i++)
        {
            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            foreach (DataColumn dc in dt.Columns)
            {
                dic2.Add(dc.ColumnName, dt.Rows[i][dc.ColumnName]);
            }
            dic.Add(i.ToString(), dic2);
        }
        return dic;
    }

    /// <summary> 
    /// 数据集转键值对数组字典 
    /// </summary> 
    /// <param name="dataSet">数据集</param> 
    /// <returns>键值对数组字典</returns> 
    public static Dictionary<string, List<Dictionary<string, object>>> ToDictionary(DataSet ds)
    {
        Dictionary<string, List<Dictionary<string, object>>> result = new Dictionary<string, List<Dictionary<string, object>>>();
        foreach (DataTable dt in ds.Tables)
        {
            result.Add(dt.TableName, ToDictionaryList(dt));
        }
        return result;
    }


    #endregion













}
#region 存储过程的返回值和Output参数信息类
/// <summary>
/// 存储过程的返回值和Output参数信息类
/// </summary>
public class SqlReturnData
{
    public SqlReturnData() { }
    /// <summary>
    /// 设置或获取存储过程可向执行调用的过程或应用程序返回的一个整数值。
    /// </summary>
    public Int32? ReturnValue { set { ReturnValue = value; } get { return ReturnValue; } }
    /// <summary>
    /// 设置或获取存储过程的Output参数值 
    /// </summary>
    public Dictionary<string, object> OutParameters { set { OutParameters = value; } get { return OutParameters; } }
}
#endregion

public class SQLServerStoredProcedureParameter {
    public string name;
    public string type;
    public int? length;
    public bool isoutparam;
    public bool isnullable;
}





