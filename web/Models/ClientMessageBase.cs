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
    private async Task<bool> ValidateSenderAsync(string senderId)
    {
        try
        {
            AVQuery<AVUser> query = new AVQuery<AVUser>().WhereEqualTo("objectId", senderId);
            AVUser user = await query.FirstAsync();
            return user != null;
        }
        catch
        {
            return false;
        }
    }
    public async Task<string> SaveClientMessage()
    {
        bool isVali = await ValidateSenderAsync(FromClientId);
        if (isVali)
        {
            LCLogger.Debug("isValiSenderID false!!!");
            return "";
        }

        var table = new AVObject("customMessage");
        table.Set("IsProcessed", false);

        JObject jsonObject = JObject.Parse(Content);
        Dictionary<string, object> content = jsonObject.ToObject<Dictionary<string, object>>();
        table.Set("content", content);
        await table.SaveAsync();

        int messType = int.Parse( content["_lctype"].ToString() );
        await ProcessTypedMessageAsync(table, messType);

        return table.ObjectId;
    }
    public async Task ProcessTypedMessageAsync(AVObject table,int messType)
    {
        switch (messType)
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