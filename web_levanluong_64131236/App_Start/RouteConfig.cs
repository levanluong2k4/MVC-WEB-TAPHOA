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
                name: "Products",
                url: "san-pham/{id}",
                defaults: new { controller = "HangHoas_64131236", action = "ProductList" }
            );
            routes.MapRoute(
    name: "ProductSearch",
    url: "HangHoas_64131236/GetSearchSuggestions",
    defaults: new { controller = "HangHoas_64131236", action = "GetSearchSuggestions" }
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
