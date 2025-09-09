using Newtonsoft.Json;

public class ClientStatusEventPayload
{
    [JsonProperty("clientId")]
    public string ClientId { get; set; } // �����ߵĿͻ��� ID

    [JsonProperty("sessionToken")]
    public string SessionToken { get; set; } // �Ự Token����ͳ�ֶΣ�

    [JsonProperty("deviceId")]
    public string DeviceId { get; set; } // �豸��ʶ�����ֶΣ����Ƽ���

    [JsonProperty("reason")]
    public string Reason { get; set; } // �� _clientOffline �У��Ͽ�ԭ��
}