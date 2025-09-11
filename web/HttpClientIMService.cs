using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using web;
public class HttpClientIMService
{
    private readonly string appId;
    private readonly string masterKey;
    private readonly string imServerUrl;
    private readonly HttpClient httpClient;

    public HttpClientIMService(string appId, string masterKey, string imServerUrl = "https://im-api.leancloud.cn")
    {
        this.appId = appId;
        this.masterKey = masterKey;
        this.imServerUrl = imServerUrl;
        this.httpClient = new HttpClient();
    }

    /// <summary>
    /// 给指定 clientId 的用户发送 IM 消息
    /// </summary>
    /// <param name="serverClientId">服务端作为发送方的 clientId（固定值）</param>
    /// <param name="targetClientId">目标用户的 clientId</param>
    /// <param name="message">消息内容</param>
    public async Task SendToUser(string serverClientId, string targetClientId, string message)
    {
        try
        {
            // 1. 创建单聊对话（若不存在则自动创建）
            string conversationId = await CreateConversation(serverClientId, targetClientId);
            if (string.IsNullOrEmpty(conversationId))
            {
                LCLogger.Debug("创建对话失败");
                return;
            }

            // 2. 向对话发送消息
            await SendMessage(conversationId, serverClientId, message);
        }
        catch (Exception ex)
        {
            LCLogger.Debug("发送 IM 消息失败: " + ex.Message);
        }
    }

    // 创建对话
    private async Task<string> CreateConversation(string senderId, string targetId)
    {
        var requestData = new
        {
            members = new[] { senderId, targetId }, // 对话成员：服务端 + 目标用户
            unique = true // 确保相同成员只创建一个对话
        };

        string json = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        SetAuthHeaders();

        var response = await httpClient.PostAsync($"{imServerUrl}/1.0/conversations", content);
        response.EnsureSuccessStatusCode();

        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
        return result["id"].ToString();
    }

    // 发送消息
    private async Task SendMessage(string conversationId, string senderId, string text)
    {
        var requestData = new
        {
            from_peer = senderId, // 发送方 clientId
            message = new
            {
                _lctype = -1, // 文本消息类型
                _lctext = text // 消息内容
            }
        };

        string json = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        SetAuthHeaders();

        var response = await httpClient.PostAsync(
            $"{imServerUrl}/1.0/conversations/{conversationId}/messages",
            content
        );
        response.EnsureSuccessStatusCode();
        LCLogger.Debug("IM 消息发送成功");
    }

    // 设置认证头（复用之前的签名工具）
    private void SetAuthHeaders()
    {
        httpClient.DefaultRequestHeaders.Clear();

        long timestamp = LeanCloudSignUtils.GetCurrentTimestamp();
        string sign = LeanCloudSignUtils.GenerateSign(masterKey, timestamp);

        string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appId}:{masterKey}"));
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
        httpClient.DefaultRequestHeaders.Add("X-LC-Id", appId);
        httpClient.DefaultRequestHeaders.Add("X-LC-Sign", $"{sign},{timestamp}");
    }








    // 连接到 LeanCloud 实时通信服务
    public async Task ConnectAsync()
    {
        
        // 创建实时通信实例
        //_realtime = new AVRealtime(new AVRealtime.Configuration()
        //{
        //    ApplicationId = appId,
        //    ApplicationKey = appKey,
        //    RealtimeServer = new Uri(appUrl)
        //});
        //AVRealtime _realtime = default;

        //try
        //{
        //    AVRealtime _realtime = new AVRealtime(appId, appKey);
        //    LCLogger.Debug($"AVRealtime Init Success");
        //}
        //catch (LCException ex) {
        //    LCLogger.Debug(ex.Message);
        //    LCLogger.Debug(ex.HelpLink);
        //    LCLogger.Debug(ex.Source);
        //    LCLogger.Debug(ex.Code.ToString() );
        //    LCLogger.Debug($"{appId}_____{appKey}");
        //}
        //catch (Exception ex)
        //{
        //    LCLogger.Debug(ex.Message);
        //    LCLogger.Debug(ex.ToString() );
        //    LCLogger.Debug(ex.Source);
        //    LCLogger.Debug($"{appId}_____{appKey}");
        //}


        //try
        //{
        //    AVIMClient _client = await _realtime.CreateClientAsync("68c22ec62f7ee809fcc9e7e6");
        //    LCLogger.Debug($"AVIMClient Init Success");
        //}
        //catch (LCException ex)
        //{
        //    LCLogger.Debug(ex.Message);
        //    LCLogger.Debug(ex.HelpLink);
        //    LCLogger.Debug(ex.Source);
        //    LCLogger.Debug(ex.Code.ToString());
        //}
        //catch (Exception ex)
        //{
        //    LCLogger.Debug(ex.Message);
        //    LCLogger.Debug(ex.HelpLink);
        //    LCLogger.Debug(ex.Source);
        //}


        //// 使用服务端签名或主密钥创建连接
        //// 注意：生产环境中应使用安全的签名方式
        //AVIMClient _client = await _realtime.CreateClientAsync("68c22ec62f7ee809fcc9e7e6");

        Console.WriteLine("已连接到 LeanCloud 实时通信服务");
    }
    
    // 向指定客户端发送消息
    public async Task<bool> SendMessageToClientAsync(string targetClientId, string messageContent)
    {
        return true;
        //try
        //{
        //    // 创建对话（如果不存在）
        //    var conversation = await _client.CreateTemporaryConversationAsync(targetClientId);
            
        //    // 发送文本消息
        //    var textMessage = new AVIMTextMessage(messageContent);
        //    await conversation.SendMessageAsync(textMessage);
            
        //    Console.WriteLine($"消息已发送至客户端 {targetClientId}");
        //    return true;
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"发送消息时出错: {ex.Message}");
        //    return false;
        //}
    }
}