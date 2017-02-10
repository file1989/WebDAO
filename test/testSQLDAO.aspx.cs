using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;

public partial class test_testSQLDAO : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
       // string sql = @"INSERT INTO [dbo].[Users]([UserId],[UserName],[UserSex],[score])VALUES(@UserId,@UserName,@UserSex,@score)";

       ////ReturnData<int> r  = SQLServerDAO.ExecuteNonQuery(sql, new SqlParameter[] { 
       ////     new SqlParameter("@UserId","l")
       ////     ,new SqlParameter("@UserName","琳琳")
       ////     ,new SqlParameter("@UserSex","男")
       ////     ,new SqlParameter("@score","10")
       //// }, CommandType.Text);
       // Dictionary<string, object> p = new Dictionary<string, object>();
        
       // p.Add("@UserId","l52");
       //// p.Add("@UserName","琳琳");
       //// p.Add("@UserSex","男");
       // p.Add("score", "10");
       //ReturnData<int> r = SQLServerDAO.ExecuteNonQuery(sql,p, CommandType.Text);

       //Response.Write(r.Data);

        //测试执行datatable
       // JSON.Write(SQLServerDAO.ExecuteDataTable("select *  from Users", null, CommandType.Text).Data);

        //JSON.Write(SQLServerDAO.ExecuteDataTable("select *  from Users where UserId=@UserId",
        //    new SqlParameter[] { 
        //        new SqlParameter("@UserId","l5")
        //    }
        //    , CommandType.Text).Data);

        //Dictionary<string, object> p = new Dictionary<string, object>();
        //p.Add("@UserId", "l52");
        //p.Add("UserName", "669张三");
        //JSON.Write(SQLServerDAO.ExecuteDataTable("update Users set UserName=@UserName where UserId=@UserId;select *  from Users where UserId=@UserId", p, CommandType.Text).Data);

        
    }
}