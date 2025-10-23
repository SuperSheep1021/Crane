using LC.Newtonsoft.Json;
using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Realtime.Internal.Protocol;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    async Task<bool> GetSysConv()
    {
        bool success = false;
        try
        {
            SysConvId = Environment.GetEnvironmentVariable("SYS_CONV_ID");
            SysIMConversation = (LCIMServiceConversation)await SysIMClient.GetConversation(SysConvId);

            success = true;
            LCLogger.Debug($"Get Sys Conv {SysIMConversation.Name} Success!!!");
        }
        catch (LCException e)
        {
            LCLogger.Error($"Get Sys Conv {SysIMConversation.Name} Failure!!  {e.Code} : {e.Message}");
        }
        catch (Exception e)
        {
            LCLogger.Error($"Get Sys Conv {SysIMConversation.Name} Failure!!  {e.Message}");
        }
        return success;
    }
    async Task<bool> OpenClient() 
    {
        bool success = false;
        try
        {
            SysIMClient = new LCIMClient(RESTAPIService.Inst.SysUser, tag: "sys" );
            await SysIMClient.Open(true);
            success = true;
            LCLogger.Debug($"SysIMClient Opened Id is {SysIMClient.Id}");
        }
        catch (LCException e)
        {
            LCLogger.Error($"SysIMClient Open Failure:{e.Code} : {e.Message}");
        }
        catch (Exception e)
        {
            LCLogger.Error($"SysIMClient Open Failure: {e.Message}");
        }
        return success;
    }
    public async Task<bool> Initialtion()
    {
        bool success = true;
        success = await OpenClient();
        success = await GetSysConv();
        return success;
    }

    //public async Task<LCIMTextMessage> SendMessageToSubscribesAsync(string text, string[] toClientIds )
    //{
    //    LCLogger.Debug($"conv id:{SysConvId}");
    //    //LCIMServiceConversation serConv = await SysIMClient.GetConversation(SysConvId) as LCIMServiceConversation;
    //    //await serConv.AddMembers(toClientIds);

    //    LCIMTextMessage message = new LCIMTextMessage(text);
    //    message.ConversationId = SysConvId;
    //    message.FromClientId = SysIMClient.Id;
    //    message["toPeers"] = toClientIds;

    //    LCIMMessageSendOptions sendOptions = LCIMMessageSendOptions.Default;
    //    //在线才能收到消息
    //    sendOptions.Transient = true;
    //    //需要回读
    //    sendOptions.Receipt = true;
    //    return await SysIMConversation.Send(message, sendOptions) as LCIMTextMessage;

    //}

    public async Task<LCIMTextMessage> SendMessageToSubscribesAsync(string text, string[] toClientIds)
    {
        LCLogger.Debug($"conv id:{SysConvId}");

        LCIMConversation conv = await SysIMClient.CreateConversation(toClientIds,unique:true);
        LCIMTextMessage message = new LCIMTextMessage(text);
        message.ConversationId = SysConvId;
        message.FromClientId = SysIMClient.Id;

        LCIMMessageSendOptions sendOptions = LCIMMessageSendOptions.Default;
        //在线才能收到消息
        sendOptions.Transient = true;
        //需要回读
        sendOptions.Receipt = true;
        return await conv.Send(message, sendOptions) as LCIMTextMessage;

    }
}