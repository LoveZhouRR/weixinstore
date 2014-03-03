using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models;
using DBC.Ors.Services;
using DBC.Utils.Storage;
using DBC.WeChat.Models;
using DBC.WeChat.Models.Conversation;
using DBC.WeChat.Models.Conversation.Msg;
using DBC.WeChat.Services.Components.Files;
using DBC.WeChat.Services.Components.Picture;
using DBC.WeChat.Services.Security;
using DBC.WeChat.UI.Components;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.ueditor.net;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class ResourceController : BaseController
    {
        private readonly string ftp = ConfigurationManager.AppSettings["ftp"];
        private PictureSize ItemSize = ServiceLocator.Resolve<PictureSize>("ItemSize");
        public IPictureService ResourcePictureService = ServiceLocator.Resolve<IPictureService>("ResourcePictureService");
        public IPictureService NewsContentPictureService = ServiceLocator.Resolve<IPictureService>("NewsContentPictureService");
        public FileService AudioService = ServiceLocator.Resolve<FileService>("AudioService");
        public FileService videoService = ServiceLocator.Resolve<FileService>("VideoService");
        public ActionResult Picture()
        {
            var query = new PictureResourceQuery()
            {
                OwnerID = OwnerID,
                Take = PageSize,
                OrderField = "LastModifiedAt",
                OrderDirection = OrderDirection.Desc,
            };
            var pics = Service.Select(query).ToList();
            foreach (var pic in pics)
            {
                pic.Name = GetName(pic, ItemSize);
            }
            ViewData["Pagination"] = Pagination.FromQuery(query);
            ViewData["Query"] = query;
            return View(pics);
        }

        public ActionResult AjaxPicQuery(PictureResourceQuery query)
        {
            query.OwnerID = OwnerID;
            var targetList = Service.Select(query);
            ViewData["Pagination"] = Pagination.FromQuery(query);
            return PartialView("Resource/PictureList", targetList.ToList());
        }

        [HttpPost]
        public JsonResult CreatePic()
        {
            var resources = GetUploadResource();
            PictureResource pic = resources.Select(item => new PictureResource(item)).FirstOrDefault();
            pic.Path = MakePath(pic);
            pic.Name = GetHashName(pic.OriginName);
            ResourcePictureService.Create(pic);
            Service.Create(pic);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    success = true,
                    resourceID = pic.ID,
                    name = pic.OriginName,
                    path = System.IO.Path.Combine(ftp, pic.Path, GetName(pic, ItemSize)),
                    size = pic.Size
                },
            };
        }

        [HttpPost]
        public JsonResult DeletePic(int id)
        {
            var pic = Service.Select(new PictureResourceQuery() { IDs = new long[] { id } }).FirstOrDefault();
            if (pic != null)
            {
                ResourcePictureService.Delete(pic);
                Service.Delete(new PictureResource { ID = id });
            }
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { success = true },
            };
        }

        public ActionResult Audio()
        {
            var query = new AudioResourceQuery()
            {
                OwnerID = OwnerID,
                Take = PageSize,
                OrderField = "LastModifiedAt",
                OrderDirection = OrderDirection.Desc,
            };
            var audio = Service.Select(query).ToList();
            ViewData["Pagination"] = Pagination.FromQuery(query);
            ViewData["Query"] = query;
            return View(audio);
        }

        public ActionResult AjaxAudioQuery(AudioResourceQuery query)
        {
            query.OwnerID = OwnerID;
            var targetList = Service.Select(query);
            ViewData["Pagination"] = Pagination.FromQuery(query);
            return PartialView("Resource/AudioList", targetList.ToList());
        }

        [HttpPost]
        public JsonResult CreateAudio()
        {
            var resources = GetUploadResource();
            AudioResource audio = resources.Select(item => new AudioResource(item)).FirstOrDefault();
            audio.Name = GetHashName(audio.OriginName);
            audio.Path = MakePath(audio);
            AudioService.Create(new FileEntry()
            {
                Content = audio.Content,
                Path = System.IO.Path.Combine(audio.Path, audio.Name),
            });
            Service.Create(audio);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { success = true, resourceID = audio.ID, name = audio.OriginName, path = System.IO.Path.Combine(ftp, audio.Path, audio.Name), size = audio.Size },
            };
        }

        public JsonResult DeleteAudio(long id)
        {
            var audio = Service.Select(new AudioResourceQuery() { IDs = new long[] { id } }).FirstOrDefault();
            if (audio != null)
            {
                AudioService.Delete(new FileEntry()
                {
                    Path = System.IO.Path.Combine(audio.Path, audio.Name),
                });
                Service.Delete(audio);
            }
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { success = true },
            };
        }

        public ActionResult Video()
        {
            var query = new VideoResourceQuery()
            {
                OwnerID = OwnerID,
                Take = PageSize,
                OrderField = "LastModifiedAt",
                OrderDirection = OrderDirection.Desc,
            };
            var video = Service.Select(query).ToList();
            ViewData["Pagination"] = Pagination.FromQuery(query);
            ViewData["Query"] = query;
            return View(video);
        }

        public ActionResult AjaxVideoQuery(VideoResourceQuery query)
        {
            query.OwnerID = OwnerID;
            var targetList = Service.Select(query);
            ViewData["Pagination"] = Pagination.FromQuery(query);
            return PartialView("Resource/VideoList", targetList.ToList());
        }

        public ActionResult CreateVideo()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateVideo(VideoResource resource)
        {
            var resources = GetUploadResource();
            VideoResource video = resources.Select(item => new VideoResource(item)).ToList().FirstOrDefault();
            if (video == null)
                return View();
            var createList = new List<FileEntry>();
            video.Title = resource.Title;
            video.Description = resource.Description;
            video.Name = GetHashName(video.OriginName);
            video.Path = MakePath(video);
            createList.Add(new FileEntry()
            {
                Content = video.Content,
                Path = System.IO.Path.Combine(video.Path, video.Name),
            });
            videoService.Create(createList.ToArray());
            Service.Create(video);
            return RedirectToAction("Video");
        }

        public ActionResult EditVideo(long id)
        {
            var video =
                Service.Select(new VideoResourceQuery() { OwnerID = OwnerID, IDs = new long[] { id } }).FirstOrDefault();
            return View(video);
        }

        [HttpPost]
        public ActionResult EditVideo(VideoResource video)
        {
            Service.Update(video);
            return View(video);
        }

        public JsonResult DeleteVideo(long id)
        {
            var video = Service.Select(new VideoResourceQuery() { IDs = new long[] { id } }).FirstOrDefault();
            if (video != null)
            {
                AudioService.Delete(new FileEntry()
                {
                    Path = System.IO.Path.Combine(video.Path, video.Name),
                });
                Service.Delete(video);
            }
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { success = true },
            };
        }

        public ActionResult News()
        {
            var query = new NewsResourceQuery()
            {
                OwnerID = OwnerID,
                Take = PageSize,
                OrderField = "LastModifiedAt",
                OrderDirection = OrderDirection.Desc,
                ParentID = 0,
            };
            var news = Service.Select(query).ToList();
            foreach (var item in news)
            {
                if (item.Single.Value)
                {
                    continue;
                }
                var children = Service.Select(new NewsResourceQuery()
                {
                    OwnerID = OwnerID,
                    ParentID = item.ID
                }).ToArray();
                if (children.Any())
                {
                    item.Resources = children;
                }
            }
            ViewData["Pagination"] = Pagination.FromQuery(query);
            ViewData["Query"] = query;
            return View(news);
        }

        public ActionResult AjaxNewsQuery(NewsResourceQuery query)
        {
            query.OwnerID = OwnerID;
            var targetList = Service.Select(query);
            ViewData["Pagination"] = Pagination.FromQuery(query);
            return PartialView("Resource/NewsList", targetList.ToList());
        }

        public ActionResult CreateNews(bool single)
        {
            ViewBag.Single = single;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateResult(NewsResource resource)
        {
            var list = GetNewsResources();
            if (list.Any())
            {
                var parent = list.FirstOrDefault();
                parent.Single = list.Count == 1;
                parent.ParentID = 0;

                Service.Create(parent);
                var children = list.GetRange(1, list.Count - 1);
                if (children.Count != 0)
                {
                    //子节点绑定parent
                    children = children.Select(o =>
                    {
                        o.ParentID = parent.ID;
                        o.Single = true;
                        return o;
                    }).ToList();
                    Service.Create(children.ToArray());
                }
                var oldlist = new List<PictureResource>();
                var newlist = new List<PictureResource>();
                var files = GetUploadResource();
                for (int i = 0; i < list.Count; i++)
                {
                    oldlist.Add(new PictureResource()
                    {
                        Path = MakeCoverPicPath(0),
                        Name = GetHashName(files[i].OriginName)
                    });
                    newlist.Add(new PictureResource()
                    {
                        Path = MakeCoverPicPath(list[i].ID.Value),
                        Name = GetHashName(files[i].OriginName)
                    });
                    list[i].PicUrl = Path.Combine(ftp, MakeCoverPicPath(list[i].ID.Value),
                        GetHashName(files[i].OriginName));
                }
                NewsContentPictureService.Move(oldlist.ToArray(), newlist.ToArray());
                Service.Update(list.ToArray());
            }
            return RedirectToAction("News");
        }

        public ActionResult EditNews(long id)
        {
            var news = Service.SelectOrEmpty(new NewsResourceQuery()
            {
                OwnerID = OwnerID,
                IDs = new long[] {id},
            }).FirstOrDefault();
            ViewBag.Single = news.Single.Value;
            if (!ViewBag.Single)
            {
                var children = Service.Select(new NewsResourceQuery()
                {
                    OwnerID = OwnerID,
                    ParentID = id,
                });
                news.Resources = children.ToArray();
            }
            return View(news);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditNews(NewsResource news)
        {
            var list = GetNewsResources();
            if (list.Any())
            {
                var parent = list.FirstOrDefault();
                var createlist = list.Where(o => o.ID == null).Select(o =>
                {
                    o.ParentID = parent.ID;
                    o.Single = true;
                    return o;
                }).ToArray();
                if (createlist.Any())
                {
                    Service.Create(createlist.ToArray()); 
                }
                var oldlist = new List<PictureResource>();
                var newlist = new List<PictureResource>();
                var files = GetUploadResource();
                for (int i = 0; i < list.Count; i++)
                {
                    if(string.IsNullOrEmpty(files[i].OriginName))
                        continue;
                    oldlist.Add(new PictureResource()
                    {
                        Path = MakeCoverPicPath(0),
                        Name = GetHashName(files[i].OriginName)
                    });
                    newlist.Add(new PictureResource()
                    {
                        Path = MakeCoverPicPath(list[i].ID.Value),
                        Name = GetHashName(files[i].OriginName)
                    });
                    list[i].PicUrl = Path.Combine(ftp, MakeCoverPicPath(list[i].ID.Value),
                        GetHashName(files[i].OriginName));
                }
                NewsContentPictureService.Move(oldlist.ToArray(), newlist.ToArray());
                Service.Update(list.ToArray());
            }
            return RedirectToAction("News");
        }

        public JsonResult DeleteNews(long id)
        {
            var deletelist = new List<NewsResource>();
            deletelist.Add(new NewsResource()
            {
                ID = id
            });
            deletelist.AddRange(Service.Select(new NewsResourceQuery()
            {
                OwnerID = OwnerID,
                ParentID = id,
            }));
            Service.Delete(deletelist.ToArray());
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { success = true, }
            };
        }

        [HttpPost]
        public JsonResult UploadPic(int index)
        {
            var picFile = Request.Files["Pic" + index];
            byte[] buffer = new byte[picFile.ContentLength];
            picFile.InputStream.Read(buffer, 0, picFile.ContentLength);
            var pic = new PictureResource()
            {
                OriginName = Path.GetFileName(picFile.FileName),
                Content = buffer,
            };
            pic.Name = GetHashName(pic.OriginName);
            pic.Path = MakeCoverPicPath(0);
            NewsContentPictureService.Create(pic);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { success = true, path = Path.Combine(ftp, pic.Path, pic.Name), name = pic.OriginName }
            };
        }

        public FileResult Download(string path, string type, string name)
        {
            path = HttpUtility.UrlDecode(path);
            path = path.Replace("\\", "/");
            var stream = new WebClient().OpenRead(path);
            return File(stream, type, name);
        }

        private List<NewsResource> GetNewsResources()
        {
            int count = Convert.ToInt32(Request.Form["Count"]);
            var response = new List<NewsResource>();
            for (int i = 0; i < count; i++)
            {
                var item = new NewsResource()
                {
                    Title = Request.Form["Title" + i],
                    OwnerID = OwnerID,
                   
                };
                if (Request.Form["NewsContent" + i]!=null)
                {
                    item.NewsContent = Request.Form["NewsContent" + i];
                }
                if (Request.Form["ID" + i] != null)
                {
                    item.ID = Convert.ToInt64(Request.Form["ID" + i]);
                }
                response.Add(item);
            }
            return response;
        }


        private List<FileResource> GetUploadResource()
        {
            List<FileResource> response = new List<FileResource>();
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var picFile = Request.Files[i];
                FileResource pic = new FileResource();
                byte[] buffer = new byte[picFile.ContentLength];
                picFile.InputStream.Read(buffer, 0, picFile.ContentLength);
                pic.Content = buffer;
                pic.OriginName = Path.GetFileName(picFile.FileName);
                pic.OwnerID = OwnerID;
                pic.Size = picFile.ContentLength / 1024;
                response.Add(pic);
            }
            return response;
        }


        private string GetName(IPicture pic, PictureSize size)
        {
            return size.Width + "_" + size.Height + "_" + pic.Name;
        }

        private string GetHashName(string name)
        {
            var extension = System.IO.Path.GetExtension(name);
            return name.Hash() + extension;
        }

        private string MakePath(Resource resource)
        {
            string path = "";
            switch (resource.Type)
            {
                case ResourceType.Picture:
                    path = System.IO.Path.Combine("Resource", resource.OwnerID.ToString(), "Picture");
                    break;
                case ResourceType.Audio:
                    path = System.IO.Path.Combine("Resource", resource.OwnerID.ToString(), "Audio");
                    break;
                case ResourceType.Video:
                    path = System.IO.Path.Combine("Resource", resource.OwnerID.ToString(), "video");
                    break;
                case ResourceType.News:
                    path = System.IO.Path.Combine("Resource", resource.OwnerID.ToString(), "News");
                    break;
            }
            return path;
        }

        private string MakeCoverPicPath(long id)
        {
            if (id == 0)
            {
                return System.IO.Path.Combine("Resource", OwnerID.ToString(), "NewsPicture", "Temp");
            }
            else
            {
                return System.IO.Path.Combine("Resource", OwnerID.ToString(), "NewsPicture", id.ToString());
            }
        }
    }
}
