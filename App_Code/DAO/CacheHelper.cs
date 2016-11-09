using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
/// <summary>
///CacheHelper 缓存帮助类
/// </summary>
public static class CacheHelper 
{
    
    /// <summary>
    /// 缓存存储过程参数或数据表列
    /// </summary>
    /// <param name="objectName">存储过程名或数据表名</param>
    /// <param name="xtype">类型。值为 P时，表示存储过程；值为 U 时，表示数据表</param>
    /// <returns></returns>
    private static bool CacheSyscolumns(string objectName,string xtype)
    {
        DataTable dt = new DataTable();
        try {
            string sql = @"SELECT A.name as procname, B.[name], C.[name] AS [type], B.length, B.isoutparam, B.isnullable
                    FROM sysobjects AS A INNER JOIN
                    syscolumns AS B ON A.id = B.id AND A.xtype = '"+xtype+@"' INNER JOIN
                    systypes C ON B.xtype = C.xtype AND C.[name] <> 'sysname'
                    where A.name='"+objectName+"'ORDER BY A.name, B.isoutparam";
            dt = SQLServerDAO.ExecuteDataTable(sql, null, CommandType.Text);
            List<Syscolumns> cols=new List<Syscolumns>();
            for (int n = 0; n < dt.Rows.Count; n++) {
                Syscolumns syscolumns = new Syscolumns();
                syscolumns.isnullable = dt.Rows[n]["isnullable"].ToString() == "1" ? true : false;
                syscolumns.isoutparam = dt.Rows[n]["isoutparam"].ToString() == "1" ? true : false;
                syscolumns.length = int.Parse(dt.Rows[n]["length"].ToString());
                syscolumns.name = dt.Rows[n]["name"].ToString();
                syscolumns.type = dt.Rows[n]["type"].ToString();
                cols.Add(syscolumns);
            }
            HttpRuntime.Cache.Remove(objectName);
            HttpRuntime.Cache.Insert(objectName, cols);
            if (cols.Count == 0 || HttpRuntime.Cache.Get(objectName) == null){ return false; }
            else { return true; }
        
        }
        catch (Exception ex) { throw new Exception(ex.Message); }
        finally { dt.Dispose(); }

    }

    /// <summary>
    /// 获取存储过程参数
    /// </summary>
    /// <param name="StoredProcedureName">存储过程名</param>
    /// <returns></returns>
    public static List<Syscolumns> getStoredProcedureParameters(string StoredProcedureName)
    {
        List<Syscolumns> cols = (List<Syscolumns>)HttpRuntime.Cache.Get(StoredProcedureName);
        if (cols==null||cols.Count == 0)
        {
            if (CacheSyscolumns(StoredProcedureName, "P"))
            {
                cols = (List<Syscolumns>)HttpRuntime.Cache.Get(StoredProcedureName);
            }
            else { return null; }
        }
        return cols;
    }

    /// <summary>
    /// 获取数据表列
    /// </summary>
    /// <param name="tableName">数据表名</param>
    /// <returns></returns>
    public static List<Syscolumns> getTableColumns(string tableName)
    {
        List<Syscolumns> cols = (List<Syscolumns>)HttpRuntime.Cache.Get(tableName);
        if (cols == null || cols.Count == 0)
        {
            if (CacheSyscolumns(tableName, "U"))
            {
                cols = (List<Syscolumns>)HttpRuntime.Cache.Get(tableName);
            }
            else { return null; }
        }
        return cols;
    }

    /// <summary>
    /// 获取数据表的某一列或存储过程的某一参数
    /// </summary>
    /// <param name="objectName">数据表名或存储过程名</param>
    /// <param name="xtype">类型。值为 P时，表示存储过程；值为 U 时，表示数据表</param>
    /// <param name="columnName">数据表列名或存储过程参数名</param>
    /// <returns></returns>
    public static Syscolumns getColumn(string objectName, string xtype, string columnName)
    {

        List<Syscolumns> cols = (List<Syscolumns>)HttpRuntime.Cache.Get(objectName);
        if (cols == null || cols.Count == 0)
        {
            cols = CacheSyscolumns(objectName, xtype) == true ? (List<Syscolumns>)HttpRuntime.Cache.Get(objectName) : null;
        }

        for (int n = 0; n < cols.Count; n++) {
            if (cols[n].name == columnName) { return cols[n]; }
        }
        cols = CacheSyscolumns(objectName, xtype) == true ? (List<Syscolumns>)HttpRuntime.Cache.Get(objectName) : null;
        for (int n = 0; n < cols.Count; n++)
        {
            if (cols[n].name == columnName) { return cols[n]; }
        }
        
        return null; 
    }

    /// <summary>
    /// 判断数据表的某一列或存储过程的某一参数是否存在。存在，则返回true。
    /// </summary>
    /// <param name="syscolumns">数据表列或存储过程参数集合</param>
    /// <param name="columnName">数据表列名或存储过程参数名</param>
    /// <returns></returns>
    public static Boolean hasColumn(List<Syscolumns> syscolumns, string columnName)
    {
        Boolean b = false;
        foreach (Syscolumns col in syscolumns) {
            if (col.name == columnName) { b = true; }
        }
        return b;
    }


}

#region 数据模型


/// <summary>
/// 数据表列或存储过程参数
/// </summary>
[Serializable]
public class Syscolumns
{
    /// <summary>
    /// 数据表列名或存储过程参数名
    /// </summary>
    public string name;
    /// <summary>
    /// 数据类型
    /// </summary>
    public string type;
    /// <summary>
    /// 数据长度
    /// </summary>
    public int length;
    /// <summary>
    /// 是否是Out输出参数
    /// </summary>
    public bool isoutparam;
    /// <summary>
    /// 是否可以为空
    /// </summary>
    public bool isnullable;

}

/// <summary>
/// 数据表或存储过程
/// </summary>
[Serializable]
public class Sysobjects
{
    /// <summary>
    /// 数据表名或存储过程名
    /// </summary>
    public string name;
    /// <summary>
    /// 类型，值为U，表示表，值为P，表示存储过程
    /// </summary>
    public string type;
    /// <summary>
    /// 数据表列或存储过程参数
    /// </summary>
    public Syscolumns[] syscolumns;

}


#endregion