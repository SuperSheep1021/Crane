using LeanCloud;
using LeanCloud.Engine;
using LeanCloud.Push;
using LeanCloud.Storage;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace web {
    public class App
    {
        // 定义接收 Hook 负载（Payload）的数据结构
        // LeanCloud 会发送一个 JSON 对象，包含以下字段
        public class ClientStatusEventPayload
        {
            [JsonProperty("clientId")]
            public string ClientId { get; set; } // 上下线的客户端 ID

            [JsonProperty("sessionToken")]
            public string SessionToken { get; set; } // 会话 Token（传统字段）

            [JsonProperty("deviceId")]
            public string DeviceId { get; set; } // 设备标识（新字段，更推荐）

            [JsonProperty("reason")]
            public string Reason { get; set; } // 仅 _clientOffline 有，断开原因
        }

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


        [LCEngineFunction("_clientOffline")]
        public static void ClientOffLine([FromBody] ClientStatusEventPayload payload)
        {
            Console.WriteLine("start execute: ClientOffLine");
            Console.WriteLine(payload.ClientId);
            Console.WriteLine("end execute: ClientOffLine");
        }

        [LCEngineFunction("_clientOnline")]
        public static void ClientOnLine([FromBody] ClientStatusEventPayload payload)
        {
            Console.WriteLine("start execute: ClientOnLine");
            Console.WriteLine(payload.ClientId);
            Console.WriteLine("end execute: ClientOnLine");
        }
    }
}
