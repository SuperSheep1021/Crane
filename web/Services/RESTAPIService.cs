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


    #region//服务号
    /// <summary>
    /// 创建服务号
    /// </summary>
    /// <param name="sysConversationName"></param>
    /// <returns></returns>
    public async Task<Dictionary<string,object>> CreateSysConvAsync(string sysConversationName)
    {
        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "name",sysConversationName},
            { "c",SysUser.ObjectId}
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
    public async Task<Dictionary<string, object>> QuerySysConvAsync(int total, string convName) 
    {
        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
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
        
        // 使用 GET 方法（推荐）
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            "1.2/rtm/service-conversations",   // 路径
            headers,                           // 请求头
            queryParams,                       // 请求数据
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
    private async Task<Dictionary<string,object>> SubscribeSysConvAsync(string conversationId, string client_id)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "服务号对话ID不能为空");

        if (string.IsNullOrEmpty(client_id))
            throw new ArgumentNullException(nameof(client_id), "用户ID不能为空");

        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "client_id", $"{client_id}" }
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
    public async Task<Dictionary<string, object>> SubscribeSysConvAsync(string client_id)
    {
        return await SubscribeSysConvAsync(SysConvId, client_id);
    }

    /// <summary>
    /// 服务号给所有订阅发送消息
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="fromClientId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    async Task<Dictionary<string, object>> SendMessageToSubscribesAsync(string conversationId, string fromClientId, string message)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "服务号对话ID不能为空");

        if (string.IsNullOrEmpty(fromClientId))
            throw new ArgumentNullException(nameof(fromClientId), "发送客户端ID不能为空");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message), "消息不能为空");

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
    public async Task<Dictionary<string, object>> SendMessageToSubscribesAsync(string message)
    {
        return await SendMessageToSubscribesAsync(SysConvId, SysIMClientService.Inst.SysIMClient.Id, message);
    }

    /// <summary>
    /// 发送消息给指定客户端
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
            throw new ArgumentNullException(nameof(conversationId), "服务号对话ID不能为空");

        if (string.IsNullOrEmpty(fromClientId))
            throw new ArgumentNullException(nameof(fromClientId), "发送客户端ID不能为空");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message), "消息不能为空");

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "from_client",fromClientId },
            { "to_clients",toClientIds},
            { "message",message },
            { "no_sync",false}
        };

        
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/messages",   // 路径
            headers,                   // 请求头
            requestData,               // 请求数据
            null,                      // 查询参数
            false                      // 使用API版本
        );

        return response;
    }
    /// <summary>
    /// 发送消息给指定客户端
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
    /// 查询服务号给某用户发的消息(查询结果包含服务号发送的订阅广播消息也包含单独发送的消息。)
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="clientId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    async Task<Dictionary<string, object>> QuerySendFormClientId(string conversationId, string clientId)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "服务号对话ID不能为空");

        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentNullException(nameof(clientId), "客户端ID不能为空");


        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/subscribers/{clientId}/messages",   // 路径
            headers,                   // 请求头
            null,                      // 查询参数
            false                      // 使用API版本
        );

        return response;
    }
    /// <summary>
    /// 查询服务号给某用户发的消息(查询结果包含服务号发送的订阅广播消息也包含单独发送的消息。)
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> QuerySendFormClientId(string clientId)
    {
        return await QuerySendFormClientId(SysConvId, clientId);
    }

    #endregion

    #region//广播
    /// <summary>
    ///  查询用户数量
    /// </summary>
    /// <returns> { "result": { "online_user_count": 10212, "user_count_today": 1002324 } } </returns>
    public async Task<Dictionary<string, object>> QueryUserTCount()
    {
        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };
        // 调用Post方法发送请求
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            "1.2/rtm/stats",                   // 路径
            headers,                           // 请求头
            null,                              // 查询参数
            false                              // 使用API版本
        );
        // 返回订阅结果
        return response;
    }

    /// <summary>
    /// 查询所有会话
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> QueryAllConversation()
    {
        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };

        // 调用Post方法发送请求
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            "1.2/rtm/all-conversations",       // 路径
            headers,                           // 请求头
            null,                              // 查询参数
            false                              // 使用API版本
        );
        // 返回订阅结果
        return response;
    }

    /// <summary>
    /// 查询所有会话
    /// </summary>
    /// <param name="total"></param>
    /// <param name="convName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<Dictionary<string, object>> QueryAllConversation(int total,string convName)
    {
        if (string.IsNullOrEmpty(convName))
            throw new ArgumentNullException(nameof(convName), "服务号对话ID不能为空");

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
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

        // 调用Post方法发送请求
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            "1.2/rtm/all-conversations",       // 路径
            headers,                           // 请求头
            queryParams,                       // 查询参数
            false                              // 使用API版本
        );
        // 返回订阅结果
        return response;
    }

    /// <summary>
    /// 全局广播
    /// </summary>
    /// <param name="fromClientId">消息的发件人 id</param>
    /// <param name="conversationId">发送到对话 id，仅限于服务号</param>
    /// <param name="message">消息内容（这里的消息内容的本质是字符串，但是我们对字符串内部的格式没有做限定，理论上开发者可以随意发送任意格式，只要大小不超过 5 KB 限制即可。）</param>
    /// <param name="valid_till">（可选）过期时间，UTC 时间戳（毫秒），最长为 1 个月之后。默认值为 1 个月后。</param>
    /// <param name="transient">（可选）默认为 false。该字段用于表示广播消息是否为暂态消息，暂态消息只会被当前在线的用户收到，不在线的用户再次上线后也收不到该消息。</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    async Task<Dictionary<string, object>> Broadcast(string fromClientId, string conversationId,string message,
        float valid_till,bool transient)
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
            { "conv_id",conversationId},
            { "message",message },
            { "valid_till",valid_till },
            { "transient",transient },
        };

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm//broadcasts",    // 路径
            headers,                   // 请求头
            requestData,               // 请求数据
            null,                      // 查询参数
            false                      // 使用API版本
        );

        return response;
    }
    /// <summary>
    /// 全局广播
    /// </summary>
    /// <param name="message">消息内容（这里的消息内容的本质是字符串，但是我们对字符串内部的格式没有做限定，理论上开发者可以随意发送任意格式，只要大小不超过 5 KB 限制即可。）</param>
    /// <param name="valid_till">（可选）过期时间，UTC 时间戳（毫秒），最长为 1 个月之后。默认值为 1 个月后。</param>
    /// <param name="transient">（可选）默认为 false。该字段用于表示广播消息是否为暂态消息，暂态消息只会被当前在线的用户收到，不在线的用户再次上线后也收不到该消息。</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> Broadcast(string message, float valid_till, bool transient)
    { 
        return await Broadcast(SysConvId, SysIMClientService.Inst.SysIMClient.Id, message, valid_till,transient);
    }
    /// <summary>
    /// 广播消息修改仅对当前还未收到该广播消息的设备生效，如果目标设备已经收到了该广播消息则无法修改。请慎重发送广播消息。
    /// </summary>
    /// <param name="fromClientId"></param>
    /// <param name="timestamp"></param>
    /// <param name="message"></param>
    /// <param name="conversationId"></param>
    /// <param name="messageid"></param>
    /// <returns>成功则返回状态码 200 OK</returns>
    /// <exception cref="ArgumentNullException"></exception>
    async Task<string> UpdateBroadcast(string fromClientId, int timestamp, string message,
        string conversationId,string messageid)
    {
        if (string.IsNullOrEmpty(fromClientId))
            throw new ArgumentNullException(nameof(fromClientId), "服务号对话ID不能为空");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message), "消息不能为空");

        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "服务号id不能为空");

        if (string.IsNullOrEmpty(messageid))
            throw new ArgumentNullException(nameof(messageid), "消息 ID 不能为空");

        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "from_client",fromClientId },
            { "message",message },
            { "timestamp", timestamp}
        };

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Put<string>(
            $"1.2/rtm/service-conversations/{conversationId}/messages/{messageid}",    // 路径
            headers,                   // 请求头
            requestData,               // 请求数据
            null,                      // 查询参数
            false                      // 使用API版本
        );

        return response;
    }

    /// <summary>
    /// 广播消息修改仅对当前还未收到该广播消息的设备生效，如果目标设备已经收到了该广播消息则无法修改。请慎重发送广播消息。
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="message">修改后的消息</param>
    /// <param name="messageId"></param>
    /// <returns>成功则返回状态码 200 OK</returns>
    public async Task<string> UpdateBroadcast(int timestamp, string message,string messageId)
    {
        return await UpdateBroadcast(SysIMClientService.Inst.SysIMClient.Id, timestamp, message, SysConvId, messageId);
    }

    /// <summary>
    /// 删除已发布的广播消息，仅对还未收到广播消息的设备生效，已收到广播消息的设备无法删除消息。
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task DeleteBroadcast(string messageId)
    {
        if (string.IsNullOrEmpty(messageId))
            throw new ArgumentNullException(nameof(messageId), "消息 ID 不能为空");

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        await LCCore.HttpClient.Delete(
            $"1.2/rtm/broadcasts/{messageId}",    // 路径
            headers,                   // 请求头
            null,                      // 查询参数
            null,                      // 查询参数
            false                      // 使用API版本
        );
    }

    #endregion

    #region//用户
    /// <summary>
    /// 查询用户已经发送的消息
    /// </summary>
    /// <param name="targetClientId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<Dictionary<string, object>> QueryUserSentMessages(string targetClientId)
    {
        if (string.IsNullOrEmpty(targetClientId))
            throw new ArgumentNullException(nameof(targetClientId), "目标客户端ID不能为空");
        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };

        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Get<Dictionary<string, object>>(
            $"1.2/rtm/clients/{targetClientId}/messages",    // 路径
            headers,                   // 请求头
            null,                      // 查询参数
            false                      // 使用API版本
        );

        return response;
    }
    /// <summary>
    /// 强制目标用户下线下线
    /// </summary>
    /// <param name="logout"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<Dictionary<string, object>> ForceLogout(string targetClientId,string logout) 
    {
        if (string.IsNullOrEmpty(targetClientId))
            throw new ArgumentNullException(nameof(targetClientId), "目标客户端ID不能为空");

        if (string.IsNullOrEmpty(logout))
            throw new ArgumentNullException(nameof(logout), "下线原因");
        
        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{ Environment.GetEnvironmentVariable("MASTER_KEY")},master"  }
        };

        var requestData = new Dictionary<string, object>
        {
            { "reason",logout}
        };

        // 调用Post方法发送请求
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/clients/{targetClientId}/kick",   // 路径
            headers,                           // 请求头
            requestData,                       // 请求数据
            null,                              // 查询参数
            false                              // 使用API版本
        );
        return response;
    }
    #endregion
}