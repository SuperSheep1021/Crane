
using LeanCloud.Storage;
using System.Threading.Tasks;

public static class HelpService 
{
    public static async Task<LCUser> ValidateClientIDAsync(string objectId)
    {
        try
        {
            LCQuery<LCUser> query = LCUser.GetQuery().WhereEqualTo("objectId", objectId);
            int count = await query.Count();
            LCUser user = await query.First();
            return user;
        }
        catch
        {
            return null;
        }
    }


}