using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBC.Ors.Models;
using DBC.WeChat.Models.Conversation;

namespace DBC.WeChat.UI.Store.Models
{
    public class ReplyResourceItemVM :ReplyResourceItem
    {
        public string Path { get; set; }
    }
    public class KeywordReplyVM : AbstractModel
    {
        public string Name { get; set; }
        public ICollection<KeyWord> Keywords { get; set; }
        public bool? ReplyAll { get; set; }
        public ICollection<TextReplyItem> TextReply { get; set; }
        public ICollection<ReplyResourceItemVM> ResourceItems { get; set; }
        public long? ReplyID { get; set; }
        public long[] DeletedKeywords { get; set; }
        public long[] DeletedTextReplies { get; set; }
        public long[] DeletedResourceItems { get; set; }
    }
}