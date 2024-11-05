using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Migration_Tool_GraphAPI.Startup))]

namespace Migration_Tool_GraphAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}