using System;
using System.Collections;
using System.Collections.Generic;
using MyJson;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
using Firebase.RemoteConfig;
#endif

namespace mygame.sdk
{
    public class FIRParserOtherConfig
    {
        public static void parserInGameConfig() //
        {
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_log_level");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                SdkUtil.levelLog = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_log_level", SdkUtil.levelLog);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_local_notification");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_local_notification", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_data_show_event");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFDataShowEvent = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_data_notification");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFDataNotfication = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_gold_default");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFGoldDefault = (int)v.LongValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_visible_ui_first_level");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFVisibleUIFirstLevel = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_disable_main_if_lower_level");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_DisableMainIfLowerLevel = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_active_battle_pass");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LevelActiveBattlePass = (int)v.LongValue;
                LogEvent.LogABTest("cf_level_active_battle_pass", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_day_loop_active_battle_pass");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_DayLoopActiveBattlePass = (int)v.LongValue;
                LogEvent.LogABTest("cf_day_loop_active_battle_pass", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_active_battle_pass");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.ActiveBattlePass = (int)v.LongValue;
                LogEvent.LogABTest("cf_active_battle_pass", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_battle_pass_point_per_level");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.BattlePass_PointPerLevel = (int)v.LongValue;
                LogEvent.LogABTest("cf_battle_pass_point_per_level", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_delay_save_user_info");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                SaveUserInfoAPI.DelaySaveUserInfo = (int)v.LongValue;
                LogEvent.LogABTest("cf_delay_save_user_info", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_active_event_race");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LevelActiveEventRace = (int)v.LongValue;
                LogEvent.LogABTest("cf_level_active_event_race", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_active_event_race");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.ActiveEventRace = (int)v.LongValue;
                LogEvent.LogABTest("cf_active_event_race", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_max_layer_in_surface");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_MaxLayerInSurface = (int)v.LongValue;
                LogEvent.LogABTest("cf_max_layer_in_surface", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_preview_surface_child");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_PreviewSurfaceChild = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_preview_surface_child", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_rotate_360");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_Rotate360 = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_rotate_360", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_section_show_main");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_SectionShowMain = (int)v.LongValue;
                LogEvent.LogABTest("cf_section_show_main", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_price_heart");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_PriceHeart = v.StringValue;
                // LogEvent.LogABTest("cf_price_heart", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_show_remove_ads");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFLevelShowRemoveAds = (int)v.LongValue;
                LogEvent.LogABTest("cf_level_show_remove_ads", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_play_game_if_not_first_open");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_PlayGameIfNotFirstOpen = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_play_game_if_not_first_open", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_default_music");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                AudioManager.CF_DefaultMusic = (int)v.LongValue == 1;
                if (!SplashLoadingCtr.isLoading) AudioManager.Instance.ChangeStateAudio();
                LogEvent.LogABTest("cf_default_music", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("time_splash_first_section");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_TimeSplashFirstSection = (int)v.LongValue;
                if (SDKManager.Instance.counSessionGame == 1)
                {
                    SDKManager.Instance.updateTimeSplash((int)v.LongValue);
                }

                LogEvent.LogABTest("time_splash_first_section", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_disable_music");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LevelDisableMusic = (int)v.LongValue;
                if (!SplashLoadingCtr.isLoading) AudioManager.Instance.ChangeStateAudio();
                LogEvent.LogABTest("cf_level_disable_music", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_unlock_full_match_box");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LevelUnlockFullMatchBox = (int)v.LongValue;
                LogEvent.LogABTest("cf_level_unlock_full_match_box", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_tutorial_about_die");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LevelTutorialAboutDie = (int)v.LongValue;
                LogEvent.LogABTest("cf_level_tutorial_about_die", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_max_level_revive");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_MaxLevelFreeRevive = (int)v.LongValue;
                LogEvent.LogABTest("cf_max_level_revive", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_default_multiplier");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                float val = (int)v.LongValue / 100f;
                PlayerPrefsUtil.CF_DefaultMultiplier = val;
                SpeedMultiplierManager.SetDefaultMultiplierDirect(val);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_type_tutorial");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_TypeTutorial = (int)v.LongValue;
                LogEvent.LogABTest("cf_type_tutorial", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_lock_input_tutorial");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LockInputTutorial = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_lock_input_tutorial", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_enable_heart");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_EnableHeart = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_enable_heart", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_enable_intro_level");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_EnableIntroLevel = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_enable_intro_level", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_reset_spend_if_lose");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_ResetSpendIfLose = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_reset_spend_if_lose", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_enable_banner");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFEnableBanner = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_enable_banner", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_index_v2");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFLevelIndex = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_type");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFLevelType = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_first_level_show_main");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_FirstLevelShowMain = (int)v.LongValue;
                LogEvent.LogABTest("cf_first_level_show_main", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ad_break_data");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFAdBreak = v.StringValue;
            }

            //v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ad_break_data_1");
            //if (v.StringValue != null && v.StringValue.Length > 0)
            //{
            //    PlayerPrefsUtil.CFAdBreak1 = v.StringValue;
            //}

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_enable_level_difficulty");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFEnableLevelDifficulty = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_monetization_v3");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LevelMonetization = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_enable_save_game");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_EnableSaveGame = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_enable_save_game", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_play_game_if_has_save");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_PlayGameIfHasSave = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_enable_fail_ui");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_EnableFailUI = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_enable_fail_ui", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_show_ads_complete");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_ShowAdsComplete = (int)v.LongValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_battle_pass");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                // ConfigManager.Instance.SetBattlePassConfig(v.StringValue);
                // LogEvent.LogABTest("cf_battle_pass", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_time_reopen_race");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_TimeReopenRace = (int)v.LongValue;
                LogEvent.LogABTest("cf_time_reopen_race", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_show_play_popup");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFLevelShowPlayPopup = (int)v.LongValue;
                LogEvent.LogABTest("cf_level_show_play_popup", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_unlock_sweetparty");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LevelUnlockSweetParty = (int)v.LongValue;
                LogEvent.LogABTest("cf_level_unlock_sweetparty", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_show_pack_on_claim_daily_reward");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_ShowPackOnClaimDailyReward = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_show_pack_on_claim_daily_reward", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_unlock_pack_weekend");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LevelUnlockPackWeekend = (int)v.LongValue;
                LogEvent.LogABTest("cf_level_unlock_pack_weekend", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_get_point_battle_pass");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_GetPointBatllePass = (int)v.LongValue;
                LogEvent.LogABTest("cf_get_point_battle_pass", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_active_pack_to_review");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_ActivePackToReview = (int)v.LongValue == 1;
                LogEvent.LogABTest("cf_active_pack_to_review", (int)v.LongValue);

                var comIAP_PackWeekend = DataManager.Instance.ComIAP_PackWeekend;
                comIAP_PackWeekend.ActivePack();
                if (PlayerPrefsUtil.CF_ActivePackToReview)
                {
                    var comIAP_ControlPack = DataManager.Instance.ComIAP_ControlPack;
                    var cache = GameManager.ReasonBackToHome;
                    GameManager.ReasonBackToHome = ReasonBackToHome.Exit;
                    comIAP_ControlPack.IsActivePackBooster(RES_type.ExtraSlot);
                    comIAP_ControlPack.IsActivePackBooster(RES_type.Shuffle);
                    comIAP_ControlPack.IsActivePackBooster(RES_type.Clear);
                    master.Observer.Notify(master.ObserverName.flash_sale_active, 1);
                    GameManager.ReasonBackToHome = cache;
                }
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("config_streak_data");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("config_streak_data", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("config_booster_magnet");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("config_booster_magnet", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("config_booster_mutil_color_box");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("config_booster_mutil_color_box", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("config_booster_add_hole");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("config_booster_add_hole", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("config_booster_break_object");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("config_booster_break_object", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("config_booster_clear");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("config_booster_clear", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_team_battle_gift_battle");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_team_battle_gift_battle", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_team_battle_gift_team");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_team_battle_gift_team", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_team_adventure_gift");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_team_adventure_gift", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_gold_buy_heart");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_gold_buy_heart", (int)v.LongValue);
                LogEvent.LogABTest("cf_gold_buy_heart", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_recover_time_heart");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_recover_time_heart", (int)v.LongValue);
                LogEvent.LogABTest("cf_recover_time_heart", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_revive_price");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_revive_price", (int)v.LongValue);
                LogEvent.LogABTest("cf_revive_price", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_show_rate");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_level_show_rate", (int)v.LongValue);
                LogEvent.LogABTest("cf_level_show_rate", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_price_booster");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_price_booster", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_revive_price_lose_increase");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_revive_price_lose_increase", (int)v.LongValue);
                LogEvent.LogABTest("cf_revive_price_lose_increase", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_all_event");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                ConfigEventController.SetDataEventConfig(v.StringValue);
            }
            
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_show_full_lose_level");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFShowFullLoseLevel = v.StringValue;
            }
            
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_refill_pack_tier_config");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                ConfigManager.CF_RefillPackTierConfig = v.StringValue;
            }


            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_data_level_chest");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                LevelChestManager.CF_LevelChest = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("config_booster_all");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("config_booster_all", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_x2_value_gold_win");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                ConfigManager.CF_X2ValueGoldWin = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_win_prize");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_win_prize", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_new_tutorial");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFNewTutorial = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_new_tutorial_hold");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFNewTutorialHold = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_section_show_banner");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFSectionShowBanner = (int)v.LongValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_enable_log_data_bucket_server");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                LogEventCustom.CF_EnableLogDataBucketServer = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("allowed_device_ids");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                DebugMenuManager.FetchRemoteConfig(v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_enable_log_request_assets");
            if (v.StringValue is { Length: > 0 })
            {
                LogEventCustom.CF_EnableLogRequestAssets = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_home_decor");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                // HomeDecor.GameManager.SetConfig(v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_num_watch_ads");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_num_watch_ads", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_gen_level_mode");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_gen_level_mode", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ratio_sound");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetFloat("cf_ratio_sound", Mathf.Clamp(v.LongValue / 100f, 0, 1));
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_auto_zoom");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_auto_zoom", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_revive_two_times");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_revive_two_times", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ads_button_position");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_ads_button_position", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_force_tut_booster");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_force_tut_booster", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_fail_no_ads");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_level_fail_no_ads", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_num_revive_ads");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_num_revive_ads", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_home_decor_gift");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_home_decor_gift", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_map_data_decor");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_map_data_decor", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_home_decor_price");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_home_decor_price", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_streak_bonus_star");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_streak_bonus_star", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_streak_active");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_streak_active", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_reduce_death_times");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_reduce_death_times", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_tutorial_guide");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_tutorial_guide", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("CF_Level_Unlock_Tray");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("CF_Level_Unlock_Tray", v.StringValue);
            }

            // key new
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_unlock_booster");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                BoosterManager.ValueLevelUnlock = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_bar_x2_reward_win");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFBarX2RewardWin = (int)v.LongValue == 1;
            }


            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_rate_app_review");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_RateAppReview = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_num_remove_basket_if_revive");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_NumRemoveBasketIfRevive = (int)v.LongValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("config_mechanic_all_1");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("config_mechanic_all_1", v.StringValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_show_full_first_time_fail");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_show_full_first_time_fail", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_enable_profile");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CFEnableProfile = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_lv_show_package_remove_ads");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                ConfigManager.CF_LevelShowPackageRemoveAds = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_lv_show_package_starter_pack");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                ConfigManager.CF_LevelShowPackageStarterPack = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_default_vibrate");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_DefaultVibrate = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_hint_click_object");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_DefaultHintClickObject = (int)v.LongValue == 1;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_time_hint_click_object");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_TimeShowHintClickObject = (int)v.LongValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_time_hint_in_level");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                LevelManager.CF_TimeHintInLevel = (int)v.LongValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_show_first_tutorial");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                ConfigManager.CF_LevelShowFirstTutorial = (int)v.LongValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_end_hint_click");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_LevelEndHintClick = (int)v.LongValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_show_intro_level");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                LevelManager.CF_ShowIntroLevel = (int)v.LongValue;
            }
            
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_num_watch_ads_daily");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_num_watch_ads_daily", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_show_intro_level");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                LevelManager.CF_ShowIntroLevel = (int)v.LongValue;
            }
            
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_lv_show_package_happy_pack");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                ConfigManager.CF_LevelShowPackageHappyPack = v.StringValue;
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_lv_show_package_weekend_pack");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                ConfigManager.CF_LevelShowPackageWeekendPack = v.StringValue;
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_extraslot_price_increase");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                BoosterManager.CF_ExtraSlot_Price_Increase = (int)v.LongValue;
                LogEvent.LogABTest("cf_extraslot_price_increase", (int)v.LongValue);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_app_icon_name");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsUtil.CF_AppIconName = v.StringValue;
            }

#endif
            GameEvent.OnReceiveFirebaseDataDone?.Invoke();
            BoosterConfig.SetConfigAll();
            LevelRemoteManager.Instance.ParserFirConfig();
            AudioManager.Instance.ChangeStateAudio();
            AdsRewardConfig.SetConfigAll();
        }
    }
}