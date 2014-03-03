using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBC.WeChat.Models.Infrastructures;

namespace DBC.WeChat.UI.Store.Models
{
    public class StoreVM
    {
        //public long BannerPictureID { get; set; }
        //public Picture BannerPicture { get; set; }

        public ICollection<BannerPicture> BannerPictures { get; set; }
        public string Announcement { get; set; }
        public string StoreName { get; set; }
    }
}