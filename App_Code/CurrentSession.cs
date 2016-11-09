using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

/// <summary>
///CurrentSession 帮助处理类
/// </summary>
public class CurrentSession:IRequiresSessionState
{
    /// <summary>
    /// 设置或获取当前CurrentUser Sesson
    /// </summary>
    public static sys_Users CurrentUser
    {
        set{
            HttpContext.Current.Session.Remove("CurrentUser");
            HttpContext.Current.Session.Add("CurrentUser", value);
        }
        get{
            return (sys_Users)HttpContext.Current.Session["CurrentUser"];
        }
    }

    /// <summary>
    /// 是否在线
    /// </summary>
    public static bool IsOnline
    {
        get
        {
            if (null == HttpContext.Current.Session["CurrentUser"]) { return false; }
            else { return true; }
        }
    }
    
    /// <summary>
    /// 下线
    /// </summary>
    /// <returns></returns>
    public static bool Offline()
    {
        HttpContext.Current.Session.Remove("CurrentUser");
        if (null == HttpContext.Current.Session["CurrentUser"]) { return true; }
        else { return false; }
    }

    
}


/// <summary>
/// 用户信息类
/// </summary>
[Serializable]
public class sys_Users
{
    /// <summary>
    /// 账号
    /// </summary>
    public string UserID;
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName;
    /// <summary>
    /// 角色
    /// </summary>
    public string Role;
    /// <summary>
    /// 联系电话
    /// </summary>
    public string Phone;
    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email;
    /// <summary>
    /// QQ号
    /// </summary>
    public string QQ;

}

