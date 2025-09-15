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
    /// 创建服务号
    /// </summary>
    /// <param name="sysConversationName"></param>
    /// <returns></returns>
    public async Task<IDictionary<string,object>> CreateSysConvAsync(string sysConversationName)
    {
        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "name",sysConversationName}
        };
        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };
        // 调用Post方法发送请求
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            "1.2/rtm/service-conversations",   // 路径
            headers,                           // 请求头
            requestData,                       // 请求数据
            null,                              // 查询参数
            false                              // 使用API版本
        );

        // 返回订阅结果
        return response;
    }

    /// <summary>
    /// 查找服务号
    /// </summary>
    /// <returns></returns>
    public async Task<IDictionary<string, object>> QuerySysConvAsync(int total, string convName) 
    {
        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
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
        
        // 使用 GET 方法（推荐）
        var response = await LCCore.HttpClient.Get<IDictionary<string, object>>(
            "1.2/rtm/service-conversations",   // 路径
            headers,                           // 请求头
            null,                       // 请求数据
            false                              // 使用API版本
        );

        return response;
    }

    /// <summary>
    /// 订阅服务号
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    private async Task<IDictionary<string,object>> SubscribeSysConvAsync(string conversationId, string subscribeUserId)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "服务号对话ID不能为空");

        if (string.IsNullOrEmpty(subscribeUserId))
            throw new ArgumentNullException(nameof(subscribeUserId), "用户ID不能为空");

        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "client_id", $"{subscribeUserId}" }
        };

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // 调用Post方法发送请求
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/subscribers",   // 路径
            headers,                   // 请求头
            requestData,               // 请求数据
            null,                      // 查询参数
            false                      // 使用API版本
        );
        // 返回订阅结果
        return response;
    }


    /// <summary>
    /// 订阅服务号
    /// </summary>
    /// <param name="subscribeUserId"></param>
    /// <returns></returns>
    public async Task<IDictionary<string, object>> SubscribeSysConvAsync(string subscribeUserId)
    {
        return await SubscribeSysConvAsync(SysConvID,subscribeUserId);
    }

    /// <summary>
    /// 服务号给所有订阅发送消息
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="fromClientId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    async Task<IDictionary<string, object>> SendMessageToSubscribesAsync(string conversationId, string fromClientId, string message)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "服务号对话ID不能为空");

        if (string.IsNullOrEmpty(fromClientId))
            throw new ArgumentNullException(nameof(conversationId), "发送客户端ID不能为空");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(conversationId), "消息不能为空");

        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "from_client",fromClientId },
            { "message",message },
        };

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/broadcasts",   // 路径
            headers,                   // 请求头
            requestData,               // 请求数据
            null,                      // 查询参数
            false                      // 使用API版本
        );

        return response;
    }

    /// <summary>
    /// 服务号给所有订阅发送消息
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<IDictionary<string, object>> SendMessageToSubscribesAsync(string message)
    {
        return await SendMessageToSubscribesAsync(SysConvID, SysClient.Id, message);
    }


   

}