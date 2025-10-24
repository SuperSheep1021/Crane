using LC.Newtonsoft.Json;
using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Engine;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace web {
    public class App
    {

        [LCEngineFunction("Test")]
        public static async Task<string> Test() 
        {
            LCObject gameconfig =await HelpService.GetGameConfigTableInfo();
            //string gameconfigjson = gameconfig.ToString();

            LCUser user = await HelpService.GetUser("68fb33b2096517792f2bc965");
            LCObject playerPropObj = user["playerPropInfo"] as LCObject;
            //LCObject playerProp = await HelpService.CreateOrGetPlayerPropsInfoFromUser(user);

            return $"gameconfig: { HelpService.ConvertTo<int>(gameconfig["perAddPower"] ) }_" +
                $"__{HelpService.ConvertTo<int>(playerPropObj["power"])} ";
        }


        [LCEngineFunction("GetSignature")]
        public static string GetSignature( [LCEngineFunctionParam("args")] string args)
        {
            return LocalSignatureFactory.GenerateSignature(args);
        }

        [LCEngineFunction("isSignUped")]
        public static async Task<bool> isSignUped([LCEngineFunctionParam("userName")] object userName)
        {
             return await HelpService.isSignUped(userName);
        }

        [LCEngineFunction("SysConvId")]
        public static string SysConvId()
        {
            return SysService.Inst.SysConvId;
        }

        [LCEngineFunction("SysAccountId")]
        public static string SysAccountId()
        {
            return SysService.Inst.SysUser.ObjectId;
        }

        [LCEngineFunction("SysUTCTime")]
        public static async Task<object> GetSysUTCTime()
        {
            return await RESTAPIService.Inst.GetSysUTCTime();
        }

        #region//增益
        [LCEngineFunction("AddPower")]
        public static async Task<bool> AddPower([LCEngineFunctionParam("userId")] string userId)
        {
            LCUser user = await HelpService.GetUser(userId);
            return await HelpService.AddPower(user);
        }
        [LCEngineFunction("AddGoldCoin")]
        public static async Task<bool> AddGoldCoin([LCEngineFunctionParam("userId")] string userId)
        {
            LCUser user = await HelpService.GetUser(userId);
            return await HelpService.AddGoldCoin(user, 10);
        }
        [LCEngineFunction("AddGem")]
        public static async Task<bool> AddGem([LCEngineFunctionParam("userId")] string userId)
        {
            LCUser user = await HelpService.GetUser(userId);
            return await HelpService.AddGem(user, 10);
        }

        #endregion



        [LCEngineFunction("OnSignUpOrLogin")]
        public static async void OnSignUpOrLogin([LCEngineFunctionParam("userId")] string userId,
            [LCEngineFunctionParam("upd")] string upd, [LCEngineFunctionParam("deviceInfo")] string deviceInfo) 
        {
            LCUser user = await HelpService.SetupUser(userId, "upd", upd);
            Dictionary<string, object> deviceDic = await LCJsonUtils.DeserializeObjectAsync<Dictionary<string, object>>(deviceInfo);

            LCObject deviceObj = await HelpService.CreateOrSetupDeviceInfo(user,deviceDic);
            await HelpService.SetupPointer(user.ObjectId , "deviceInfo", deviceObj);

            LCObject playerPropObj = await HelpService.CreateOrGetPlayerPropsInfoFromUser(user);

            if (playerPropObj == null) return;
            await HelpService.SetupPointer(user.ObjectId, "playerPropInfo", playerPropObj);


            string playerPropJson = await LCJsonUtils.SerializeObjectAsync(playerPropObj);
            LCLogger.Debug($"================={playerPropJson}********************");

            await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId },HelpService.ON_LOGIN, new Dictionary<string, object>()
            {
                { "playerProp",playerPropObj.ToString() }
            });
        }


        [LCEngineFunction("StartGame")]
        public static async Task<bool> StartGame([LCEngineFunctionParam("userId")] string userId, [LCEngineFunctionParam("clientTimes")] string parameters)
        {
            LCUser user = await HelpService.GetUser(userId);
            bool success = await HelpService.ConsumePower(user);
            if (!success) { return false; }


            bool isCreateSpecialDoll = await HelpService.isCreateSpecialDoll();
            string CreateSpecialName = string.Empty;
            if (isCreateSpecialDoll)
            {
                CreateSpecialName = HelpService.SpecialDollsGenerator.Generate();
            }

            Dictionary<string, object> dic = await LCJsonUtils.DeserializeObjectAsync<Dictionary<string, object>>(parameters);
            LCObject startGameInfo = await HelpService.CreateStartGameInfo(user, dic);
            await HelpService.SetupPointer(user.ObjectId, "startGameInfo", startGameInfo);

            await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, HelpService.START_GAME, new Dictionary<string, object>()
            {
                {"createSpecialName",CreateSpecialName }
            });

            return success;
        }


        #region//测试用

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
            return await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync( clientIdsStrarr.ToArray(), text);
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
            return await SysService.Inst.SendMessageToSubscribesAsync(text, clientIdsStrarr.ToArray());
        }
        #endregion

        #region //登录 在线离线

        [LCEngineUserHook(LCEngineUserHookType.OnLogin)]
        public static void OnLogin(LCUser loginUser)
        {
            LCLogger.Debug($"user login {loginUser.Username}");
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOnline)]
        public static async void OnClientOnline(Dictionary<string, object> parameters)
        {
            await HelpService.SetupOnline(parameters["peerId"].ToString(), true);
            LCLogger.Debug($"OnClientOnline {parameters["peerId"]}");
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOffline)]
        public static async void OnClientOffline(Dictionary<string, object> parameters)
        {
            await HelpService.SetupOnline(parameters["peerId"].ToString(), false);
            LCLogger.Debug($"OnClientOffline {parameters["peerId"]}");
        }
        #endregion

        #region//保存数据库时
        /// <summary>
        /// 当 “Review” 对象即将被保存到数据库时执行（LCEngineObjectHookType.BeforeSave）
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        [LCEngineClassHook("Review", LCEngineObjectHookType.BeforeSave)]
        public static LCObject ReviewBeforeSave(LCObject review)
        {
            return review;
        }


        /// <summary>
        /// 当 “Review” 对象成功保存到数据库后执行（LCEngineObjectHookType.AfterSave），且是异步方法（async Task）。
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        [LCEngineClassHook("Review", LCEngineObjectHookType.AfterSave)]
        public static async Task ReviewAfterSave(LCObject review)
        {
            LCObject post = review["post"] as LCObject;
            await post.Fetch();
            post.Increment("comments", 1);
            await post.Save();
        }
        #endregion

        #region//消息接收发送
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageReceived)]
        public static object OnMessageReceived(Dictionary<string, object> parameters)
        {
            return parameters;
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageSent)]
        public static Dictionary<string, object> OnMessageSent(Dictionary<string, object> parameters)
        {
            return parameters;
        }
        #endregion

        #region//会话操作
        /// <summary>
        /// 创建对话，在签名校验（如果开启）之后，实际创建之前调用。开发者在这里可以为新的「对话」添加其他内部属性，或完成操作鉴权，以及其他类似操作。
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationStart)]
        public static object OnConversationStart(Dictionary<string, object> parameters)
        {
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
            return parameters;
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationAdd)]
        public static object OnConversationAdd(Dictionary<string, object> parameters)
        {
            return parameters;
        }
        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationAdded)]
        public static object OnConversationAdded(Dictionary<string, object> parameters)
        {
            return parameters;
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationRemove)]
        public static object OnConversationRemove(Dictionary<string, object> parameters)
        {
            return parameters;
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationRemoved)]
        public static void OnConversationRemoved(Dictionary<string, object> parameters)
        {
            LCLogger.Debug($"OnConversationRemoved imclient object id is {parameters.ToString()}");
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ConversationUpdate)]
        public static object OnConversationUpdate(Dictionary<string, object> parameters)
        {
            return parameters;
        }
        #endregion


    }
}
