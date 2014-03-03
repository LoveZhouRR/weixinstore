<%@ WebHandler Language="C#" Class="imageManager" %>
/**
 * Created by visual studio2010
 * User: xuheng
 * Date: 12-3-7
 * Time: 下午16:29
 * To change this template use File | Settings | File Templates.
 */
using System;
using System.Collections;
using System.Configuration;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.SessionState;
using DBC;
using DBC.Utils.Storage.Services.Ftp;
using DBC.WeChat.Services.Components.Picture;
using NPOI.SS.Formula.PTG;

public class imageManager : IHttpHandler, IRequiresSessionState
{
    public IPictureService NewsContentPictureService = ServiceLocator.Resolve<IPictureService>("NewsContentPictureService");
    public Options Options = ServiceLocator.Resolve<Options>("FtpOptions");
    private readonly string fileftp = ConfigurationManager.AppSettings["fileFtp"];
    private readonly string ftp = ConfigurationManager.AppSettings["ftp"];
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        string action = context.Server.HtmlEncode(context.Request["action"]);
        var path = System.IO.Path.Combine(fileftp,"Resource", context.Session["OwnerID"].ToString(), "News");
        //遍历ftp目录
        FtpWebRequest req = (FtpWebRequest)WebRequest.Create(path.Replace("\\","/"));
        req.Method = WebRequestMethods.Ftp.ListDirectory;
        req.Credentials = new NetworkCredential(Options.UserName, Options.Password);
        if (action == "get")
        {
            String str = String.Empty;
            WebResponse response = req.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string lsdirectory = reader.ReadLine();
            while (lsdirectory != null)
            {
                var pathi = Path.Combine(ftp, "Resource", context.Session["OwnerID"].ToString(), "News", Path.GetFileName(lsdirectory));
                str += pathi+ "ue_separate_ue";
                lsdirectory = reader.ReadLine();
            }


            //foreach (string path in paths)
            //{
            //    DirectoryInfo info = new DirectoryInfo(context.Server.MapPath(path));

            //    //目录验证
            //    if (info.Exists)
            //    {
            //        DirectoryInfo[] infoArr = info.GetDirectories();
            //        foreach (DirectoryInfo tmpInfo in infoArr)
            //        {
            //            foreach (FileInfo fi in tmpInfo.GetFiles())
            //            {
            //                if (Array.IndexOf(filetype, fi.Extension) != -1)
            //                {
            //                    str += path+"/" + tmpInfo.Name + "/" + fi.Name + "ue_separate_ue";
            //                }
            //            }
            //        }
            //    }
            //}

            context.Response.Write(str);
        }
    }


    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}