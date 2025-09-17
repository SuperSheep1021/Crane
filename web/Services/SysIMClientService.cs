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
    public LCIMConversation SysIMConversation { get; private set; }
    public string SysConvId { get; private set; }
    public async Task Initialtion(LCUser sysUser)
    {
        SysConvId = Environment.GetEnvironmentVariable("SYS_CONV_ID");

        LCLogger.Debug($"{this} Initialtion start!!");
        SysIMClient = new LCIMClient(sysUser, tag: "sys");
        await SysIMClient.Open();

        SysIMConversation = await SysIMClient.GetConversation(SysConvId);
        await SysIMConversation.Join();
        LCLogger.Debug($"m_SysIMClient.Open():{SysIMClient.Tag}");

        LCLogger.Debug($"{this} Initialtion end!!");
    }


    /// <summary>
    /// �����Ự
    /// </summary>
    /// <param name="targetClientId"></param>
    /// <returns></returns>
    private async Task<LCIMConversation> CreateOrGetConv(string targetClientId) 
    {
        //�������ΪΨһ�Ի����ƶ˻���������ĳ�Ա�б��Ƚ���һ�β�ѯ������Ѿ������ð�����Щ��Ա�ĶԻ����ڣ���ô�ͷ����Ѿ����ڵĶԻ�������Ŵ���һ���µĶԻ���
        LCIMConversation con = await SysIMClient.CreateConversation(new List<string>()
        {
            SysIMClient.Id, targetClientId
        }, $"{SysIMClient.Id} and {targetClientId} conv!", true, new Dictionary<string, object>() 
        {
            { "sys",false}
        });
        return con;
    }
    public async Task<CustomIMMessageBase> SendMessageToClientId(string targetClientId,string text,bool transient, Dictionary<string,object> content) 
    {
        LCIMConversation con =await CreateOrGetConv(targetClientId);
        CustomIMMessageBase message = new CustomIMMessageBase(text);
        foreach (KeyValuePair<string,object> kv in content) 
        {
            message.SetupContent(kv.Key,kv.Value);
        }
        LCIMMessageSendOptions sendOptions = LCIMMessageSendOptions.Default;
        //���߲����յ���Ϣ
        sendOptions.Transient = transient;
        //��Ҫ�ض�
        sendOptions.Receipt = true;
        return (CustomIMMessageBase)await con.Send(message,sendOptions);
    }


    public async Task<CustomIMMessageBase> SendMessageToSubscribesAsync(string text,Dictionary<string,object> content)
    {
        CustomIMMessageBase message = new CustomIMMessageBase(text);
        message.ConversationId = SysIMConversation.Id;
        message.FromClientId = SysIMClient.Id;
        message["from_client"] = SysIMClient.Id;
        message["message"] = "ccccccccccccccccc";

        //message.SetupContent("from_client", SysIMClient.Id);
        //message.SetupContent("message", "cccccccccccccccccccccccccccccc");
        foreach (KeyValuePair<string, object> kv in content)
        {
            message.SetupContent(kv.Key, kv.Value);
        }
        LCIMMessageSendOptions sendOptions = LCIMMessageSendOptions.Default;
        //���߲����յ���Ϣ
        sendOptions.Transient = true;
        //��Ҫ�ض�
        sendOptions.Receipt = true;
        return (CustomIMMessageBase)await SysIMConversation.Send(message, sendOptions);
    }

}