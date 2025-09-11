using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace web
{
    public static class LeanCloudConfig
    {
        public static void InitializeFromEnvironment()
        {
            ////// 从环境变量获取配置信息
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");
            string appUrl    = Environment.GetEnvironmentVariable("APP_URL");

            AVClient.Initialize(appId, appKey, appUrl);
            AVClient.CurrentConfiguration.MasterKey = masterKey;
            AVClient.UseMasterKey = true;

            LCLogger.Debug($"Config Success!!!{appUrl}");
        }
        

        

    }
}