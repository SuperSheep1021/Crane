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
            ////// 从环境变量获取配置信息
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");
            string appUrl = Environment.GetEnvironmentVariable("APP_URL");

            AVClient.Initialize(appId, appKey, appUrl);
            AVClient.CurrentConfiguration.MasterKey = masterKey;
            AVClient.UseMasterKey = true;

            ////// 创建Realtime实例
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

                // 发送消息
                var message = new AVIMTextMessage("StyMessage");
                message.Content = Json.Encode(content);

                await conversation.SendAsync(message);
                return true;
            }
            catch (LCException ex)
            {
                LCLogger.Debug($"给用户 {targetUserId} 发送消息失败: {ex.Message}");
                return false;
            }
        }

    }
}