using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DontPanicAdventures.Startup))]
namespace DontPanicAdventures
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
