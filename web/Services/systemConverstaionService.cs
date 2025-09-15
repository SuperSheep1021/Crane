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

public class SystemConverstaionService
{
    static SystemConverstaionService inst;
    public static SystemConverstaionService Inst
    {
        get
        {
            if (inst == null)
            {
                inst = new SystemConverstaionService();
            }
            return inst;
        }
    }

    const string SysUserName = "systemAccount";
    const string SysUserPassword = "123123";

    const string SysConvID = "68c7cab316ec9e2c7d13b42a";

    public LCUser SysUser { get;private set; }
    public LCIMClient SysClient { get; private set; }

    private async Task Init()
    {
        LCLogger.Debug($"{this} Initialtion start!!");

        SysUser = await LCUser.Login(SysUserName, SysUserPassword);
        LCLogger.Debug($"SysUserName Logined:{SysUserName}");

        SysClient = new LCIMClient(SysUser, tag: "sys");
        await SysClient.Open();
        LCLogger.Debug($"m_SysClient.Open():{SysClient.Tag}");

        LCLogger.Debug($"{this} Initialtion end!!");
    }
    public void Initialtion()
    {
        Task.Run(async () =>
        {
            await Init();
        });
    }

    /// <summary>
    /// ���������
    /// </summary>
    /// <param name="sysConversationName"></param>
    /// <returns></returns>
    public async Task<IDictionary<string,object>> CreateSysConvAsync(string sysConversationName)
    {
        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "name",sysConversationName}
        };
        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };
        // ����Post������������
        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            "1.2/rtm/service-conversations",   // ·��
            headers,                           // ����ͷ
            requestData,                       // ��������
            null,                              // ��ѯ����
            false                              // ʹ��API�汾
        );

        // ���ض��Ľ��
        return response;
    }

    /// <summary>
    /// ���ҷ����
    /// </summary>
    /// <returns></returns>
    public async Task<IDictionary<string, object>> QuerySysConvAsync(int total, string convName) 
    {
        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };

        var whereDic = new Dictionary<string, object>() 
        {
           { "name", convName }
        };
        var json =await LCJsonUtils.SerializeAsync(whereDic);
        var queryParams = new Dictionary<string, object>() 
        {
             { "skip", 1 },
             { "limit", total }
        };
        
        // ʹ�� GET �������Ƽ���
        var response = await LCCore.HttpClient.Get<IDictionary<string, object>>(
            "1.2/rtm/service-conversations",   // ·��
            headers,                           // ����ͷ
            null,                       // ��������
            false                              // ʹ��API�汾
        );

        return response;
    }

    /// <summary>
    /// ���ķ����
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    private async Task<IDictionary<string,object>> SubscribeSysConvAsync(string conversationId, string subscribeUserId)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "����ŶԻ�ID����Ϊ��");

        if (string.IsNullOrEmpty(subscribeUserId))
            throw new ArgumentNullException(nameof(subscribeUserId), "�û�ID����Ϊ��");

        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "client_id", $"{subscribeUserId}" }
        };

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
            false                      // ʹ��API�汾
        );
        // ���ض��Ľ��
        return response;
    }


    /// <summary>
    /// ���ķ����
    /// </summary>
    /// <param name="subscribeUserId"></param>
    /// <returns></returns>
    public async Task<IDictionary<string, object>> SubscribeSysConvAsync(string subscribeUserId)
    {
        return await SubscribeSysConvAsync(SysConvID,subscribeUserId);
    }

    /// <summary>
    /// ����Ÿ����ж��ķ�����Ϣ
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="fromClientId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    async Task<IDictionary<string, object>> SendMessageToSubscribesAsync(string conversationId, string fromClientId, string message)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "����ŶԻ�ID����Ϊ��");

        if (string.IsNullOrEmpty(fromClientId))
            throw new ArgumentNullException(nameof(conversationId), "���Ϳͻ���ID����Ϊ��");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(conversationId), "��Ϣ����Ϊ��");

        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "from_client",fromClientId },
            { "message",message },
        };

        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/broadcasts",   // ·��
            headers,                   // ����ͷ
            requestData,               // ��������
            null,                      // ��ѯ����
            false                      // ʹ��API�汾
        );

        return response;
    }

    /// <summary>
    /// ����Ÿ����ж��ķ�����Ϣ
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<IDictionary<string, object>> SendMessageToSubscribesAsync(string message)
    {
        return await SendMessageToSubscribesAsync(SysConvID, SysClient.Id, message);
    }


   

}