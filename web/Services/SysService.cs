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




public class SysService 
{
    static SysService inst;
    public static SysService Inst
    {
        get
        {
            if (inst == null)
            {
                inst = new SysService();
            }
            return inst;
        }
    }
    public string SysUserName { get; private set; }
    public string SysUserPassword { get; private set; }
    public LCUser SysUser { get; private set; }
    public LCIMClient SysIMClient { get; private set; }
    public string SysConvId { get; private set; }
    public LCIMServiceConversation SysIMConversation { get; private set; }
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
            string monsterKey = Environment.GetEnvironmentVariable("LEANCLOUD_APP_MASTER_KEY");
            SysIMClient = new LCIMClient(SysService.Inst.SysUser, tag: "sys",signatureFactory: new LocalSignatureFactory(monsterKey)  );
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
    LCACL CreateSysACL()
    {
        LCACL acl = new LCACL();
        acl.SetUserWriteAccess(SysUser, true);
        acl.SetUserReadAccess(SysUser, true);
        acl.PublicReadAccess = true;
        acl.PublicWriteAccess = true;
        return acl;
    }
    async Task<bool> LoginSysAccount()
    {
        bool success = false;
        try {
            SysUserName = Environment.GetEnvironmentVariable("SYS_USER_NAME");
            SysUserPassword = Environment.GetEnvironmentVariable("SYS_USER_PASSWORD");
            SysConvId = Environment.GetEnvironmentVariable("SYS_CONV_ID");
            SysUser = await LCUser.Login(SysUserName, SysUserPassword);
            SysUser.ACL = CreateSysACL();
            await SysUser.Save();

            success = true;
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
        bool success = await LoginSysAccount();
        if (!success) { return false; }
        LCLogger.Warn($"sys Account login success");

        success = await OpenClient();
        if (!success) { return false; }
        LCLogger.Warn($"sys Account Open Client success");

        success = await GetSysConv();
        if (!success) { return false; }
        LCLogger.Warn($"sys Account Get Sys Conv success");
        return success;
    }
    public async Task<bool> Online(string userId,bool online)
    {
        bool success = false;
        try
        {
            LCQuery<LCUser> userLCQuery = LCUser.GetQuery();
            userLCQuery.WhereEqualTo("objectId",userId);
            LCUser user= await userLCQuery.First();
            user["online"] = online;
            await user.Save( );
            success = true;
        }
        catch (LCException ex)
        {
            LCLogger.Error(ex.Code + ":" + ex.Message);
        }
        return success;
    }
    public async Task<bool> isSignUped(object userName)
    {
        LCLogger.Debug($"=========query user============={userName}========================");
        LCQuery<LCUser> query = LCUser.GetQuery();
        query.WhereEqualTo("username", userName);
        LCUser firstUser = await query.First();
        if (firstUser != null)
        {
            return true;
        }
        else
        {
            return false;
        }
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