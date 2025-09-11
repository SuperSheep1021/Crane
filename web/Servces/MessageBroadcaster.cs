using LeanCloud.Realtime;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class MessageBroadcaster
{
    // 系统服务账号
    private const string SystemClientId = "system_broadcast";
    AVIMClient _systemClient;
    AVRealtime m_Realtime;
    // 初始化
    public async Task Initialize(string appId, string masterKey, string serverUrl)
    {
        LCApplication.Initialize(appId, masterKey, serverUrl);
        await LCApplication.Start();

        // 登录系统账号
        _systemClient = new AVIMClient(SystemClientId,"StyClient",);
    }

    // 获取所有用户ID（分页查询）
    private async Task<List<string>> GetAllUserIds(int batchSize = 100)
    {
        var userIds = new List<string>();
        int skip = 0;
        bool hasMore = true;

        try
        {
            while (hasMore)
            {
                // 查询用户表获取用户ID
                var query = new LCQuery<LCUser>("_User");
                query.Limit = batchSize;
                query.Skip = skip;
                query.Select(new List<string> { "objectId" }); // 只获取ID，提高效率

                var users = await query.Find();
                if (users.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                // 提取用户ID
                userIds.AddRange(users.Select(u => u.ObjectId));
                skip += batchSize;

                // 避免请求过于频繁
                await Task.Delay(100);
            }

            Console.WriteLine($"共获取到 {userIds.Count} 个用户");
            return userIds;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取用户列表失败: {ex.Message}");
            return userIds;
        }
    }

    // 给所有用户发送文本消息
    public async Task BroadcastTextMessage(string content, int batchSendSize = 20)
    {
        if (_systemClient == null)
        {
            throw new InvalidOperationException("请先初始化服务");
        }

        // 1. 获取所有用户ID
        var allUserIds = await GetAllUserIds();
        if (allUserIds.Count == 0)
        {
            Console.WriteLine("没有用户需要发送消息");
            return;
        }

        // 2. 构建消息内容
        var messageContent = new Dictionary<string, object>
        {
            { "_lctype", 1 },
            { "_lctext", content }
        };

        // 3. 批量发送消息（控制并发数量）
        int successCount = 0;
        int failCount = 0;

        // 分批次处理，每批发送一定数量
        for (int i = 0; i < allUserIds.Count; i += batchSendSize)
        {
            var batchUserIds = allUserIds.Skip(i).Take(batchSendSize).ToList();
            var tasks = batchUserIds.Select(userId =>
                SendToSingleUser(userId, messageContent)
                    .ContinueWith(t =>
                    {
                        if (t.Result) successCount++;
                        else failCount++;
                    })
            );

            // 等待当前批次完成
            await Task.WhenAll(tasks);
            Console.WriteLine($"已处理 {Math.Min(i + batchSendSize, allUserIds.Count)}/{allUserIds.Count} 个用户");

            // 每批发送后休息一下，避免触发频率限制
            if (i + batchSendSize < allUserIds.Count)
            {
                await Task.Delay(1000);
            }
        }

        Console.WriteLine($"广播完成 - 成功: {successCount}, 失败: {failCount}");
    }

    // 给单个用户发送消息
    private async Task<bool> SendToSingleUser(string targetUserId, Dictionary<string, object> content)
    {
        try
        {
            // 获取或创建与该用户的对话
            var conversation = await GetOrCreateConversation(targetUserId);
            if (conversation == null)
            {
                Console.WriteLine($"无法与用户 {targetUserId} 创建对话");
                return false;
            }

            // 发送消息
            var message = new LCIMTypedMessage
            {
                ConversationId = conversation.Id,
                FromClientId = SystemClientId,
                ToClientIds = new List<string> { targetUserId },
                Content = content
            };

            await conversation.Send(message);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"给用户 {targetUserId} 发送消息失败: {ex.Message}");
            return false;
        }
    }

    // 获取或创建与单个用户的对话
    private async Task<LCIMConversation> GetOrCreateConversation(string targetUserId)
    {
        try
        {
            // 尝试查找已有对话
            var conversations = await _systemClient.GetConversations();
            var existingConv = conversations.FirstOrDefault(
                c => c.Members.Contains(targetUserId) && c.Members.Count == 2
            );

            if (existingConv != null)
            {
                return existingConv;
            }

            // 创建新对话
            return await _systemClient.CreateConversation(
                members: new List<string> { targetUserId },
                name: "系统广播",
                isUnique: true
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"与用户 {targetUserId} 处理对话失败: {ex.Message}");
            return null;
        }
    }

    // 使用示例
    public static async Task Main(string[] args)
    {
        var broadcaster = new MessageBroadcaster();

        // 初始化（替换为你的配置）
        await broadcaster.Initialize(
            appId: "你的AppID",
            masterKey: "你的MasterKey",
            serverUrl: "你的服务器地址"
        );

        // 发送广播消息
        await broadcaster.BroadcastTextMessage("这是一条系统广播消息，所有用户都会收到！");
    }
}