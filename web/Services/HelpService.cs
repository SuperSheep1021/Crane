
using LC.Newtonsoft.Json;
using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;


public static class HelpService 
{
    public static string DeviceTable = "DeviceInfos";
    public static string StartGameTable = "StartGameInfos";
    public static string GameConfigTable = "GameConfig";
    public static string PlayerPropsTable = "PlayerProps";
    public static string SpecialDollsTable = "SpecialDolls";

    public static string ON_LOGIN = "100000";
    public static string ON_SIGUP = "100001";
    public static string START_GAME = "100002";

    public static string CONSUME_POWER_FAILURE = "100100";


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

    private static async Task<LCUser> GetUser(string objectId)
    {
        LCQuery<LCUser> query = LCUser.GetQuery();
        query.WhereEqualTo("objectId", objectId);
        LCUser user = await query.First();
        return user;
    }
    private static async Task<bool> isOnline(string objectId) 
    {
        LCUser user = await GetUser(objectId);
        return user["online"].ConvertTo<bool>();
    }
    
    public static async Task UserSetupPointer(string userId,string key,LCObject lcobject)
    {
        LCUser user = await GetUser(userId);
        user[key] = lcobject;
        await user.Save();
    }

    public static async Task<bool> ValidateClientID(string clientUserId,string parametersClientUserId)
    {
        bool success = false;
        try
        {
            bool online = await isOnline(clientUserId);
            if (online)
            {
                //验证
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
    public static LCACL SetupACL(string clientUserId) 
    {
        LCACL acl = new LCACL();
        acl.SetUserIdReadAccess(clientUserId, true);
        acl.SetUserIdWriteAccess(clientUserId, true);
        acl.SetUserIdReadAccess(SysIMClientService.Inst.SysIMClient.Id, true);
        acl.SetUserIdWriteAccess(SysIMClientService.Inst.SysIMClient.Id, true);
        return acl;
    }
    public static async Task<LCObject> CreateStartGameInfo(Dictionary<string,object> dic) 
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

        return startGameInfo;
    }
    public static async Task<LCObject> CreateOrSetupDeviceInfo(Dictionary<string, object> dic) 
    {
        SysIMClientService.Inst.SysIMClient.GetQuery();
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

        return devTable;
    }
    public static async Task<LCObject> CreateDefaultPlayerPropsInfoFromUser(string userId)
    {
        LCObject playerProp = new LCObject(PlayerPropsTable);
        playerProp.ACL = SetupACL(userId);
        await playerProp.Save();
        return playerProp;
    }
    private static async Task<LCObject> GetGameConfigTableInfo()
    {
        LCQuery<LCObject> devQuery = new LCQuery<LCObject>(GameConfigTable);
        LCObject gameConfig = await devQuery.First();
        return gameConfig;
    }
    private static async Task<ReadOnlyCollection<LCObject>> GetSpecialDollsTableInfo()
    {
        LCQuery<LCObject> devQuery = new LCQuery<LCObject>(SpecialDollsTable);
        ReadOnlyCollection<LCObject> objs = await devQuery.Find();
        return objs;
    }
    public static async Task<LCObject> GetPlayerPropsInfoFromUser(string userId)
    {
        LCQuery<LCObject> query = new LCQuery<LCObject>(PlayerPropsTable);
        query.WhereEqualTo("userId", userId);
        var palyerProp = await query.First();
        return palyerProp;
    }
    //public static async Task<T> GetPlayerPropsInfoFromUser<T>(string userId) where T : LCObject
    //{
    //    LCQuery<T> query = new LCQuery<T>(PlayerPropsTable);
    //    query.WhereEqualTo("userId", userId);
    //    var palyerProp = await query.First();
    //    return palyerProp;
    //}

    #region//Power Value
    public static async Task<bool> AddConsumePower(string userId)
    {
        bool success = false;
        LCObject palyerProp = await GetPlayerPropsInfoFromUser(userId);
        if (palyerProp != null)
        {
            LCObject gameConfig = await GetGameConfigTableInfo();
            int addPowerValue = ConvertTo<int>( gameConfig["perAddPower"] );

            int userPower = ConvertTo<int>(palyerProp["power"]);
            userPower += addPowerValue;

            palyerProp["power"] = userPower;
            await palyerProp.Save();
            success = true;
        }
        return success;
    }
    public static async Task<bool> ConsumePower(string userId)
    {
        bool success = false;
        LCObject palyerProp = await GetPlayerPropsInfoFromUser(userId);
        if (palyerProp != null)
        {
            LCObject gameConfig = await GetGameConfigTableInfo();
            int conSumeValue = ConvertTo<int>(gameConfig["perConsumePower"]);
            int userPower = ConvertTo<int>(palyerProp["power"]);
            if (userPower >= conSumeValue)
            {
                userPower -= conSumeValue;
                palyerProp["power"] = userPower;
                await palyerProp.Save();
                success = true;
            }
        }
        return success;
    }
    #endregion

    #region// GoldCoin
    public static async Task<bool> ConsumeGoldCoin(string userId, int consumeCount)
    {
        bool success = false;
        LCObject palyerProp = await GetPlayerPropsInfoFromUser(userId);
        if (palyerProp != null)
        {
            int userGoldCoin = ConvertTo<int>(palyerProp["goldCoin"]);
            if (userGoldCoin>= consumeCount) 
            {
                userGoldCoin -= consumeCount;
            }
            palyerProp["goldCoin"] = userGoldCoin;
            await palyerProp.Save();
            success = true;
        }
        return success;
    }
    public static async Task<bool> AddGoldCoin(string userId,int count) 
    {
        bool success = false;
        LCObject palyerProp = await GetPlayerPropsInfoFromUser(userId);
        if (palyerProp != null)
        {
            int userGoldCoin = ConvertTo<int>(palyerProp["goldCoin"]);
            userGoldCoin += count;

            palyerProp["goldCoin"] = userGoldCoin;
            await palyerProp.Save();
            success = true;
        }
        return success;
    }
    #endregion

    #region// Gem
    public static async Task<bool> ConsumeGem(string userId, int consumeCount)
    {
        bool success = false;
        LCObject palyerProp = await GetPlayerPropsInfoFromUser(userId);
        if (palyerProp != null)
        {
            int userGem = ConvertTo<int>(palyerProp["gem"]);
            if (userGem >= consumeCount)
            {
                userGem -= consumeCount;
            }
            palyerProp["gem"] = userGem;
            await palyerProp.Save();
            success = true;
        }
        return success;
    }
    public static async Task<bool> AddGem(string userId, int count)
    {
        bool success = false;
        LCObject palyerProp = await GetPlayerPropsInfoFromUser(userId);
        if (palyerProp != null)
        {
            int userGem = ConvertTo<int>(palyerProp["gem"]);
            userGem += count;

            palyerProp["gem"] = userGem;
            await palyerProp.Save();
            success = true;
        }
        return success;
    }
    #endregion

    #region //ProbabilityTool 
    public static WeightedGenerator<string> SpecialDollsGenerator { get;private set; }
    public static async Task<bool> SetupSpecialDollsWeight() 
    {
        bool success = false;
        ReadOnlyCollection<LCObject> speDolls = await GetSpecialDollsTableInfo();
        if (speDolls.Count>0) 
        {
            SpecialDollsGenerator = new WeightedGenerator<string>();
            for (int i = 0; i < speDolls.Count; i++)
            {
                string name = ConvertTo<string>(speDolls[i]["dollName"]);
                int weight = ConvertTo<int>(speDolls[i]["weighted"]);
                SpecialDollsGenerator.AddItem(name, weight);
            }
            success = true;
        }
        return success;
    }

    public static async Task<bool> isCreateSpecialDoll() 
    {
        LCObject gameConfig = await GetGameConfigTableInfo();
        int probability = ConvertTo<int>( gameConfig["specialToyProbability"] );
        return ShouldExecute(probability);
    }


    // 线程本地存储的Random实例（确保多线程安全，避免种子重复）
    private static readonly ThreadLocal<Random> _threadLocalRandom = new ThreadLocal<Random>(
        () => new Random(Guid.NewGuid().GetHashCode()) // 用GUID哈希作为种子，降低重复概率
    );

    /// <summary>
    /// 根据概率百分比判断是否执行
    /// </summary>
    /// <param name="probabilityPercent">概率百分比（0-100，包含0和100）</param>
    /// <returns>true：执行；false：不执行</returns>
    /// https://www.doubao.com/thread/w168a32c6ad71453a
    /// <exception cref="ArgumentOutOfRangeException">当概率超出0-100范围时抛出</exception>
    private static bool ShouldExecute(int probabilityPercent)
    {
        // 校验参数合法性
        if (probabilityPercent < 0 || probabilityPercent > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(probabilityPercent),
                "概率百分比必须在0-100之间"
            );
        }

        // 边界情况优化：0%一定不执行，100%一定执行（无需生成随机数）
        if (probabilityPercent == 0) return false;
        if (probabilityPercent == 100) return true;

        // 转换为0-1之间的概率值（例如30% → 0.3）
        double probability = probabilityPercent / 100.0;

        // 生成[0.0, 1.0)的随机数，若小于目标概率则返回true
        return _threadLocalRandom.Value.NextDouble() < probability;
    }
    #endregion

}