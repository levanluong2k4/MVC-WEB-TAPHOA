using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(web_levanluong_64131236.Startup))]
namespace web_levanluong_64131236
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
