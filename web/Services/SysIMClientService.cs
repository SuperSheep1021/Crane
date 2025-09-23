using LC.Newtonsoft.Json;
using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Realtime.Internal.Protocol;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
    public LCIMServiceConversation SysIMConversation { get; private set; }
    public string SysConvId { get; private set; }
    public async Task Initialtion()
    {
        LCUser user = await LCUser.GetCurrent();
        SysIMClient = new LCIMClient(user, tag: "sys");
        SysIMClient.OnMembersJoined = (conversation, newMembers, operatorId) =>
        {
            LCLogger.Debug($"OnMembersJoined {conversation.Name} + newmembers is {newMembers} + operatorid is {operatorId}");

        };

        SysIMClient.OnMembersLeft = (conversation, newMembers, operatorId) =>
        {
            LCLogger.Debug($"OnMembersLeft {conversation.Name} + newmembers is {newMembers} + operatorid is {operatorId}");
        };
        await SysIMClient.Open(true);
        LCLogger.Debug($"SysIMClient Open ");


        SysConvId = Environment.GetEnvironmentVariable("SYS_CONV_ID");
        LCIMConversationQuery convQuery = SysIMClient.GetQuery();
        convQuery.WhereEqualTo("name", "sysconv");
        convQuery.WhereEqualTo("sys", true);
        SysIMConversation = (LCIMServiceConversation)await convQuery.First();
        LCLogger.Debug($"SysIMConversation.First():{SysIMConversation.Name}");

        
    }
    public async Task<LCIMTextMessage> SendMessageToSubscribesAsync(string text, List<string> toClientIds, Dictionary<string,object> content)
    {
        LCLogger.Debug($"conv id:{SysConvId}");
        //LCIMServiceConversation serConv = await SysIMClient.GetConversation(SysConvId) as LCIMServiceConversation;
        //await serConv.AddMembers(toClientIds);
        LCIMTextMessage message = new LCIMTextMessage(text);
        message.ConversationId = SysIMConversation.Id;
        message.FromClientId = SysIMClient.Id;
        await SysIMConversation.AddMembers(toClientIds);


        // 1. 构造要发送的数据对象
        //var mmmm = new MessageData
        //{
        //    LcText = "ttttttttttt",
        //    LcAttrs = new LcAttributes
        //    {
        //        A = "_lcattrs aaaaaaaaaaaaaaaaa"
        //    }
        //};
        var mmmm = new Dictionary<string,object>
        {
            { "LcText" ,"ttttttttttt"},
            { "LcType",-1},
        };
        // 2. 将对象序列化为JSON字符串
        string jsonData = JsonConvert.SerializeObject(mmmm, Formatting.Indented);
        Console.WriteLine("要发送的JSON数据:");
        Console.WriteLine(jsonData);

        // 3. 创建HTTP请求内容
        //var ccccc = new StringContent(
        //    jsonData,
        //    Encoding.UTF8,
        //    "application/json"
        //);

        message.Text = "ssssssssssssssssssssss122ssssssssssssssssssssssssssss";
        //LCIMPartiallySuccessResult result = await SysIMConversation.AddMembers(toClientIds);

        //foreach (KeyValuePair<string, object> kv in content)
        //{
        //    message[kv.Key] = kv.Value;
        //}


        //message["toPeers"] = new string[1] { toClientIds[0] };

        //LCLogger.Debug("sssssssssssssssssssssssssssssssssss"+message.ToString());

        LCIMMessageSendOptions sendOptions = LCIMMessageSendOptions.Default;
        //在线才能收到消息
        sendOptions.Transient = true;
        //需要回读
        sendOptions.Receipt = true;
        return await SysIMConversation.Send(message, sendOptions) as LCIMTextMessage;

    }

    public async Task<int> SubscribesTotal() 
    { 
        int total = await SysIMConversation.GetMembersCount();
        return total;
    }

}