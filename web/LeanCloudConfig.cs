using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace web
{
    public static class LeanCloudConfig
    {
        const string SystemClientId = "68c22ec62f7ee809fcc9e7e6";
        static AVRealtime m_StyRealtime;
        static AVIMClient m_SysClient;
        public static void InitializeFromEnvironment()
        {
            ////// �ӻ���������ȡ������Ϣ
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");
            string appUrl    = Environment.GetEnvironmentVariable("APP_URL");

            AVClient.Initialize(appId, appKey, appUrl);
            AVClient.CurrentConfiguration.MasterKey = masterKey;
            AVClient.UseMasterKey = true;

            LCLogger.Debug($"Config Success!!!{appUrl}");
        }
        public static async Task CreateImClientAsync()
        {
            m_StyRealtime = new AVRealtime(LeanCloud.Engine.Cloud.Singleton.AppId, 
                LeanCloud.Engine.Cloud.Singleton.AppKey);
            m_SysClient= await m_StyRealtime.CreateClientAsync(SystemClientId, tag: "StyemBroadcast");
        }

        //public static async Task<bool> SendToSingleUser(string targetUserId, Dictionary<string, object> content)
        //{
        //    if (m_SysClient == null)
        //    {
        //        m_SysClient = await m_StyRealtime.CreateClientAsync(SystemClientId, tag: "StyemBroadcast");
        //    }
        //    if (m_SysClient == null)
        //    {
        //        return false;
        //    }

        //    try
        //    {
        //        //AVIMConversation conversation2 = await m_SysClient.GetConversationAsync(SystemConversationId);
        //        AVIMConversation conversation = await m_SysClient.CreateConversationAsync(member: targetUserId, isSystem: true, isUnique: true);

        //        // ������Ϣ
        //        var message = new AVIMTextMessage("StyMessage");
        //        message.Content = Json.Encode(content);

        //        await conversation.SendAsync(message);
        //        return true;
        //    }
        //    catch (LCException ex)
        //    {
        //        LCLogger.Debug($"���û� {targetUserId} ������Ϣʧ��: {ex.Message}");
        //        return false;
        //    }
        //}

    }
}