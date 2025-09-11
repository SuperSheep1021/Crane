using LeanCloud.Realtime;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class MessageBroadcaster
{
    // ϵͳ�����˺�
    private const string SystemClientId = "system_broadcast";
    AVIMClient _systemClient;
    AVRealtime m_Realtime;
    // ��ʼ��
    public async Task Initialize(string appId, string masterKey, string serverUrl)
    {
        LCApplication.Initialize(appId, masterKey, serverUrl);
        await LCApplication.Start();

        // ��¼ϵͳ�˺�
        _systemClient = new AVIMClient(SystemClientId,"StyClient",);
    }

    // ��ȡ�����û�ID����ҳ��ѯ��
    private async Task<List<string>> GetAllUserIds(int batchSize = 100)
    {
        var userIds = new List<string>();
        int skip = 0;
        bool hasMore = true;

        try
        {
            while (hasMore)
            {
                // ��ѯ�û����ȡ�û�ID
                var query = new LCQuery<LCUser>("_User");
                query.Limit = batchSize;
                query.Skip = skip;
                query.Select(new List<string> { "objectId" }); // ֻ��ȡID�����Ч��

                var users = await query.Find();
                if (users.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                // ��ȡ�û�ID
                userIds.AddRange(users.Select(u => u.ObjectId));
                skip += batchSize;

                // �����������Ƶ��
                await Task.Delay(100);
            }

            Console.WriteLine($"����ȡ�� {userIds.Count} ���û�");
            return userIds;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"��ȡ�û��б�ʧ��: {ex.Message}");
            return userIds;
        }
    }

    // �������û������ı���Ϣ
    public async Task BroadcastTextMessage(string content, int batchSendSize = 20)
    {
        if (_systemClient == null)
        {
            throw new InvalidOperationException("���ȳ�ʼ������");
        }

        // 1. ��ȡ�����û�ID
        var allUserIds = await GetAllUserIds();
        if (allUserIds.Count == 0)
        {
            Console.WriteLine("û���û���Ҫ������Ϣ");
            return;
        }

        // 2. ������Ϣ����
        var messageContent = new Dictionary<string, object>
        {
            { "_lctype", 1 },
            { "_lctext", content }
        };

        // 3. ����������Ϣ�����Ʋ���������
        int successCount = 0;
        int failCount = 0;

        // �����δ���ÿ������һ������
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

            // �ȴ���ǰ�������
            await Task.WhenAll(tasks);
            Console.WriteLine($"�Ѵ��� {Math.Min(i + batchSendSize, allUserIds.Count)}/{allUserIds.Count} ���û�");

            // ÿ�����ͺ���Ϣһ�£����ⴥ��Ƶ������
            if (i + batchSendSize < allUserIds.Count)
            {
                await Task.Delay(1000);
            }
        }

        Console.WriteLine($"�㲥��� - �ɹ�: {successCount}, ʧ��: {failCount}");
    }

    // �������û�������Ϣ
    private async Task<bool> SendToSingleUser(string targetUserId, Dictionary<string, object> content)
    {
        try
        {
            // ��ȡ�򴴽�����û��ĶԻ�
            var conversation = await GetOrCreateConversation(targetUserId);
            if (conversation == null)
            {
                Console.WriteLine($"�޷����û� {targetUserId} �����Ի�");
                return false;
            }

            // ������Ϣ
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
            Console.WriteLine($"���û� {targetUserId} ������Ϣʧ��: {ex.Message}");
            return false;
        }
    }

    // ��ȡ�򴴽��뵥���û��ĶԻ�
    private async Task<LCIMConversation> GetOrCreateConversation(string targetUserId)
    {
        try
        {
            // ���Բ������жԻ�
            var conversations = await _systemClient.GetConversations();
            var existingConv = conversations.FirstOrDefault(
                c => c.Members.Contains(targetUserId) && c.Members.Count == 2
            );

            if (existingConv != null)
            {
                return existingConv;
            }

            // �����¶Ի�
            return await _systemClient.CreateConversation(
                members: new List<string> { targetUserId },
                name: "ϵͳ�㲥",
                isUnique: true
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"���û� {targetUserId} ����Ի�ʧ��: {ex.Message}");
            return null;
        }
    }

    // ʹ��ʾ��
    public static async Task Main(string[] args)
    {
        var broadcaster = new MessageBroadcaster();

        // ��ʼ�����滻Ϊ������ã�
        await broadcaster.Initialize(
            appId: "���AppID",
            masterKey: "���MasterKey",
            serverUrl: "��ķ�������ַ"
        );

        // ���͹㲥��Ϣ
        await broadcaster.BroadcastTextMessage("����һ��ϵͳ�㲥��Ϣ�������û������յ���");
    }
}