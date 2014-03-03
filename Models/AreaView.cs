using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBC.WeChat.UI.Store.Models
{
    public class AreaView
    {
        public long? ID { get; set; }
        public string Name { get; set; }
        public long? Parent { get; set; }
    }

}