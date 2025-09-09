
public class ClientStatusEventPayload
{
    public string ClientId { get; set; } // 上下线的客户端 ID

    public string SessionToken { get; set; } // 会话 Token（传统字段）

    public string DeviceId { get; set; } // 设备标识（新字段，更推荐）

    public string Reason { get; set; } // 仅 _clientOffline 有，断开原因
}