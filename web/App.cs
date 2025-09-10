using LeanCloud;
using LeanCloud.Engine;
using LeanCloud.Push;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using LeanCloud.Storage.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using web.Models;

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

        public class ClientMessage
        {
            public string ClientId;
            public string Conversation;
            public int MessageType;
            public Dictionary<string, object> Data = new Dictionary<string, object>();
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageReceived)]
        public static async Task<object> OnMessageReceived(dynamic request)
        {
            LCLogger.Debug("OnMessageReceived Start");
            try
            {
                //ClientMessage message = new ClientMessage();

                //message.ClientId = dic["fromPeer"];
                //message.Conversation = dic["convId"];
                //message.MessageType = dic["content"]["_lctype"];

                var dic = request;
                foreach (KeyValuePair<string, object> item in dic)
                {
                    LCLogger.Debug(item.Key + "______" + item.Value);
                    if (item.Key == "content")
                    {
                        JObject jsonObject = JObject.Parse( item.Value.ToString() );
                        Dictionary<string, object> content = jsonObject.ToObject<Dictionary<string, object>>();

                        foreach (KeyValuePair<string, object> contentItem in content)
                        {
                            //message.Data.Add(contentItem.Key, contentItem.Value);
                            LCLogger.Debug(contentItem.Key + ":" + contentItem.Value);
                        }
                    }
                }

            }
            catch (LCException ex)
            {
                LCLogger.Error(ex.Message);
            }
            // 您的业务逻辑，比如更新用户状态等
            LCLogger.Debug("OnMessageReceived end");
            return new { success = true };
        }

        [LCEngineRealtimeHook(LCEngineRealtimeHookType.MessageSent)]
        public static async Task<object> OnMessageSent(dynamic request)
        {
            LCLogger.Debug("OnMessageSent Start");
            try
            {
                var dic = request;
                foreach (KeyValuePair<string, object> item in dic)
                {
                    LCLogger.Debug(item.Key + "______" + item.Value);
                    if (item.Key == "content")
                    {
                        JObject jsonObject = JObject.Parse(item.Value.ToString());
                        Dictionary<string, object> content = jsonObject.ToObject<Dictionary<string, object>>();

                        foreach (KeyValuePair<string, object> contentItem in content)
                        {
                            //message.Data.Add(contentItem.Key, contentItem.Value);
                            LCLogger.Debug(contentItem.Key + ":" + contentItem.Value);
                        }
                    }
                }

            }
            catch (LCException ex)
            {
                LCLogger.Error(ex.Message);
            }
            // 您的业务逻辑，比如更新用户状态等
            LCLogger.Debug("OnMessageSent End");
            return new { success = true };
        }


        [LCEngineClassHook("Review", LCEngineObjectHookType.BeforeSave)]
        public static LCObject ReviewBeforeSave(LCObject review)
        {
            if (string.IsNullOrEmpty(review["comment"].ToString() ))
            {
                throw new Exception("No comment provided!");
            }
            string comment = review["comment"] as string;
            if (comment.Length > 140)
            {
                review["comment"] = string.Format($"{comment.Substring(0, 140)}...");
            }
            return review;
        }


        [LCEngineClassHook("Review", LCEngineObjectHookType.AfterSave)]
        public static async Task ReviewAfterSave(LCObject review)
        {
            LCObject post = review["post"] as LCObject;
            await post.Fetch();
            post.Increment("comments", 1);
            await post.Save();
        }

    }
}
