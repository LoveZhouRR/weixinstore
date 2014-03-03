using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.Models.Sales;
using System.ComponentModel;
using DBC.Ors.Models;

namespace DBC.WeChat.UI.Store.Models
{
    /// <summary>
    /// 会员信息汇总展示模型
    /// </summary>
    public class MemberMasterVM
    {
        //微信相关联的信息
        public string DisplayID { get; set; }
        public Fan WeChatMember { get; set; }
        public long? WeChatOrderCount { get; set; }
        //ICollection<SaleOrder> WeChatOrder { get; set; }


        //主系统会员信息
        public long? CustomerID { get; set; }
        public ICollection<string> CustomerCode { get; set; }
        public long? WebOrderCount { get; set; }
        public int? WebCreadit { get; set; }
        //ICollection<WebSaleOrder> WebOrder { get; set; }
    }

    /// <summary>
    /// 用户订单显示模型
    /// </summary>
    public class MemberSaleOrderVM
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public long? FanId { get; set; }
        /// <summary>
        /// 订单系统编号，便于关联数据
        /// </summary>
        public long? OrderId { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 收件人姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 收件人联系方式
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal? Amount { get; set; }
        /// <summary>
        /// 运费
        /// </summary>
        public decimal? DeliveryFee{get;set;}
        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 订单来源----微信商城、在线商城
        /// </summary>
        public string OrderSource { get; set; }
        /// <summary>
        /// 交易号
        /// </summary>
        public string TradeCode { get; set; }
        /// <summary>
        /// 支付类型,是否为货到付款
        /// </summary>
        public bool? IsCOD { get; set; }
        /// <summary>
        /// 订单状态编号
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 订单状态名称
        /// </summary>
        public string StatusName { get; set; }
        /// <summary>
        /// 配送方式
        /// </summary>
        public long? DeliveryMethodID { get; set; }
        /// <summary>
        /// 配送方式名称
        /// </summary>
        public string DeliveryMethodName { get; set; }
        /// <summary>
        /// 物流编号
        /// </summary>
        public string DeliveryCode { get; set; }
    }

    public class MemberSaleOrderVMQuery
    {
        public long? Skip { get; set; }
        public long? Take { get; set; }
    }

    /// <summary>
    /// 个人信息展示模型
    /// </summary>
    public class MemberPersonalVM
    {
        public Fan WeChatMember { get; set; }
        public WeChatPersonalExtra WeChatExtra { get; set; }
        public WebPersonalExtra WebExtra { get; set; }
    }

    /// <summary>
    /// 微信会员性别信息
    /// </summary>
    public enum MemberGender
    {
        /// <summary>
        /// 未知
        /// </summary>
        [Description("未知")]
        Unknown = 0,

        /// <summary>
        /// 男
        /// </summary>
        [Description("男")]
        Male = 1,

        /// <summary>
        /// 女
        /// </summary>
        [Description("女")]
        Female = 2
    }

    /// <summary>
    /// 微信会员基本信息
    /// </summary>
    public class WeChatPersonalExtra
    {
        public string NickName { get; set; }
        public int? Gender { get; set; }
        public string GenderName { get; set; }
        //地址信息
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public DateTime? FollowDate { get; set; }
    }

    /// <summary>
    /// 主系统会员扩展信息
    /// </summary>
    public class WebPersonalExtra
    {
        public long? CustomerSysNo { get; set; }
        public List<string> CustomerId { get; set; }
        public int? CustomerCredit { get; set; }
        public string CustomerSecret { get; set; }
        public string CustomerRank { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerMobile { get; set; }
    }

    /// <summary>
    /// 微信会员地址信息
    /// </summary>
    public class WeChatContactVM
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string AddressPrefix { get; set; }
        public Boolean? isDefault { get; set; }
    }

    /// <summary>
    /// 微信会员积分展示模型
    /// </summary>
    public class MemberCredit
    {
        public string CreditReason { get; set;}
        public int? CreditAmount { get; set; }
        public int? RemainAmount { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}