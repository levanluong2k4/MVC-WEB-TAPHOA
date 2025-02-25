﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace web_levanluong_64131236.Models
{
    public class AdminAuthorizeAttribute: AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var admin = httpContext.Session["Admin"];
            return admin != null && (bool)admin;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/QuanTris_64131236/Login");
        }
    }
}