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
        LCLogger.Debug($"{this}开始初始化");
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
        
        LCLogger.Debug($"{this}结束初始化");

    }
    private void OnIMMessageReceived(LCIMConversation conversation, LCIMMessage message)
    {
        LCLogger.Warn($"收到来自对话 [{conversation.Name}] 的消息");

    }
    public async Task<string> CreateServiceConversationAsync()
    {
        // 构建请求URL
        //var url = $"https://{LCCore.AppRouter.GetApiServer() }/1.2/rtm/service-conversations";

        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "name","My First Service-conversation"}
        };
        //var jsonData = JsonConvert.SerializeObject(requestData);
        //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");


        //string json = await LCJsonUtils.SerializeAsync(requestData);

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" }
        };

        // 调用Post方法发送请求
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            "1.2/rtm/service-conversations",   // 路径
            headers,                   // 请求头
            requestData,               // 请求数据
            null,                      // 查询参数
            false                       // 使用API版本
        );



        // 返回订阅结果
        return response.ToString();
    }


    public async Task<string> SubscribeServiceConversationAsync(string conversationId, string userId)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "服务号对话ID不能为空");

        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId), "用户ID不能为空");


        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "client_id", $"{userId}" }
        };

        //var jsonData = JsonConvert.SerializeObject(requestData);
        //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");


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
            false                       // 使用API版本
        );

        // 返回订阅结果
        return response.ToString();
    }


    public async Task<string> SendSubscribeServiceConversationAsync(string conversationId )
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId), "服务号对话ID不能为空");

        // 构建请求数据（添加用户到订阅者列表）
        var requestData = new Dictionary<string, object>
        {
            { "from_client","68c22ec62f7ee809fcc9e7e6" },
            { "message","service send subscribe message!!!!" },
        };

        //var jsonData = JsonConvert.SerializeObject(requestData);
        //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");


        //string json = await LCJsonUtils.SerializeAsync(requestData);

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
        {
            // 例如添加认证信息或其他必要头信息
            { "X-LC-Key",$"{Environment.GetEnvironmentVariable("MASTER_KEY")},master" },
        };
        // 调用Post方法发送请求
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            $"1.2/rtm/service-conversations/{conversationId}/broadcasts",   // 路径
            headers,                   // 请求头
            requestData,               // 请求数据
            null,                      // 查询参数
            false                       // 使用API版本
        );

        // 返回订阅结果
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
        message["数据1"] = "asdasd";
        message["数据2"] = "消息2";
        message["数据3"] = 123123;
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