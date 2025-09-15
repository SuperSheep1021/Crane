using LeanCloud;
using LeanCloud.Engine;
using LeanCloud.Storage;
using LC.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using LeanCloud.Common;

namespace web {
    public class App
    {

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
        //[LCEngineFunction("SendMessageToTargetUserID")]
        //public static async void SendMessageToTargetUserID()
        //{
        //    //await SystemConverstaionService.Inst.SendTextMessage($"服务端发送的消息:{System.DateTime.Now}");
        //}

        [LCEngineFunction("CreateServiceConversationAsync")]
        public static async Task<IDictionary<string,object>> CreateServiceConversationAsync([LCEngineFunctionParam("name")] string sysConversationName)
        {
            return await SystemConverstaionService.Inst.CreateSysConvAsync(sysConversationName);
        }

        [LCEngineFunction("QuerySysConvAsync")]
        public static async Task<IDictionary<string, object>> QuerySysConvAsync([LCEngineFunctionParam("total")] int total, [LCEngineFunctionParam("convName")] string convName)
        {
            return await SystemConverstaionService.Inst.QuerySysConvAsync(total, convName);
        }

        [LCEngineFunction("SubscribeServiceConversationAsync")]
        public static async Task<IDictionary<string, object>> SubscribeSysConvAsync([LCEngineFunctionParam("clientID")] string clientID)
        {
            return await SystemConverstaionService.Inst.SubscribeSysConvAsync(clientID);
        }

        [LCEngineFunction("SendSubscribeServiceConversationAsync")]
        public static async Task<IDictionary<string, object>> SendMessageToSubscribesAsync([LCEngineFunctionParam("message")] string message)
        {
            return await SystemConverstaionService.Inst.SendMessageToSubscribesAsync(message);
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
        public static async Task OnLogin(LCUser user)
        {
            LCLogger.Debug(string.Format("1 {0} login", user["username"]));
            LCLogger.Debug($"2 login client id is {user["objectId"]} ");
            LCLogger.Debug($"3 login client name is {user.Username} ");
            LCLogger.Debug($"4 login client user.ObjectId is {user.ObjectId} ");
            //await SysClientService.Inst.AddMembers(user.ObjectId);
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
        //    if (string.IsNullOrEmpty(review["comment"].ToString()))
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
            foreach (KeyValuePair<string, object> kv in parameters)
            {
                LCLogger.Debug($"Key:{kv.Key}======Value:{kv.Value}");
            }
            if (parameters.ContainsKey("content"))
            {
                LCLogger.Debug(parameters["content"].ToString());
            }

            return new Dictionary<string, object> { { "content", parameters["content"] } };
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageSent)]
        public static Dictionary<string, object> OnMessageSent(Dictionary<string, object> parameters)
        {
            return default;
        }
    }
}
