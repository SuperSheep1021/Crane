using System;
using LeanCloud;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace web
{
    public static class LeanCloudConfig
    {
        /// <summary>
        /// 从环境变量初始化 LeanCloud 配置
        /// </summary>
        public static void InitializeFromEnvironment()
        {
            // 从环境变量获取配置信息
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");
            string appUrl = Environment.GetEnvironmentVariable("APP_URL");

            AVClient.Initialize(appId, appKey, appUrl);
            AVClient.CurrentConfiguration.MasterKey = masterKey;
            AVClient.UseMasterKey = true;
            Console.WriteLine("配置成功!!!");
        }

    }
}