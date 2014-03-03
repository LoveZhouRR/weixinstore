using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBC.WeChat.Models.Infrastructures;

namespace DBC.WeChat.UI.Store.Models
{
    public class HomePageVM
    {
        public TimeSpan UsedTime { get; set; }
        public int TradeCount { get; set; }
        public TenantAccountType Type { get; set; }
        public bool EmailVerified { get; set; }
        public bool MobileVerified { get; set; }
        public bool IdentityVerified { get; set; }
        public bool WeChatAccountBinded { get; set; }
        public bool AlipayAccountBinded { get; set; }
        public string Account { get; set; }
    }
}