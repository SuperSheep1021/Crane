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

    async Task Init()
    {
        LCLogger.Debug($"{this}��ʼ��ʼ��");
        if (m_SysUser ==null) 
        {
            m_SysUser = await LCUser.Login(SysUserName, SysUserPassword);
            LCLogger.Debug($"SysUserName Logined:{SysUserName}");
        }
        if (m_SysClient == null)
        {
            m_SysClient = new LCIMClient(m_SysUser, tag: "sys");
            await m_SysClient.Open();
            LCLogger.Debug($"m_SysClient.Open():{m_SysClient.Tag}");

            m_SysClient.OnMessage += OnIMMessageReceived;
        }
        if (m_SysConversation ==null) 
        {
            m_SysConversation = await m_SysClient.GetConversation(SysConversationID);
            LCLogger.Debug($"GetConversation:{m_SysConversation.Name}");
            //await m_SysConversation.Join();
        }
        
        LCLogger.Debug($"{this}������ʼ��");

    }
    private void OnIMMessageReceived(LCIMConversation conversation, LCIMMessage message)
    {
        LCLogger.Warn($"�յ����ԶԻ� [{conversation.Name}] ����Ϣ");

    }
    public async Task<string> CreateServiceConversationAsync()
    {
        // ��������URL
        //var url = $"https://{LCCore.AppRouter.GetApiServer() }/1.2/rtm/service-conversations";

        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "name","My First Service-conversation"}
        };
        //var jsonData = JsonConvert.SerializeObject(requestData);
        //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");


        //string json = await LCJsonUtils.SerializeAsync(requestData);

        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" }
        };

        // ����Post������������
        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            "1.2/rtm/service-conversations",   // ·��
            headers,                   // ����ͷ
            requestData,               // ��������
            null,                      // ��ѯ����
            false                       // ʹ��API�汾
        );



        // ���ض��Ľ��
        return response.ToString();
    }


    public async Task<string> SubscribeServiceConversationAsync(string conversationId, string userId)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "����ŶԻ�ID����Ϊ��");

        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId), "�û�ID����Ϊ��");


        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "client_id", $"{userId}" }
        };

        //var jsonData = JsonConvert.SerializeObject(requestData);
        //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");


        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // ����Post������������
        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/subscribers",   // ·��
            headers,                   // ����ͷ
            requestData,               // ��������
            null,                      // ��ѯ����
            false                       // ʹ��API�汾
        );

        // ���ض��Ľ��
        return response.ToString();
    }


    public async Task<string> SendSubscribeServiceConversationAsync(string conversationId )
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "����ŶԻ�ID����Ϊ��");

        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "from_client","68c22ec62f7ee809fcc9e7e6" },
            { "message","service send subscribe message!!!!" },
        };

        //var jsonData = JsonConvert.SerializeObject(requestData);
        //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");


        //string json = await LCJsonUtils.SerializeAsync(requestData);

        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };
        // ����Post������������
        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/broadcasts",   // ·��
            headers,                   // ����ͷ
            requestData,               // ��������
            null,                      // ��ѯ����
            false                       // ʹ��API�汾
        );

        // ���ض��Ľ��
        return response.ToString();
    }


    public async Task AddMembers(string clientId) 
    {
        await m_SysConversation.AddMembers(new List<string>() { clientId });
    }
    public void Initialtion()
    {
        Task.Run(async () => 
        {
            await Init();
        });
    }
    public async Task<int> GetMembersCount() 
    {
        return await m_SysConversation.GetMembersCount();
    }

    public async Task SendTextMessage(string text,Dictionary<string,object> content = null) 
    {
        //await Initialtion();

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