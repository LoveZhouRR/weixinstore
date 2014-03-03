<%@ WebHandler Language="C#" Class="imageUp" %>

using System;
using System.Configuration;
using System.Web;
using System.IO;
using System.Collections;
using System.Linq;
using System.Web.SessionState;
using DBC;
using DBC.Ors.Services;
using DBC.WeChat.Models.Conversation;
using DBC.WeChat.Services.Components.Picture;
using DBC.WeChat.Services.Security;
using DBC.WeChat.UI.Store.ueditor.net;

public class imageUp : IHttpHandler, IRequiresSessionState
{
    public IModelService Service { get { return ServiceLocator.Resolve<IModelService>("Internal"); } }
    public IPictureService NewsContentPictureService = ServiceLocator.Resolve<IPictureService>("NewsContentPictureService");
    private readonly string ftp = ConfigurationManager.AppSettings["ftp"];
    public void ProcessRequest(HttpContext context)
    {
        if (!String.IsNullOrEmpty(context.Request.QueryString["fetch"]))
        {
            context.Response.AddHeader("Content-Type", "text/javascript;charset=utf-8");
            context.Response.Write(String.Format("updateSavePath([{0}]);", String.Join(", ", UConfig.ImageSavePath.Select(x => "\"" + x + "\""))));
            return;
        }
        context.Response.ContentType = "text/plain";
        //上传图片
        Hashtable info = new Hashtable();
        var file = context.Request.Files[0];
        byte[] buffer = new byte[file.ContentLength];
        file.InputStream.Read(buffer, 0, file.ContentLength);
        PictureResource pic = new PictureResource()
        {
            Content = buffer,
            Path = System.IO.Path.Combine("Resource", context.Session["OwnerID"].ToString(), "News"),
            OriginName = Path.GetFileName(file.FileName),

        };
        pic.Name = GetHashName(pic.OriginName);
        NewsContentPictureService.Create(pic);
        string url = System.IO.Path.Combine(ftp, pic.Path, pic.Name);
        HttpContext.Current.Response.Write("{'url':'" + url.Replace("\\","/") + "','title':'" + pic.OriginName + "','original':'" + pic.OriginName + "','state':'SUCCESS'}");  //向浏览器返回数据json数据
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

    private string GetHashName(string name)
    {
        var extension = System.IO.Path.GetExtension(name);
        return name.Hash() + extension;
    }
}