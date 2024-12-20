using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace web_levanluong_64131236
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}"); // Đặt IgnoreRoute lên đầu

        

            routes.MapRoute(
                name: "ProductsByCategory",
                url: "danh-muc/{id}",
                defaults: new { controller = "LoaiHangs", action = "ShowProducts" }
            );

            // Route mặc định đặt cuối cùng
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
