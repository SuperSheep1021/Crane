using System;
using System.Security.Cryptography;
using System.Text;

public static class LeanCloudSignUtils
{
    /// <summary>
    /// ���� LeanCloud API ����ǩ��
    /// </summary>
    /// <param name="masterKey">Ӧ�õ� Master Key</param>
    /// <param name="timestamp">UTC ʱ������뼶��</param>
    /// <returns>���ɵ�ǩ���ַ���</returns>
    public static string GenerateSign(string masterKey, long timestamp)
    {
        // ǩ�����򣺽� timestamp �� masterKey ƴ�Ӻ���� SHA1 ��ϣ����תΪСдʮ������
        string input = $"{timestamp}{masterKey}";
        using (var sha1 = SHA1.Create())
        {
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.AppendFormat("{0:x2}", b); // תΪʮ�����ƣ�Сд��
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// ��ȡ��ǰ UTC ʱ������뼶��
    /// </summary>
    public static long GetCurrentTimestamp()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}