using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using web;

public class IMService
{
    static IMService inst;
    public static IMService Inst 
    {
        get {
            if (inst ==null) {
                inst = new IMService();
            }
            return inst;
        }
    }


    const string SysClientID = "68c22ec62f7ee809fcc9e7e6";
    const string SysConversationID = "68c3e5ce16ec9e2c7d1396c0";

    LCIMClient m_SysClient;
    LCIMConversation m_SysConversation;

    public async Task InitialtionIM()
    {
        m_SysClient = new LCIMClient(SysClientID, tag: "sys");
        LCLogger.Debug($"创建系统客户端成功:{m_SysClient.Tag}");
        LCIMConversationQuery conQuery= m_SysClient.GetQuery();

        m_SysConversation = await conQuery.Get(SysConversationID);
        LCLogger.Debug($"创建系统会话成功:{m_SysConversation.Name}");
    }
    public async Task SendMessage(string text) 
    {
        LCIMTextMessage message = new LCIMTextMessage(text);
        await m_SysConversation.Send(message);
    }

}