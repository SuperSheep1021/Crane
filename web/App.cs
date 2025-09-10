using LeanCloud;
using LeanCloud.Engine;
using LeanCloud.Push;
using LeanCloud.Storage;
using LeanCloud.Storage.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace web {
    public class App
    {

        // Function
        [LCEngineFunction("hello")]
        public static string Hello([LCEngineFunctionParam("name")] string name)
        {
            string msg = $"hello, {name}";
            Console.WriteLine(msg);
            return msg;
        }
        [LCEngineFunction("TestCloudFunc")]
        public static string TestCloudFunc([LCEngineFunctionParam("name")] string name)
        {
            return "TestCloudFunc:" + name;
        }
        private static async Task<AVUser> ValidateSenderAsync(string senderId)
        {
            try
            {
                AVQuery<AVUser> query = new AVQuery<AVUser>().WhereEqualTo("objectId", senderId);
                AVUser user = await query.FirstAsync();
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
            AVUser validateUser = await ValidateSenderAsync(user.ObjectId);
            if (validateUser != null)
            {
                if (validateUser.ContainsKey("loginTime"))
                {
                    validateUser["loginTime"] = DateTime.Now.ToString();
                }
                else
                {
                    validateUser.Add("loginTime", DateTime.Now.ToString());
                }
                await validateUser.SaveAsync();
                LCLogger.Debug(string.Format("{0} login", validateUser["username"]));
            }
            else
            {
                LCLogger.Debug(string.Format("无效的登陆{0}", user.ObjectId));
            }
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOnline)]
        public static async Task<object> ClientOnLine(dynamic request)
        {
            string data = JsonConvert.SerializeObject(request);
            LCLogger.Debug($"客户端上线: {request}");
            try
            {
                var dic = request;
                foreach (KeyValuePair<string, object> item in dic)
                {
                    LCLogger.Debug(item.Key + ":" + item.Value);
                }

            }
            catch (LCException ex)
            {
                LCLogger.Error(ex.Message);
            }
            return new { success = true };
        }



        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOffline)]
        public static async Task<object> ClientOffLine(dynamic request)
        {
            string data = JsonConvert.SerializeObject(request);
            LCLogger.Debug($"客户端下线: {request}");
            try
            {
                var dic = request;
                foreach (KeyValuePair<string, object> item in dic)
                {
                    LCLogger.Debug(item.Key + ":" + item.Value);
                }

            }
            catch (LCException ex)
            {
                LCLogger.Error(ex.Message);
            }
            // 您的业务逻辑，比如更新用户状态等

            return new { success = true };
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageReceived)]
        public static async Task<object> OnMessageReceived(dynamic request)
        {
            string data = JsonConvert.SerializeObject(request);
            LCLogger.Debug($"OnMessageReceived: {request}");
            try
            {
                var dic = request;
                foreach (KeyValuePair<string, object> item in dic)
                {
                    LCLogger.Debug(item.Key + ":" + item.Value);
                }

            }
            catch (LCException ex)
            {
                LCLogger.Error(ex.Message);
            }
            // 您的业务逻辑，比如更新用户状态等

            return new { success = true };
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageSent)]
        public static async Task<object> OnMessageSent(dynamic request)
        {
            string data = JsonConvert.SerializeObject(request);
            LCLogger.Debug($"OnMessageSent: {request}");
            try
            {
                var dic = request;
                foreach (KeyValuePair<string, object> item in dic)
                {
                    LCLogger.Debug(item.Key + ":" + item.Value);
                }

            }
            catch (LCException ex)
            {
                LCLogger.Error(ex.Message);
            }
            // 您的业务逻辑，比如更新用户状态等

            return new { success = true };
        }

    }
}
