using LC.Google.Protobuf;
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

public class RESTAPIService
{
    static RESTAPIService inst;
    public static RESTAPIService Inst
    {
        get
        {
            if (inst == null)
            {
                inst = new RESTAPIService();
            }
            return inst;
        }
    }

    public string SysUserName { get; private set; }
    public string SysUserPassword { get; private set; }
    public string SysConvId { get;private set; }
    public LCUser SysUser { get;private set; }

    private async Task Init()
    {
        SysUserName = Environment.GetEnvironmentVariable("SYS_USER_NAME");
        SysUserPassword = Environment.GetEnvironmentVariable("SYS_USER_PASSWORD");
        SysConvId = Environment.GetEnvironmentVariable("SYS_CONV_ID");

        LCLogger.Debug($"{this} Initialtion start!!");

        SysUser = await LCUser.Login(SysUserName, SysUserPassword);
        LCLogger.Debug($"SysUserName Logined:{SysUserName}");

        await SysIMClientService.Inst.Initialtion(SysUser);

        LCLogger.Debug($"{this} Initialtion end!!");
    }
    public void Initialtion()
    {
        Task.Run(async () =>
        {
            await Init();
        });
    }


    #region//�����
    /// <summary>
    /// ���������
    /// </summary>
    /// <param name="sysConversationName"></param>
    /// <returns></returns>
    public async Task<Dictionary<string,object>> CreateSysConvAsync(string sysConversationName)
    {
        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "name",sysConversationName},
            { "c",SysUser.ObjectId}
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
    public async Task<Dictionary<string, object>> QuerySysConvAsync(int total, string convName) 
    {
        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };

        var where = new Dictionary<string, object>() 
        {
           { "name", convName }
        };
        var whereJson = await LCJsonUtils.SerializeAsync(where);

        var queryParams = new Dictionary<string, object>() 
        {
             { "where", whereJson },
             { "skip", 0 },
             { "limit", total }
        };
        
        // ʹ�� GET �������Ƽ���
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            "1.2/rtm/service-conversations",   // ·��
            headers,                           // ����ͷ
            queryParams,                       // ��������
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
    private async Task<Dictionary<string,object>> SubscribeSysConvAsync(string conversationId, string client_id)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "����ŶԻ�ID����Ϊ��");

        if (string.IsNullOrEmpty(client_id))
            throw new ArgumentNullException(nameof(client_id), "�û�ID����Ϊ��");

        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "client_id", $"{client_id}" }
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
    public async Task<Dictionary<string, object>> SubscribeSysConvAsync(string client_id)
    {
        return await SubscribeSysConvAsync(SysConvId, client_id);
    }

    /// <summary>
    /// ����Ÿ����ж��ķ�����Ϣ
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="fromClientId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    async Task<Dictionary<string, object>> SendMessageToSubscribesAsync(string conversationId, string fromClientId, string message)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "����ŶԻ�ID����Ϊ��");

        if (string.IsNullOrEmpty(fromClientId))
            throw new ArgumentNullException(nameof(fromClientId), "���Ϳͻ���ID����Ϊ��");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message), "��Ϣ����Ϊ��");

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
    public async Task<Dictionary<string, object>> SendMessageToSubscribesAsync(string message)
    {
        return await SendMessageToSubscribesAsync(SysConvId, SysIMClientService.Inst.SysIMClient.Id, message);
    }

    /// <summary>
    /// ������Ϣ��ָ���ͻ���
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="fromClientId"></param>
    /// <param name="toClientIds"></param>
    /// <param name="transient"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    async Task<Dictionary<string, object>> SendMessageToSubscribesClientsAsync(string conversationId, string fromClientId, string[] toClientIds,bool transient, string message)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "����ŶԻ�ID����Ϊ��");

        if (string.IsNullOrEmpty(fromClientId))
            throw new ArgumentNullException(nameof(fromClientId), "���Ϳͻ���ID����Ϊ��");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message), "��Ϣ����Ϊ��");

        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "from_client",fromClientId },
            { "to_clients",toClientIds},
            { "message",message },
            { "no_sync",false}
        };

        
        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/messages",   // ·��
            headers,                   // ����ͷ
            requestData,               // ��������
            null,                      // ��ѯ����
            false                      // ʹ��API�汾
        );

        return response;
    }
    /// <summary>
    /// ������Ϣ��ָ���ͻ���
    /// </summary>
    /// <param name="toClientIds"></param>
    /// <param name="transient"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> SendMessageToSubscribesClientsAsync(string[] toClientIds, bool transient, string message)
    {
        return await SendMessageToSubscribesClientsAsync(SysConvId, SysIMClientService.Inst.SysIMClient.Id,toClientIds,true,message);
    }


    /// <summary>
    /// ��ѯ����Ÿ�ĳ�û�������Ϣ(��ѯ�����������ŷ��͵Ķ��Ĺ㲥��ϢҲ�����������͵���Ϣ��)
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="clientId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    async Task<Dictionary<string, object>> QuerySendFormClientId(string conversationId, string clientId)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "����ŶԻ�ID����Ϊ��");

        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentNullException(nameof(clientId), "�ͻ���ID����Ϊ��");


        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/subscribers/{clientId}/messages",   // ·��
            headers,                   // ����ͷ
            null,                      // ��ѯ����
            false                      // ʹ��API�汾
        );

        return response;
    }
    /// <summary>
    /// ��ѯ����Ÿ�ĳ�û�������Ϣ(��ѯ�����������ŷ��͵Ķ��Ĺ㲥��ϢҲ�����������͵���Ϣ��)
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> QuerySendFormClientId(string clientId)
    {
        return await QuerySendFormClientId(SysConvId, clientId);
    }

    #endregion

    #region//�㲥
    /// <summary>
    ///  ��ѯ�û�����
    /// </summary>
    /// <returns> { "result": { "online_user_count": 10212, "user_count_today": 1002324 } } </returns>
    public async Task<Dictionary<string, object>> QueryUserTCount()
    {
        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };
        // ����Post������������
        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            "1.2/rtm/stats",                   // ·��
            headers,                           // ����ͷ
            null,                              // ��ѯ����
            false                              // ʹ��API�汾
        );
        // ���ض��Ľ��
        return response;
    }

    /// <summary>
    /// ��ѯ���лỰ
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> QueryAllConversation()
    {
        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };

        // ����Post������������
        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            "1.2/rtm/all-conversations",       // ·��
            headers,                           // ����ͷ
            null,                              // ��ѯ����
            false                              // ʹ��API�汾
        );
        // ���ض��Ľ��
        return response;
    }

    /// <summary>
    /// ��ѯ���лỰ
    /// </summary>
    /// <param name="total"></param>
    /// <param name="convName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<Dictionary<string, object>> QueryAllConversation(int total,string convName)
    {
        if (string.IsNullOrEmpty(convName))
            throw new ArgumentNullException(nameof(convName), "����ŶԻ�ID����Ϊ��");

        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };

        var where = new Dictionary<string, object>()
        {
           { "name", convName }
        };
        var whereJson = await LCJsonUtils.SerializeAsync(where);

        var queryParams = new Dictionary<string, object>()
        {
             { "where", whereJson },
             { "skip", 0 },
             { "limit", total }
        };

        // ����Post������������
        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            "1.2/rtm/all-conversations",       // ·��
            headers,                           // ����ͷ
            queryParams,                       // ��ѯ����
            false                              // ʹ��API�汾
        );
        // ���ض��Ľ��
        return response;
    }

    /// <summary>
    /// ȫ�ֹ㲥
    /// </summary>
    /// <param name="fromClientId">��Ϣ�ķ����� id</param>
    /// <param name="conversationId">���͵��Ի� id�������ڷ����</param>
    /// <param name="message">��Ϣ���ݣ��������Ϣ���ݵı������ַ������������Ƕ��ַ����ڲ��ĸ�ʽû�����޶��������Ͽ����߿������ⷢ�������ʽ��ֻҪ��С������ 5 KB ���Ƽ��ɡ���</param>
    /// <param name="valid_till">����ѡ������ʱ�䣬UTC ʱ��������룩���Ϊ 1 ����֮��Ĭ��ֵΪ 1 ���º�</param>
    /// <param name="transient">����ѡ��Ĭ��Ϊ false�����ֶ����ڱ�ʾ�㲥��Ϣ�Ƿ�Ϊ��̬��Ϣ����̬��Ϣֻ�ᱻ��ǰ���ߵ��û��յ��������ߵ��û��ٴ����ߺ�Ҳ�ղ�������Ϣ��</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    async Task<Dictionary<string, object>> Broadcast(string fromClientId, string conversationId,string message,
        float valid_till,bool transient)
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
            { "conv_id",conversationId},
            { "message",message },
            { "valid_till",valid_till },
            { "transient",transient },
        };

        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm//broadcasts",    // ·��
            headers,                   // ����ͷ
            requestData,               // ��������
            null,                      // ��ѯ����
            false                      // ʹ��API�汾
        );

        return response;
    }
    /// <summary>
    /// ȫ�ֹ㲥
    /// </summary>
    /// <param name="message">��Ϣ���ݣ��������Ϣ���ݵı������ַ������������Ƕ��ַ����ڲ��ĸ�ʽû�����޶��������Ͽ����߿������ⷢ�������ʽ��ֻҪ��С������ 5 KB ���Ƽ��ɡ���</param>
    /// <param name="valid_till">����ѡ������ʱ�䣬UTC ʱ��������룩���Ϊ 1 ����֮��Ĭ��ֵΪ 1 ���º�</param>
    /// <param name="transient">����ѡ��Ĭ��Ϊ false�����ֶ����ڱ�ʾ�㲥��Ϣ�Ƿ�Ϊ��̬��Ϣ����̬��Ϣֻ�ᱻ��ǰ���ߵ��û��յ��������ߵ��û��ٴ����ߺ�Ҳ�ղ�������Ϣ��</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> Broadcast(string message, float valid_till, bool transient)
    { 
        return await Broadcast(SysConvId, SysIMClientService.Inst.SysIMClient.Id, message, valid_till,transient);
    }
    /// <summary>
    /// �㲥��Ϣ�޸Ľ��Ե�ǰ��δ�յ��ù㲥��Ϣ���豸��Ч�����Ŀ���豸�Ѿ��յ��˸ù㲥��Ϣ���޷��޸ġ������ط��͹㲥��Ϣ��
    /// </summary>
    /// <param name="fromClientId"></param>
    /// <param name="timestamp"></param>
    /// <param name="message"></param>
    /// <param name="conversationId"></param>
    /// <param name="messageid"></param>
    /// <returns>�ɹ��򷵻�״̬�� 200 OK</returns>
    /// <exception cref="ArgumentNullException"></exception>
    async Task<string> UpdateBroadcast(string fromClientId, int timestamp, string message,
        string conversationId,string messageid)
    {
        if (string.IsNullOrEmpty(fromClientId))
            throw new ArgumentNullException(nameof(fromClientId), "����ŶԻ�ID����Ϊ��");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message), "��Ϣ����Ϊ��");

        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "�����id����Ϊ��");

        if (string.IsNullOrEmpty(messageid))
            throw new ArgumentNullException(nameof(messageid), "��Ϣ ID ����Ϊ��");

        // �����������ݣ�����û����������б�
        var requestData = new Dictionary<string, object>
        {
            { "from_client",fromClientId },
            { "message",message },
            { "timestamp", timestamp}
        };

        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Put<string>(
            $"1.2/rtm/service-conversations/{conversationId}/messages/{messageid}",    // ·��
            headers,                   // ����ͷ
            requestData,               // ��������
            null,                      // ��ѯ����
            false                      // ʹ��API�汾
        );

        return response;
    }

    /// <summary>
    /// �㲥��Ϣ�޸Ľ��Ե�ǰ��δ�յ��ù㲥��Ϣ���豸��Ч�����Ŀ���豸�Ѿ��յ��˸ù㲥��Ϣ���޷��޸ġ������ط��͹㲥��Ϣ��
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="message">�޸ĺ����Ϣ</param>
    /// <param name="messageId"></param>
    /// <returns>�ɹ��򷵻�״̬�� 200 OK</returns>
    public async Task<string> UpdateBroadcast(int timestamp, string message,string messageId)
    {
        return await UpdateBroadcast(SysIMClientService.Inst.SysIMClient.Id, timestamp, message, SysConvId, messageId);
    }

    /// <summary>
    /// ɾ���ѷ����Ĺ㲥��Ϣ�����Ի�δ�յ��㲥��Ϣ���豸��Ч�����յ��㲥��Ϣ���豸�޷�ɾ����Ϣ��
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task DeleteBroadcast(string messageId)
    {
        if (string.IsNullOrEmpty(messageId))
            throw new ArgumentNullException(nameof(messageId), "��Ϣ ID ����Ϊ��");

        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        await LCCore.HttpClient.Delete(
            $"1.2/rtm/broadcasts/{messageId}",    // ·��
            headers,                   // ����ͷ
            null,                      // ��ѯ����
            null,                      // ��ѯ����
            false                      // ʹ��API�汾
        );
    }

    #endregion

    #region//�û�
    /// <summary>
    /// ��ѯ�û��Ѿ����͵���Ϣ
    /// </summary>
    /// <param name="targetClientId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<Dictionary<string, object>> QueryUserSentMessages(string targetClientId)
    {
        if (string.IsNullOrEmpty(targetClientId))
            throw new ArgumentNullException(nameof(targetClientId), "Ŀ��ͻ���ID����Ϊ��");
        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            $"1.2/rtm/clients/{targetClientId}/messages",    // ·��
            headers,                   // ����ͷ
            null,                      // ��ѯ����
            false                      // ʹ��API�汾
        );

        return response;
    }
    /// <summary>
    /// ǿ��Ŀ���û���������
    /// </summary>
    /// <param name="logout"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<Dictionary<string, object>> ForceLogout(string targetClientId,string logout) 
    {
        if (string.IsNullOrEmpty(targetClientId))
            throw new ArgumentNullException(nameof(targetClientId), "Ŀ��ͻ���ID����Ϊ��");

        if (string.IsNullOrEmpty(logout))
            throw new ArgumentNullException(nameof(logout), "����ԭ��");
        
        // ������Ӷ��������ͷ�������Ҫ��
        var headers = new Dictionary<string, object>
        {
            // ���������֤��Ϣ��������Ҫͷ��Ϣ
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };

        var requestData = new Dictionary<string, object>
        {
            { "reason",logout}
        };

        // ����Post������������
        // ����API�汾�Ѿ���Post�����ڲ�����withAPIVersion��Ϊtrue
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/clients/{targetClientId}/kick",   // ·��
            headers,                           // ����ͷ
            requestData,                       // ��������
            null,                              // ��ѯ����
            false                              // ʹ��API�汾
        );
        return response;
    }
    #endregion
}