using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

                        //options.ListenAnyIP(443, listenOptions =>
                        //{
                        //    //listenOptions.UseHttps(Path.Combine(AppContext.BaseDirectory, "5248918_www.yuanlonghai.com.pfx"), "RoP1eifb");
                        //    listenOptions.UseHttps(Path.Combine(AppContext.BaseDirectory, "server.pfx"), "linezero");
                        //    //listenOptions.UseHttps("server.pfx","dahai");
                        //});

                    });
                    webBuilder.UseStartup<Startup>();
                    //.UseUrls("http://*:5000;https://*:5001");
                });

    }
}
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;

//namespace yy.blog.file
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    // ¿ªÆô´óÎÄ¼þÉÏ´«ÏÞÖÆ
//                    webBuilder.ConfigureKestrel((context, options) =>
//                    {
//                        //ÉèÖÃÓ¦ÓÃ·þÎñÆ÷KestrelÇëÇóÌå×î´óÎª50MB  52428800 ,
//                        options.Limits.MaxRequestBodySize = long.MaxValue;
//                    });
//                    webBuilder.UseStartup<Startup>();
//                   //.UseUrls("https://*:80");
//                });

//    }
//}