using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yy.blog.file
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // 开启大文件上传限制
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        //设置应用服务器Kestrel请求体最大为50MB  52428800 ,
                        options.Limits.MaxRequestBodySize = long.MaxValue;
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
