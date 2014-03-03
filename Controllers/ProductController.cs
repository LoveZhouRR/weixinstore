using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models;
using DBC.Ors.Models.Sales;
using DBC.Ors.Services;
using DBC.WeChat.Models;
using DBC.WeChat.Models.Conversation;
using DBC.WeChat.Models.Excetion;
using DBC.WeChat.Models.Sales;
using DBC.WeChat.Services.Components.Picture;
using DBC.WeChat.Services.Conversation.Components;
using DBC.WeChat.Services.Security;
using DBC.WeChat.UI.Components;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.Models;
using Product = DBC.WeChat.Models.Sales.Product;
using ProductPicture = DBC.WeChat.Models.Sales.ProductPicture;
using ProductPictureQuery = DBC.WeChat.Models.Sales.ProductPictureQuery;
using ProductQuery = DBC.WeChat.Models.Sales.ProductQuery;
using Specification = DBC.WeChat.Models.Sales.Specification;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class ProductController : BaseController
    {

        private readonly string ftp = ConfigurationManager.AppSettings["ftp"];
        private readonly IModelService svc = ServiceLocator.Resolve<IModelService>("Internal");
        private readonly IPictureService _pictureService = ServiceLocator.Resolve<IPictureService>("Ftp");
        private readonly IConversationService _conversationService = ServiceLocator.Resolve<IConversationService>("ConversationService");
        private PictureSize MallSize = ServiceLocator.Resolve<PictureSize>("MallSize");
        private PictureSize ListSize = ServiceLocator.Resolve<PictureSize>("ListSize");
        private List<PictureSize> sizes;

        //提示字符串
        private const string PicTypeError = "文件{0}类型不正确";
        private const string PicSizeError = "图片{0}大小超出范围";
        private const string EmptyProductName = "商品名称不能为空";
        private const string ProductCannotEdit = "已上架商品不能编辑";

        public ActionResult Index()
        {
            var query = new ProductQuery()
                {
                    OwnerIDs = new long[] { GetOwnerID() },
                    Take = PageSize,
                    OrderField = "LastModifiedAt",
                    OrderDirection = OrderDirection.Desc,
                    Includes = new string[] { "Specifications", "ProductPictures" }
                };
            var initList = svc.Select(query).ToArray();
            ViewData["Pagination"] = Pagination.FromQuery(query);
            ViewData["picList"] = GetItemPath(initList);
            return View(initList);
        }

        public ActionResult AjaxQuery(ProductQuery query)
        {
            //读取StoreID
            if (query == null) query = new ProductQuery();
            Boolean hasQueryed = false;
            if (Request.Form["hasQuery"] != null)
            {
                hasQueryed = Convert.ToBoolean(Request.Form["hasQuery"]);
            }
            if (hasQueryed)
            {
                if (query.NamePattern != null)
                {
                    query.NamePattern = query.NamePattern.Trim();
                }
            }
            else
            {
                query.NamePattern = null;
            }
            if (query.OrderField != null && query.OrderField != "")
            {
                query.OrderField = query.OrderField;
            }
            query.OwnerIDs = new long[] { GetOwnerID() };
            query.Includes = new string[] { "Specifications", "ProductPictures" };
            query.Take = PageSize;
            var products = svc.Select(query).ToArray();
            var pro = Pagination.FromQuery(query);
            ViewBag.NamePattern = query.NamePattern;
            ViewData["Pagination"] = pro;
            ViewData["picList"] = GetItemPath(products);
            return PartialView("Product/ProductList", products);
        }

        //public ActionResult PartlyQuery(ProductQuery query)
        //{
        //    ProductQuery localQuery = new ProductQuery();
        //    if (query != null)
        //    {
        //        localQuery.OrderDirection = query.OrderDirection;
        //        localQuery.OrderField = query.OrderField;
        //        localQuery.Skip = query.Skip;
        //    }

        //    localQuery.OwnerIDs = new long[] { GetOwnerID() };
        //    localQuery.Includes = new string[] { "Specifications", "ProductPictures" };
        //    localQuery.Take = PageSize;
        //    var products = svc.Select(localQuery).ToArray();
        //    var pro = Pagination.FromQuery(localQuery);
        //    ViewData["Pagination"] = pro;
        //    ViewData["picList"] = GetItemPath(products);
        //    return PartialView("Product/ProductList", products);
        //}

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(Product product)
        {
            try
            {
                if (string.IsNullOrEmpty(product.Name))
                {
                    ErrorView error = new ErrorView() { message = EmptyProductName };
                    return View("Error", error);
                }
                product.Specifications = GetSpecifications(product);
                var orignpics = GetPictures(product);
                product.OwnerID = GetOwnerID();
                product.ProductPictures = orignpics;
                svc.Create(product);
                if (orignpics.Count > 0)
                {
                    var newpics = new ProductPicture[orignpics.Count()];
                    for (int i = 0; i < orignpics.Count(); i++)
                    {
                        ProductPicture pic = new ProductPicture()
                        {
                            ID = orignpics[i].ID,
                            Name = orignpics[i].Name,
                            Path = MakePath(product.ID.Value),
                        };
                        newpics[i] = pic;
                    }
                    svc.Update(newpics);
                    _pictureService.Move(orignpics.ToArray(), newpics);
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ErrorView error = new ErrorView() { message = e.Message };
                return View("Error", error);
            }
        }

        public ActionResult Edit(long Id)
        {
            var product = svc.Select(new ProductQuery() { IDs = new long[] { Id }, Includes = new string[] { "Specifications", "ProductPictures" } }).FirstOrDefault();
            if (product != null)
            {
                ViewData["MallNames"] = Makenames(product, MallSize);
                ViewBag.Tags = GetTags(Id);
            }
            return View(product);
        }

        [HttpPost]
        public ActionResult Edit(Product product)
        {
            if (product.Shelved != null && product.Shelved.Value)
            {
                return View("Error", new ErrorView() { message = ProductCannotEdit });
            }
            try
            {
                var pics = GetPictures(product);
                foreach (ProductPicture pic in pics)
                {
                    if (pic.ID == 0)
                    {
                        svc.Create(pic);
                    }
                    else
                    {
                        svc.Update(pic);
                    }
                }
                var specifications = GetSpecifications(product);
                foreach (var specification in specifications)
                {
                    if (specification.ID == 0)
                    {
                        svc.Create(specification);
                    }
                    else
                    {
                        svc.Update(specification);
                    }
                }

                //******************************************************
                product.Description = product.Description ?? "";
                product.Weight = product.Weight ?? 0;
                //******************************************************

                svc.Update(product);
                product.Specifications = specifications;
                product.ProductPictures = pics;
                ViewData["MallNames"] = Makenames(product, MallSize);
                ViewBag.Tags = GetTags(product.ID.Value);
                return View(product);
            }
            catch (Exception e)
            {
                return View("Error", new ErrorView() { message = e.Message });
            }
        }


        [HttpPost]
        public JsonResult Upload()
        {
            JsonResult result = new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                ContentType = "text/html",
            };
            long productID = Request.Form["productID"] == null ? 0 : Convert.ToInt64(Request.Form["productID"]);
            try
            {
                var pics = GetUploadPictures(productID).ToArray();
                if (!pics.Any())
                    result.Data = new { success = false };
                else
                {
                    _pictureService.Create(pics);
                    if (productID != 0)
                    {
                        Service.Create(pics);
                    }
                    var first = pics.FirstOrDefault();
                    var fullpath = Path.Combine(ftp, first.Path, _pictureService.GetName(first, MallSize));
                    result.Data =
                        new
                            {
                                success = true,
                                Id = first.ID != null ? first.ID.Value : 0,
                                MallName = fullpath,
                                OriginName=first.OriginName,
                                Name = first.Name,
                                Path = first.Path,
                                IsFirst = first.IsFirst
                            };
                }
                return result;
            }
            catch (PicTypeException e)
            {
                result.Data = new { success = false, message = string.Format(PicTypeError, e.pic.Name) };
                return result;
            }
            catch (PicSizeException e)
            {
                result.Data = new { success = false, message = string.Format(PicSizeError, e.pic.Name) };
                return result;
            }
            catch (Exception e)
            {
                result.Data = new { success = false, message = e.Message };
                return result;
            }
        }

        [HttpPost]
        public JsonResult PicDelete(long productID, long picID)
        {
            JsonResult result = new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            if (picID == 0)
            {
                return result;
            }
            else
            {
                ProductPicture _PicModel = svc.SelectOrEmpty(
                    new ProductPictureQuery()
                    {
                        IDs = new long[] { picID }
                    }).FirstOrDefault();
                if (_PicModel != null)
                {
                    _pictureService.Delete(_PicModel);
                    svc.Delete(_PicModel);
                    result.Data = new { Success = true };
                }
                else
                {
                    result.Data = new { Success = false };
                }
                return result;
            }
        }

        [HttpPost]
        public JsonResult ScenePic(long id)
        {
            var response=new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                
            };
            var exist = Service.Select(new ProductSceneQuery()
            {
                OwnerID = OwnerID,
                ProductID = id,
            }).ToArray();
            if (exist.Any())
            {
                var productscene = exist.FirstOrDefault();
                var scene = Service.Select(new SceneQuery()
                {
                    IDs = new[] {productscene.SceneID.Value},
                }).FirstOrDefault();
                if (scene == null)
                {
                    response.Data = new {success = false, msg = "场景不存在"};
                }
                response.Data = new {success = true, path=System.IO.Path.Combine(ftp,"Scene",OwnerID.ToString(),scene.WechatSceneID+".jpg")};
            }
            Service.Create(new ProductScene()
            {
                ProductID = id,
                OwnerID = OwnerID,
            });
            return response;
        }

        private Collection<Specification> GetSpecifications(Product product)
        {
            int count = Convert.ToInt32(Request.Form["RowsCount"]);
            var response = new Collection<Specification>();
            //默认规格
            Specification defaultS = new Specification()
                {
                    ID = Convert.ToInt32(Request.Form["id0"]),
                    ProductName = product.Name,
                    Name = "",
                    Price = !String.IsNullOrEmpty(Request.Form["price0"]) ? Convert.ToDecimal(Request.Form["price0"]) : 0,
                    Stock = !String.IsNullOrEmpty(Request.Form["stock0"]) ? Convert.ToInt32(Request.Form["stock0"]) : 0,
                    ReferencePrice = 0,
                    WarningStock = 0,
                    IsDefault = true,
                    IsShow = count == 0,
                    OwnerID = product.OwnerID,
                    ProductID = product.ID,
                };
            response.Add(defaultS);
            for (int i = 1; i < count + 1; i++)
            {
                Specification specification = new Specification()
                {
                    ID = Convert.ToInt32(Request.Form["id" + i]),
                    ProductName = product.Name,
                    Name = Request.Form["name" + i],
                    Price = !String.IsNullOrEmpty(Request.Form["price" + i]) ? Convert.ToDecimal(Request.Form["price" + i]) : 0,
                    Stock = !String.IsNullOrEmpty(Request.Form["stock" + i]) ? Convert.ToInt32(Request.Form["stock" + i]) : 0,
                    ReferencePrice = 0,
                    WarningStock = 0,
                    IsDefault = false,
                    IsShow = Convert.ToBoolean(Request.Form["isShow" + i]),
                    OwnerID = product.OwnerID,
                    ProductID = product.ID,
                };
                response.Add(specification);
            }
            return response;
        }

        private Collection<ProductPicture> GetUploadPictures(long productID)
        {
            Collection<ProductPicture> response = new Collection<ProductPicture>();
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var picFile = Request.Files[i];
                ProductPicture pic = new ProductPicture();
                byte[] buffer = new byte[picFile.ContentLength];
                picFile.InputStream.Read(buffer, 0, picFile.ContentLength);
                pic.Content = buffer;
                pic.OriginName = Path.GetFileName(picFile.FileName);
                pic.Name = GetHashName(pic.OriginName);
                pic.Path = MakePath(productID);
                pic.IsFirst = false;
                pic.ProductID = productID;
                response.Add(pic);
            }
            return response;
        }

        private Collection<ProductPicture> GetPictures(Product product)
        {
            int count = Convert.ToInt32(Request.Form["PicCount"]);
            Collection<ProductPicture> pictures = new Collection<ProductPicture>();
            for (int i = 0; i < count; i++)
            {
                if (!string.IsNullOrEmpty(Request.Form["picId" + i]))
                {
                    ProductPicture pic = new ProductPicture()
                    {
                        ID = Convert.ToInt32(Request.Form["picId" + i]),
                        Name = GetHashName(Request.Form["picName" + i]),
                        OriginName = Request.Form["picName" + i],
                        Path = Request.Form["picPath" + i],
                        IsFirst = Convert.ToBoolean(Request.Form["picIsFirst" + i]),
                        OwnerID = product.OwnerID,
                        ProductID = product.ID,
                    };
                    pictures.Add(pic);
                }

            }
            return pictures;
        }

        private string GetHashName(string name)
        {
            var extension = Path.GetExtension(name);
            return name.Hash() + extension;
        }

        private long GetOwnerID()
        {
            return OwnerID;
        }

        [HttpPost]
        public JsonResult Shelve(bool doShelve)
        {
            var requestIDs = Request.Form.GetValues("ids[]");
            if (requestIDs != null)
            {
                long[] ids = requestIDs.Select(p => Convert.ToInt64(p)).ToArray();
                var product = ids.Select(o => new Product() { ID = o, Shelved = doShelve }).ToArray();
                svc.Update(product);
                return new JsonResult()
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { IDs = ids, IsShelve = doShelve },
                    };
            }
            return new JsonResult();
        }

        [HttpPost]
        public JsonResult SetTop(bool isTop)
        {
            var requestIDs = Request.Form.GetValues("ids[]");
            if (requestIDs != null)
            {
                long[] ids = requestIDs.Select(p => Convert.ToInt64(p)).ToArray();
                var product = ids.Select(o => new Product() { ID = o, IsTop = isTop }).ToArray();
                svc.Update(product);
                return new JsonResult()
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { IDs = ids, IsTop = isTop },
                    };
            }
            return new JsonResult();
        }

        [HttpPost]
        public JsonResult SetFirst(long productID, long firstID)
        {
            var response = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            if (firstID == 0)
                return response;
            var oldFirst = svc.Select(new ProductPictureQuery() { ProductIDs = new long[] { productID }, IsFirst = true }).FirstOrDefault();
            if (oldFirst != null)
            {
                oldFirst.IsFirst = false;
                svc.Update(oldFirst);
            }
            var newFirst = svc.Select(new ProductPictureQuery() { IDs = new long[] { firstID } }).FirstOrDefault();
            if (newFirst != null)
            {
                newFirst.IsFirst = true;
                svc.Update(newFirst);
                response.Data = new { Success = true };
            }
            return response;

        }


        private string MakePath(long productID)
        {
            string path = "";
            //根
            path = Path.Combine("Picture", GetOwnerID().ToString());
            //sb.Append("/Picture/").Append(GetOwnerID()).Append("/");
            if (productID == 0)
            {
                path = Path.Combine("Picture", GetOwnerID().ToString(), "Temp");
            }
            else
            {
                path = Path.Combine("Picture", GetOwnerID().ToString(), productID.ToString());
            }
            return path;
        }

        private Dictionary<long, string> Makenames(Product product, PictureSize size)
        {
            Dictionary<long, string> Names = new Dictionary<long, string>();
            if (product != null)
                foreach (var pic in product.ProductPictures)
                {
                    Names.Add(pic.ID.Value, _pictureService.GetName(pic, size));
                }
            return Names;
        }

        private List<string> GetItemPath(IEnumerable<Product> products)
        {
            var picList = new List<string>();
            foreach (var product in products)
            {
                string picpath = "";
                if (product.ProductPictures != null)
                {
                    picpath = product.ProductPictures.Where(o => o.IsFirst != null && o.IsFirst.Value)
                            .Select(o => Path.Combine(ftp, o.Path, _pictureService.GetName(o, ListSize))).FirstOrDefault();
                }
                picList.Add(picpath);
            }
            return picList;
        }

        private IEnumerable<Tag> GetTags(long productID)
        {
            var productTags = svc.SelectOrEmpty(new DBC.WeChat.Models.Sales.ProductTagQuery()
            {
                ProductID = productID,
            });
            var tags = svc.SelectOrEmpty(new TagQuery()
            {
                IDs = productTags.Select(o => o.TagID).OfType<long>().ToArray(),
            });
            return tags.ToArray();
        }
    }
}
