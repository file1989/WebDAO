using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Reflection;
/// <summary>
/// 不检查在线状态，调用Ajax处理程序标记
/// </summary>
public interface INotCheckSessionOnline { }
/// <summary>
/// Ajax处理程序基类。Ajax处理程序类只要继承此类（继承标记INotCheckSessionOnline接口，可不检查在线状态，调用Ajax处理程序），类中方法类似public void test(){}的即可调用。
/// </summary>
public class AjaxBaseHandler : IHttpHandler,IRequiresSessionState
{
    #region 基础变量及方法
    /// <summary>
    /// 当前 Request 对象。
    /// </summary>
    public HttpRequest Request { get { return HttpContext.Current.Request; } }
    /// <summary>
    /// 当前 Response 对象。
    /// </summary>
    public HttpResponse Response { get { return HttpContext.Current.Response; } }
    /// <summary>
    /// 当前 Server 对象。
    /// </summary>
    public HttpServerUtility Server { get { return HttpContext.Current.Server; } }
    /// <summary>
    /// 当前 Session 对象。
    /// </summary>
    public HttpSessionState Session { get { return HttpContext.Current.Session; } }
    public bool IsReusable { get { return false; } }
    public void ProcessRequest(HttpContext context)
    {
        try
        {
            //context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //context.Response.ContentType = "text/json";
            context.Response.ContentType = "text/plain";
            context.Response.Charset = "UTF-8";

            //默认需要登录
            if (typeof(INotCheckSessionOnline).IsAssignableFrom(this.GetType()) == false && CurrentSession.IsOnline)
            {
                throw new Exception("请登录后，再进行操作");
            }

            //获取处理方法
            string method = context.Request["method"] ?? string.Empty;
            if (null == method || "" == method) { throw new Exception("参数 method 异常"); }
            MethodInfo methodInfo = this.GetType().GetMethod(method);
            if (null == methodInfo) { throw new Exception(string.Format("暂时不提供 {0} 功能", method)); }
            methodInfo.Invoke(this,null);
        }
        catch (Exception ex)
        {
            JSON.Write(false, ex.Message, ex.StackTrace);
        }
        finally
        {
            context.Response.Flush();
            context.Response.End();
        }
    }

    #endregion


    //public void test() {
    //    JSON.Write(true , "test", null);
    //}

}