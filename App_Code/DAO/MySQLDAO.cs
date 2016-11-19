
/*
 * MySQL 数据库访问对象帮助处理类
 * 
 * 作者：lzp
 * 日期：2016.11.19
 * 版本：v1.0
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

/// <summary>
/// MySQL 数据库访问对象帮助处理类
/// </summary>
public class MySQLDAO
{
    /// <summary>
    /// 获取配置文件web.config的数据库连接字符串
    /// </summary>
    private static string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlDB"].ConnectionString;
    /// <summary>
    /// 数据库连接对象【已打开】
    /// </summary>
    /// <returns></returns>
    public static MySqlConnection GetMySqlConnection() {
        MySqlConnection conn = new MySqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }
    /// <summary>
    /// 数据库连接对象【已打开】
    /// </summary>
    /// <param name="ConnectionString">连接字符串</param>
    /// <returns></returns>
    public static MySqlConnection GetMySqlConnection(string ConnectionString)
    {
        MySqlConnection conn = new MySqlConnection(ConnectionString);
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
    public static MySqlCommand GetMySqlCommand(MySqlConnection connection, string sql, object parameter, CommandType commandType)
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
        using (MySqlCommand cmd = new MySqlCommand(sql, connection)) {
            cmd.CommandTimeout = 60;
            cmd.CommandType = commandType;
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
                     * 支持参数不带‘@’。
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
                            cmd.Parameters.Add(new MySqlParameter(key, value));
                        }

                    }
                    cmd.CommandText = sql;
                }
                #endregion

                #region 存储过程的参数处理

                else if (CommandType.StoredProcedure == commandType)
                {
                    cmd.Parameters.AddRange(ParameterFactory.GetMySqlParameters(sql, _parameter));
                }
                #endregion

            }
            else if (parameter is MySqlParameter[])
            {
                cmd.Parameters.AddRange((MySqlParameter[])parameter);
            }
            else
            {
                throw new Exception("不支持的参数类型。");
            }
            return cmd;
        }
    }
    /// <summary>
    /// 获取数据库命令
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="commandType">命令类型</param>
    /// <returns></returns>
    public static MySqlCommand GetMySqlCommand(string sql, object parameter, CommandType commandType) {
        return GetMySqlCommand(GetMySqlConnection(), sql, parameter, commandType);
    }

    /// <summary>
    /// 读取返回值。包括存储过程返回值和输出参数值
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    /// <param name="Data">泛型类型对象</param>
    /// <param name="cmd">已执行的数据库命令</param>
    /// <returns></returns>
    public static ReturnData<T> GetReturnData<T>(T Data, MySqlCommand cmd)
    {
        ReturnData<T> rd = new ReturnData<T>();
        rd.Data = Data;
        /*读取存储过程返回值和输出参数值*/
        foreach (MySqlParameter p in cmd.Parameters)
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

    #region 执行 MySqlCommand 的ExecuteNonQuery、ExecuteReader、ExecuteScalar方法

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
        MySqlCommand cmd = GetMySqlCommand(sql, parameter, cmdType);
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
        finally
        {
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
        return ExecuteNonQuery(sql, parameter, cmdType, false);
    }
    /// <summary>
    /// 执行SQL语句或存储过程
    /// </summary>
    /// <param name="sql">SQL语句或存储过程名称</param>
    /// <param name="parameter">参数。类型为Dictionary字典（参数名带不带@都可以）或 SqlParameter[] </param>
    /// <param name="cmdType">命令类型</param>
    /// <param name="IsUsedTransaction">是否使用事务</param>
    /// <returns></returns>
    public static ReturnData<MySqlDataReader> ExecuteReader(string sql, object parameter, CommandType cmdType, bool IsUsedTransaction)
    {

        MySqlCommand cmd = GetMySqlCommand(sql, parameter, cmdType);
        try
        {
            if (true == IsUsedTransaction)
            {
                cmd.Connection.BeginTransaction();
                try
                {
                    MySqlDataReader sdr = cmd.ExecuteReader();
                    cmd.Transaction.Commit();
                    return GetReturnData<MySqlDataReader>(sdr, cmd);
                }
                catch (Exception ex)
                {
                    cmd.Transaction.Rollback();
                    throw ex;
                }

            }
            else
            {
                return GetReturnData<MySqlDataReader>(cmd.ExecuteReader(), cmd);
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
    public static ReturnData<MySqlDataReader> ExecuteReader(string sql, object parameter, CommandType cmdType)
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
        MySqlCommand cmd = GetMySqlCommand(sql, parameter, cmdType);
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
                return GetReturnData<object>(cmd.ExecuteScalar(), cmd);
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
        MySqlCommand cmd = GetMySqlCommand(sql, parameter, cmdType);
        MySqlDataAdapter sda = new MySqlDataAdapter(cmd);
        try
        {
            using (DataTable dt = new DataTable())
            {
                if (true == IsUsedTransaction)
                {
                    MySqlTransaction tran = cmd.Connection.BeginTransaction();
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

        MySqlCommand cmd = GetMySqlCommand(sql, parameter, cmdType);
        MySqlDataAdapter sda = new MySqlDataAdapter(cmd);
        try
        {
            using (DataSet ds = new DataSet())
            {
                if (true == IsUsedTransaction)
                {
                    MySqlTransaction tran = cmd.Connection.BeginTransaction();
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


    public static ReturnData<T> ExecuteStoredProcedure<T>(string sql, object parameter, bool IsUsedTransaction)
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
            else if (typeof(T) == typeof(MySqlDataReader))
            {
                return (ReturnData<T>)Convert.ChangeType(ExecuteReader(sql, parameter, CommandType.StoredProcedure, IsUsedTransaction), typeof(ReturnData<T>));

            }
            
            else
            {
                throw new Exception("方法 ExecuteStoredProcedure<T> 的泛型只能为DataSet、DataTable 或 SqlDataReader。");
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }





}