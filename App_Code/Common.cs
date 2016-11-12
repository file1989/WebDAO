using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

/// <summary>
/// Common 的摘要说明
/// </summary>
public class Common
{
	public Common(){}

    #region 获取根目录或虚拟目录名称
    /// <summary>
    /// 获取根目录或虚拟目录名称,如 ‘/PFMS/’
    /// </summary>
    public static string SystemVirtualDirectory
    {
        get
        {
            //AppDomainAppVirtualPath不依赖请求，更保险
            string strVirtualDirectory = HttpRuntime.AppDomainAppVirtualPath;
            //不是根目录是在虚拟目录后加‘/’
            if (strVirtualDirectory != "/")
                strVirtualDirectory += "/";
            return strVirtualDirectory;
        }
    }
    #endregion

    /// <summary>
    /// 获取GUID（全局统一标识符）
    /// </summary>
    /// <param name="format">一个单格式说明符，它指示如何格式化此 Guid 的值。 format 参数可以是“N”、“D”、“B”、“P”或“X”。 如果 format 为 null 或空字符串 ("")，则使用“D”（即默认值）。 </param>
    /// <returns></returns>
    public static string GetGUID(string format = null)
    {
        if (format == null || format == "") { format = "D"; }
        return Guid.NewGuid().ToString(format);
    }
    /// <summary>
    /// 时间戳转为C#格式时间
    /// </summary>
    /// <param name=”timeStamp”></param>
    /// <returns></returns>
    public static DateTime ToDateTime(string timeStamp)
    {
        DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        long lTime = long.Parse(timeStamp + "0000000");
        TimeSpan toNow = new TimeSpan(lTime);
        return dtStart.Add(toNow);
    }
    /// <summary>
    /// DateTime时间格式转换为Unix时间戳格式
    /// </summary>
    /// <param name=”time”></param>
    /// <returns></returns>
    public static int ToTimestamp(System.DateTime time)
    {
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        return (int)(time - startTime).TotalSeconds;
    }
    /// <summary>
    /// 获取用户IP地址
    /// </summary>
    /// <returns></returns>
    public static string GetUserIPAddress()
    {
        string result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        if (String.IsNullOrEmpty(result))
        {
            result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        if (String.IsNullOrEmpty(result))
        {
            result = HttpContext.Current.Request.UserHostAddress;
        }

        if (String.IsNullOrEmpty(result))
        {
            result = "0.0.0.0";
        }
        return result;
    }
    /// <summary>
    /// 字典转为SqlParameter[]参数数组
    /// </summary>
    /// <param name="dict">参数字典</param>
    /// <returns>SqlParameter[]</returns>
    public static System.Data.SqlClient.SqlParameter[] DictionaryToSqlParameters(Dictionary<string, object> dict)
    {
        ///http://www.cmono.net/index.php?post=436
        List<System.Data.SqlClient.SqlParameter> sqlParams = new List<System.Data.SqlClient.SqlParameter>();
        foreach (KeyValuePair<string, object> kvp in dict)
        {
            sqlParams.Add(new System.Data.SqlClient.SqlParameter("@" + kvp.Key, kvp.Value));
        }
        return sqlParams.ToArray();
    }
    /// <summary>
    /// MD5加密
    /// </summary>
    /// <param name="pws"></param>
    /// <returns></returns>
    public static string MD5(string str)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] Encrypt = md5.ComputeHash(System.Text.Encoding.Unicode.GetBytes(str));
        return BitConverter.ToString(Encrypt).Replace("-", "");
    }
    /// <summary>
    /// 获取错误信息的第一行
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string GetErrorMsg(string message)
    {
        //取错误信息的第一行。
        string s = message.Substring(0, message.IndexOf("\n"));
        if (s == "")
            return message;
        else
            return s;
    }
    /// <summary>
    /// 哈希加密
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string HashPassword(string source)
    {
        return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(source, "SHA1");
    }
    /// <summary>
    /// 获取来自客户端的参数
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, object> getRequestParameters()
    {
        Dictionary<string, object> dic = new Dictionary<string, object>();
        HttpRequest Request = HttpContext.Current.Request;
        if (Request.QueryString.Count > 0)
            foreach (string Key in Request.QueryString.Keys)
            {
                dic[Key] = Request.Params[Key];
            }
        if (Request.Form.Count > 0)
            foreach (string Key in Request.Form.Keys)
            {
                dic[Key] = Request.Params[Key];
            }
        //if (Request.Cookies.Count > 0)
        //    foreach (string Key in Request.Cookies.AllKeys)
        //    {
        //        dic[Key] = Request.Params[Key];
        //    }
        if (dic.Count > 0)
            return dic;
        else return null;
    }
    /// <summary>
    /// Base64字符串转图片
    /// </summary>
    /// <param name="base64String">图片的Base64字符串</param>
    /// <returns></returns>
    public static Image ToImage(string base64) {
        try
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(
                    Convert.FromBase64String(base64)))
            {
                //return Image.FromStream(ms);
                /*解决 GDI+ 中发生一般性错误。*/
                return (Image)new Bitmap(Image.FromStream(ms));
            }
        }
        catch (Exception ex) { throw new Exception(ex.Message); }
    }
    /// <summary>
    /// 图片转Base64字符串
    /// </summary>
    /// <param name="Image"></param>
    /// <returns></returns>
    public static string ToBase64(Image Image) {
        try
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                Image.Save(stream, ImageFormat.Jpeg);
                return Convert.ToBase64String(stream.GetBuffer());
            }
        }
        catch (Exception ex) { throw new Exception(ex.Message); }
    }
    /// <summary>
    /// 无损压缩图片
    /// </summary>
    /// <param name="sFile">原图片</param>
    /// <param name="dFile">压缩后保存位置</param>
    /// <param name="Height">高度</param>
    /// <param name="Width"></param>
    /// <param name="flag">压缩质量 1-100</param>
    /// <returns></returns>
    public static bool ImageCompress(string sFile, string dFile, int Height, int Width, int flag)
    {
        try
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            GetImageThumbnail(iSource, Height, Width, flag).Save(dFile);
            return true;
        }
        catch { return false; }
    }
    /// <summary>
    /// 获取缩略图
    /// </summary>
    /// <param name="image">原图片</param>
    /// <param name="Height">高度</param>
    /// <param name="Width">宽度</param>
    /// <param name="flag">压缩质量 1-100</param>
    /// <returns></returns>
    public static Image GetImageThumbnail(Image image, int Height, int Width, int flag)
    {
        ImageFormat tFormat = image.RawFormat;
        int sW = 0, sH = 0;
        //按比例缩放
        Size tem_size = new Size(image.Width, image.Height);
        if (tem_size.Width > Height || tem_size.Width > Width) //将**改成c#中的或者操作符号
        {
            if ((tem_size.Width * Height) > (tem_size.Height * Width))
            {
                sW = Width;
                sH = (Width * tem_size.Height) / tem_size.Width;
            }
            else
            {
                sH = Height;
                sW = (tem_size.Width * Height) / tem_size.Height;
            }
        }
        else
        {
            sW = tem_size.Width;
            sH = tem_size.Height;
        }
        Bitmap ob = new Bitmap(Width, Height);
        Graphics g = Graphics.FromImage(ob);
        g.Clear(Color.WhiteSmoke);
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(image, new Rectangle((Width - sW) / 2, (Height - sH) / 2, sW, sH), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
        g.Dispose();
        //以下代码为保存图片时，设置压缩质量
        EncoderParameters ep = new EncoderParameters();
        long[] qy = new long[1];
        qy[0] = flag;//设置压缩的比例1-100
        EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
        ep.Param[0] = eParam;
        try
        {
            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo jpegICIinfo = null;
            for (int x = 0; x < arrayICI.Length; x++)
            {
                if (arrayICI[x].FormatDescription.Equals("JPEG"))
                {
                    jpegICIinfo = arrayICI[x];
                    break;
                }
            }
            MemoryStream ms = new MemoryStream();
            if (jpegICIinfo != null)
            {
                ob.Save(ms, jpegICIinfo, ep);
            }
            else
            {
                ob.Save(ms, tFormat);
            }
            return (Image)new Bitmap(Image.FromStream(ms));/*解决 GDI+ 中发生一般性错误。*/
        }
        catch(Exception ex)
        {
            throw ex;
        }
        finally
        {
            image.Dispose();
            ob.Dispose();
        }
    }
}