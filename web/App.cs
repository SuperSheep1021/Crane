using LeanCloud;
using LeanCloud.Engine;
using LeanCloud.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace web {
    public class App
    {
        public static HttpClientIMService _httpClientService;
        const string TestTargetUserID = "68b9286c49adb47c41678afb";
        //// Function
        //[LCEngineFunction("hello")]
        //public static string Hello([LCEngineFunctionParam("name")] string name)
        //{
        //    string msg = $"hello, {name}";
        //    Console.WriteLine(msg);
        //    return msg;
        //}
        //[LCEngineFunction("TestCloudFunc")]
        //public static string TestCloudFunc([LCEngineFunctionParam("name")] string name)
        //{
        //    return "TestCloudFunc:" + name;
        //}


        [LCEngineFunction("SendMessageToTargetUserID")]
        public static async void SendMessageToTargetUserID()
        {
            //string appId = Environment.GetEnvironmentVariable("APP_ID");
            //string appKey = Environment.GetEnvironmentVariable("APP_KEY");
            //string appUrl = Environment.GetEnvironmentVariable("APP_URL");
            //string masterKey = Environment.GetEnvironmentVariable("MASTER_KEY");
            //LCLogger.Debug($"URL {appUrl}");

            //_httpClientService = new HttpClientIMService(appId, appKey, appUrl);
            //string conversationId = await _httpClientService.CreateConversation(Styem_BroadcastUserID, TestTargetUserID );
            //await _httpClientService.SendMessage(conversationId, Styem_BroadcastUserID, TestTargetUserID, "服务端消息发送");
            
            await IMService.Inst.AddMembers(TestTargetUserID);
            await IMService.Inst.SendMessage($"服务端发送的消息:{System.DateTime.Now}");
        }


        private static async Task<LCUser> ValidateSenderAsync(string senderId)
        {
            try
            {
                LCQuery<LCUser> query = LCUser.GetQuery().WhereEqualTo("objectId", senderId);
                int count = await query.Count();
                LCUser user = await query.First();
                return user;
            }
            catch
            {
                return null;
            }
        }



        [LCEngineUserHook(LCEngineUserHookType.OnLogin)]
        public static void OnLogin(LCUser user)
        {
            LCLogger.Debug(string.Format("{0} login", user["username"]));
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOnline)]
        public static void OnClientOnline(Dictionary<string, object> parameters)
        {
            LCLogger.Debug($"客户端上线");
            LCLogger.Debug($"客户端上线{parameters["peerId"]} online.");
        }
        // 注意，C# 代码示例中没有更新 LeanCache，仅仅输出了用户状态
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOffline)]
        public static void OnClientOffline(Dictionary<string, object> parameters)
        {
            LCLogger.Debug($"客户端离线");
            LCLogger.Debug($"客户端离线{parameters["peerId"]} offline");
        }


        //[LCEngineClassHook("Review", LCEngineObjectHookType.BeforeSave)]
        //public static LCObject ReviewBeforeSave(LCObject review)
        //{
        //    if (string.IsNullOrEmpty(review["comment"].ToString() ))
        //    {
        //        throw new Exception("No comment provided!");
        //    }
        //    string comment = review["comment"] as string;
        //    if (comment.Length > 140)
        //    {
        //        review["comment"] = string.Format($"{comment.Substring(0, 140)}...");
        //    }
        //    return review;
        //}


        //[LCEngineClassHook("Review", LCEngineObjectHookType.AfterSave)]
        //public static async Task ReviewAfterSave(LCObject review)
        //{
        //    LCObject post = review["post"] as LCObject;
        //    await post.Fetch();
        //    post.Increment("comments", 1);
        //    await post.Save();
        //}


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageReceived)]
        public static object OnMessageReceived(Dictionary<string, object> parameters)
        {
            LCLogger.Debug("OnMessageReceived");
            foreach (KeyValuePair<string,object> kv in parameters) 
            {
                LCLogger.Debug($"Key:{kv.Key}======Value:{kv.Value}");
            }
            if (parameters.ContainsKey("content") )
            {
                LCLogger.Debug(parameters["content"].ToString() );
            }

            return new Dictionary<string, object> {{ "content", parameters["content"] } };
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageSent)]
        public static Dictionary<string, object> OnMessageSent(Dictionary<string, object> parameters)
        {
            LCLogger.Debug(JsonConvert.SerializeObject(parameters));
            return default;
        }
    }
}
