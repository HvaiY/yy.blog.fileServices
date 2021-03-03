using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperExtend;
using yy.blog.file.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;

namespace yy.blog.file
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
            //services.AddHttpsRedirection(options => {
            //    options.HttpsPort = 443;
            //    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            //});
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.ForwardedHeaders =
            //        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            //}); 
            services.AddControllers();
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});
            var connection = Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
            services.AddDapperMysqlDI(options =>
            {
                options.ConnectionString = connection;
            });

            services.AddScoped<IFileService, FileService>();

            // 允许大文件上传
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = long.MaxValue;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "yy.blog.file", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "yy.blog.file v1"));
            }
            else {
                // 生产环境暴露swagger 
                // 访问  /swagger/index.html
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "yy.blog.file v1"));
            }

            // 服务展示不使用 https
            //app.UseForwardedHeaders();
            //app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();
            //app.Use(async (context, next) =>
            //{
            //    //Do something here
               
            //    //Invoke next middleware
            //    await next.Invoke();

            //    //Do something here

            //});
            app.UseEndpoints(endpoints =>
            {
                
                endpoints.MapControllers();
               
                //endpoints.MapControllerRoute(
                //   name: "default",
                //   pattern: "swagger/index.html");
                //endpoints.MapControllerRoute(
                //   name: "default",
                //   pattern: "api/{controller=File}/{action=Index}/{id?}");
            });
        }
    }
}
