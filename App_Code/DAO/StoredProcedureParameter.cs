using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
/// 
/// </summary>
public class StoredProcedureParameterFactory {
    private static Dictionary<string, List<StoredProcedureParameter>> StoredProcedureParameterPool = new Dictionary<string, List<StoredProcedureParameter>>();
    /// <summary>
    /// 
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


    public List<StoredProcedureParameter> this[string ProcedureName]
    {
        get
        {
            if (StoredProcedureParameterPool.ContainsKey(ProcedureName))
            {
                return StoredProcedureParameterPool[ProcedureName];
            }
            else { return null; }
        }
        set
        {
            if (StoredProcedureParameterPool.ContainsKey(ProcedureName))
            {
                StoredProcedureParameterPool[ProcedureName] = value;
            }
            else
            {
                StoredProcedureParameterPool.Add(ProcedureName, value);
            }
        }
    }

}