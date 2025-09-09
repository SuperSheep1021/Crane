using Newtonsoft.Json;

public class ClientStatusEventPayload
{
    [JsonProperty("clientId")]
    public string ClientId { get; set; } // 上下线的客户端 ID

    [JsonProperty("sessionToken")]
    public string SessionToken { get; set; } // 会话 Token（传统字段）

    [JsonProperty("deviceId")]
    public string DeviceId { get; set; } // 设备标识（新字段，更推荐）

    [JsonProperty("reason")]
    public string Reason { get; set; } // 仅 _clientOffline 有，断开原因
}