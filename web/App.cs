using LeanCloud;
using LeanCloud.Engine;
using LeanCloud.Push;
using LeanCloud.Storage;
using LeanCloud.Storage.Internal;
using Microsoft.AspNetCore.Mvc;
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
            Console.WriteLine("Execute OnLogin");
            AVUser validateUser = await ValidateSenderAsync(user.ObjectId);
            Console.WriteLine("ValidateSenderAsync");
            if (validateUser != null)
            {
                if (validateUser.ContainsKey("loginTime"))
                {
                    validateUser["loginTime"] = DateTime.Now.ToString();
                    Console.WriteLine("ContainsKey loginTime" + DateTime.Now.ToString());
                }
                else
                {
                    validateUser.Add("loginTime", DateTime.Now.ToString());
                    Console.WriteLine("!ContainsKey loginTime" + DateTime.Now.ToString());
                }
                await validateUser.SaveAsync();
                Console.WriteLine(string.Format("{0} login", validateUser["username"]));
            }
            else
            {
                Console.WriteLine(string.Format("无效的登陆{0}", user.ObjectId));
            }
            Console.WriteLine("final OnLogin");
        }


        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOffline)]
        public static async Task<object> ClientOffLine([FromBody] ClientStatusEventPayload payload)
        {
            // 打印整个request对象（转换为JSON字符串以便查看）
            //string requestJson = Json.Encode(request);
            //Console.WriteLine($"收到_clientOnline请求: {requestJson}");

            // 您也可以访问request中的具体属性
            string clientId = payload.ClientId;
            string sessionToken = payload.SessionToken;
            string deviceId = payload.DeviceId;

            Console.WriteLine($"客户端上线: {clientId}, 设备ID: {deviceId}");

            // 您的业务逻辑，比如更新用户状态等

            return new { success = true };
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.ClientOnline)]
        public static async Task<object> ClientOnLine([FromBody] ClientStatusEventPayload payload)
        {
            Console.WriteLine("start execute: ClientOnLine");
            Console.WriteLine($"客户端上线: {payload.ClientId}, 设备ID: {payload.DeviceId}");
            Console.WriteLine("end execute: ClientOnLine");
            return new { success = true };
        }
    }
}
