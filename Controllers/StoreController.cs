using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Services;
using DBC.WeChat.Models.Infrastructures;
using System.Configuration;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.Models;
using System.Collections.ObjectModel;
using DBC.WeChat.Services.Components.Picture;
using DBC.WeChat.Models;
using DBC.WeChat.Models.Excetion;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class StoreController : BaseController
    {
        private readonly string ftp = ConfigurationManager.AppSettings["ftp"];
        private readonly IModelService svc = ServiceLocator.Resolve<IModelService>("Internal");
        private readonly IPictureService _pictureService = ServiceLocator.Resolve<IPictureService>("BannerFtp");

        //提示字符串
        private const string PicTypeError = "文件{0}类型不正确";
        private const string PicSizeError = "图片{0}大小超出范围";


        //
        // GET: /Store/
        [HttpGet]
        public ActionResult Index()
        {
            var model = svc.SelectOrEmpty(new StoreQuery() { IDs = new long[] { OwnerID } }).FirstOrDefault();

            var banQuery = new BannerPictureQuery
            {
                StoreIDs = new long[] { OwnerID },
                PictureTypes = new int[] { (int)StorePicType.BannerPicture }
            };

            var banModel = svc.SelectOrEmpty(banQuery);

            if (banModel != null && banModel.Any())
            {
                if (model != null)
                {
                    Collection<BannerPicture> bCollection = new Collection<BannerPicture>();
                    foreach (var item in banModel.ToList())
                    {
                        item.Path = Path.Combine(ftp, item.Path, item.Name);
                        bCollection.Add(item);
                    }

                    model.BannerPictures = bCollection;
                }
                else
                {
                    model = new WeChat.Models.Infrastructures.Store();
                }
            }

            var vm = new StoreVM()
            {
                BannerPictures = model.BannerPictures,
                Announcement = model.Announcement ?? "",
                StoreName = model.Name
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult Index(StoreVM model)
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Intro()
        {
            DBC.WeChat.Models.Infrastructures.Store StoreModel = GetStore();

            var banQuery = new BannerPictureQuery
            {
                StoreIDs = new long[] { OwnerID },
                PictureTypes = new int[] { (int)StorePicType.IntroPicture }
            };

            var banModel = svc.SelectOrEmpty(banQuery);

            if (StoreModel == null)
            {
                StoreModel = new WeChat.Models.Infrastructures.Store();
            }

            if (banModel != null && banModel.Any())
            {
                Collection<BannerPicture> bCollection = new Collection<BannerPicture>();
                foreach (var item in banModel.ToList())
                {
                    item.Path = Path.Combine(ftp, item.Path, item.Name);
                    bCollection.Add(item);
                }

                StoreModel.BannerPictures = bCollection;
            }

            return View(StoreModel);
        }

        /// <summary>
        /// 店铺修改信息
        /// </summary>
        /// <param name="Phone"></param>
        /// <param name="Email"></param>
        /// <param name="Address"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AjaxIntro(string Phone, string Email, string Address, string Description)
        {
            JsonResult result = new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                DBC.WeChat.Models.Infrastructures.Store StoreModel = GetStore();

                StoreModel.Address = Address;
                StoreModel.Phone = Phone;
                StoreModel.Email = Email;
                StoreModel.Description = Description;

                svc.Update(StoreModel);

                result.Data =
                        new
                        {
                            success = true,
                            message = "修改成功"
                        };
            }
            catch (Exception e)
            {
                result.Data =
                        new
                        {
                            success = false,
                            message = e.Message
                        };
            }

            return result;
        }

        /// <summary>
        /// 上传banner图片
        /// </summary>
        /// <param name="form"></param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public ActionResult UploadBanner(FormCollection form)
        {
            return DoUpload((int)StorePicType.BannerPicture);
        }

        /// <summary>
        /// 上传公司介绍图片
        /// </summary>
        /// <param name="form"></param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public ActionResult UploadIntro(FormCollection form)
        {
            return DoUpload((int)StorePicType.IntroPicture);
        }

        /// <summary>
        /// 统一处理图片上传工作
        /// </summary>
        /// <param name="res">需要返回的操作结果</param>
        /// <param name="picType">带处理的图片类型</param>
        /// <returns>操作结果</returns>
        public JsonResult DoUpload(int picType)
        {
            JsonResult result = new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                ContentType = "text/html"
            };
            try
            {
                var pics = GetUploadPictures(picType).ToArray();
                if (!pics.Any())
                    result.Data =
                        new
                        {
                            success = false,
                            Message = "文件列表为空"
                        };
                else
                {
                    List<BannerPicture> _List = new List<BannerPicture>();
                    foreach (var item in pics.ToList())
                    {
                        item.Name = DateTime.Now.ToString("yyMMdd_HHmmss_") + item.Name;

                        _pictureService.Create(item);

                        svc.Create(item);

                        var fullpath = Path.Combine(ftp, item.Path, item.Name);

                        BannerPicture _item = item;
                        _item.Path = fullpath;
                        _List.Add(_item);
                    }
                    //清空Content,否则反序列化时会因数据太大而失败 
                    foreach (var item in _List)
                    {
                        item.Content = null;
                    }
                    result.Data =
                        new
                        {
                            success = true,
                            picList = _List
                        };
                }

                return result;
            }
            catch (PicTypeException e)
            {
                //文件格式不正确，提示错误时，需还原成本地文件名称，去除区分同名文件所加上的时间戳字符串
                result.Data = new { success = false, Message = string.Format(PicTypeError, e.pic.Name.Substring(14)) };
                return result;
            }
            catch (PicSizeException e)
            {
                //文件大小超出，提示错误时，需还原成本地文件名称，去除区分同名文件所加上的时间戳字符串
                result.Data = new { success = false, Message = string.Format(PicSizeError, e.pic.Name.Substring(14)) };
                return result;
            }
            catch (Exception e)
            {
                result.Data = new { success = false, Message = e.Message };
                return result;
            }
        }

        /// <summary>
        /// 获取待上传的图片列表信息
        /// </summary>
        /// <returns>待上传的图片列表</returns>
        private Collection<BannerPicture> GetUploadPictures(int picType)
        {
            Collection<BannerPicture> BannerPicCollection = new Collection<BannerPicture>();

            for (int i = 0; i < Request.Files.Count; i++)
            {
                var picFile = Request.Files[i];

                BannerPicture pic = new BannerPicture();
                byte[] buffer = new byte[picFile.ContentLength];
                picFile.InputStream.Read(buffer, 0, picFile.ContentLength);
                pic.Content = buffer;
                pic.Name = Path.GetFileName(picFile.FileName);
                pic.Path = MakePath(picType);
                pic.MemorySize = picFile.ContentLength;
                pic.Store = GetStore();
                pic.StoreID = OwnerID;
                pic.PictureType = picType;

                BannerPicCollection.Add(pic);
            }

            return BannerPicCollection;
        }

        /// <summary>
        /// 根据店铺ID生成图片上传路径
        /// 路径规则Banner图片:Banner/StoreID/图片
        /// 路径规则Intro图片:Intro/StoreID/图片
        /// </summary>
        /// <param name="PicType">图片类型</param>
        /// <returns>图片上传路径</returns>
        private string MakePath(int PicType)
        {
            string path = "";

            if (PicType == (int)StorePicType.BannerPicture)
            {
                path = Path.Combine("Banner", OwnerID.ToString());
            }
            else if (PicType == (int)StorePicType.IntroPicture)
            {
                path = Path.Combine("Store", OwnerID.ToString());
            }

            return path;
        }

        /// <summary>
        /// 获取店铺信息
        /// </summary>
        /// <returns>店铺信息</returns>
        private DBC.WeChat.Models.Infrastructures.Store GetStore()
        {
            DBC.WeChat.Models.Infrastructures.Store _Stroe = svc.SelectOrEmpty(new StoreQuery() { IDs = new long[] { OwnerID } }).FirstOrDefault();

            return _Stroe;
        }

        /// <summary>
        /// 删除图片
        /// </summary>
        /// <param name="picID">Banner图片系统编号</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public ActionResult PicDelete(long picID)
        {
            JsonResult result = new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            try
            {
                //删除物理文件
                BannerPicture _PicModel = svc.SelectOrEmpty(new BannerPictureQuery() { IDs = new long[] { picID } }).FirstOrDefault();
                _pictureService.Delete(_PicModel);

                //删除数据库记录
                svc.Delete(new BannerPicture() { ID = picID });

                result.Data =
                    new
                    {
                        Success = true
                    };

                return result;
            }
            catch (Exception e)
            {
                result.Data =
                    new
                    {
                        Success = false,
                        Message = e.Message
                    };

                return result;
            }
        }

        /// <summary>
        /// 保存更新Store表中Announcement字段
        /// </summary>
        /// <param name="announcement"></param>
        /// <returns>操作结果</returns>
        public ActionResult SaveAnnouncement(string sname, string announcement)
        {
            JsonResult result = new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            try
            {
                DBC.WeChat.Models.Infrastructures.Store _Store = GetStore();

                _Store.Name = sname;
                _Store.Announcement = announcement;

                svc.Update(_Store);

                result.Data =
                    new
                    {
                        Success = true
                    };

                return result;
            }
            catch (Exception e)
            {
                result.Data =
                    new
                    {
                        Success = false,
                        Message = e.Message
                    };

                return result;
            }
        }
    }
}
