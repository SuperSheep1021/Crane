using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;


namespace web
{
    public static class LeanCloudConfig
    {
        /// <summary>
        /// 从环境变量初始化 LeanCloud 配置
        /// </summary>
        public static async Task InitializeFromEnvironment()
        {
            // 从环境变量获取配置信息
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");
            string appUrl = Environment.GetEnvironmentVariable("APP_URL");

            AVClient.Initialize(appId, appKey, appUrl);
            AVClient.CurrentConfiguration.MasterKey = masterKey;
            AVClient.UseMasterKey = true;

            ////// 创建Realtime实例
            //AVRealtime realtime = new AVRealtime(appId, appKey);
            //AVIMClient client =await realtime.CreateClientAsync("123123");

            ////// 使用用户登录（这里假设已有用户会话）
            ////var user = await AVUser.LoginBySessionToken("用户的SessionToken");
            ////var imClient = realtime.CreateClientAsync(user);

            LCLogger.Debug("Config Success!!!");
        }

    }
}