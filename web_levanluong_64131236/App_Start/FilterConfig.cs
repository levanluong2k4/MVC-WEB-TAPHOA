﻿using System.Web;
using System.Web.Mvc;

namespace web_levanluong_64131236
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
