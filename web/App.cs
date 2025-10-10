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
        [LCEngineFunction("GetSignatureKey")]
        public static string GetSignatureKey()
        {
            return Environment.GetEnvironmentVariable("LEANCLOUD_APP_MASTER_KEY");
        }

        [LCEngineFunction("isSignUped")]
        public static async Task<bool> isSignUped([LCEngineFunctionParam("userName")] object userName)
        {
             return await RESTAPIService.Inst.isSignUped(userName);
        }

        [LCEngineFunction("SysConvId")]
        public static string SysConvId()
        {
            return RESTAPIService.Inst.SysConvId;
        }

        [LCEngineFunction("SysAccountId")]
        public static string SysAccountId()
        {
            return RESTAPIService.Inst.SysUser.ObjectId;
        }

        [LCEngineFunction("SysUTCTime")]
        public static async Task<object> GetSysUTCTime()
        {
            return await RESTAPIService.Inst.GetSysUTCTime();
        }

        [LCEngineFunction("SysLocalTime")]
        public static async Task<object> GetSysLocalTime()
        {
            DateTime utc =(DateTime) await RESTAPIService.Inst.GetSysUTCTime();
            return utc.ToLocalTime();
        }


        [LCEngineFunction("初始化服务器")]
        public static async Task<bool> InitialService()
        {
            bool success = true;
            await RESTAPIService.Inst.Initialtion();
            await SysIMClientService.Inst.Initialtion();
            return success;
        }



        [LCEngineFunction("SignUpOrLogin")]
        public static async Task<bool> SignUpOrLogin([LCEngineFunctionParam("userId")] string userId, [LCEngineFunctionParam("parameters")] string parameters)
        {
            bool success = true;
            Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters);
            //todo验证
            if (userId == dic["userId"].ToString() ) 
            {
                LCLogger.Debug($"验证通过");
            }
            await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync("100000", new string[] { userId });
            LCLogger.Debug($"验证{userId}用户登录");
            return success;
        }

        [LCEngineFunction("开始游戏")]
        public static async Task<bool> StartGame([LCEngineFunctionParam("userId")] string userId, [LCEngineFunctionParam("parameters")] string parameters)
        {
            bool success = true;

            Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters);
            //todo验证
            if (userId == dic["userId"].ToString())
            {
                LCLogger.Debug($"验证通过");
            }


            LCObject lcobj = new LCObject("DeviceInfo");
            foreach (KeyValuePair<string,object> kv in dic) 
            {
                lcobj[kv.Key] = kv.Value;
                LCLogger.Debug($"key{kv.Key} value{kv.Value}");
            }
            await lcobj.Save();

           
            await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync("100001", new string[] { userId });
            LCLogger.Debug($"验证{userId}用户登录");
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
        [LCEngineFunction("RA查询服务号给客户端发送的消息(string targetClientId)")]
        public static async Task<IDictionary<string, object>> QuerySendFormClientId([LCEngineFunctionParam("targetClientId")] string targetClientId)
        {
            return await RESTAPIService.Inst.QuerySendFormClientId(targetClientId);
        }




        [LCEngineFunction("RA发送消息给所有订阅者")]
        public static async Task<IDictionary<string, object>> SendMessageToSubscribes([LCEngineFunctionParam("text")] string text)
        {
            return await RESTAPIService.Inst.SendMessageToSubscribesAsync(text);
        }
        [LCEngineFunction("RA发送消息给指定订阅者")]
        public static async Task<IDictionary<string, object>> SendMessageToSubscribes([LCEngineFunctionParam("text")] string text,
            [LCEngineFunctionParam("clientIds")] List<object> clientIds)
        {
            List<string> clientIdsStrarr= new List<string>();
            foreach (object clientid in clientIds) 
            {
                clientIdsStrarr.Add(clientid.ToString() );
            }
            return await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync( text, clientIdsStrarr.ToArray());
        }


        [LCEngineFunction("发送消息给指定订阅者")]
        public static async Task<LCIMTextMessage> SendMessageToSubscribesAsync([LCEngineFunctionParam("text")] string text,
            [LCEngineFunctionParam("clientIds")] List<object> clientIds)
        {
            List<string> clientIdsStrarr = new List<string>();
            foreach (object clientid in clientIds)
            {
                clientIdsStrarr.Add(clientid.ToString());
            }
            return await SysIMClientService.Inst.SendMessageToSubscribesAsync(text, clientIdsStrarr.ToArray());
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
            //LCLogger.Debug("=======================================");
            //LCLogger.Debug($"=================={JsonConvert.SerializeObject(parameters)}=====================");
            //LCLogger.Debug($"=================={parameters["toPeers"]}=====================");
            //LCLogger.Debug(" =======================================");

            //if (parameters["toPeers"] == null)
            //{
            //    string contentjson = parameters["content"] as string;
            //    var contentdic = JsonConvert.DeserializeObject<Dictionary<string, object>>(contentjson);
            //    string lcattrsjson = contentdic["_lcattrs"].ToString();
            //    var lcattrsdic = JsonConvert.DeserializeObject<Dictionary<string, object>>(lcattrsjson);
            //    var topeerarray = JsonConvert.DeserializeObject<string[]>(lcattrsdic["toPeers"].ToString());
            //    parameters["toPeers"] = topeerarray;


            //    contentdic.Remove("_lcattrs");
            //    parameters["content"] = JsonConvert.SerializeObject(contentdic);

            //    LCLogger.Debug($"================execute parameters repair========={parameters["content"]}=======");
            //}
            return parameters;
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageSent)]
        public static Dictionary<string, object> OnMessageSent(Dictionary<string, object> parameters)
        {
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
            //string convId = parameters["convId"] as string;
            //LCLogger.Debug($"{convId} OnConversationStart");
            return parameters;
        }
        /// <summary>
        /// 创建对话完成后调用。开发者在这里可以完成业务统计，或将对话数据中转备份到己方服务器，以及其他类似操作。
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationStarted)]
        public static object OnConversationStartedAsync(Dictionary<string, object> parameters)
        {
            //string convId = parameters["convId"] as string;
            //LCLogger.Debug($"{convId} OnConversationStarted");

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
        public static async Task<object> OnConversationAddedAsync(Dictionary<string, object> parameters)
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
            //if (m_ImSend)
            //{
            //    await SysIMClientService.Inst.SendMessageToSubscribesAsync("login success", new string[] { members[0].ToString() });
            //}
            //else {
            //    await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync("login success", new string[] { members[0].ToString() });
            //}
            //await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync("100000", new string[] { members[0].ToString() });
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
