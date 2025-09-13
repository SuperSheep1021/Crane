using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using LeanCloud.Storage.Internal.Codec;
using Microsoft.VisualBasic;
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

    const string SysUserName = "System_Broadcast2";
    const string SysUserPassword = "123123";
    const string SysConversationID = "68c3e5ce16ec9e2c7d1396c0";

    LCUser m_SysUser;
    LCIMClient m_SysClient;
    LCIMConversation m_SysConversation;

    public async Task InitialtionIM()
    {
        m_SysUser = await LCUser.Login(SysUserName, SysUserPassword);
        LCLogger.Debug($"系统用户登录成功:{m_SysUser.ObjectId}");

        m_SysClient = new LCIMClient(m_SysUser,tag:"sys");
        await m_SysClient.Open();
        LCLogger.Debug($"创建系统客户端成功:{m_SysClient.Tag}");

        m_SysConversation = await m_SysClient.GetConversation(SysConversationID);
        await m_SysConversation.Join();
        LCLogger.Debug($"服务端{SysUserName}加入 {m_SysConversation.Name} 会话成功");

        m_SysClient.OnMembersJoined += (conv, memberList, initBy) =>
        {
            LCLogger.Debug($"{memberList} 加入了 {conv.Id} 对话；操作者为：{initBy}");
        };

        LCLogger.Debug($"创建系统会话完成:{m_SysConversation.Name}");
    }
    public async Task AddMembers(string clientId) 
    {
        //var memberids = m_SysConversation.MemberIds;
        //if (!memberids.Contains(clientId) )
        //{
        //    LCIMPartiallySuccessResult result = await m_SysConversation.AddMembers(new string[] { m_SysUser.ObjectId, clientId });
        //    if (result.IsSuccess)
        //    {
        //        LCLogger.Debug($"{m_SysConversation.Name}添加成员{clientId} 成功!");
        //    }
        //    else {
        //        LCLogger.Debug($"{m_SysConversation.Name}添加成员{clientId} 失败!");
        //    }
        //    await m_SysConversation.Fetch();
        //    LCLogger.Debug("刷新会话!");

        //    int membersCount = await m_SysConversation.GetMembersCount();
        //    LCLogger.Debug($"当前刷新Members Total = {membersCount}!!!!!!");
        //}
    }
    public async Task SendMessage(string text) 
    {
        LCIMTextMessage message = new LCIMTextMessage(text);
        message["消息1"] = "asdasd";
        message["消息2"] = "消息2";
        message["消息3"] = 123123;
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