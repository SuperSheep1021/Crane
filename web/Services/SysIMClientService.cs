using LC.Newtonsoft.Json;
using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using LeanCloud.Storage.Internal.Codec;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public class CustomIMMessageBase : LCIMTextMessage
{
    public override int MessageType => 1001;
    public CustomIMMessageBase(string text) : base(text)
    {
    }

    public void SetupContent(string key, object value)
    {
        data.Add(key, value);
    }
    public object GetContent(string key)
    {
        return data[key];
    }

    public void DebugContent()
    {
    }
}
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

    const string SysConvName = "sysconvname";
    public LCIMServiceConversation SysIMConversation { get; private set; }
    public string SysConvId { get; private set; }
    public async Task Initialtion(LCUser sysUser)
    {
        SysIMClient = new LCIMClient(sysUser, tag: "sys");
        await SysIMClient.Open();
        LCLogger.Debug($"m_SysIMClient.Open():{SysIMClient.Tag}");


        SysConvId = Environment.GetEnvironmentVariable("SYS_CONV_ID");
        LCIMConversationQuery query = SysIMClient.GetQuery();
        query.WhereEqualTo("name", SysConvName);
        query.WhereEqualTo("objectId", SysConvId);
        SysIMConversation = (LCIMServiceConversation) await query.First();

        LCLogger.Debug($"SysIMConversation.First():{SysIMConversation.Name}");
    }

    public async Task<CustomIMMessageBase> SendMessageToSubscribesAsync(string text, string[] toClientIds, Dictionary<string,object> content)
    {
        CustomIMMessageBase message = new CustomIMMessageBase(text);
        message.ConversationId = SysIMConversation.Id;
        message.FromClientId = SysIMClient.Id;
        message["from_client"] = SysIMClient.Id;
        message["to_clients"] = toClientIds;
        message["message"] = text;
        message["no_sync"] = false;

        //message.SetupContent("from_client", SysIMClient.Id);
        //message.SetupContent("message", "cccccccccccccccccccccccccccccc");
        foreach (KeyValuePair<string, object> kv in content)
        {
            message.SetupContent(kv.Key, kv.Value);
        }
        LCIMMessageSendOptions sendOptions = LCIMMessageSendOptions.Default;
        //在线才能收到消息
        sendOptions.Transient = true;
        //需要回读
        sendOptions.Receipt = true;
        return (CustomIMMessageBase)await SysIMConversation.Send(message, sendOptions);
    }

}