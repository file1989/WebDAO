using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Config 的摘要说明
/// </summary>
public class Config
{
    private static System.Collections.Specialized.NameValueCollection appSetting = System.Web.Configuration.WebConfigurationManager.AppSettings;
    public Config() { }
    /// <summary>
    /// 获取连接字符串
    /// </summary>
    public static string ConnectionString
    {
        get{ return System.Web.Configuration.WebConfigurationManager.ConnectionStrings["db"].ConnectionString; }
    }

    /// <summary>
    /// 取得指定配置节的值
    /// </summary>
    /// <param name="settingName"></param>
    /// <returns></returns>
    public static string GetSetting(string settingName)
    {
        return appSetting[settingName];
    }


}