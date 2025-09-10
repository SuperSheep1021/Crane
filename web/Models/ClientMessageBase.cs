using LeanCloud;
using LeanCloud.Core.Internal;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;


[AVIMTypedMessageTypeInt(1001)]
public class ClientMessageBase : AVIMTextMessage
{
    //Dictionary<string, object> m_content = new Dictionary<string, object>();
    //public async Task<string> SaveClientMessage()
    //{
        
    //    var table = new AVObject("customMessage");
    //    table.Set("IsProcessed", false);

    //    JObject jsonObject = JObject.Parse(Content);
    //    m_content = jsonObject.ToObject<Dictionary<string, object>>();

    //    table.Set("content", m_content);
    //    await table.SaveAsync();
    //    return table.ObjectId;
    //}

    //// 处理初始化后的类型化消息
    //public async Task ProcessTypedMessage()
    //{
    //    int messageType = int.Parse(m_content["_lctype"].ToString() );
    //    switch (messageType) 
    //    { 
    //        case 0:

    //        break;
    //    }
    //}

}