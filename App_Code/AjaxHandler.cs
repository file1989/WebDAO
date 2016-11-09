using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// AjaxHandler 的摘要说明
/// </summary>
public class AjaxHandler:AjaxBaseHandler
{
	public void t()
	{
        JSON.Write(true, "测试", Request["method"]);
	}
    
}