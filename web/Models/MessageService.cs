using LeanCloud;
using LeanCloud.Realtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace web.Models 
{
    public class MeeeageServicr
    {
        static MeeeageServicr inst;
        public static MeeeageServicr Inst
        {
            get
            {
                if (inst == null)
                {
                    inst = new MeeeageServicr();
                }
                return inst;
            }
        }
        AVRealtime m_StyRealtime;
        AVIMClient m_SysClient;
        const string SystemClientId = "68c22ec62f7ee809fcc9e7e6";

        public async Task<bool> CreateImClientAsync()
        {
            m_StyRealtime = new AVRealtime(LeanCloud.Engine.Cloud.Singleton.AppId,
                LeanCloud.Engine.Cloud.Singleton.AppKey);
            m_SysClient = await m_StyRealtime.CreateClientAsync(SystemClientId, tag: "StyemBroadcast");
            return true;
        }
        //public async Task<bool> SendToSingleUser(string targetUserId, Dictionary<string, object> content)
        //{
        //    try
        //    {
        //        //AVIMConversation conversation2 = await m_SysClient.GetConversationAsync(SystemConversationId);
        //        AVIMConversation conversation = await m_SysClient.CreateConversationAsync(member: targetUserId, isSystem: true, isUnique: true);

        //        // 发送消息
        //        var message = new AVIMTextMessage("StyMessage");
        //        //message.Content = Json.Encode(content);

        //        //await conversation.SendAsync(message);
        //        return true;
        //    }
        //    catch (LCException ex)
        //    {
        //        LCLogger.Debug($"给用户 {targetUserId} 发送消息失败: {ex.Message}");
        //        return false;
        //    }
        //}
    }
}
