using System;
using LeanCloud;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace web
{
    public static class LeanCloudConfig
    {
        /// <summary>
        /// �ӻ���������ʼ�� LeanCloud ����
        /// </summary>
        public static void InitializeFromEnvironment()
        {
            // �ӻ���������ȡ������Ϣ
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");


            AVClient.Initialize(appId, appKey);
            AVClient.CurrentConfiguration.MasterKey = masterKey;
            AVClient.UseMasterKey = true;
            Console.WriteLine(AVClient.CurrentConfiguration.MasterKey);
        }

    }
}