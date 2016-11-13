<%@ WebHandler Language="C#" Class="Handler" %>

using System;
using System.Web;
using System.Drawing;
using System.IO;
public class Handler : IHttpHandler {
    public bool IsReusable { get { return false; } }
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/html";
        TestToBase64(context);
        
    }
 

    public void TestToBase64(HttpContext context)
    {
        
        Image img = Image.FromStream(context.Request.Files[0].InputStream);
        string t = Common.ToBase64(img);
        context.Response.Write("<img src=\"data:img/jpg;base64,"+t+ "\"/>" );
        Common.ToImage(t).Save("d:/11/11.jpg");
        Common.GetImageThumbnail(img, 360, 400, 10).Save("d:/11/Thumbnail-11.Jpeg");
        Common.ImageCompress("d:/11/11.jpg", "d:/11/ImageCompress-l.jpg", 4160, 3120, 80);

        
    }

}