using ketnoicungcau.business.Caching;
using ketnoicungcau.business.Service;
using ketnoicungcau.business.Service.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ketnoicungcau.business.Model;
using ketnoicungcau.business.ServiceWeb;
using ketnoicungcau.business.ServiceWeb.Interface;
using Microsoft.EntityFrameworkCore;
using ketnoicungcau.business.Framework;
using ketnoicungcau.business.UI;
using ketnoicungcau.business.infrastructure;
using ketnoicungcau.business.Framework.Infrastructure.Extensions;
using Microsoft.Extensions.FileProviders;
using System.IO;
using ketnoicungcau.business.Helpers.Interface;
using ketnoicungcau.business.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using ketnoicungcau.business.Framework.Middleware;

namespace ketnoicungcau.vn
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
            services.AddControllersWithViews();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });

            services.ConfigureApplicationServices(Configuration);

            services.Configure<MemoryDistributedCacheOptions>(Configuration);

            services.AddHttpContextAccessor();

            DependencyInjection(services);

            services.AddDbContext<dbContext>(option => option.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", "images")),
                RequestPath = new PathString("/MyImages")
            });

            app.UseAntiXssMiddleware();

            app.UsePageNotFound();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    "NotFound",
                    "404",
                        new { controller = "Common", action = "NotFound" }
                    );

                endpoints.MapControllerRoute(
                   "Tim-kiem",
                   "tim-kiem",
                        new { controller = "Search", action = "Index" }
                    );
                endpoints.MapControllerRoute(
                   "FAQ",
                   "hoi-dap",
                        new { controller = "FAQ", action = "Index" }
                    );

                //endpoints.MapControllerRoute(
                //   "DanhSachTintuc",
                //   "tin-tuc",
                //        new { controller = "News", action = "Index" }
                //    );

                //endpoints.MapControllerRoute(
                //   "DanhSachTintuc2",
                //   "tin-tuc/{CategoryUrl}",
                //        new { controller = "News", action = "Index" },
                //        new { CategoryUrl = @"^(\w|-|\d)+$" }
                //    );

                //endpoints.MapControllerRoute(
                //   "ChiTietTinTuc",
                //   "tin-tuc/{NewsUrl}-{NewsId}",
                //        new { controller = "News", action = "Detail" },
                //        new { NewsUrl = @"^(\w|-|\d)+$", NewsId = @"^\d$" }
                //    );

                endpoints.MapControllerRoute(
                    "Product-Detail-V1",
                    "sp-{ProductId}",
                        new { controller = "Product", action = "Index" }
                    );

                endpoints.MapControllerRoute(
                   "Product-Detail-V2",
                   "san-pham-{ProductId}",
                       new { controller = "Product", action = "Index" }
                   );

                endpoints.MapControllerRoute(
                    "Product-Detail-V3",
                    "{CategoryUrl}/{ProductUrl}-{ProductId}",
                        new { controller = "Product", action = "Index" },
                        new { CategoryUrl = @"^(\w|-|\d)+$", ProductUrl = @"^(\w|-|\d)+$" }
                   );

                endpoints.MapControllerRoute(
                    "Cate",
                    "{ProductType}",
                       new { controller = "Category", action = "Index" },
                       new { ProductType = @"(chao-ban|chao-mua)" }
                   );

                endpoints.MapControllerRoute(
                    "CateSell",
                    "{CategoryUrl}-chao-ban",
                       new { controller = "Category", action = "Sell" },
                       new { CategoryUrl = @"^(\w|-|\d)+$" }
                   );

                endpoints.MapControllerRoute(
                    "CateBuy",
                    "{CategoryUrl}-chao-mua",
                       new { controller = "Category", action = "Buy" },
                       new { CategoryUrl = @"^(\w|-|\d)+$" }
                   );

                endpoints.MapControllerRoute(
                    "CateSell",
                    "{CategoryUrl}",
                       new { controller = "Category", action = "Index" },
                       new { CategoryUrl = @"^(\w|-|\d)+$" }
                   );
            });

            app.ConfigureRequestPipeline();

        }

        private void DependencyInjection(IServiceCollection services)
        {
            services.AddTransient<ILoggerService, LoggerService>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IMwgFileProvider, MwgFileProvider>();
            services.AddScoped<IPageHeadBuilder, PageHeadBuilder>();
            services.AddScoped<IWorkContext, WebWorkContext>();

            services.AddSingleton<ICache, ManagerCache>();

            services.AddTransient<IUserHelpers, UserHelpers>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<INewsService, NewsService>();
            services.AddTransient<IBannerService, BannerService>();
            services.AddTransient<IUtilitiesService, UtilitiesService>();
            services.AddTransient<ICompanyService, CompanyService>();

            services.AddTransient<IUtilitiesServiceWeb, UtilitiesServiceWeb>();
            services.AddTransient<IBannerServiceWeb, BannerServiceWeb>();
            services.AddTransient<ICategoryServiceWeb, CategoryServiceWeb>();
            services.AddTransient<ICompanyServiceWeb, CompanyServiceWeb>();
            services.AddTransient<IProductServiceWeb, ProductServiceWeb>();
            services.AddTransient<ISearchServiceWeb, SearchServiceWeb>();
            services.AddTransient<INewsServiceWeb, NewsServiceWeb>();
            services.AddTransient<IFAQServiceWeb, FAQServiceWeb>();
            services.AddTransient<ISystemService, SystemService>();
        }
    }
}
