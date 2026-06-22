using System;
using System.Collections.Generic;
using mygame.sdk;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Fetches remote experiment segments from <see cref="ExperimentManager"/> and
/// dispatches the JSON result to the appropriate handler based on rule name.
/// </summary>
public class ExperimentSegmentHandler : MonoBehaviour
{
    #region Constants

    // ── Rule names ───────────────────────────────────────────────────────────
    public const string RULE_LEVEL_CONFIG = "bus_fever_config_rule_1000";
#if UNITY_ANDROID
    public const string RULE_SEGMENT_USER = "segment_user_android";
#else
    public const string RULE_SEGMENT_USER = "segment_user_ios";
#endif

    #endregion

    #region Params

#if TEST_CAMPAIGN_COUNTRY
    private bool isWaitingForTestInput = false;
    private string testCampaign = "";
    private string testCountry = "";
    private const string PREF_TEST_DONE = "Test_FirstFetchDone";
    private const string PREF_TEST_CAMPAIGN = "Test_CampaignStr";
    private const string PREF_TEST_COUNTRY = "Test_CountryStr";
#endif

    Dictionary<string, string> parameters = new Dictionary<string, string>()
    {
        { "level", "1" },
        { "campaign", "" },
        { "platform", "" },
        { "country", "" },
        { "count_purchase", "" },
        { "total_purchase", "" },
        { "verapp", "" },
        { "retention_day", "" },
    };

    #endregion

    public static ExperimentSegmentHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        GameEvent.OnIncreaseLevel += OnFinishLevel;
        InappHelper.OnPackagePurchased += OnPackagePurchased;
        AppsFlyerHelperScript.OnConversionDataDone += OnConversionDataDone;
    }

    void Start()
    {
#if TEST_CAMPAIGN_COUNTRY
        if (PlayerPrefs.GetInt(PREF_TEST_DONE, 0) == 0)
        {
            isWaitingForTestInput = true;
            return;
        }
        else
        {
            testCampaign = PlayerPrefs.GetString(PREF_TEST_CAMPAIGN, "");
            testCountry = PlayerPrefs.GetString(PREF_TEST_COUNTRY, "");
        }
#endif
        FetchConfig(new string[] { RULE_LEVEL_CONFIG, RULE_SEGMENT_USER });
    }

    private void OnDestroy()
    {
        GameEvent.OnIncreaseLevel -= OnFinishLevel;
        InappHelper.OnPackagePurchased -= OnPackagePurchased;
        AppsFlyerHelperScript.OnConversionDataDone -= OnConversionDataDone;
    }

    private void OnFinishLevel()
    {
        FetchConfig(RULE_LEVEL_CONFIG);
    }

    private void OnPackagePurchased(string productId)
    {
        FetchConfig(RULE_SEGMENT_USER);
    }

    private void OnConversionDataDone(bool arg1, string arg2)
    {
        if (arg1)
        {
            FetchConfig(RULE_SEGMENT_USER);
        }
    }

    #region Public Methods

    /// <summary>
    /// Central dispatcher. Fetches the JSON for each rule in <paramref name="ruleNames"/>,
    /// then routes to the dedicated handler method for that rule.
    /// </summary>
    /// <param name="ruleNames">One or more RULE_* constants, e.g. <c>RULE_LEVEL_CONFIG</c>.</param>
    public void FetchConfig(params string[] ruleNames)
    {
        if (ruleNames == null || ruleNames.Length == 0) return;
        SetParameters();
        foreach (string rule in ruleNames)
        {
            string capturedRule = rule;
            this.FetchRuleName(capturedRule, (success, messages) =>
            {
                if (!success) return;

                switch (capturedRule)
                {
                    case RULE_LEVEL_CONFIG:
                        this.GetConfig(messages);
                        break;
                    case RULE_SEGMENT_USER:
                        this.GetSegmentUser(messages);
                        break;

                    default:
                        Debug.Log($"[ExperimentSegmentHandler] No handler for rule: {capturedRule}");
                        break;
                }
            }, parameters);
        }
    }

    #endregion

    #region Private Methods

    private void SetParameters()
    {
        var country = GameHelper.Instance != null ? GameHelper.Instance.countryCode : "";
        if (string.IsNullOrEmpty(country))
        {
            country = GameHelper.Instance.countryCode;
        }

        if (!string.IsNullOrEmpty(country))
        {
            country = country.ToLower();
        }

        parameters["level"] = DataManager.Level.ToString();
        parameters["campaign"] = SDKManager.Instance.mediaCampain;
        parameters["platform"] = Application.platform.ToString();
        parameters["country"] = country;
        parameters["count_purchase"] = InappHelper.CountPurchase.ToString();
        parameters["total_purchase"] = InappHelper.TotalPurchase.ToString();
        parameters["verapp"] = AppConfig.verapp.ToString();
        parameters["retention_day"] = LogEventCustom.RetentionDay.ToString();

#if TEST_CAMPAIGN_COUNTRY
        if (PlayerPrefs.GetInt(PREF_TEST_DONE, 0) == 1)
        {
            if (!string.IsNullOrEmpty(testCampaign)) parameters["campaign"] = testCampaign;
            if (!string.IsNullOrEmpty(testCountry)) parameters["country"] = testCountry;
        }
#endif
    }

    /// <summary>
    /// Fetches raw JSON from <see cref="ExperimentManager"/> for the given rule.
    /// Parses the response array and invokes <paramref name="onDone"/> once with the full message list.
    /// </summary>
    private void FetchRuleName(string ruleName, Action<bool, string> onDone,
        Dictionary<string, string> parameters = null)
    {
        ExperimentManager.Instance.FetchUserSegments(ruleName, (success, json) =>
        {
            if (!success)
            {
                Debug.LogWarning($"[ExperimentSegmentHandler] FetchRuleName failed ({ruleName}): {json}");
                onDone?.Invoke(false, null);
                return;
            }

            onDone?.Invoke(true, json);
        }, parameters);
    }

    /// <summary>
    /// Parses a JSON array of <c>{"message": "...", "name": "..."}</c> objects.
    /// Sets <see cref="LogEventManager.LogABTestName"/> from the first valid entry.
    /// </summary>
    /// <returns>List of message strings extracted from the array.</returns
    /// <summary>Handles messages received for rule <c>level_config</c>.</summary>
    private void GetConfig(string json)
    {
        RequestConfigLevel204++;
        LevelRemoteManager.Instance.GetNewConfig(json);
    }

    private void GetSegmentUser(string json)
    {
        try
        {
            Debug.Log($"[ExperimentSegmentHandler] GetSegmentUser {json}");
            var config = SegmentUserConfig.Parse(json);
            if (config == null) return;
            config.Apply();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public static int RequestConfigLevel204
    {
        get => PlayerPrefs.GetInt("request_config_level204", 0);
        set => PlayerPrefs.SetInt("request_config_level204", value);
    }

#if TEST_CAMPAIGN_COUNTRY
    public void SetTestOverridesAndFetch(string campaign, string country)
    {
        testCampaign = campaign;
        testCountry = country;
        PlayerPrefs.SetString(PREF_TEST_CAMPAIGN, testCampaign);
        PlayerPrefs.SetString(PREF_TEST_COUNTRY, testCountry);
        PlayerPrefs.SetInt(PREF_TEST_DONE, 1);
        PlayerPrefs.Save();
        
        isWaitingForTestInput = false;
        FetchConfig(RULE_SEGMENT_USER);
    }
#endif

    #endregion
}