
using LC.Newtonsoft.Json;
using LeanCloud;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public static class HelpService 
{
    public static T ConvertTo<T>(this object obj)
    {
        if (obj == null)
            return default(T);

        if (obj is T t)
            return t;

        try
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }
        catch
        {
            return default(T);
        }
    }

    //private static T ConvertTo<T>(object value)
    //{
    //    if (value == null)
    //        return default(T);

    //    if (value is T t)
    //        return t;

    //    try
    //    {
    //        return (T)Convert.ChangeType(value, typeof(T));
    //    }
    //    catch
    //    {
    //        return default(T);
    //    }
    //}
    private static async Task<bool> isOnline(string clientUserId) 
    {
        LCQuery<LCUser> query = LCUser.GetQuery().WhereEqualTo("objectId", clientUserId);
        int count = await query.Count();
        LCUser user = await query.First();
        return user["online"].ConvertTo<bool>();
    }
    public static async Task<bool> ValidateClientID(string clientUserId,string parametersClientUserId)
    {
        bool success = false;
        try
        {
            bool online = await isOnline(clientUserId);
            if (online)
            {
                //—È÷§
                if (clientUserId == parametersClientUserId)
                {
                    success = true;
                }
            }
            else {
                success = false;
            }
        }
        catch
        {
            success=false;
        }
        return success;
    }

    public static string DeviceTable = "DeviceInfos";
    public static string StartGameTable = "StartGameInfo";


    public static LCACL SetupACL(string clientUserId) 
    {
        LCACL acl = new LCACL();
        acl.SetUserIdReadAccess(clientUserId, true);
        acl.SetUserIdWriteAccess(clientUserId, true);
        acl.SetUserIdReadAccess(SysIMClientService.Inst.SysIMClient.Id, true);
        acl.SetUserIdWriteAccess(SysIMClientService.Inst.SysIMClient.Id, true);
        return acl;
    }

    public static async Task<string> CreateStartGameInfo(Dictionary<string,object> dic) 
    {
        LCQuery<LCObject> devQuery = new LCQuery<LCObject>(HelpService.StartGameTable);
        devQuery.WhereEqualTo("userId", dic["userId"]);
        devQuery.WhereEqualTo("userName", dic["userName"]);
        devQuery.OrderByDescending("createdAt");
        ReadOnlyCollection<LCObject> sGameTables = await devQuery.Find();

        if (sGameTables.Count > 10)
        {
            await sGameTables[9].Delete();
        }


        LCObject startGameInfo = new LCObject(StartGameTable);
        foreach (KeyValuePair<string, object> kv in dic)
        {
            startGameInfo[kv.Key] = kv.Value;
        }

        DateTime sysUtcTime = DateTime.ParseExact(
            $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
           "yyyy-MM-dd HH:mm:ss",
           System.Globalization.CultureInfo.InvariantCulture,
           System.Globalization.DateTimeStyles.AssumeUniversal
       );
        DateTime sysLocalTime = DateTime.ParseExact(
             $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            "yyyy-MM-dd HH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal
        );

        startGameInfo["sysUtc"] = sysUtcTime;
        startGameInfo["sysLocal"] = sysLocalTime;

        startGameInfo.ACL = SetupACL( dic["userId"].ConvertTo<string>() );

        await startGameInfo.Save();

        return startGameInfo.ObjectId;
    }

    public static async Task<string> CreateDeviceInfo(Dictionary<string, object> dic) 
    {
        LCQuery<LCObject> devQuery = new LCQuery<LCObject>(DeviceTable);
        devQuery.WhereEqualTo("userId", dic["userId"]);
        devQuery.WhereEqualTo("userName", dic["userName"]);
        LCObject devTable = await devQuery.First();

        if (devTable == null)
        {
            devTable = new LCObject(HelpService.DeviceTable);
            foreach (KeyValuePair<string, object> kv in dic)
            {
                devTable[kv.Key] = kv.Value;
            }
            devTable.ACL = SetupACL( dic["userId"].ConvertTo<string>() );

            await devTable.Save();
        }
        else
        {
            foreach (KeyValuePair<string, object> kv in dic)
            {
                devTable[kv.Key] = kv.Value;
            }
            await devTable.Save();
        }

        return devTable.ObjectId;
    }

}