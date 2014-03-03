using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBC.WeChat.UI.Store.Models
{
    public class AreaVM
    {
        public string text;
        public int check;
        public long pid;
        public AreaVM[] children;
        public long id;
    }
}