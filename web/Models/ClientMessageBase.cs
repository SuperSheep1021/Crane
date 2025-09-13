using LeanCloud;
using LeanCloud.Common;
using LC.Newtonsoft.Json.Linq;
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ClientMessageBase 
{
    private static async Task<bool> ValidateSenderAsync(string senderId)
    {
        try
        {
            LCQuery<LCUser> query = LCUser.GetQuery().WhereEqualTo("objectId", senderId);
            LCUser user = await query.First();
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<string> SaveClientMessage(string senderId,string message)
    {
        bool isVali = await ValidateSenderAsync(senderId);
        if (isVali)
        {
            LCLogger.Debug("isValiSenderID false!!!");
            return "";
        }

        var table = new LCObject("customMessage");
        table.Add("IsProcessed", false);


        JObject jsonObject = JObject.Parse(message);
        Dictionary<string, object> content = jsonObject.ToObject<Dictionary<string, object>>();
        table.Add("content", content);
        await table.Save();

        int messType = int.Parse(content["_lctype"].ToString());
        await ProcessTypedMessageAsync(table, messType);

        return table.ObjectId;
    }
    public async Task ProcessTypedMessageAsync(LCObject table, int messType)
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
        table.Add("IsProcessed", true);
        await table.Save();
    }

}