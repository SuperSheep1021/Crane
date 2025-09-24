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
        public static async Task<bool> TestCloudFuncAsync()
        {
            bool success = true;
            await RESTAPIService.Inst.Initialtion();
            await SysIMClientService.Inst.Initialtion();
            return success;
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
            return await SysIMClientService.Inst.SendMessageToSubscribesAsync(message, new string[] { "68b9286c49adb47c41678afb", "68bc9adb47c41678afb" });
        }


        #region // onlogin OnClient
        [LCEngineUserHook(LCEngineUserHookType.OnLogin)]
        public static void OnLogin(LCUser loginUser)
        {
            LCLogger.Debug($"user login {loginUser.Username}");
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOnline)]
        public static void OnClientOnline(Dictionary<string, object> parameters)
        {
            LCLogger.Debug($"OnClientOnline {parameters["peerId"]}");
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOffline)]
        public static void OnClientOffline(Dictionary<string, object> parameters)
        {
            LCLogger.Debug($"OnClientOffline {parameters["peerId"]}");
        }
        #endregion

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
            if (!parameters.ContainsKey("toPeers")) 
            {
                string contentJson = parameters["content"] as string;
                var contentDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(contentJson);
                string lcattrsJson = contentDic["_lcattrs"].ToString();
                var lcattrsDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(lcattrsJson);
                var topeerArray = JsonConvert.DeserializeObject<string[]>(lcattrsDic["toPeers"].ToString());
                parameters["toPeers"] = topeerArray;


                contentDic.Remove("_lcattrs");
                parameters["content"] = contentDic;
            }
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
        /// <summary>
        /// 创建对话，在签名校验（如果开启）之后，实际创建之前调用。开发者在这里可以为新的「对话」添加其他内部属性，或完成操作鉴权，以及其他类似操作。
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationStart)]
        public static object OnConversationStart(Dictionary<string, object> parameters)
        {
            string convId = parameters["convId"] as string;
            LCLogger.Debug($"{convId} OnConversationStart");
            return parameters;
        }
        /// <summary>
        /// 创建对话完成后调用。开发者在这里可以完成业务统计，或将对话数据中转备份到己方服务器，以及其他类似操作。
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationStarted)]
        public static object OnConversationStarted(Dictionary<string, object> parameters)
        {
            string convId = parameters["convId"] as string;
            LCLogger.Debug($"{convId} OnConversationStarted");
            return parameters;
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationAdd)]
        public static object OnConversationAdd(Dictionary<string, object> parameters)
        {
            List<object> members = new List<object>();
            object membersObj = parameters["members"];
            if (membersObj is List<object>)
            {
                members = membersObj as List<object>;
            }
            else {
                members.Add(membersObj.ToString() );
            }

            foreach (string str in members)
            {
                LCLogger.Debug($"OnConversationAdd imclient object id is {str.ToString() }");
            }
            return parameters;
        }
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationAdded)]
        public static object OnConversationAdded(Dictionary<string, object> parameters)
        {
            List<object> members = new List<object>();
            object membersObj = parameters["members"];
            if (membersObj is List<object>)
            {
                members = membersObj as List<object>;
            }
            else
            {
                members.Add(membersObj.ToString());
            }

            foreach (string str in members)
            {
                LCLogger.Debug($"OnConversationAdded imclient object id is {str.ToString()}");
            }
            return parameters;
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationRemove)]
        public static object OnConversationRemove(Dictionary<string, object> parameters)
        {
            List<object> members = new List<object>();
            object membersObj = parameters["members"];
            if (membersObj is List<object>)
            {
                members = membersObj as List<object>;
            }
            else
            {
                members.Add(membersObj.ToString());
            }

            foreach (string str in members)
            {
                LCLogger.Debug($"OnConversationRemove imclient object id is {str.ToString()}");
            }
            return default;
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationRemoved)]
        public static void OnConversationRemoved(Dictionary<string, object> parameters)
        {
            List<object> members = new List<object>();
            object membersObj = parameters["members"];
            if (membersObj is List<object>)
            {
                members = membersObj as List<object>;
            }
            else
            {
                members.Add(membersObj.ToString());
            }

            foreach (string str in members)
            {
                LCLogger.Debug($"OnConversationRemoved imclient object id is {str.ToString()}");
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
