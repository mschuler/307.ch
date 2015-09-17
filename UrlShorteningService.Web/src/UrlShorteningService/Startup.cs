using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;
using UrlShorteningService.Services;
using UrlShorteningService.Services.Impl;

namespace UrlShorteningService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<IRepository, Repository>();
            services.AddSingleton<IBase58Converter, Base58Converter>();
            services.AddSingleton<IIdGenerator, IdGenerator>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
