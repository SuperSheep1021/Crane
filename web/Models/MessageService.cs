using LeanCloud;
using LeanCloud.Realtime;
using LeanCloud.Storage.Internal;
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
        
        const string SystemClientId = "68c22ec62f7ee809fcc9e7e6";

        public async Task<bool> SendToSingleUser(string targetUserId, Dictionary<string, object> content)
        {
            AVRealtime styRealtime = new AVRealtime(LeanCloud.Engine.Cloud.Singleton.AppId,
                LeanCloud.Engine.Cloud.Singleton.AppKey);
            AVIMClient sysClient = await styRealtime.CreateClientAsync(SystemClientId, tag: "StyemBroadcast");

            AVIMConversation conversation = await sysClient.CreateConversationAsync(members: new List<string> { "targetUserId" }, isSystem: true);




            ////    // 发送消息
            //var message = new AVIMTextMessage("StyMessage");
            //message.Content = Json.Encode(content);
            //await conversation.SendAsync(message);

            //try
            //{
            //    AVIMConversation conversation = await m_SysClient.CreateConversationAsync(member: targetUserId, isSystem: true, isUnique: true);

            //    // 发送消息
            //    var message = new AVIMTextMessage("StyMessage");
            //    //message.Content = Json.Encode(content);

            //    //await conversation.SendAsync(message);
            //    return true;
            //}
            //catch (LCException ex)
            //{
            //    LCLogger.Debug($"给用户 {targetUserId} 发送消息失败: {ex.Message}");
            //    return false;
            //}
            return false;
        }
    }
}
