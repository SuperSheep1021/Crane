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
            if (validateUser!=null)
            {
                validateUser["loginTime"] = DateTime.Now.ToString();
                Console.WriteLine(string.Format("{0} login", validateUser["username"]));
            }
            else {
                Console.WriteLine(string.Format("无效的登陆{0}", user.ObjectId) );
            }
            Console.WriteLine("final OnLogin");
        }


        [LCEngineFunction("TestCloudFunc")]
        public static string TestCloudFunc([LCEngineFunctionParam("name")] string name)
        {
            return "TestCloudFunc:" + name;
        }


        [LCEngineFunction("GetAllUserIdsAsync")]
        public static async Task<string>  GetAllUserIdsAsync()
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

            string ids = string.Empty;
            foreach (string id in userIds) {
                ids += id;
            }
            return ids;
        }



    }
}
