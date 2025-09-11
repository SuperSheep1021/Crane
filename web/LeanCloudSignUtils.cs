using System;
using System.Security.Cryptography;
using System.Text;

public static class LeanCloudSignUtils
{
    /// <summary>
    /// 生成 LeanCloud API 请求签名
    /// </summary>
    /// <param name="masterKey">应用的 Master Key</param>
    /// <param name="timestamp">UTC 时间戳（秒级）</param>
    /// <returns>生成的签名字符串</returns>
    public static string GenerateSign(string masterKey, long timestamp)
    {
        // 签名规则：将 timestamp 与 masterKey 拼接后进行 SHA1 哈希，再转为小写十六进制
        string input = $"{timestamp}{masterKey}";
        using (var sha1 = SHA1.Create())
        {
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.AppendFormat("{0:x2}", b); // 转为十六进制（小写）
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// 获取当前 UTC 时间戳（秒级）
    /// </summary>
    public static long GetCurrentTimestamp()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}