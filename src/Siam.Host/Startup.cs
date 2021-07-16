using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platformex.Application;
using Platformex.Web.GraphQL;
using Platformex.Web.Swagger;
using Siam.Application;
using Siam.Data.MemoContext;

namespace Siam.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // kestrel
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // IIS
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            
            services.AddControllers();

            var connectionString = Configuration.GetConnectionString("SqlServer");
            services.AddDbContext<MemoDbContext>(options =>  options.UseSqlServer(connectionString));
            services.AddScoped<IDbProvider<MemoModel>, MemoDbProvider>();
            
            services.AddSingleton<IApiDescriptionGroupCollectionProvider,
                                    CommandsApiDescriptionGroupCollectionProvider>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseHttpsRedirection();
            app.UsePlatformex();

        }
    }
}
