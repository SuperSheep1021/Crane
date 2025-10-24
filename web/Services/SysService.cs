using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Realtime.Internal.Protocol;
using LeanCloud.Storage;
using System;
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
    public async Task<LCIMTextMessage> SendMessageToSubscribesAsync(string text, string[] toClientIds)
    {
        LCIMConversation conv = await SysIMClient.CreateConversation(toClientIds,unique:true);
        LCIMTextMessage message = new LCIMTextMessage(text);
        message.ConversationId = SysConvId;
        message.FromClientId = SysIMClient.Id;

        LCIMMessageSendOptions sendOptions = LCIMMessageSendOptions.Default;
        //���߲����յ���Ϣ
        sendOptions.Transient = true;
        //��Ҫ�ض�
        sendOptions.Receipt = true;
        return await conv.Send(message, sendOptions) as LCIMTextMessage;
    }
}