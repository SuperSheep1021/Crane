using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using LeanCloud.Storage.Internal.Codec;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using web;

public class SysClientService
{
    static SysClientService inst;
    public static SysClientService Inst
    {
        get
        {
            if (inst == null)
            {
                inst = new SysClientService();
            }
            return inst;
        }
    }

    const string SysUserName = "System_Broadcast";
    const string SysUserPassword = "123123";
    const string SysConversationID = "68c77b00cb7fbfc7a4ddb31e";


    LCUser m_SysUser;
    LCIMClient m_SysClient;
    LCIMConversation m_SysConversation;

    public async Task Initialtion()
    {
        LCLogger.Debug($"{this}��ʼ��ʼ��");
        if (m_SysUser ==null) 
        {
            m_SysUser = await LCUser.Login(SysUserName, SysUserPassword);
            LCLogger.Debug($"ϵͳ�û���¼�ɹ�:{m_SysUser.ObjectId}");
        }
        if (m_SysClient == null)
        {
            m_SysClient = new LCIMClient(m_SysUser, tag: "sys");
            await m_SysClient.Open();
            LCLogger.Debug($"����ϵͳ�ͻ��˳ɹ�:{m_SysClient.Tag}");
        }

        if (m_SysConversation ==null)
        {
            m_SysConversation = await m_SysClient.CreateConversation( new List<string>() { m_SysClient.Id},name:"ϵͳ�Ự",
                unique: true, properties:new Dictionary<string, object>() 
                {
                    { "sys",true}
                });

            await m_SysConversation.Join();
        }
        LCLogger.Debug($"{this}������ʼ��");
    }
    public async Task<int> GetMembersCount() 
    {
        return await m_SysConversation.GetMembersCount();
    }

    public async Task SendTextMessage(string text,Dictionary<string,object> content = null) 
    {
        await Initialtion();
        LCIMTextMessage message = new LCIMTextMessage(text);
        message["����1"] = "asdasd";
        message["����2"] = "��Ϣ2";
        message["����3"] = 123123;
        try
        {
            await m_SysConversation.Send(message);
        }
        catch (LCException ex)
        {
            LCLogger.Debug(ex.Message);
        }
        catch (Exception ex)
        {
            LCLogger.Debug(ex.Message);
        }
    }
}