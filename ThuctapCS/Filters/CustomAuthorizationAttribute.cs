using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ThuctapCS.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedRoles;

        public CustomAuthorizeAttribute(params string[] roles)
        {
            this.allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session["Role"] == null)
            {
                return false; // Chưa đăng nhập
            }

            var userRole = httpContext.Session["Role"].ToString();
            return allowedRoles.Contains(userRole); // Kiểm tra vai trò người dùng
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/Login/Login"); // Chuyển hướng tới trang đăng nhập nếu không có quyền
        }
    }
}