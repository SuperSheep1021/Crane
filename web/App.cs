using LeanCloud;
using LeanCloud.Engine;
using LeanCloud.Push;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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


        [LCEngineFunction("GetAllUserIdsAsync")]
        public static async Task GetAllUserIdsAsync()
        {
            var userIds = new List<string>();

            // 创建用户查询
            var query = new AVQuery<AVUser>();

            // 只选择 objectId 字段以提高性能
            query.Select("objectId");

            // 设置跳过记录数
            int skip = 0;
            const int limit = 100; // 每次查询的最大记录数

            while (true)
            {
                // 设置查询参数
                query.Limit(limit);
                query.Skip(skip);

                // 执行查询
                var users = await query.FindAsync();

                // 如果没有更多数据，退出循环
                if (users == null || users.Count() == 0)
                    break;

                // 提取用户 ID
                foreach (var user in users)
                {
                    userIds.Add(user.ObjectId);
                }

                // 如果返回的记录数小于限制，说明已获取所有数据
                if (users.Count() < limit)
                    break;

                // 增加跳过记录数
                skip += limit;
            }

            foreach (string id in userIds) {
                Console.WriteLine(id);
            }
            //return userIds;
        }

        /// <summary>
        /// 获取所有用户的详细信息（谨慎使用，可能数据量很大）
        /// </summary>
        /// <returns>用户对象列表</returns>
        public async Task<List<AVUser>> GetAllUsersAsync()
        {
            var allUsers = new List<AVUser>();

            var query = new AVQuery<AVUser>();
            int skip = 0;
            const int limit = 1000;

            while (true)
            {
                query.Limit(limit);
                query.Skip(skip);

                var users = await query.FindAsync();

                if (users == null || users.Count() == 0)
                    break;

                allUsers.AddRange(users);

                if (users.Count() < limit)
                    break;

                skip += limit;
            }

            return allUsers;
        }
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
