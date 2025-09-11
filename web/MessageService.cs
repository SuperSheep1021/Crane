using System;
using System.Threading.Tasks;
using LeanCloud;
using LeanCloud.Realtime;

public class MessageService
{
    private readonly AVRealtime _realtime;
    private AVIMClient _client;
    public MessageService()
    {
        // 创建实时通信实例
        _realtime = new AVRealtime(new AVRealtime.Configuration()
        {
            ApplicationId = "你的AppId",
            ApplicationKey = "你的AppKey",
            RealtimeServer = new Uri("你的服务器地址")
        });
    }
    
    // 连接到 LeanCloud 实时通信服务
    public async Task ConnectAsync()
    {
        

        // 使用服务端签名或主密钥创建连接
        // 注意：生产环境中应使用安全的签名方式
        _client = await _realtime.CreateClientAsync("68c22ec62f7ee809fcc9e7e6");
        
        Console.WriteLine("已连接到 LeanCloud 实时通信服务");
    }
    
    // 向指定客户端发送消息
    public async Task<bool> SendMessageToClientAsync(string targetClientId, string messageContent)
    {
        try
        {
            // 创建对话（如果不存在）
            var conversation = await _client.CreateTemporaryConversationAsync(targetClientId);
            
            // 发送文本消息
            var textMessage = new AVIMTextMessage(messageContent);
            await conversation.SendMessageAsync(textMessage);
            
            Console.WriteLine($"消息已发送至客户端 {targetClientId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发送消息时出错: {ex.Message}");
            return false;
        }
    }
}