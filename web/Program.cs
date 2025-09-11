using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace web {
    public class Program {
        public static async Task Main(string[] args)
        {
            //LeanCloudConfig.InitializeFromEnvironment();
            //CreateHostBuilder(args).Build().Run();

            var host = CreateHostBuilder(args).Build();
            // 获取消息服务并连接
            var messageService = host.Services.GetRequiredService<MessageService>();
            await messageService.ConnectAsync();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => 
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
