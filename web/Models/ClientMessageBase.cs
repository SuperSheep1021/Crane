using LeanCloud;
using LeanCloud.Core.Internal;
using LeanCloud.Realtime;
using LeanCloud.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;


public class ClientMessageBase : AVIMTextMessage
{
    int m_messType = -1;
    public async Task<string> SaveClientMessage()
    {
        var table = new AVObject("customMessage");
        table.Set("IsProcessed", false);

        JObject jsonObject = JObject.Parse(Content);
        Dictionary<string, object> content = jsonObject.ToObject<Dictionary<string, object>>();
        table.Set("content", content);
        await table.SaveAsync();

        m_messType = int.Parse( content["_lctype"].ToString() );

        await ProcessTypedMessageAsync(table);

        return table.ObjectId;
    }

    // 处理初始化后的类型化消息
    public async Task ProcessTypedMessageAsync(AVObject table)
    {
        switch (m_messType)
        {
            case 1001:
                LCLogger.Debug("1001");
                break;
            default:
                LCLogger.Debug("default");
                break;
        }
        table.Set("IsProcessed", true);
        await table.SaveAsync();
    }

}