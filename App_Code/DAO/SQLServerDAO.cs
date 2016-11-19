/*
 * SQLServer 数据库访问对象帮助处理类
 * 
 * 作者：lzp
 * 日期：2016.11.19
 * 版本：v1.0
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
/// <summary>
/// SQLServer 数据库访问对象帮助处理类
/// </summary>
public class SQLServerDAO
{
    /// <summary>
    /// 获取配置文件web.config的数据库连接字符串
    /// </summary>
    private static string ConnectionString = ConfigurationManager.ConnectionStrings["SQLServerDB"].ConnectionString;
    /// <summary>
    /// 建立数据库连接，并打开连接。
    /// </summary>
    /// <returns></returns>
    public static SqlConnection GetConnection()
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }
    /// <summary>
    /// 建立数据库连接，并打开连接。
    /// </summary>
    /// <returns></returns>
    public static SqlConnection GetConnection(string ConnectionString)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }
    /// <summary>
    /// 获取数据库命令
    /// </summary>
    /// <param name="connection">数据库连接对象【已打开】</param>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="commandType">命令类型</param>
    /// <returns></returns>
    public static SqlCommand GetSqlCommand(SqlConnection connection, string sql, object parameter, CommandType commandType)
    {
        try
        {
            if (connection.State == ConnectionState.Broken)
            {
                connection.Close();
                connection.Open();
            }
            else if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.CommandType = commandType;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Clear();
                if (parameter == null)
                {
                    return cmd;
                }
                else if (parameter is Dictionary<string, object>)
                {
                    Dictionary<string, object> _parameter = (Dictionary<string, object>)parameter;

                    #region SQL语句的参数处理

                    if (CommandType.Text == commandType)
                    {
                        /*
                         * SQL语句的参数处理。
                         * 支持参数带不带‘@’都行。
                         */
                        MatchCollection ms = Regex.Matches(sql, @"@\w+");
                        if (ms.Count > 0)
                        {
                            foreach (Match m in ms)
                            {
                                string key = m.Value;
                                string key2 = m.Value.Substring(1);//去除 @
                                Object value;
                                if (_parameter.ContainsKey(key2) || _parameter.ContainsKey(key))
                                {
                                    value = _parameter.ContainsKey(key2) ? _parameter[key2] : _parameter[key];
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
                    {
                        /*
                         * 存储过程的参数处理。
                         */
                        cmd.Parameters.AddRange(ParameterFactory.GetSQLServerParameters(sql, _parameter));

                    }
                    #endregion

                }
                else if (parameter is SqlParameter[])
                {
                    cmd.Parameters.AddRange((SqlParameter[])parameter);
                }
                else
                {
                    throw new Exception("不支持的参数类型。");
                }
                return cmd;
            }
        }
        catch (Exception ex) {
            throw ex;
        }

    }
    /// <summary>
    /// 获取数据库命令
    /// </summary>
    /// <param name="sql">语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="commandType">命令类型</param>
    /// <returns></returns>
    public static SqlCommand GetSqlCommand(string sql, object parameter, CommandType commandType)
    {
        try
        {
            return GetSqlCommand(GetConnection(), sql, parameter, commandType);
        }
        catch(Exception ex) {
            throw ex;
        }
    }
    ///// <summary>
    ///// 读取返回值。包括存储过程返回值和输出参数值
    ///// </summary>
    ///// <typeparam name="T">泛型类型</typeparam>
    ///// <param name="ReturnData">数据库执行返回值</param>
    ///// <param name="cmd">已执行的数据库命令</param>
    ///// <returns></returns>
    //public static ReturnData<T> GetReturnData<T>(ReturnData<T> ReturnData, SqlCommand cmd)
    //{
    //    /*读取存储过程返回值和输出参数值*/
    //    foreach (SqlParameter p in cmd.Parameters)
    //    {
    //        if (p.Direction == ParameterDirection.Output)
    //        {
    //            ReturnData.OutParameters.Add(p.ParameterName, p.Value);
    //        }
    //        else if (p.Direction == ParameterDirection.ReturnValue)
    //        {
    //            ReturnData.ReturnValue = (int?)p.Value;
    //        }
    //    }
    //    return ReturnData;
    //}

    /// <summary>
    /// 读取返回值。包括存储过程返回值和输出参数值
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    /// <param name="Data">泛型类型对象</param>
    /// <param name="cmd">已执行的数据库命令</param>
    /// <returns></returns>
    public static ReturnData<T> GetReturnData<T>(T Data, SqlCommand cmd)
    {
        ReturnData<T> rd = new ReturnData<T>();
        rd.Data = Data;
        /*读取存储过程返回值和输出参数值*/
        foreach (SqlParameter p in cmd.Parameters)
        {
            if (p.Direction == ParameterDirection.Output)
            {
                rd.OutParameters.Add(p.ParameterName, p.Value);
            }
            else if (p.Direction == ParameterDirection.ReturnValue)
            {
                rd.ReturnValue = (int?)p.Value;
            }
        }
        return rd;
    }

    #region 执行 SqlCommand 的ExecuteNonQuery、ExecuteReader、ExecuteScalar、ExecuteXmlReader方法
    
    /// <summary>
    /// 执行SQL语句或存储过程，返回受影响的行数
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">命令类型</param>
    /// <param name="IsUsedTransaction">是否使用事务</param>
    /// <returns></returns>
    public static ReturnData<int> ExecuteNonQuery(string sql, object parameter, CommandType cmdType, bool IsUsedTransaction)
    {
        SqlCommand cmd=GetSqlCommand(sql, parameter, cmdType);
        try
        {
            if (IsUsedTransaction == true)
            {
                cmd.Connection.BeginTransaction();
                try
                {
                    int r = cmd.ExecuteNonQuery();
                    cmd.Transaction.Commit();
                    return GetReturnData<int>(r, cmd);
                }
                catch (Exception ex)
                {
                    cmd.Transaction.Rollback();
                    throw ex;
                }
                
            }
            else
            {
                return GetReturnData<int>(cmd.ExecuteNonQuery(), cmd);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally {
            cmd.Connection.Close();
            cmd.Dispose();
        }
    }
    /// <summary>
    /// 执行SQL语句或存储过程，返回受影响的行数
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">命令类型</param>
    public static ReturnData<int> ExecuteNonQuery(string sql, object parameter, CommandType cmdType)
    {
        return ExecuteNonQuery(sql,parameter,cmdType,false);
    }
    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">命令类型</param>
    /// <param name="IsUsedTransaction">是否使用事务</param>
    /// <returns></returns>
    public static ReturnData<SqlDataReader> ExecuteReader(string sql, object parameter, CommandType cmdType, bool IsUsedTransaction)
    {

        SqlCommand cmd = GetSqlCommand(sql, parameter, cmdType);
        try
        {
            if (true == IsUsedTransaction)
            {
                cmd.Connection.BeginTransaction();
                try
                {
                    SqlDataReader sdr = cmd.ExecuteReader();
                    cmd.Transaction.Commit();
                    return GetReturnData<SqlDataReader>(sdr,cmd);
                }
                catch (Exception ex)
                {
                    cmd.Transaction.Rollback();
                    throw ex;
                }
                
            }
            else
            {
                return GetReturnData<SqlDataReader>(cmd.ExecuteReader(), cmd);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally {
            cmd.Connection.Close();
            cmd.Dispose();
        }
    }
    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">命令类型</param>
    /// <returns></returns>
    public static ReturnData<SqlDataReader> ExecuteReader(string sql, object parameter, CommandType cmdType)
    {
        return ExecuteReader(sql, parameter, cmdType, false);
    }
    /// <summary>
    /// 执行SQL语句或存储过程，返回查询所返回的结果集中的第一行第一列
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">命令类型</param>
    /// <param name="IsUsedTransaction">是否使用事务</param>
    /// <returns></returns>
    public static ReturnData<object> ExecuteScalar(string sql, object parameter, CommandType cmdType, bool IsUsedTransaction)
    {
        SqlCommand cmd = GetSqlCommand(sql, parameter, cmdType);
        try
        {
            if (true == IsUsedTransaction)
            {
                cmd.Connection.BeginTransaction();
                try
                {
                    object r = cmd.ExecuteScalar();
                    cmd.Transaction.Commit();
                    return GetReturnData<object>(r, cmd);
                }
                catch (Exception ex)
                {
                    cmd.Transaction.Rollback();
                    throw ex;
                }
                
            }
            else
            {
                return GetReturnData<object>(cmd.ExecuteScalar(),cmd);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            cmd.Connection.Close();
            cmd.Dispose();
        }
    }
    /// <summary>
    /// 执行SQL语句或存储过程，返回查询所返回的结果集中的第一行第一列
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">命令类型</param>
    /// <returns></returns>
    public static ReturnData<object> ExecuteScalar(string sql, object parameter, CommandType cmdType)
    {
        return ExecuteScalar(sql, parameter, cmdType, false);
    }
    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">命令类型</param>
    /// <param name="IsUsedTransaction">是否使用事务</param>
    /// <returns></returns>
    public static ReturnData<XmlReader> ExecuteXmlReader(string sql, object parameter, CommandType cmdType, bool IsUsedTransaction)
    {
        SqlCommand cmd = GetSqlCommand(sql, parameter, cmdType);
        try
        {
            if (true == IsUsedTransaction)
            {
                cmd.Connection.BeginTransaction();
                try
                {
                    System.Xml.XmlReader xr = cmd.ExecuteXmlReader();
                    cmd.Transaction.Commit();
                    return GetReturnData<XmlReader>(xr, cmd);
                }
                catch (Exception ex)
                {
                    cmd.Transaction.Rollback();
                    throw ex;
                }

            }
            else
            {
                return GetReturnData<XmlReader>(cmd.ExecuteXmlReader(), cmd);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            cmd.Connection.Close();
            cmd.Dispose();
        }
    }
    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">命令类型</param>
    /// <returns></returns>
    public static ReturnData<XmlReader> ExecuteXmlReader(string sql, object parameter, CommandType cmdType)
    {
        return ExecuteXmlReader(sql, parameter, cmdType, false);
    }

    #endregion


    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">执行类型</param>
    /// <param name="IsUsedTransaction">是否使用事务</param>
    /// <returns></returns>
    public static ReturnData<DataTable> ExecuteDataTable(string sql, object parameter, CommandType cmdType, bool IsUsedTransaction)
    {
        SqlCommand cmd = GetSqlCommand(sql, parameter, cmdType);
        SqlDataAdapter sda = new SqlDataAdapter(cmd);
        try
        {
            using (DataTable dt = new DataTable())
            {
                if (true == IsUsedTransaction)
                {
                    SqlTransaction tran = cmd.Connection.BeginTransaction();
                    sda.InsertCommand.Transaction = tran;
                    sda.UpdateCommand.Transaction = tran;
                    sda.DeleteCommand.Transaction = tran;
                    sda.SelectCommand.Transaction = tran;
                    try
                    {
                        sda.Fill(dt);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        tran.Dispose();
                    }
                }
                else
                {
                    sda.Fill(dt);
                }

                return GetReturnData<DataTable>(dt, cmd);
            }

        }
        catch (Exception e)
        {
            throw e;
        }
        finally
        {
            sda.Dispose();
            cmd.Connection.Close();
            cmd.Dispose();
        }
    }
    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">执行类型</param>
    public static ReturnData<DataTable> ExecuteDataTable(string sql, object parameter, CommandType cmdType)
    {
        return ExecuteDataTable(sql, parameter, cmdType, false);
    }
    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">执行类型</param>
    /// <param name="IsUsedTransaction">是否使用事务</param>
    /// <returns></returns>
    public static ReturnData<DataSet> ExecuteDataSet(string sql, object parameter, CommandType cmdType, bool IsUsedTransaction)
    {

        SqlCommand cmd = GetSqlCommand(sql, parameter, cmdType);
        SqlDataAdapter sda = new SqlDataAdapter(cmd);
        try
        {
            using (DataSet ds = new DataSet())
            {
                if (true == IsUsedTransaction)
                {
                    SqlTransaction tran = cmd.Connection.BeginTransaction();
                    sda.InsertCommand.Transaction = tran;
                    sda.UpdateCommand.Transaction = tran;
                    sda.DeleteCommand.Transaction = tran;
                    sda.SelectCommand.Transaction = tran;
                    try
                    {
                        sda.Fill(ds);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        tran.Dispose();
                    }
                }
                else
                {
                    sda.Fill(ds);
                }
                return GetReturnData<DataSet>(ds, cmd);
            }

        }
        catch (Exception e)
        {
            throw e;
        }
        finally
        {
            sda.Dispose();
            cmd.Connection.Close();
            cmd.Dispose();
        }
    }
    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">执行类型</param>
    /// <returns></returns>
    public static ReturnData<DataSet> ExecuteDataSet(string sql, object parameter, CommandType cmdType)
    {
        return ExecuteDataSet(sql, parameter, cmdType, false);
    }


    public static ReturnData<T> ExecuteStoredProcedure<T>(string sql, object parameter,bool IsUsedTransaction)
    {
        try
        {
            
            if (typeof(T) == typeof(DataSet))
            {
                return (ReturnData<T>)Convert.ChangeType(ExecuteDataSet(sql, parameter, CommandType.StoredProcedure, IsUsedTransaction), typeof(ReturnData<T>));

            }
            else if (typeof(T) == typeof(DataTable))
            {
                return (ReturnData<T>)Convert.ChangeType(ExecuteDataTable(sql, parameter, CommandType.StoredProcedure, IsUsedTransaction), typeof(ReturnData<T>));

            }
            else if (typeof(T) == typeof(SqlDataReader)) {
                return (ReturnData<T>)Convert.ChangeType(ExecuteReader(sql, parameter, CommandType.StoredProcedure, IsUsedTransaction), typeof(ReturnData<T>));

            }
            else if (typeof(T) == typeof(XmlReader)) {
                return (ReturnData<T>)Convert.ChangeType(ExecuteXmlReader(sql, parameter, CommandType.StoredProcedure, IsUsedTransaction), typeof(ReturnData<T>));

            }
            else
            {
                throw new Exception("方法 ExecuteStoredProcedure<T> 的泛型只能为DataSet、DataTable、SqlDataReader 或 XmlReader。");
            }
        }
        catch (Exception e) {
            throw e;
        }
    }




}

