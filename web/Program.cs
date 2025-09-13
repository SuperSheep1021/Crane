using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace web {
    public class Program 
    {
        static string StyClientID = "68c22ec62f7ee809fcc9e7e6";
        static string StyConversationID = "68c3e5ce16ec9e2c7d1396c0";
        public static async Task Main(string[] args)
        {
            //LeanCloudConfig.InitializeFromEnvironment();
            //CreateHostBuilder(args).Build().Run();

            ////// 从环境变量获取配置信息
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string appUrl = Environment.GetEnvironmentVariable("APP_URL");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");


            LCCore.UseMasterKey = true;
            LCCore.Initialize(appId, appKey, appUrl, masterKey);
            LCLogger.Debug($"LCCore.Initialize Success URL:{appUrl}");

            var host = CreateHostBuilder(args).Build();
            LCIMClient client = new LCIMClient(StyClientID, tag:"sys");
            LCLogger.Debug($"创建系统客户端成功:{client.Tag}");
            LCIMConversation conversation = await client.GetConversation(StyConversationID);
            LCLogger.Debug($"创建系统会话成功:{conversation.Name}");
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
