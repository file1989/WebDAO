using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ServiceReference1;

public partial class asmx_test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        lyfePortsSoapClient c = new lyfePortsSoapClient();
        Response.Write(c.HelloWorld());




    }
}