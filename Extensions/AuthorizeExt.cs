using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ketnoicungcau.vn.Extensions
{
    public class AuthorizeExt : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {

            var companyid = context.HttpContext.Session.GetObjectFromJson<long?>(ConfigConstants.SS_LOGIN_WEB);
            if (companyid == null)
            {
                var cookie = GetCookie(context, WebHelpers.EncryptMD5(ConfigConstants.COOKIE_LOGIN_WEB));
                if (!string.IsNullOrEmpty(cookie))
                {
                    try
                    {
                        companyid = Newtonsoft.Json.JsonConvert.DeserializeObject<long?>(WebHelpers.DecryptData(cookie, true));
                        context.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_LOGIN_WEB, companyid);
                        var crUrl = context.HttpContext.Request.Path.HasValue ? context.HttpContext.Request.Path.Value : "";
                        if (context.HttpContext.Request.QueryString.HasValue && !string.IsNullOrEmpty(context.HttpContext.Request.QueryString.Value))
                            crUrl += context.HttpContext.Request.QueryString.Value;
                        context.Result = new RedirectResult(!string.IsNullOrEmpty(crUrl) ? crUrl : "/profile");
                    }
                    catch (Exception ex)
                    {
                        //throw;
                    }
                }
                else
                {
                    if (context.HttpContext.Request.IsAjaxRequest())
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    else
                        context.Result = new RedirectResult("/dang-nhap");
                }
            }
        }
        private string GetCookie(ActionExecutingContext context, string cookie_name)
        {
            return context.HttpContext.Request.Cookies[cookie_name] != null
                ? context.HttpContext.Request.Cookies[cookie_name].ToString()
                : "";
        }
    }
}
