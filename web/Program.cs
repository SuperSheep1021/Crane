using LeanCloud;
using LeanCloud.Common;
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

            ////// �ӻ���������ȡ������Ϣ
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string appUrl = Environment.GetEnvironmentVariable("APP_URL");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");

            LCCore.Initialize(appId, appKey, appUrl, masterKey);
            LCLogger.Debug("LCCore.Initialize Success");



            // ��ȡ��Ϣ��������
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
