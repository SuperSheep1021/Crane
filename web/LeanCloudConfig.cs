using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Storage.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace web
{
    public static class LeanCloudConfig
    {
        
        const string SystemClientId = "68c22ec62f7ee809fcc9e7e6";
        static AVIMClient m_SysClient;
        static AVRealtime m_StyRealtime;
        public static void InitializeFromEnvironment()
        {
            ////// �ӻ���������ȡ������Ϣ
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");
            string appUrl = Environment.GetEnvironmentVariable("APP_URL");

            AVClient.Initialize(appId, appKey, appUrl);
            AVClient.CurrentConfiguration.MasterKey = masterKey;
            AVClient.UseMasterKey = true;

            ////// ����Realtimeʵ��
            //m_StyRealtime = new AVRealtime(appId, appKey);
            //m_SysClient = await m_StyRealtime.CreateClientAsync(SystemClientId,tag:"StyemBroadcast");
            
            LCLogger.Debug($"Config Success!!!{appUrl}");
        }

        public static async Task<bool> SendToSingleUser(string targetUserId, Dictionary<string, object> content)
        {
            try
            {
                //AVIMConversation conversation2 = await m_SysClient.GetConversationAsync(SystemConversationId);
                AVIMConversation conversation = await m_SysClient.CreateConversationAsync(member: targetUserId,isSystem:true,isUnique:true);

                // ������Ϣ
                var message = new AVIMTextMessage("StyMessage");
                message.Content = Json.Encode(content);

                await conversation.SendAsync(message);
                return true;
            }
            catch (LCException ex)
            {
                LCLogger.Debug($"���û� {targetUserId} ������Ϣʧ��: {ex.Message}");
                return false;
            }
        }

    }
}