
public class ClientStatusEventPayload
{
    public string ClientId { get; set; } // �����ߵĿͻ��� ID

    public string SessionToken { get; set; } // �Ự Token����ͳ�ֶΣ�

    public string DeviceId { get; set; } // �豸��ʶ�����ֶΣ����Ƽ���

    public string Reason { get; set; } // �� _clientOffline �У��Ͽ�ԭ��
}