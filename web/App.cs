using LeanCloud;
using LeanCloud.Engine;
using LeanCloud.Push;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace web {
    public class App {
        // Function
        [LCEngineFunction("hello")]
        public static string Hello([LCEngineFunctionParam("name")] string name) {
            string msg = $"hello, {name}";
            Console.WriteLine(msg);
            return msg;
        }


        [LCEngineFunction("TestCloudFunc")]
        public static string TestCloudFunc([LCEngineFunctionParam("name")] string name)
        {
            return "TestCloudFunc:"+ name;
        }

        [LCEngineFunction("SendWeeklyMessage")]
        public static async Task SendWeeklyMessage()
        {
            // 获取所有用户
            LCQuery<LCUser> userQuery = LCUser.GetQuery();
            ReadOnlyCollection<LCUser> users = await userQuery.Find();

            foreach (LCUser uu in users) {
                Console.WriteLine(uu.ObjectId);
                Console.WriteLine(uu.Password);
            }


            //// 创建消息内容
            //string messageTitle = "每周更新与新闻";
            //string messageContent = "亲爱的用户，欢迎阅读本周更新！我们很高兴为您带来新功能和改进。";

            //// 给每个用户发送消息
            //foreach (LCUser user in users)
            //{
            //    // 创建消息对象
            //    LCObject message = new LCObject("Message");
            //    message["title"] = messageTitle;
            //    message["content"] = messageContent;
            //    message["recipient"] = user;
            //    message["isRead"] = false;

            //    // 保存到数据库
            //    await message.Save();

            //    // 发送推送通知（如果需要）
            //    LCPush push = new LCPush();
            //    push.Data = new Dictionary<string, object> {{ "alert", messageTitle }};
            //    //push.Query = new LCQuery<LCInstallation>().WhereEqualTo("user", user);
            //    await push.Send();
            //}

            //// 记录发送任务
            //LCObject taskRecord = new LCObject("SendTask");
            //taskRecord["type"] = "weekly";
            //taskRecord["userCount"] = users.Count;
            //taskRecord["status"] = "completed";
            //await taskRecord.Save();
        }


        //[LCEngineFunction("throwLCException")]
        //public static void ThrowLCException()
        //{
        //    throw new LCException(123, "自定义错误信息。");
        //}


        ////在创建新对象之前，可以对数据做一些清理或验证。例如，一条电影评论不能过长，否则界面上显示不开，需要将其截断至 140 个字符：
        //[LCEngineClassHook("Review", LCEngineObjectHookType.BeforeSave)]
        //public static LCObject ReviewBeforeSave(LCObject review)
        //{
        //    if (string.IsNullOrEmpty((string)review["comment"]))
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


        ////在创建新对象后触发指定操作，比如当一条留言创建后再更新一下所属帖子的评论总数：
        //[LCEngineClassHook("Review", LCEngineObjectHookType.AfterSave)]
        //public static async Task ReviewAfterSave(LCObject review)
        //{
        //    LCObject post = review["post"] as LCObject;
        //    await post.Fetch();
        //    post.Increment("comments", 1);
        //    await post.Save();
        //}

        ////在用户注册成功之后，给用户增加一个新的属性 from 并保存：
        //[LCEngineClassHook("_User", LCEngineObjectHookType.AfterSave)]
        //public static async Task UserAfterSave(LCObject user)
        //{
        //    user["from"] = "LeanCloud";
        //    await user.Save();
        //}

        ////在更新已存在的对象前执行操作，这时你可以知道哪些字段已被修改，还可以在特定情况下拒绝本次修改：

        //[LCEngineClassHook("Review", LCEngineObjectHookType.BeforeUpdate)]
        //public static LCObject ReviewBeforeUpdate(LCObject review)
        //{
        //    ReadOnlyCollection<string> updatedKeys = review.GetUpdatedKeys();
        //    if (updatedKeys.Contains("comment"))
        //    {
        //        string comment = review["comment"] as string;
        //        if (comment.Length > 140)
        //        {
        //            throw new Exception("comment 长度不得超过 140 字符。");
        //        }
        //    }
        //    return review;
        //}
    }
}
