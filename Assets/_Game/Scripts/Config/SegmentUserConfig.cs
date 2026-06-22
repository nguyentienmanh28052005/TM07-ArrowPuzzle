using System;
using System.Collections.Generic;
using mygame.sdk;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Wrapper chứa toàn bộ config từ data bucket segment_user.
/// Parse 1 lần, apply 1 lần, query bất cứ lúc nào qua <see cref="Current"/>.
/// </summary>
public class SegmentUserConfig
{
    // ── Parsed fields ───────────────────────────────────────
    public string SegmentUser       { get; private set; }
    public string AdsReviveJson     { get; private set; }
    public int    NumAdsDaily       { get; private set; } = -1;
    public int    NumAdsRevive      { get; private set; } = -1;
    public int    FullAdsLevelStart { get; private set; } = -1;
    public int    FullAfterPurchase { get; private set; } = -1;
    public string AbTestName        { get; private set; }

    // ── Static accessor ─────────────────────────────────────
    /// <summary>Config mới nhất từ data bucket. Null nếu chưa fetch.</summary>
    public static SegmentUserConfig Current { get; private set; }

    // ── Parse ───────────────────────────────────────────────
    /// <summary>
    /// Parse JSON từ data bucket thành typed config.
    /// Trả về null nếu JSON invalid hoặc rỗng.
    /// </summary>
    public static SegmentUserConfig Parse(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;

        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (data == null || data.Count == 0) return null;

        var config = new SegmentUserConfig();

        if (data.TryGetValue("segment_user", out var segmentUser))
            config.SegmentUser = segmentUser;

        if (data.TryGetValue("config_ads_revive", out var adsRevive))
            config.AdsReviveJson = adsRevive;

        if (data.TryGetValue("config_num_ads_daily", out var numAdsDaily)
            && int.TryParse(numAdsDaily, out int dailyVal))
            config.NumAdsDaily = dailyVal;

        if (data.TryGetValue("config_num_ads_revive", out var numAdsRevive)
            && int.TryParse(numAdsRevive, out int reviveVal))
            config.NumAdsRevive = reviveVal;

        if (data.TryGetValue("config_full_ads_level_start", out var fullLevel)
            && int.TryParse(fullLevel, out int fullLevelVal))
            config.FullAdsLevelStart = fullLevelVal;

        if (data.TryGetValue("config_full_after_purchase", out var fullPurchase)
            && int.TryParse(fullPurchase, out int purchaseVal))
            config.FullAfterPurchase = purchaseVal;

        if (data.TryGetValue("abtest_name", out var abTestName))
            config.AbTestName = abTestName;

        return config;
    }

    // ── Apply ───────────────────────────────────────────────
    /// <summary>
    /// Dispatch config values tới các hệ thống game.
    /// Cập nhật <see cref="Current"/> sau khi apply.
    /// </summary>
    public void Apply()
    {
        if (!string.IsNullOrEmpty(SegmentUser))
            LogEventCustom.UserSegment = SegmentUser;

        if (!string.IsNullOrEmpty(AdsReviveJson))
            AdsRewardConfig.SetConfigDataBucket(AdsReviveJson);

        if (NumAdsDaily >= 0)
            BoosterManager.SetNumWatchAdsDailyConfig(NumAdsDaily);

        if (NumAdsRevive >= 0)
            AdsRewardConfig.SetNumAdsRevive(NumAdsRevive);

        if (FullAdsLevelStart >= 0)
        {
            if (AdsHelper.Instance != null && AdsHelper.Instance.currConfig != null)
                AdsHelper.Instance.currConfig.fullLevelStart = FullAdsLevelStart;
            PlayerPrefs.SetInt("cf_fullLevelStart", FullAdsLevelStart);
        }

        if (FullAfterPurchase >= 0)
        {
            if (AdsHelper.Instance != null && AdsHelper.Instance.currConfig != null)
                AdsHelper.Instance.currConfig.fullTimeAfterPurchase = FullAfterPurchase;
            PlayerPrefs.SetInt("cf_fullTimeAfterPurchase", FullAfterPurchase);
        }

        if (!string.IsNullOrEmpty(AbTestName))
            LogEventManager.LogSegmentName = AbTestName;

        Current = this;
    }
}
