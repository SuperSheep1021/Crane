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
        /// �ӻ���������ʼ�� LeanCloud ����
        /// </summary>
        public static async Task InitializeFromEnvironment()
        {
            // �ӻ���������ȡ������Ϣ
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");
            string appUrl = Environment.GetEnvironmentVariable("APP_URL");

            AVClient.Initialize(appId, appKey, appUrl);
            AVClient.CurrentConfiguration.MasterKey = masterKey;
            AVClient.UseMasterKey = true;

            ////// ����Realtimeʵ��
            //AVRealtime realtime = new AVRealtime(appId, appKey);
            //AVIMClient client =await realtime.CreateClientAsync("123123");

            ////// ʹ���û���¼��������������û��Ự��
            ////var user = await AVUser.LoginBySessionToken("�û���SessionToken");
            ////var imClient = realtime.CreateClientAsync(user);

            LCLogger.Debug("Config Success!!!");
        }

    }
}