using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using web;

#region//接收http消息

// 消息模型
public class IMMessageResult
{
    public List<IMMessage> Results { get; set; }
}

public class IMMessage
{
    public string Id { get; set; } // 消息 ID
    public string FromPeer { get; set; } // 发送者 clientId
    public MessageContent Message { get; set; } // 消息内容
    public long SentAt { get; set; } // 发送时间戳
}

public class MessageContent
{
    [JsonProperty("_lctype")]
    public int Type { get; set; } // -1 表示文本消息

    [JsonProperty("_lctext")]
    public string Text { get; set; } // 文本内容
}

#endregion

public class HttpClientIMService
{
    private readonly string appId;
    private readonly string masterKey;
    private readonly string imServerUrl;

    public static HttpClient httpClient;


    public HttpClientIMService(string appId, string masterKey, string imServerUrl)
    {
        this.appId = appId;
        this.masterKey = masterKey;
        this.imServerUrl = imServerUrl;
        httpClient = new HttpClient();
    }


    public async Task<bool> SendMessageToTable()
    {
        var requestData = new
        {
            IsProcessed = false,
            content = new Dictionary<string, object>
                {
                    {"aaa",123}
                }
        };

        string json = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // 可以添加额外的请求头（如果需要）
        var headers = new Dictionary<string, object>
            {
                // 例如添加认证信息或其他必要头信息
                 {"customHeaders", "Authorization"}
            };

        // 调用Post方法发送请求
        // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
        var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
            "classes/customMessage",  // 路径
            headers,                   // 请求头
            requestData,               // 请求数据
            null,                      // 查询参数
            true                       // 使用API版本
        );
        return true;
    }


    // 创建对话
    public async Task<string> CreateConversation(string senderId, string targetId)
    {
        if (string.IsNullOrEmpty(senderId)) 
        {
            throw new ArgumentNullException(nameof(senderId), "发送者ID不能为空");
        }
            

        if (string.IsNullOrEmpty(targetId)) 
        {
            throw new ArgumentNullException(nameof(senderId), "目标用户ID不能为空");
        }

        try
        {
            // 准备请求数据
            var requestData = new
            {
                members = new[] { senderId, targetId },
                unique = true,
                sys = true,
            };

            string json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 可以添加额外的请求头（如果需要）
            var headers = new Dictionary<string, object>
            {
                // 例如添加认证信息或其他必要头信息
                 {"customHeaders", "Authorization"}
            };

            // 调用Post方法发送请求
            // 假设API版本已经在Post方法内部处理，withAPIVersion设为true
            var response = await LCCore.HttpClient.Post<Dictionary<string, object>>(
                "classes/_Conversation",   // 路径
                headers,                   // 请求头
                requestData,               // 请求数据
                null,                      // 查询参数
                true                       // 使用API版本
            );

            //SetAuthHeaders();

            // 验证是否包含id字段
            if (!response.TryGetValue("objectId", out object objectId) || objectId == null)
            {
                throw new InvalidOperationException(
                    $"创建会话返回结果不包含有效的id字段，响应内容: {response}");
            }

            return objectId.ToString();

        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("解析会话创建响应失败", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("与服务器通信时发生错误", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("创建会话时发生意外错误", ex);
        }

    }
    public async Task<string> SendMessage(string conversationId, string senderId,string targetID, string messageContent)
    {
        if (string.IsNullOrEmpty(conversationId))
            throw new ArgumentNullException(nameof(conversationId));

        if (string.IsNullOrEmpty(senderId))
            throw new ArgumentNullException(nameof(senderId));

        if (string.IsNullOrEmpty(messageContent))
            throw new ArgumentNullException(nameof(messageContent));

        try
        {

            // 构建消息数据
            var messageData = new Dictionary<string, object>
            {
                { "conv_Id", conversationId },
                { "to_peer_ids",targetID },
                { "from_peer", senderId },
                { "processed",false},
                { "msg", new Dictionary<string, object>
                    {
                        { "type", "text" },
                        { "text", messageContent }
                    }
                },
                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
            };

            // 可以添加额外的请求头（如果需要）
            var headers = new Dictionary<string, object>
            {
                // 例如添加认证信息或其他必要头信息
                 {"X-User-Id", targetID },
            };

            // 发送消息
            var response = await LCCore.HttpClient.Post<Dictionary<string, object>>
            (
                "classes/customMessages", // LeanCloud消息发送端点
                headers: headers,         // 请求头
                data: messageData,        // 请求数据
                null,                     // 查询参数
                true                      // 使用API版本
            );

            // 验证是否包含id字段
            if (!response.TryGetValue("objectId", out object objectId) || objectId == null)
            {
                throw new InvalidOperationException("发送消息失败，未返回有效的msgId");
            }

            return objectId.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"发送消息时发生错误: {ex.Message}", ex);
        }
    }

    // 设置认证头
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
            await SendMessage(conversationId, serverClientId, targetClientId, message);
        }
        catch (Exception ex)
        {
            LCLogger.Debug("发送 IM 消息失败: " + ex.Message);
        }
    }



    /// <summary>
    /// 轮询拉取指定对话的新消息
    /// </summary>
    /// <param name="conversationId">对话 ID</param>
    /// <param name="lastMessageId">上次拉取的最后一条消息 ID（用于增量拉取）</param>
    public async Task PollMessages(string conversationId, string lastMessageId = "")
    {
        try
        {
            // 构建请求 URL（带分页和增量拉取参数）
            string url = $"{imServerUrl}/1.0/conversations/{conversationId}/messages?limit=20";
            if (!string.IsNullOrEmpty(lastMessageId))
            {
                url += $"&since_id={lastMessageId}"; // 只拉取此 ID 之后的消息
            }

            // 设置认证头
            SetAuthHeaders();

            // 发送 GET 请求拉取消息
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // 解析消息
            string result = await response.Content.ReadAsStringAsync();
            var messages = JsonConvert.DeserializeObject<IMMessageResult>(result);

            // 处理消息
            foreach (var msg in messages.Results)
            {
                Console.WriteLine($"收到消息：{msg.Message.Text}（来自 {msg.FromPeer}）");
                // 业务逻辑处理（如存储、转发等）
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"拉取消息失败：{ex.Message}");
        }
    }


}