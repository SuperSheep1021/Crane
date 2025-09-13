using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace web {
    public class Program 
    {
        public static async Task Main(string[] args)
        {
            //LeanCloudConfig.InitializeFromEnvironment();
            //CreateHostBuilder(args).Build().Run();

            ////// 从环境变量获取配置信息
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string appUrl = Environment.GetEnvironmentVariable("APP_URL");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");

            //LCCore.UseMasterKey = true;
            //LCCore.Initialize(appId, appKey, appUrl, masterKey);
            //LCStorage.Initialize(appId, appKey, appUrl, masterKey);
            //LCCore.PersistenceController = new PersistenceController(new UnityPersistence());
            LCApplication.Initialize(appId, appKey, appUrl, masterKey);
            LCLogger.Debug($"LCApplication.Initialize Success URL:{appUrl}");

            var host = CreateHostBuilder(args).Build();

            await IMService.Inst.InitialtionIM();


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
