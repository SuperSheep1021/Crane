using LC.Newtonsoft.Json;
using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Engine;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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

        [LCEngineFunction("初始化服务器")]
        public static async Task TestCloudFuncAsync()
        {
            await RESTAPIService.Inst.Initialtion();
            await SysIMClientService.Inst.Initialtion();
        }



        [LCEngineFunction("RA创建系统会话(string name)")]
        public static async Task<IDictionary<string,object>> CreateServiceConversation([LCEngineFunctionParam("name")] string sysConversationName)
        {
            return await RESTAPIService.Inst.CreateSysConvAsync(sysConversationName);
        }

        [LCEngineFunction("RA查询系统会话(int total,string convName)")]
        public static async Task<IDictionary<string, object>> QuerySysConvAsync([LCEngineFunctionParam("total")] int total, [LCEngineFunctionParam("convName")] string convName)
        {
            return await RESTAPIService.Inst.QuerySysConvAsync(total, convName);
        }

        [LCEngineFunction("RA订阅系统对话(string clientId)")]
        public static async Task<IDictionary<string, object>> SubscribeSysConv([LCEngineFunctionParam("clientId")] string clientId)
        {
            return await RESTAPIService.Inst.SubscribeSysConvAsync(clientId);
        }

        [LCEngineFunction("RA发送消息给所有订阅者")]
        public static async Task<IDictionary<string, object>> SendMessageToSubscribes()
        {
            return await RESTAPIService.Inst.SendMessageToSubscribesAsync();
        }
        [LCEngineFunction("RA发送消息给指定订阅者")]
        public static async Task<IDictionary<string, object>> SendMessageToSubscribes(/*[LCEngineFunctionParam("clientIds")] List<object> clientIds,*/
            [LCEngineFunctionParam("message")] string message)
        {
            return await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { "68b9286c49adb47c41678afb" }, message);
        }

        [LCEngineFunction("RA查询服务号给客户端发送的消息(string targetClientId)")]
        public static async Task<IDictionary<string, object>> QuerySendFormClientId([LCEngineFunctionParam("targetClientId")] string targetClientId)
        {
            return await RESTAPIService.Inst.QuerySendFormClientId(targetClientId);
        }




        [LCEngineFunction("发送消息给指定订阅者")]
        public static async Task<LCIMTextMessage> SendMessageToSubscribesAsync([LCEngineFunctionParam("message")] string message
            /*,[LCEngineFunctionParam("clientIds")] List<object>  clientIds*/)
        {
            return await SysIMClientService.Inst.SendMessageToSubscribesAsync(message, new string[] { "68b9286c49adb47c41678afb" }, new Dictionary<string, object>() {
                { "通过imclient send message",1}
            });
        }



        [LCEngineUserHook(LCEngineUserHookType.OnLogin)]
        public static void OnLogin(LCUser loginUser)
        {
            //await Task.Delay(2000);
            LCLogger.Debug(string.Format("1 {0} login", loginUser["username"]));
            LCLogger.Debug($"2 login client id is {loginUser["objectId"]} ");
            LCLogger.Debug($"3 login client name is {loginUser.Username} ");
            LCLogger.Debug($"4 login client user.ObjectId is {loginUser.ObjectId} ");
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOnline)]
        public static void OnClientOnline(Dictionary<string, object> parameters)
        {
            LCLogger.Debug($"客户端上线");
            LCLogger.Debug($"客户端上线{parameters["peerId"]} online.");
        }


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
            parameters["toPeers"] = new string[] { "68b9286c49adb47c41678afb" };
            return parameters;
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageSent)]
        public static Dictionary<string, object> OnMessageSent(Dictionary<string, object> parameters)
        {
            //LCLogger.Debug(JsonConvert.SerializeObject(parameters));
            //parameters["onlinePeers"] = new string[] { "68b9286c49adb47c41678afb" };
            ////parameters["toPeers"] = new string[] { "68b9286c49adb47c41678afb" };
            ////parameters.Remove("onlinePeers");
            ////parameters.Remove("offlinePeers");
            //LCLogger.Debug("=================setup=================");
            //LCLogger.Debug(JsonConvert.SerializeObject(parameters));

            return parameters;
        }

        #region//conversation
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationStart)]
        public static object OnConversationStart(Dictionary<string, object> parameters)
        {
            return parameters;
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationStarted)]
        public static object OnConversationStarted(Dictionary<string, object> parameters)
        {
            string convId = parameters["convId"] as string;
            Console.WriteLine($"{convId} started");
            return parameters;
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationAdd)]
        public static object OnConversationAdd(Dictionary<string, object> parameters)
        {
            object membersObj = parameters["members"];
            string[] membersArray = null;
            if (membersObj is string[] existingArray)
            {
                membersArray = existingArray;
            }else if (membersObj != null)
            {
                membersArray = new string[] { membersObj.ToString() };
            }

            foreach (string str in membersArray)
            {
                LCLogger.Debug($"add imclient object id is {str}");
            }
            return parameters;
        }
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationAdded)]
        public static object OnConversationAdded(Dictionary<string, object> parameters)
        {
            object membersObj = parameters["members"];
            string[] membersArray = null;
            if (membersObj is string[] existingArray)
            {
                membersArray = existingArray;
            }
            else if (membersObj != null)
            {
                membersArray = new string[] { membersObj.ToString() };
            }
            foreach (string str in membersArray)
            {
                LCLogger.Debug($"added imclient object id is {str}");
            }
            return parameters;
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationRemove)]
        public static object OnConversationRemove(Dictionary<string, object> parameters)
        {
            object membersObj = parameters["members"];
            string[] membersArray = null;
            if (membersObj is string[] existingArray)
            {
                membersArray = existingArray;
            }
            else if (membersObj != null)
            {
                membersArray = new string[] { membersObj.ToString() };
            }
            foreach (string str in membersArray)
            {
                LCLogger.Debug($"remove imclient object id is {str}");
            }
            return default;
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationRemoved)]
        public static void OnConversationRemoved(Dictionary<string, object> parameters)
        {
            object membersObj = parameters["members"];
            string[] membersArray = null;
            if (membersObj is string[] existingArray)
            {
                membersArray = existingArray;
            }
            else if (membersObj != null)
            {
                membersArray = new string[] { membersObj.ToString() };
            }
            foreach (string str in membersArray)
            {
                LCLogger.Debug($"removed imclient object id is {str}");
            }
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationUpdate)]
        public static object OnConversationUpdate(Dictionary<string, object> parameters)
        {
        //    Dictionary<string, object> attr = parameters["attr"] as Dictionary<string, object>;
        //    if (attr != null && attr.ContainsKey("name"))
        //    {
        //        return new Dictionary<string, object> {
        //    { "reject", true },
        //    { "code", 1949 },
        //    { "detail", "对话名称不可修改" }
        //};
        //    }
            return parameters;
        }
        #endregion



    }
}
