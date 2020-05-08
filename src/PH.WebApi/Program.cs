using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;

namespace PH.WebApi
{
    using PH.UnitOfWork.UnitOfWork;
    using PH.WebApi.Initialize;
    using PH.DbContexts;
    using WebCore.Logger;

    public class Program
    {
        public static void Main(string[] args)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            try
            {
                var host = CreateHostBuilder(args).Build();

                logger.Trace("网站启动中...");

                using (IServiceScope scope = host.Services.CreateScope())
                {
                    logger.Trace("初始化NLog");

                    //确保NLog.config中连接字符串与appsettings.json中同步
                    NLogExtensions.EnsureNlogConfig("NLog.config", "mssql", scope.ServiceProvider.GetRequiredService<IConfiguration>().GetSection("ConnectionStrings:PHDbContext").Value);

                    logger.Trace("初始化数据库");

                    //初始化数据库
                    DBSeed.Initialize(scope.ServiceProvider.GetRequiredService<IUnitOfWork<PHDbContext>>());

                    //for test -start
                    //用于查看彩色控制台样式，以及日志等级过滤
                    logger.Trace("Test For Trace");
                    logger.Debug("Test For Debug");
                    logger.Info("Test For Info");
                    logger.Warn("Test For Warn");
                    logger.Error("Test For Error");
                    logger.Fatal("Test For Fatal");
                    //for test -end
                }
                logger.Trace("网站启动完成");

                host.Run();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "网站启动失败");
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).AddNlogService();
    }
}
