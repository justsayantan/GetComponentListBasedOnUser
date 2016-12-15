using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GetComponent.Startup))]
namespace GetComponent
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
