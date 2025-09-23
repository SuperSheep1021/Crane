using LC.Newtonsoft.Json;
using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public class SysIMClientService 
{
    static SysIMClientService inst;
    public static SysIMClientService Inst
    {
        get
        {
            if (inst == null)
            {
                inst = new SysIMClientService();
            }
            return inst;
        }
    }


    public LCIMClient SysIMClient { get; private set; }

    const string SysConvName = "sysconv";
    public LCIMServiceConversation SysIMConversation { get; private set; }
    public string SysConvId { get; private set; }
    public async Task Initialtion()
    {
        LCUser user = await LCUser.GetCurrent();
        SysIMClient = new LCIMClient(user, tag: "sys");
        await SysIMClient.Open();
        LCLogger.Debug($"m_SysIMClient.Open():{SysIMClient.Tag}");


        SysConvId = Environment.GetEnvironmentVariable("SYS_CONV_ID");
        LCIMConversationQuery query = SysIMClient.GetQuery();
        query.WhereEqualTo("name", SysConvName);
        query.WhereEqualTo("sys", true);
        SysIMConversation = (LCIMServiceConversation)await query.First();
        LCLogger.Debug($"SysIMConversation.First():{SysIMConversation.Name}");


        SysIMClient.OnMembersJoined = (conversation, newMembers, operatorId) =>
        {
            LCLogger.Debug($"OnMembersJoined {conversation.Name} + newmembers is {newMembers} + operatorid is {operatorId}");
            
        };

        SysIMClient.OnMembersLeft = (conversation, newMembers, operatorId) =>
        {
            LCLogger.Debug($"OnMembersLeft {conversation.Name} + newmembers is {newMembers} + operatorid is {operatorId}");
        };
    }
    public async Task<LCIMTextMessage> SendMessageToSubscribesAsync(string text, List<string> toClientIds, Dictionary<string,object> content)
    {
        LCIMTextMessage message = new LCIMTextMessage(text);
        //message.ConversationId = SysIMConversation.Id;
        //message.FromClientId = SysIMClient.Id;
        message.MentionIdList.AddRange(toClientIds);
        //message["from_client"] = SysIMClient.Id;
        //message["message"] = text;
        //message["no_sync"] = false;
        
        //message.SetupContent("from_client", SysIMClient.Id);
        //message.SetupContent("message", "cccccccccccccccccccccccccccccc");
        foreach (KeyValuePair<string, object> kv in content)
        {
            message[kv.Key] = kv.Value;
        }
        LCIMMessageSendOptions sendOptions = LCIMMessageSendOptions.Default;
        //在线才能收到消息
        sendOptions.Transient = true;
        //需要回读
        sendOptions.Receipt = true;
        return await SysIMConversation.Send(message, sendOptions) as LCIMTextMessage;
    }

}