//#define ENABLE_TIKTOK_ANALYTIC

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_TIKTOK_ANALYTIC
using SDK;
#endif

namespace mygame.sdk
{
    public class TiktokBusiness : MonoBehaviour
    {
        public static SDKManager Instance { get; private set; }
        public bool isTest = false;
        public string iOSAppSecret = "";
        public string iOSTitokAppId = "";
        public string AndroidAppSecret = "";
        public string AndroidTitokAppId = "";

        private void Awake()
        {
            if (Instance == null)
            {
#if ENABLE_TIKTOK_ANALYTIC
                string AndroidAppId = AppConfig.appid;
                string iOSAppId = AppConfig.appid;
#if UNITY_ANDROID
                iOSAppId = "";
#else
                AndroidAppId = "";
#endif
                TikTokConfig config = new TikTokConfig(
                    iOSAppSecret, 
                    iOSAppId, 
                    iOSTitokAppId, 
                    AndroidAppSecret,
                    AndroidAppId, 
                    AndroidTitokAppId
                );
                if (isTest)
                {
                    config.OpenDebugMode();
                    config.SetLogLevel(TiktokLogLevel.Debug);
                }
                TikTokBusinessSDK.InitializeSdk(config, (status, flag, des) =>
                {
                    Debug.Log($"mysdk: TiktokBusiness InitializeSdk {status}-{flag}-{des}");
                });
#endif

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (this != Instance) Destroy(gameObject);
            }
        }
        // Start is called before the first frame update
        void Start()
        {
        }

        public static void logAdRevenueAdmob(string adformat, string adSource, string adunitId, int precisionType, long valueMicros, Dictionary<string, string> dicparams)
        {
#if ENABLE_TIKTOK_ANALYTIC && !UNITY_EDITOR
            Dictionary<string, object> adRevenue = new Dictionary<string, object>();
            adRevenue.Add("value", valueMicros);
            adRevenue.Add("currency_code", dicparams["currency_code"]);
            adRevenue.Add("precision", precisionType);
            adRevenue.Add("ad_unit_id", adunitId);
            adRevenue.Add("ad_source_name", dicparams["ad_source_name"]);
            adRevenue.Add("ad_source_id", dicparams["ad_source_id"]);
            adRevenue.Add("ad_source_instance_name", dicparams["ad_source_instance_name"]);
            adRevenue.Add("ad_source_instance_id", dicparams["ad_source_instance_id"]);
            adRevenue.Add("mediation_group_name", dicparams["mediation_group_name"]);
            adRevenue.Add("mediation_ab_test_name", dicparams["mediation_ab_test_name"]);
            adRevenue.Add("mediation_ab_test_variant", dicparams["mediation_ab_test_variant"]);
            adRevenue.Add("device_ad_mediation_platform", "admob_sdk");
            adRevenue.Add("ad_format", convertAdformatAdmob(adformat));

            TikTokAdRevenueEvent adRevenueEvent = new TikTokAdRevenueEvent(adRevenue, "");
            TikTokBusinessSDK.TrackTTEvent(adRevenueEvent);

            logDic("tiktok logEventAds Admob", adRevenue);
#endif
        }

        public static void logAdRevenueIron(string auction_id, string ad_unit, string ad_network, string instance_name, string instance_id, string country, string placement, double revenue, string precision, string ab, string segment_name, double lifetime_revenue, string encrypted_cpm, string conversion_value)
        {
#if ENABLE_TIKTOK_ANALYTIC && !UNITY_EDITOR
            Dictionary<string, object> adRevenue = new Dictionary<string, object>();

            adRevenue.Add("device_ad_mediation_platform", "ironsource_sdk");
            adRevenue.Add("auction_id", auction_id);
            adRevenue.Add("ad_unit", ad_unit);
            adRevenue.Add("ad_network", ad_network);
            adRevenue.Add("instance_name", instance_name);
            adRevenue.Add("instance_id", instance_id);
            adRevenue.Add("country", country);
            adRevenue.Add("placement", placement);
            adRevenue.Add("revenue", revenue);
            adRevenue.Add("precision", precision);
            adRevenue.Add("ab", ab);
            adRevenue.Add("segment_name", segment_name);
            adRevenue.Add("lifetime_revenue", lifetime_revenue);
            adRevenue.Add("encrypted_cpm", encrypted_cpm);
            adRevenue.Add("conversion_value", conversion_value);
        
            TikTokAdRevenueEvent adRevenueEvent = new TikTokAdRevenueEvent(adRevenue, "");
            TikTokBusinessSDK.TrackTTEvent(adRevenueEvent);

            logDic("tiktok logEventAds Iron", adRevenue);
#endif
        }

        public static void logAdRevenueMax(double revenue, string countryCode, string networkName, string format, string adUnitIdentifier, string placement, string networkPlacement)
        {
#if ENABLE_TIKTOK_ANALYTIC && !UNITY_EDITOR
            Dictionary<string, object> adRevenue = new Dictionary<string, object>();
            
            adRevenue.Add("device_ad_mediation_platform","applovin_max_sdk");
            adRevenue.Add("revenue", revenue);
            adRevenue.Add("country_code", countryCode);
            adRevenue.Add("network_name", networkName);
            adRevenue.Add("ad_format", format);  
            adRevenue.Add("ad_unit_id", adUnitIdentifier);
            adRevenue.Add("placement", placement);
            adRevenue.Add("network_placement", networkPlacement);
                        
            TikTokAdRevenueEvent adRevenueEvent = new TikTokAdRevenueEvent(adRevenue, "");
            TikTokBusinessSDK.TrackTTEvent(adRevenueEvent);

            logDic("tiktok logEventAds Max", adRevenue);
#endif
        }

        static string convertAdformatAdmob(string adformat)
        {
            string re = adformat;
            if (adformat.StartsWith("native_"))
            {
                re = "native";
            }
            else if (adformat.CompareTo("openad") == 0)
            {
                re = "splash";
            }
            else if (adformat.CompareTo("rewarded_interstitial") == 0)
            {
                re = "rewarded interstitial";
            }
            else if (adformat.StartsWith("banner_"))
            {
                re = "banner";
            }

            return re;
        }

        public static Dictionary<string, string> getAdmobParam(string data)
        {
            Dictionary<string, string> re = new Dictionary<string, string>();
            string[] arrdata = data.Split(';');
            string[] df = { "", "", "", "", "", "", "", "" };
            for (int i = 0; i < 8; i++)
            {
                if (arrdata.Length > i)
                {
                    df[i] = arrdata[i];
                }
            }

            re.Add("currency_code", df[0]);
            re.Add("ad_source_name", df[1]);
            re.Add("ad_source_id", df[2]);
            re.Add("ad_source_instance_name", df[3]);
            re.Add("ad_source_instance_id", df[4]);
            re.Add("mediation_group_name", df[5]);
            re.Add("mediation_ab_test_name", df[6]);
            re.Add("mediation_ab_test_variant", df[7]);
            return re;
        }

        public static void logDic(string msg, Dictionary<string, object> dic)
        {
            if (SdkUtil.isLog())
            {
                string diccontent = "";
                if (dic != null)
                {
                    foreach (var item in dic)
                    {
                        if (item.Value != null)
                        {
                            diccontent += item.Key + ":" + item.Value.ToString() + " ";
                        }
                    }
                }
                Debug.Log($"mysdk: TiktokBusiness {msg} {diccontent}");
            }
        }
    }
}
