using System;
using LeanCloud.Engine;

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
    }
}
