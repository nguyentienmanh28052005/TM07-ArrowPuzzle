using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace mygame.sdk
{
    public static class LogPrefs
    {
        public static bool GetBool(string key, bool defaultValue = true)
            => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

        public static void SetBool(string key, bool value)
            => PlayerPrefs.SetInt(key, value ? 1 : 0);

        public static string GetString(string key, string defaultValue = "")
            => PlayerPrefs.GetString(key, defaultValue);

        public static void SetString(string key, string value)
            => PlayerPrefs.SetString(key, value);

        public static int GetInt(string key, int defaultValue = 0)
            => PlayerPrefs.GetInt(key, defaultValue);

        public static void SetInt(string key, int value)
            => PlayerPrefs.SetInt(key, value);
    }

    public class IgnoreEmptyStringResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization serialization)
        {
            var prop = base.CreateProperty(member, serialization);
            if (prop.PropertyType == typeof(string))
            {
                prop.ShouldSerialize = instance => !string.IsNullOrEmpty((string)prop.ValueProvider.GetValue(instance));
            }

            if (prop.PropertyType == typeof(double))
            {
                prop.ShouldSerialize = instance => !((double)prop.ValueProvider.GetValue(instance)).Equals(-1.0);
            }

            return prop;
        }
    }

    [Serializable]
    public class ObjectLog
    {
        public Dictionary<string, object> log;
        [JsonIgnore] public bool sended;
        [JsonIgnore] public bool sendedServer;
    }

    public interface ILogEvent
    {
        public bool EnableLog { get; }
        public void Initialized();
        public void AddLogCache(ObjectLog log);
        public void LogData();
    }

    public delegate Dictionary<string, object> ProvidePropertiesDelegate();

    public class LogEventManager : MonoBehaviour
    {
        public static ProvidePropertiesDelegate OnProvideProperties;

        public static string CF_LogABTestKey
        {
            get => PlayerPrefs.GetString("cf_log_ab_test_key", "");
            private set => PlayerPrefs.SetString("cf_log_ab_test_key", value);
        }

        private const int SectionTime = 1800000;
        private static LogEventManager _instance;
        public static LogEventManager Instance => _instance ??= FindObjectOfType<LogEventManager>();

        public static readonly JsonSerializerSettings jsonSetting = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new IgnoreEmptyStringResolver()
        };

        public static string[] ABTestKeys { get; private set; }

        private readonly HashSet<string> recentLogHashes = new();
        private readonly Queue<string> hashQueue = new();
        private const int MaxCachedLogs = 1;

        private Coroutine checkDayRunTime;
        private bool isReady;
        private bool initialized;
        private bool isFirstOpen = false;
        private long sessionStartTime;
        private readonly List<ILogEvent> logEvents = new();

        private string memPlayType = "";
        private string memPlayMode = "";

        public static string User_ID
        {
            get
            {
                var v = LogPrefs.GetString("log_user_id");
                if (!string.IsNullOrEmpty(v)) return v;
                v = Guid.NewGuid().ToString();
                LogPrefs.SetString("log_user_id", v);
                return v;
            }
        }

        public static int SessionNumber
        {
            get => LogPrefs.GetInt("log_session_number", 0);
            private set => LogPrefs.SetInt("log_session_number", value);
        }

        public static long FirstTimeJoinGame
        {
            get => long.Parse(LogPrefs.GetString("log_first_time_join_game", "0"));
            set => LogPrefs.SetString("log_first_time_join_game", value.ToString());
        }

        private static long lastDayLogin
        {
            get => long.Parse(LogPrefs.GetString("log_last_day_login", "0"));
            set => LogPrefs.SetString("log_last_day_login", value.ToString());
        }

        public static int InstallDay
            => int.Parse(SdkUtil
                .timeStamp2DateTime(FirstTimeJoinGame == 0 ? GameHelper.CurrentTimeMilisReal() : FirstTimeJoinGame)
                .ToString("yyyyMMdd"));

        private static string _sessionID;
        public static string SessionID
        {
            get
            {
                if (string.IsNullOrEmpty(_sessionID))
                {
                    _sessionID = $"{User_ID}:{GameHelper.CurrentTimeMilisReal()}";
                }
                return _sessionID;
            }
            private set => _sessionID = value;
        }

        public static int RetentionDay
            => FirstTimeJoinGame == 0 ? 0 : (int)((GameHelper.CurrentTimeMilisReal() - FirstTimeJoinGame) / 86400000L);

        public static int ActiveDay
        {
            get => LogPrefs.GetInt("active_day", -1);
            set => LogPrefs.SetInt("active_day", value);
        }

        public static int IsAllowTracking => LogPrefs.GetInt("user_allow_track_ads", 0);

        public static void SetABTestKey(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            try
            {
                ABTestKeys = JsonConvert.DeserializeObject<string[]>(value);
                CF_LogABTestKey = value;
            }
            catch (Exception e)
            {
                ABTestKeys = null;
            }
        }

        internal static string LogSegmentName
        {
            get => PlayerPrefs.GetString("log_segment_name", "default");
            set => PlayerPrefs.SetString("log_segment_name", value);
        }

        private static string GetLogABTest()
        {
            return LogSegmentName;
			if (ABTestKeys == null) return "default";
            var abTests = ABTestKeys.Select(PlayerPrefs.GetString)
                .Where(value => !string.IsNullOrEmpty(value) && value != "default").ToList();
            return abTests.Count > 0 ? string.Join(",", abTests) : "default";
        }

        public void LogAdsShow(string position, string ad_format, string ad_platform, string ad_network, bool is_show,
            string reason, float duration)
        {
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER
            if (AdsHelper.isRemoveAds(0) && ad_format == "full") return;
            if (ad_format is "banner" or "cl" or "rect" or "native") return;
            if (string.IsNullOrEmpty(ad_network))
            {
                ad_network = "none";
            }

            if (string.IsNullOrEmpty(ad_format))
            {
                ad_format = "none";
            }

            if (string.IsNullOrEmpty(ad_platform))
            {
                ad_platform = "none";
            }

            if (string.IsNullOrEmpty(position))
            {
                position = "none";
            }

            LogEvent("ad_show", new Dictionary<string, object>
            {
                { "position", position },
                { "ad_format", ad_format },
                { "ad_network", ad_network },
                { "ad_platform", ad_platform },
                { "is_show", is_show ? 1 : 0 },
                { "reason", reason },
                { "duration", duration }
            });
#endif
        }

        public void LogAdsRevenue(string ad_format, string position, string ad_network, string ad_platform,
            double ad_value)
        {
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER
            if (string.IsNullOrEmpty(position))
            {
                position = "none";
            }

            if (string.IsNullOrEmpty(ad_format))
            {
                ad_format = "none";
            }

            if (string.IsNullOrEmpty(ad_network))
            {
                ad_network = "none";
            }

            if (string.IsNullOrEmpty(ad_platform))
            {
                ad_platform = "none";
            }

            LogEvent("ad_revenue", new Dictionary<string, object>
            {
                { "position", position },
                { "ad_format", ad_format },
                { "ad_network", ad_network },
                { "ad_platform", ad_platform },
                { "value", ad_value }
            });
#endif
        }

        public void LogAdLoad(string ad_format, string ad_platform, string ad_network, string position, bool is_load,
            float load_time)
        {
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER
            if (string.IsNullOrEmpty(ad_network))
            {
                ad_network = "none";
            }

            if (string.IsNullOrEmpty(ad_format))
            {
                ad_format = "none";
            }

            if (string.IsNullOrEmpty(ad_platform))
            {
                ad_platform = "none";
            }

            if (string.IsNullOrEmpty(position))
            {
                position = "none";
            }

            LogEvent("ad_load", new Dictionary<string, object>
            {
                { "ad_format", ad_format },
                { "ad_platform", ad_platform },
                { "ad_network", ad_network },
                { "position", position },
                { "load_time", load_time },
                { "is_load", is_load ? 1 : 0 }
            });
#endif
        }

        public void LogAdClick(string ad_format, string ad_platform, string ad_network, string position)
        {
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER
            if (string.IsNullOrEmpty(ad_network))
            {
                ad_network = "none";
            }

            if (string.IsNullOrEmpty(ad_format))
            {
                ad_format = "none";
            }

            if (string.IsNullOrEmpty(ad_platform))
            {
                ad_platform = "none";
            }

            if (string.IsNullOrEmpty(position))
            {
                position = "none";
            }

            LogEvent("ad_click", new Dictionary<string, object>
            {
                { "ad_format", ad_format },
                { "ad_platform", ad_platform },
                { "ad_network", ad_network },
                { "position", position }
            });
#endif
        }

        /// <param name="level">Level number.</param>
        /// <param name="play_type">How the level was started (e.g., "home", "retry").</param>
        /// <param name="play_index">How many times the player has played this level.</param>
        /// <param name="mode">The game mode (e.g., "classic", "challenge").</param>
        public void LogLevelPlay(int level, string play_type, int play_index, string mode)
        {
            string eventPlay = $"lv{level:0000}_play";
            if(mode != null && mode.Length > 0)
            {
                eventPlay = mode + "_" + eventPlay;
            }
            mygame.sdk.FIRhelper.logAdEvent(eventPlay);
            if (play_type != null && play_type.Length > 0)
            {
                mygame.sdk.FIRhelper.logAdEvent(eventPlay + "_" + play_type);
            }
            memPlayMode = mode;
            memPlayType = play_type;
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER
            LogEvent("level_play", new Dictionary<string, object>
            {
                { "level", level },
                { "play_type", play_type },
                { "play_index", play_index },
                { "mode", mode }
            });
#endif
        }


        /// <param name="level">Level number.</param>
        /// <param name="play_time">Time spent playing.</param>
        /// <param name="play_type">Play context (e.g., "home", "retry").</param>
        /// <param name="play_index">Play attempt number.</param>
        /// <param name="mode">Game mode (e.g., "classic", "challenge").</param>
        /// <param name="result">Level result (e.g., "win", "exit").</param>
        public void LogLevelEnd(int level, float play_time, string play_type, int play_index, string mode,
            string result)
        {
            string eventPlay = $"lv{level:0000}_{result}";
            if(mode != null && mode.Length > 0)
            {
                eventPlay = mode + "_" + eventPlay;
            }
            mygame.sdk.FIRhelper.logAdEvent(eventPlay);
            if (play_type != null && play_type.Length > 0)
            {
                mygame.sdk.FIRhelper.logAdEvent(eventPlay + "_" + play_type);
            }
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER
            LogEvent("level_end", new Dictionary<string, object>
            {
                { "level", level },
                { "play_time", play_time },
                { "play_type", play_type },
                { "play_index", play_index },
                { "mode", mode },
                { "result", result }
            });
#endif
        }

        /// <param name="level">Level number.</param>
        /// <param name="bonus_type">Type of bonus received (e.g., "time", "moves").</param>
        /// <param name="bonus_amount">Amount of bonus (seconds or moves).</param>
        public void LogLevelSecondChance(int level, string bonus_type = "None", int bonus_amount = 0)
        {
            string eventPlay = $"lv{level:0000}_play_sc";
            if(memPlayMode != null && memPlayMode.Length > 0)
            {
                eventPlay = memPlayMode + "_" + eventPlay;
            }
            mygame.sdk.FIRhelper.logAdEvent(eventPlay);
            if (memPlayType != null && memPlayType.Length > 0)
            {
                mygame.sdk.FIRhelper.logAdEvent(eventPlay + "_" + memPlayType);
            }
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER
            LogEvent("level_second_chance", new Dictionary<string, object>
            {
                { "level", level },
                { "bonus_type", bonus_type },
                { "bonus_amount", bonus_amount }
            });
#endif
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER

            var c = logEvents.Count(logEvent => !logEvent.EnableLog);
            if (c >= logEvents.Count)
            {
                return;
            }

            var propertys = OnProvideProperties?.Invoke();
            var logObj = BuildObjectLog(eventName, propertys, parameters);
            LogEvent(logObj);
#endif
        }

        private ObjectLog BuildObjectLog(string eventName, Dictionary<string, object> propertys,
            Dictionary<string, object> parameters)
        {
            var now = GameHelper.CurrentTimeMilisReal();
            var dtNow = SdkUtil.timeStamp2DateTime(now);
            Dictionary<string, object> log;
            if (propertys != null)
            {
                log = new Dictionary<string, object>(propertys);
            }
            else
            {
                log = new Dictionary<string, object>();
            }

            if (parameters != null)
            {
                log.Add("eventData", parameters);
            }

            AddIfMissing("eventName", eventName);
            AddIfMissing("sequenceId", NextLogSequenceID());
            AddIfMissing("_ts", now);
            AddIfMissing("timestamp", now);
            AddIfMissing("user_id", User_ID);
            AddIfMissing("app_name", Application.productName);
            AddIfMissing("app_version", Application.version);
            AddIfMissing("language", GameHelper.Instance.languageCode);
            AddIfMissing("country", GameHelper.Instance.countryCode);
            AddIfMissing("device_info",
                $"{LogEventCustom.GetDeviceModel()},{SystemInfo.deviceName},{SystemInfo.systemMemorySize},{SystemInfo.processorType},{SystemInfo.processorCount},{SystemInfo.processorFrequency}");
            AddIfMissing("event_date", int.Parse(dtNow.ToString("yyyyMMdd")));
            AddIfMissing("session_number", SessionNumber);
            AddIfMissing("session_id", SessionID);
            AddIfMissing("install_day", InstallDay);
            AddIfMissing("retention_day", RetentionDay);
            AddIfMissing("active_day", ActiveDay);
            AddIfMissing("local_hour", DateTime.Now.Hour);
            AddIfMissing("local_weekday", $"{((int)dtNow.DayOfWeek + 1)} - {dtNow.DayOfWeek}");
            AddIfMissing("allow_tracking", IsAllowTracking);
            AddIfMissing("connection_type", GetConnectionType());
            AddIfMissing("media_source", SDKManager.Instance.mediaSource);
            AddIfMissing("campaign", SDKManager.Instance.mediaCampain);
            AddIfMissing("af_adset", SDKManager.Instance.afAdset);
            AddIfMissing("af_ad", SDKManager.Instance.afAd);
            AddIfMissing("af_ad_id", SDKManager.Instance.afAdId);
            AddIfMissing("gcm_id", FIRhelper.Instance.gcm_id);
            AddIfMissing("ab_test", GetLogABTest());
            AddIfMissing("current_level", GameRes.LevelCommon());
            AddIfMissing("platform", Application.platform.ToString());
            return new ObjectLog { log = log };

            void AddIfMissing(string key, object value)
            {
                log.TryAdd(key, value);
            }
        }

        private void LogEvent(ObjectLog log)
        {
            if (log == null /*|| IsDuplicate(log)*/) return;
            foreach (var logEvent in logEvents)
            {
                logEvent.AddLogCache(log);
            }

            LogData();
        }

        public void LogData()
        {
            if (!isReady) return;
            CheckInternetConnection(status =>
            {
                if (!status) return;
                foreach (var logEvent in logEvents)
                {
                    logEvent.LogData();
                }
            });
        }

        private bool IsDuplicate(ObjectLog log)
        {
            var hash = GetHash(log);
            if (!recentLogHashes.Add(hash)) return true;
            hashQueue.Enqueue(hash);
            if (hashQueue.Count > MaxCachedLogs)
                recentLogHashes.Remove(hashQueue.Dequeue());
            return false;
        }

        private string GetHash(ObjectLog log)
        {
            var excludedFields = new[] { "sequenceId", "_ts", "balance_hearttime" };
            var json = JObject.FromObject(log.log, JsonSerializer.Create(jsonSetting));
            foreach (var field in excludedFields) json.Remove(field);
            return GetMD5Hash(JsonConvert.SerializeObject(json, jsonSetting));
        }

        private static string GetMD5Hash(string input)
        {
            using var md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "")
                .ToLowerInvariant();
        }

        public static string ToDataBucket(ObjectLog objectLog)
        {
            return JsonConvert.SerializeObject(objectLog.log, jsonSetting);
        }

        private void CheckInternetConnection(System.Action<bool> callback)
        {
            StartCoroutine(CheckInternetCoroutine(callback));
        }

        private IEnumerator CheckInternetCoroutine(Action<bool> callback)
        {
            using var www = UnityWebRequest.Get("https://www.apple.com/library/test/success.html");
            www.timeout = 10;
            yield return www.SendWebRequest();
            callback(www.result == UnityWebRequest.Result.Success);
        }

        private static int logSequenceID;

        private static int NextLogSequenceID()
        {
            if (logSequenceID == 0) logSequenceID = LogPrefs.GetInt("logsequenceid", 1);
            var id = logSequenceID++;
            if (logSequenceID >= int.MaxValue - 1) logSequenceID = 1;
            LogPrefs.SetInt("logsequenceid", logSequenceID);
            return id;
        }

        public static string GetConnectionType()
        {
            return Application.internetReachability switch
            {
                NetworkReachability.NotReachable => "offline",
                NetworkReachability.ReachableViaCarrierDataNetwork => "mobile_data",
                NetworkReachability.ReachableViaLocalAreaNetwork => "wifi",
                _ => "unknown"
            };
        }

        private void Awake()
        {
            SetABTestKey(CF_LogABTestKey);
            if (FirstTimeJoinGame == 0)
            {
                FirstTimeJoinGame = GameHelper.CurrentTimeMilisReal();
                isFirstOpen = true;
            }

            _instance = this;
#if ENABLE_LOGDATA_BUCKET
            logEvents.Add(new LogDataBucket());
#endif
#if ENABLE_LOGDATA_MYSERVER
            logEvents.Add(new LogServer());
#endif
        }

        private void Start()
        {
            Initialized();
            SessionID = $"{User_ID}:{GameHelper.CurrentTimeMilisReal()}";
            sessionStartTime = GameHelper.CurrentTimeMilisReal();
            SessionNumber++;
            isReady = true;
            CheckNewDay();
            if (isFirstOpen)
            {
                FirstOpen();
            }

            SessionStart();
            checkDayRunTime = StartCoroutine(CheckNewDayRunTime());
        }

        private void Initialized()
        {
            foreach (var logEvent in logEvents)
            {
                logEvent.Initialized();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                if (sessionStartTime == 0) return;
                CheckNewDay();

                if (GameHelper.CurrentTimeMilisReal() - sessionStartTime > SectionTime)
                {
                    sessionStartTime = GameHelper.CurrentTimeMilisReal();
                    ResetSession();
                    SessionStart();
                }

                checkDayRunTime = StartCoroutine(CheckNewDayRunTime());
            }
            else
            {
                LogData();
                sessionStartTime = GameHelper.CurrentTimeMilisReal();
                if (checkDayRunTime != null) StopCoroutine(checkDayRunTime);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                checkDayRunTime = StartCoroutine(CheckNewDayRunTime());
            }
            else
            {
                LogData();
                if (checkDayRunTime != null) StopCoroutine(checkDayRunTime);
            }
        }

        private IEnumerator CheckNewDayRunTime()
        {
            while (true)
            {
                yield return new WaitForSeconds(10);
                CheckNewDay();
            }
        }

        private static void CheckNewDay()
        {
            if (!IsNewDay(lastDayLogin)) return;
            lastDayLogin = GameHelper.CurrentTimeMilisReal();
            ActiveDay++;
        }

        public void SessionStart()
        {
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER
            LogEvent("session_start");
#endif
        }

        public void FirstOpen()
        {
#if ENABLE_LOGDATA_BUCKET || ENABLE_LOGDATA_MYSERVER
            LogEvent("first_open");
#endif
        }

        public static void ResetSession()
        {
            SessionNumber++;
            SessionID = $"{User_ID}:{GameHelper.CurrentTimeMilisReal()}";
        }

        public static bool IsNewDay(long timeLogin)
        {
            var d1 = TicksToDateTime(TimestampToTicks(timeLogin));
            var d2 = TicksToDateTime(TimestampToTicks(GameHelper.CurrentTimeMilisReal()));
            return d2.Date > d1.Date;

            static long TimestampToTicks(long timstamp)
            {
                return (timstamp * 10000 + 621355968000000000);
            }

            static DateTime TicksToDateTime(long ticks)
            {
                return new DateTime(ticks);
            }
        }
    }
}