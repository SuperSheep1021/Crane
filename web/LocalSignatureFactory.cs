using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class LocalSignatureFactory : ILCIMSignatureFactory
{
    static string m_MasterKey;
    public LocalSignatureFactory(string key) 
    {
        m_MasterKey = key;
    }
    private static string SignSHA1(string key, string text)
    {
        HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
        byte[] bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
        string signature = BitConverter.ToString(bytes).Replace("-", string.Empty);
        return signature;
    }
    private static string NewNonce()
    {
        byte[] bytes = new byte[10];
        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }
    public static string GenerateSignature(params string[] args)
    {
        string text = string.Join(":", args);
        string signature = SignSHA1(m_MasterKey, text);
        return signature;
    }
    public Task<LCIMSignature> CreateConnectSignature(string clientId)
    {
        long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();                     
        string nonce = NewNonce();
        string signature = GenerateSignature( LCCore.AppId,LCCore.AppKey ,clientId,timestamp.ToString(), nonce);
        return Task.FromResult(new LCIMSignature
        {
            Signature = signature,
            Timestamp = timestamp,
            Nonce = nonce
        });
    }
    public Task<LCIMSignature> CreateStartConversationSignature(string clientId, IEnumerable<string> memberIds)
    {
        string sortedMemberIds = string.Empty;
        if (memberIds != null)
        {
            List<string> sortedMemberList = memberIds.ToList();
            sortedMemberList.Sort();
            sortedMemberIds = string.Join(":", sortedMemberList);
        }
        long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        string nonce = NewNonce();
        string signature = GenerateSignature(LCCore.AppId, LCCore.AppKey, clientId, sortedMemberIds, timestamp.ToString(), nonce);
        return Task.FromResult(new LCIMSignature
        {
            Signature = signature,
            Timestamp = timestamp,
            Nonce = nonce
        });
    }
    public Task<LCIMSignature> CreateConversationSignature(string clientId, string conversationId, IEnumerable<string> memberIds, string action)
    {
        string sortedMemberIds = string.Empty;
        if (memberIds != null)
        {
            List<string> sortedMemberList = memberIds.ToList();
            sortedMemberList.Sort();
            sortedMemberIds = string.Join(":", sortedMemberList);
        }
        long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        string nonce = NewNonce();
        string signature = GenerateSignature(LCCore.AppId, LCCore.AppKey,clientId, conversationId, sortedMemberIds, timestamp.ToString(), nonce, action);
        return Task.FromResult(new LCIMSignature
        {
            Signature = signature,
            Timestamp = timestamp,
            Nonce = nonce
        });
    }
    public Task<LCIMSignature> CreateBlacklistSignature(string clientId, string conversationId, IEnumerable<string> memberIds, string action )
    {
        string sortedMemberIds = string.Empty;
        if (memberIds != null)
        {
            List<string> sortedMemberList = memberIds.ToList();
            sortedMemberList.Sort();
            sortedMemberIds = string.Join(":", sortedMemberList);
        }
        long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        string nonce = NewNonce();
        string signature = GenerateSignature(LCCore.AppId, LCCore.AppKey, clientId, conversationId, sortedMemberIds, timestamp.ToString(), nonce, action);
        return Task.FromResult(new LCIMSignature
        {
            Signature = signature,
            Timestamp = timestamp,
            Nonce = nonce
        });
    }
}