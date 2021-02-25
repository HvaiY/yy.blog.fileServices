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
                    // �������ļ��ϴ�����
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        //����Ӧ�÷�����Kestrel���������Ϊ50MB  52428800 ,
                        options.Limits.MaxRequestBodySize = long.MaxValue;
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
