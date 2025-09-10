using LeanCloud;
using LeanCloud.Core.Internal;
using LeanCloud.Realtime;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ClientMessageBase : AVIMTypedMessage
{
    public async Task Save() 
    {
        //JObject jsonObject = JObject.Parse(Content);
        //var content = jsonObject.ToObject<Dictionary<string, object>>();

        var table = new AVObject("customMessage");
        table.Set("IsProcessed", false);
        table.Set("content", Content);
        await table.SaveAsync();
    }

}