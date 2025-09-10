using LeanCloud;
using LeanCloud.Core.Internal;
using LeanCloud.Realtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ClientMessageBase : AVIMTypedMessage
{
    public async Task Save() 
    {
        var table = new AVObject("customMessage");
        table.Set("IsProcessed", false);

        JObject jsonObject = JObject.Parse(Content);
        var content = jsonObject.ToObject<Dictionary<string, object>>();
        table.Set("content", content);
        await table.SaveAsync();
    }

}