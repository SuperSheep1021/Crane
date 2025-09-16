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
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;


public class CustomIMMessageBase : LCIMTextMessage
{
    public override int MessageType => 1001;
    public CustomIMMessageBase(string text) : base(text)
    {
    }
    public void SetupContent(string key, object value)
    {
        this[key] = value;
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
    public async Task Initialtion(LCUser sysUser)
    {
        LCLogger.Debug($"{this} Initialtion start!!");
        SysIMClient = new LCIMClient(sysUser, tag: "sys");
        await SysIMClient.Open();
        LCLogger.Debug($"m_SysIMClient.Open():{SysIMClient.Tag}");

        LCLogger.Debug($"{this} Initialtion end!!");
    }


    /// <summary>
    /// 创建会话
    /// </summary>
    /// <param name="targetClientId"></param>
    /// <returns></returns>
    private async Task<LCIMConversation> CreateOrGetConv(string targetClientId) 
    {
        //如果设置为唯一对话，云端会根据完整的成员列表先进行一次查询，如果已经有正好包含这些成员的对话存在，那么就返回已经存在的对话，否则才创建一个新的对话。
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
        //在线才能收到消息
        sendOptions.Transient = transient;
        //需要回读
        sendOptions.Receipt = true;
        return (CustomIMMessageBase)await con.Send(message,sendOptions);
    }

}