using LeanCloud;
using LeanCloud.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace web {
    public class Program 
    {
        public static async Task Main(string[] args)
        {
            //LeanCloudConfig.InitializeFromEnvironment();
            //CreateHostBuilder(args).Build().Run();

            ////// �ӻ���������ȡ������Ϣ
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string appUrl = Environment.GetEnvironmentVariable("APP_URL");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");


            LCCore.UseMasterKey = true;
            LCCore.Initialize(appId, appKey, appUrl, masterKey);
            LCLogger.Debug("LCCore.Initialize Success");


            await CreateHostBuilder(args).Build().RunAsync();
            // ��ʼ��IM����
            App._httpClient = new HttpClientIMService(appId, appKey, appUrl);
            //await imService.SendToUser("68c22ec62f7ee809fcc9e7e6", "68b9286c49adb47c41678afb", "�������Ϣ����");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => 
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
