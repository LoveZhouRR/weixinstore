using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBC.Ors.Models;
using DBC.Ors.Services;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.Services.Components.Email;
using DBC.WeChat.Services.Components.Email.Senders;
using DBC.WeChat.Services.Infrastructures.Configurations;
using DBC.WeChat.Services.Security;

namespace DBC.WeChat.UI.Store.Models
{
    public class RegEmail
    {
        public string Account { get; set; }
        public string Returl { get; set; }
        public string Name { get; set; }
        public static void Send(Tenant t, HttpRequestBase request)
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            if (t.EmailVerified == true)
            {
                return;
            }

            var current =
                svc.SelectOrEmpty(new EmailVerifyQuery()
                    {
                        ReferID = t.ID.Value,
                        Type = (int) EmailVerifyTypes.TenantRegister,
                        OrderDirection = OrderDirection.Desc,
                        OrderField = "ID"
                    }).FirstOrDefault();
            if (current != null)
            {
                if (current.CreatedAt != null && (current.CreatedAt - DateTime.Now) < new TimeSpan(0, 10, 0))
                {
                    throw new RuleViolatedException("请10分钟后再试");
                }
                Guardian.Invoke(() => svc.Delete(current));
            }
            
            var verify = new EmailVerify()
                {
                    Code = Guid.NewGuid().ToString().Hash(),
                    ReferID = t.ID,
                    Type = (int)EmailVerifyTypes.TenantRegister,
                    Email = t.Account,
                    Name = t.Name
                };
            var uri = new UriBuilder("http", request.Url.Host, request.Url.Port, "BindEmail",
                                     "?code=" + verify.Code);
            verify.Returl = uri.ToString();
            Guardian.Invoke(() => svc.Create(verify));
        }
    }
}