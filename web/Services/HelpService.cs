
using LC.Newtonsoft.Json;
using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Storage;
using LeanCloud.Storage.Internal.Codec;
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
    public static string START_GAME = "100001";

    
    public static string CONSUME_POWER_FAILURE = "100100";
    public static string CONSUME_POWER_SUCCESS = "100101";

    public static string CONSUME_GOLD_COIN_FAILURE = "100102";
    public static string CONSUME_GOLD_COIN_SUCCESS = "100103";

    public static string CONSUME_GEM_FAILURE = "100104";
    public static string CONSUME_GEM_SUCCESS = "100105";



    public static string ADD_POWER_SUCCESS = "100200";
    public static string ADD_GOLD_COIN_SUCCESS = "100201";
    public static string ADD_GEM_SUCCESS = "100202";

    /// <summary>
    /// 结算获得玩偶
    /// </summary>
    public static string ADD_SPECIAL_DOLL = "100300";


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

    #region//用户相关
    public static async Task<LCUser> GetUser(string objectId)
    {
        LCQuery<LCUser> query = LCUser.GetQuery();
        //query.WhereEqualTo("objectId", objectId);
        LCUser user = await query.Get(objectId);
        return user;
    }
    private static async Task<bool> isOnline(string objectId) 
    {
        LCUser user = await GetUser(objectId);
        return user["online"].ConvertTo<bool>();
    }
    public static async Task<bool> isSignUped(object userName)
    {
        LCQuery<LCUser> query = LCUser.GetQuery();
        query.WhereEqualTo("username", userName);
        LCUser firstUser = await query.First();
        if (firstUser != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static async Task<bool> SetupOnline(string objectId, bool online)
    {
        bool success = false;
        try
        {
            LCUser user = await GetUser(objectId);
            user["online"] = online;
            await user.Save();
            success = true;
        }
        catch (LCException ex)
        {
            LCLogger.Error(ex.Code + ":" + ex.Message);
        }
        return success;
    }

    private static async Task SetupPointer(LCUser user, string key, LCObject lcobject)
    {
        user[key] = lcobject;
        await user.Save();
    }
    public static async Task<LCUser> SetupPointer(string userId,string key,LCObject lcobject)
    {
        LCUser user = await GetUser(userId);
        user[key] = lcobject;
        await user.Save();
        return user;
    }
    public static async Task<LCUser> SetupUser(string userId, string key, object value)
    {
        LCUser user = await GetUser(userId);
        user[key] = value;
        await user.Save();
        return user;
    }
    public static async Task<bool> AddSpecialDoll(LCUser user)
    {
        LCObject currStartGameObj = await GetCurrentStartGameInfo(user);
        string getSpecialDoll = currStartGameObj["containsSpecialDoll"] as string;
        LCLogger.Debug($"==== {getSpecialDoll}");
       

        if (getSpecialDoll != null)
        {
            LCObject playerProp = await CreateOrGetPlayerPropsInfoFromUser(user);
            LCLogger.Debug($"==== {playerProp["specialDolls"] }");


            List<object> dolls = new List<object>();
            if (playerProp["specialDolls"] != null)
            {
                dolls.AddRange((object[])playerProp["specialDolls"]);
            }
            dolls.Add(getSpecialDoll);

            playerProp["specialDolls"] = dolls.ToArray();
            await playerProp.Save();

            await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, HelpService.ADD_SPECIAL_DOLL, new Dictionary<string, object>()
            {
                {"addSpecialDoll",getSpecialDoll }
            });
        }
        return true;
    }
    #endregion

    /// <summary>
    /// default ACL Setup
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static LCACL SetupACL(LCUser user) 
    {
        LCACL acl = new LCACL();
        acl.SetUserReadAccess(user, true);
        acl.SetUserWriteAccess(user, true);
        acl.SetUserReadAccess(SysService.Inst.SysUser, true);
        acl.SetUserWriteAccess(SysService.Inst.SysUser, true);
        return acl;
    }

    /// <summary>
    /// StartGameInfo
    /// </summary>
    /// <param name="user"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static async Task<LCObject> CreateStartGameInfo(LCUser user,Dictionary<string,object> dic) 
    {
        LCQuery<LCObject> devQuery = new LCQuery<LCObject>(StartGameTable);
        devQuery.WhereEqualTo("userId", user.ObjectId );
        devQuery.WhereEqualTo("userName", user.Username );
        devQuery.OrderByDescending("createdAt");
        ReadOnlyCollection<LCObject> sGameTables = await devQuery.Find();

        if (sGameTables.Count >= 50 )
        {
            await sGameTables[49].Delete();
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

        startGameInfo.ACL = SetupACL(user);

        await startGameInfo.Save();

        return startGameInfo;
    }
    /// <summary>
    /// DeviceInfo
    /// </summary>
    /// <param name="user"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static async Task<LCObject> CreateOrSetupDeviceInfo(LCUser user,Dictionary<string, object> dic) 
    {
        LCObject pointer = user["deviceInfo"] as LCObject;
        LCObject deviceObj = default;
        if (pointer == null)
        {
            deviceObj = LCObject.Create(DeviceTable);
            deviceObj.ACL = SetupACL(user);
            foreach (KeyValuePair<string, object> kv in dic)
            {
                deviceObj[kv.Key] = kv.Value;
            }
            await deviceObj.Save();
            await SetupPointer(user, "deviceInfo", deviceObj);
        }
        else
        {
            LCQuery<LCObject> query = new LCQuery<LCObject>(DeviceTable);
            deviceObj = await query.Get(pointer.ObjectId);
            foreach (KeyValuePair<string, object> kv in dic)
            {
                deviceObj[kv.Key] = kv.Value;
            }
            await deviceObj.Save();
        }
        return deviceObj;
    }

    /// <summary>
    /// 获取用户最近的startGameInfo
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static async Task<LCObject> GetCurrentStartGameInfo(LCUser user)
    {
        LCObject pointer = user["startGameInfo"] as LCObject;
        LCQuery<LCObject> query = new LCQuery<LCObject>(StartGameTable);
        LCObject startGameObj = await query.Get(pointer.ObjectId);
        return startGameObj;
    }

    /// <summary>
    /// GameConfig
    /// </summary>
    /// <returns></returns>
    public static async Task<LCObject> GetGameConfigTableInfo()
    {
        LCQuery<LCObject> devQuery = new LCQuery<LCObject>(GameConfigTable);
        LCObject gameconfigs = await devQuery.First();
        return gameconfigs;
    }

    /// <summary>
    /// SpecialDolls
    /// </summary>
    /// <returns></returns>
    private static async Task<ReadOnlyCollection<LCObject>> GetSpecialDollsTableInfo()
    {
        LCQuery<LCObject> devQuery = new LCQuery<LCObject>(SpecialDollsTable);
        ReadOnlyCollection<LCObject> objs = await devQuery.Find();
        return objs;
    }
    /// <summary>
    /// PlayerPropsInfo
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static async Task<LCObject> CreateOrGetPlayerPropsInfoFromUser(LCUser user)
    {
        LCObject pointer = user["playerPropInfo"] as LCObject;
        LCObject playerProp = default;

        if (pointer == null)
        {
            playerProp = LCObject.Create(PlayerPropsTable);
            playerProp.ACL = SetupACL(user);
            playerProp["userId"] = user.ObjectId;
            playerProp["userName"] = user.Username;

            //playerProp["gem"] = 0;
            //playerProp["goldCoin"] = 100;
            //playerProp["power"] = 50;
            //playerProp["specialDolls"] = user.Username;
            await playerProp.Save();
            await SetupPointer(user, "playerPropInfo", playerProp );
        }
        else {
            LCQuery<LCObject> query = new LCQuery<LCObject>(PlayerPropsTable);
            playerProp = await query.Get(pointer.ObjectId);
        }
        return playerProp;
    }


    #region// Power Value
    public static async Task<bool> AddPower(LCUser user)
    {
        bool success = false;
        LCObject palyerProp = await CreateOrGetPlayerPropsInfoFromUser(user);
        if (palyerProp != null)
        {
            LCObject gameConfig = await GetGameConfigTableInfo();
            int addPowerValue = ConvertTo<int>( gameConfig["perAddPower"] );

            int userPower = ConvertTo<int>(palyerProp["power"]);
            userPower += addPowerValue;

            palyerProp["power"] = userPower;
            await palyerProp.Save();
            


            await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, HelpService.ADD_POWER_SUCCESS, new Dictionary<string, object>()
            {
                { "AddCount",10}
            });

            success = true;
        }
        return success;
    }
    public static async Task<bool> ConsumePower(LCUser user)
    {
        bool success = false;
        LCObject palyerProp = await CreateOrGetPlayerPropsInfoFromUser(user);
        if (palyerProp != null)
        {
            LCObject gameConfig = await GetGameConfigTableInfo();
            int consumeCount = ConvertTo<int>(gameConfig["perConsumePower"]);
            int userPower = ConvertTo<int>(palyerProp["power"]);

            if (userPower >= consumeCount)
            {
                success = true;
            }

            if (success)
            {
                palyerProp["power"] = userPower - consumeCount;
                await palyerProp.Save();
                await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, CONSUME_POWER_SUCCESS, new Dictionary<string, object>()
                {
                    { "consumeCount",consumeCount}
                });
            }
            else {
                await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, CONSUME_POWER_FAILURE, new Dictionary<string, object>()
                {
                    { "missingCount",Math.Abs(userPower - consumeCount) }
                });
            }
        }
        return success;
    }
    #endregion

    #region// GoldCoin
    public static async Task<bool> ConsumeGoldCoin(LCUser user, int consumeCount)
    {
        bool success = false;
        LCObject palyerProp = await CreateOrGetPlayerPropsInfoFromUser(user);
        if (palyerProp != null)
        {
            int userGoldCoin = ConvertTo<int>(palyerProp["goldCoin"]);
            if (userGoldCoin>= consumeCount) 
            {
                success = true;
            }


            if (success)
            {
                palyerProp["goldCoin"] = userGoldCoin - consumeCount;
                await palyerProp.Save();
                await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, CONSUME_GOLD_COIN_SUCCESS, new Dictionary<string, object>()
                {
                    { "consumeCount",consumeCount}
                });
            }
            else {
                await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, CONSUME_GOLD_COIN_FAILURE, new Dictionary<string, object>()
                {
                    { "missingCount",Math.Abs(userGoldCoin - consumeCount) }
                });
            }


            success = true;
        }
        return success;
    }
    public static async Task<bool> AddGoldCoin(LCUser user, int count) 
    {
        bool success = false;
        LCObject palyerProp = await CreateOrGetPlayerPropsInfoFromUser(user);
        if (palyerProp != null)
        {
            int userGoldCoin = ConvertTo<int>(palyerProp["goldCoin"]);
            palyerProp["goldCoin"] = userGoldCoin + count;
            await palyerProp.Save();
            

            await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, HelpService.ADD_GOLD_COIN_SUCCESS, new Dictionary<string, object>()
            {
                { "AddCount",count}
            });

            success = true;
        }
        return success;
    }
    #endregion

    #region// Gem
    public static async Task<bool> ConsumeGem(LCUser user, int consumeCount)
    {
        bool success = false;
        LCObject palyerProp = await CreateOrGetPlayerPropsInfoFromUser(user);
        if (palyerProp != null)
        {
            int userGem = ConvertTo<int>(palyerProp["gem"]);
            if (userGem >= consumeCount)
            {
                success = true;
            }

            if (success)
            {
                palyerProp["gem"] = userGem - consumeCount;
                await palyerProp.Save();
                await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, CONSUME_GEM_SUCCESS, new Dictionary<string, object>()
                {
                    { "consumeCount",consumeCount}
                });
            }
            else
            {
                await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, CONSUME_GEM_FAILURE, new Dictionary<string, object>()
                {
                    { "missingCount",Math.Abs(userGem - consumeCount) }
                });
            }

        }
        return success;
    }
    public static async Task<bool> AddGem(LCUser user, int count)
    {
        bool success = false;
        LCObject palyerProp = await CreateOrGetPlayerPropsInfoFromUser(user);
        if (palyerProp != null)
        {
            int userGem = ConvertTo<int>(palyerProp["gem"]);
            palyerProp["gem"] = userGem + count;
            await palyerProp.Save();

            await RESTAPIService.Inst.SendMessageToSubscribesClientsAsync(new string[] { user.ObjectId }, HelpService.ADD_GEM_SUCCESS, new Dictionary<string, object>()
            {
                { "AddCount",count }
            });

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