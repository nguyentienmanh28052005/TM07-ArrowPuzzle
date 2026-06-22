using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using System.Linq;

namespace mygame.sdk
{
    public class PlayerPrefsUtil
    {
        public static bool CF_RateAppReview
        {
            get => PlayerPrefs.GetInt("cf_rate_app_review", 1) == 1;
            set => PlayerPrefs.SetInt("cf_rate_app_review", value ? 1 : 0);
        }
        
        public static bool CFEnableProfile
        {
            get => PlayerPrefs.GetInt("cf_enable_profile", 1) == 1;
            set => PlayerPrefs.SetInt("cf_enable_profile", value ? 1 : 0);
        }
        
        public static float CFLoopSpeed
        {
            get => PlayerPrefs.GetFloat("cf_loop_speed", 1.5f);
            set => PlayerPrefs.SetFloat("cf_loop_speed", value);
        }

        public static float CFLerpSpeedTime
        {
            get => PlayerPrefs.GetFloat("cf_lerp_speed_time", 1.5f);
            set => PlayerPrefs.SetFloat("cf_lerp_speed_time", value);
        }

        public static float CF_DefaultMultiplier
        {
            get => PlayerPrefs.GetFloat("cf_default_multiplier", 0.85f);
            set => PlayerPrefs.SetFloat("cf_default_multiplier", value);
        }

        public static int CFTutorialNewMechanicGuide
        {
            get => PlayerPrefs.GetInt("cf_tutorial_new_mechanic_guide", 1);
            set => PlayerPrefs.SetInt("cf_tutorial_new_mechanic_guide", value);
        }
        
        public static int CF_LevelShowBooster
        {
            get => PlayerPrefs.GetInt("cf_level_show_booster", 0);
            set => PlayerPrefs.SetInt("cf_level_show_booster", value);
        }

        public static string CFShowFullLoseLevel
        {
            get => PlayerPrefs.GetString("cf_show_full_lose_level", "");
            set => PlayerPrefs.SetString("cf_show_full_lose_level", value);
        }

        public static int CF_MaxLayerInSurface
        {
            get => PlayerPrefs.GetInt("cf_max_layer_in_surface", 8);
            set => PlayerPrefs.SetInt("cf_max_layer_in_surface", value);
        }

        public static bool CF_ResetSpendIfLose
        {
            get => PlayerPrefs.GetInt("cf_reset_spend_if_lose", 0) == 1;
            set => PlayerPrefs.SetInt("cf_reset_spend_if_lose", value ? 1 : 0);
        }

        public static bool CF_PreviewSurfaceChild
        {
            get
            {
#if UNITY_EDITOR
                return true;
#endif
                return PlayerPrefs.GetInt("cf_preview_surface_child", 1) == 1;
            }
            set => PlayerPrefs.SetInt("cf_preview_surface_child", value ? 1 : 0);
        }

        public static int CF_NumReviveShowAds
        {
            get => PlayerPrefs.GetInt("cf_num_revive_ads", 2);
            set => PlayerPrefs.SetInt("cf_num_revive_ads", value);
        }

        public static string CFEnableAdsReward
        {
            get => PlayerPrefs.GetString("cf_enable_ads_reward",
                "{\"levelStart\":99999999,\"levelEnd\":0,\"position\":{\"ui_level_complete\":{\"Item1\":10}}}");
            set => PlayerPrefs.SetString("cf_enable_ads_reward", value);
        }

        public static int CFLevelFailNoAds
        {
            get => PlayerPrefs.GetInt("cf_level_fail_no_ads", 10);
            set => PlayerPrefs.SetInt("cf_level_fail_no_ads", value);
        }

        public static bool CFNewTutorial
        {
            get => PlayerPrefs.GetInt("cf_new_tutorial", 0) == 1;
            set => PlayerPrefs.SetInt("cf_new_tutorial", value ? 1 : 0);
        }

        public static bool CFNewTutorialHold
        {
            get => PlayerPrefs.GetInt("cf_new_tutorial_hold", 0) == 1;
            set => PlayerPrefs.SetInt("cf_new_tutorial_hold", value ? 1 : 0);
        }
        
        public static bool CF_AdsButtonPosition
        {
            get => PlayerPrefs.GetInt("cf_ads_button_position", 0) == 1;
            set => PlayerPrefs.SetInt("cf_ads_button_position", value ? 1 : 0);
        }

        public static bool CF_Revive2Time
        {
            get => PlayerPrefs.GetInt("cf_revive_two_times", 0) != 0;
            set => PlayerPrefs.SetInt("cf_revive_two_times", value ? 1 : 0);
        }

        public static string CFLevelIndex
        {
            get => PlayerPrefs.GetString("cf_level_index_v2", "");
            set => PlayerPrefs.SetString("cf_level_index_v2", value);
        }

        public static string CFLevelType
        {
            get => PlayerPrefs.GetString("cf_level_type", "");
            set => PlayerPrefs.SetString("cf_level_type", value);
        }


        public static string CFAdBreak
        {
            get => PlayerPrefs.GetString("cf_ad_break_data", "");
            set => PlayerPrefs.SetString("cf_ad_break_data", value);
        }

        public static bool CFBarX2RewardWin
        {
            get => PlayerPrefs.GetInt("cf_bar_x2_reward_win", 0) == 1;
            set => PlayerPrefs.SetInt("cf_bar_x2_reward_win", value ? 1 : 0);
        }

        public static bool CFEnableBanner
        {
            get => PlayerPrefs.GetInt("is_enable_banner", 0) == 1;
            set => PlayerPrefs.SetInt("is_enable_banner", value ? 1 : 0);
        }

        public static int CFSectionShowBanner
        {
            get => PlayerPrefs.GetInt("cf_section_show_banner", 0);
            set => PlayerPrefs.SetInt("cf_section_show_banner", value);
        }

        public static int CFLevelShowRemoveAds
        {
            get => PlayerPrefs.GetInt("cf_level_show_remove_ads", 6);
            set => PlayerPrefs.SetInt("cf_level_show_remove_ads", value);
        }

        public static bool CFEnableLevelDifficulty
        {
            get => PlayerPrefs.GetInt("cf_enable_level_difficulty", 1) == 1;
            set => PlayerPrefs.SetInt("cf_enable_level_difficulty", value ? 1 : 0);
        }

        public static string CFDataShowEvent
        {
            get => PlayerPrefs.GetString("cf_data_show_event", "");
            set => PlayerPrefs.GetString("cf_data_show_event", value);
        }

        public static string CFDataNotfication
        {
            get => PlayerPrefs.GetString("cf_data_notification", "");
            set => PlayerPrefs.GetString("cf_data_notification", value);
        }


        public static int CFGoldDefault
        {
            get => PlayerPrefs.GetInt("cf_gold_default", 1000);
            set => PlayerPrefs.SetInt("cf_reduce_diff_rate", value);
        }

        public static bool CFVisibleUIFirstLevel
        {
            get => PlayerPrefs.GetInt("cf_visible_ui_first_level", 0) == 1;
            set => PlayerPrefs.SetInt("cf_visible_ui_first_level", value ? 1 : 0);
        }

        public static bool CF_DefaultVibrate
        {
            #if UNITY_ANDROID 
            get => PlayerPrefs.GetInt("cf_default_vibrate", 1) == 1;
            #else 
            get => PlayerPrefs.GetInt("cf_default_vibrate", 1) == 1;
            #endif
            set => PlayerPrefs.SetInt("cf_default_vibrate", value ? 1 : 0);
        }

        public static bool AudioVibrateSetting
        {
            get => PlayerPrefs.GetInt(GameHelper.KeyConfigVibrate, CF_DefaultVibrate ? 1 : 0) == 1;
            set => PlayerPrefs.SetInt(GameHelper.KeyConfigVibrate, value ? 1 : 0);
        }

        public static int CF_LevelEndHintClick
        {
            get => PlayerPrefs.GetInt("cf_level_end_hint_click", -1);
            set => PlayerPrefs.SetInt("cf_level_end_hint_click", value);
        }
        public static bool CF_DefaultHintClickObject
        {
            get => PlayerPrefs.GetInt("cf_hint_click_object", 1) == 1;
            set => PlayerPrefs.SetInt("cf_hint_click_object", value ? 1 : 0);
        }

        public static int CF_TimeShowHintClickObject
        {
            get => PlayerPrefs.GetInt("cf_time_hint_click_object", 4);
            set => PlayerPrefs.SetInt("cf_time_hint_click_object", value);
        }

        public static bool HintClickObjectSetting
        {
            get => PlayerPrefs.GetInt("hint_click_object_setting", CF_DefaultHintClickObject ? 1 : 0) == 1;
            set => PlayerPrefs.SetInt("hint_click_object_setting", value ? 1 : 0);
        }
        
        public static bool LevelAutoRotate
        {
            get => PlayerPrefs.GetInt("level_auto_rotate", 1) == 1;
            set => PlayerPrefs.SetInt("level_auto_rotate", value ? 1 : 0);
        }

        public static int CF_LevelActiveBattlePass
        {
            get => PlayerPrefs.GetInt("level_active_battle_pass", 7);
            set => PlayerPrefs.SetInt("level_active_battle_pass", value);
        }

        public static int CF_LevelUnlockFullMatchBox
        {
            get => PlayerPrefs.GetInt("cf_level_unlock_full_match_box", 0);
            set => PlayerPrefs.SetInt("cf_level_unlock_full_match_box", value);
        }

        public static int CF_LevelTutorialAboutDie
        {
            get => PlayerPrefs.GetInt("cf_level_tutorial_about_die", 4);
            set => PlayerPrefs.SetInt("cf_level_tutorial_about_die", value);
        }

        public static int CF_DayLoopActiveBattlePass
        {
            get => PlayerPrefs.GetInt("day_loop_active_battle_pass", 3);
            set => PlayerPrefs.SetInt("day_loop_active_battle_pass", value);
        }

        public static int ActiveBattlePass
        {
            get => PlayerPrefs.GetInt("active_battle_pass", 1);
            set => PlayerPrefs.SetInt("active_battle_pass", value);
        }

        public static int BattlePass_PointPerLevel
        {
            get => PlayerPrefs.GetInt("battle_pass_point_per_level", 100);
            set => PlayerPrefs.SetInt("battle_pass_point_per_level", value);
        }

        public static bool IsFirstShowChangeName
        {
            get => PlayerPrefs.GetInt("is_first_show_change_name", 1) == 1;
            set => PlayerPrefs.SetInt("is_first_show_change_name", value ? 1 : 0);
        }

        public static int ActiveEventRace
        {
            get => PlayerPrefs.GetInt("active_event_race", 1);
            set => PlayerPrefs.SetInt("active_event_race", value);
        }

        public static int CF_LevelActiveEventRace
        {
            get => PlayerPrefs.GetInt("level_active_event_race", 12);
            set => PlayerPrefs.SetInt("level_active_event_race", value);
        }

        public static int CF_TimeReopenRace
        {
            get => PlayerPrefs.GetInt("cf_time_reopen_race", 4);
            set => PlayerPrefs.SetInt("cf_time_reopen_race", value);
        }

        public static int CF_FirstLevelShowMain
        {
            get => PlayerPrefs.GetInt("cf_first_level_show_main", 11);
            set => PlayerPrefs.SetInt("cf_first_level_show_main", value);
        }

        public static bool CF_DisableMainIfLowerLevel
        {
            get => PlayerPrefs.GetInt("cf_disable_main_if_lower_level", 0) == 1;
            set => PlayerPrefs.SetInt("cf_disable_main_if_lower_level", value ? 1 : 0);
        }

        public static bool CF_PlayGameIfNotFirstOpen
        {
            get => PlayerPrefs.GetInt("cf_play_game_if_not_first_open", 1) == 1;
            set => PlayerPrefs.SetInt("cf_play_game_if_not_first_open", value ? 1 : 0);
        }

        public static int CF_TypeTutorial
        {
            get => PlayerPrefs.GetInt("cf_type_tutorial", 1);
            set => PlayerPrefs.SetInt("cf_type_tutorial", value);
        }

        public static bool CF_LockInputTutorial
        {
            get => PlayerPrefs.GetInt("cf_lock_input_tutorial", 1) == 1;
            set => PlayerPrefs.SetInt("cf_lock_input_tutorial", value ? 1 : 0);
        }

        public static int CF_MaxLevelFreeRevive
        {
            get => PlayerPrefs.GetInt("cf_max_level_revive", 3);
            set => PlayerPrefs.SetInt("cf_max_level_revive", value);
        }

        public static string CF_PriceHeart
        {
            get => PlayerPrefs.GetString("cf_price_heart", "{\"resType\":0,\"amount\":60}");
            set => PlayerPrefs.SetString("cf_price_heart", value);
        }

        public static bool CF_EnableHeart
        {
            get => PlayerPrefs.GetInt("cf_enable_heart", 1) == 1;
            set => PlayerPrefs.SetInt("cf_enable_heart", value ? 1 : 0);
        }

        public static int CF_LevelDisableMusic
        {
            get => PlayerPrefs.GetInt("cf_level_disable_music", 1);
            set => PlayerPrefs.SetInt("cf_level_disable_music", value);
        }
        
        public static int CF_NumRemoveBasketIfRevive
        {
            get => PlayerPrefs.GetInt("cf_num_remove_basket_if_revive", 2);
            set => PlayerPrefs.SetInt("cf_num_remove_basket_if_revive", value);
        }


        public static int CF_TimeSplashFirstSection
        {
            get => PlayerPrefs.GetInt("time_splash_first_section", 2);
            set => PlayerPrefs.SetInt("time_splash_first_section", value);
        }

        public static int CF_SectionShowMain
        {
            get => PlayerPrefs.GetInt("cf_section_show_main", 2);
            set => PlayerPrefs.SetInt("cf_section_show_main", value);
        }

        public static bool CF_EnableIntroLevel
        {
            get => PlayerPrefs.GetInt("cf_enable_intro_level", 1) == 1;
            set => PlayerPrefs.SetInt("cf_enable_intro_level", value ? 1 : 0);
        }

        public static bool CF_Rotate360
        {
#if UNITY_ANDROID
            get => PlayerPrefs.GetInt("cf_rotate_360", 1) == 1;
#else
            get => PlayerPrefs.GetInt("cf_rotate_360", 1) == 1;
#endif
            set => PlayerPrefs.SetInt("cf_rotate_360", value ? 1 : 0);
        }

        public static bool CF_EnableOtherGame
        {
            get => PlayerPrefs.GetInt("cf_enable_other_game", 0) == 1;
            set => PlayerPrefs.SetInt("cf_enable_other_game", value ? 1 : 0);
        }

        public static int LastIndexFailShowAds
        {
            get => PlayerPrefs.GetInt("last_index_fail_show_ads", 0);
            set => PlayerPrefs.SetInt("last_index_fail_show_ads", value);
        }

        public static string CF_LevelMonetization
        {
            get => PlayerPrefs.GetString("cf_level_monetization_v3", "");
            set => PlayerPrefs.SetString("cf_level_monetization_v3", value);
        }

        public static int CF_ShowAdsComplete
        {
            get => PlayerPrefs.GetInt("cf_show_ads_complete", 0);
            set => PlayerPrefs.SetInt("cf_show_ads_complete", value);
        }

        public static bool CF_EnableSaveGame
        {
            get => PlayerPrefs.GetInt("cf_enable_save_game", 1) == 1;
            set => PlayerPrefs.SetInt("cf_enable_save_game", value ? 1 : 0);
        }

        public static bool CF_PlayGameIfHasSave
        {
            get => PlayerPrefs.GetInt("cf_play_game_if_has_save", 1) == 1;
            set => PlayerPrefs.SetInt("cf_play_game_if_has_save", value ? 1 : 0);
        }

        public static bool CF_EnableFailUI
        {
            get => PlayerPrefs.GetInt("cf_enable_fail_ui", 1) == 1;
            set => PlayerPrefs.SetInt("cf_enable_fail_ui", value ? 1 : 0);
        }

        public static string CacheLevelIndex
        {
            get => PlayerPrefs.GetString("s_cache_level_index", "");
            set => PlayerPrefs.SetString("s_cache_level_index", value);
        }

        public static int CFLevelShowPlayPopup
        {
            get => PlayerPrefs.GetInt("cf_level_show_play_popup", 9999999);
            set => PlayerPrefs.SetInt("cf_level_show_play_popup", value);
        }

        public static int CF_LevelUnlockSweetParty
        {
            get => PlayerPrefs.GetInt("cf_level_unlock_sweetparty", 10);
            set => PlayerPrefs.SetInt("cf_level_unlock_sweetparty", value);
        }

        public static int CF_LevelUnlockPackWeekend
        {
            get => PlayerPrefs.GetInt("cf_level_unlock_pack_weekend", 8);
            set => PlayerPrefs.SetInt("cf_level_unlock_pack_weekend", value);
        }

        public static bool CF_ShowPackOnClaimDailyReward
        {
            get => PlayerPrefs.GetInt("cf_show_pack_on_claim_daily_reward", 1) >= 1;
            set => PlayerPrefs.SetInt("cf_show_pack_on_claim_daily_reward", value ? 1 : 0);
        }

        public static bool CF_ActivePackToReview
        {
            get => PlayerPrefs.GetInt("cf_active_pack_to_review", 0) >= 1;
            set => PlayerPrefs.SetInt("cf_active_pack_to_review", value ? 1 : 0);
        }

        /// <summary>
        /// 1 Screw In Level, Default Level
        /// </summary>
        public static int CF_GetPointBatllePass
        {
            get => PlayerPrefs.GetInt("cf_get_point_battle_pass", 0);
            set => PlayerPrefs.SetInt("cf_get_point_battle_pass", value);
        }

        public static int CF_LevelShowRate
        {
            get => PlayerPrefs.GetInt("cf_level_show_rate", 10);
            set => PlayerPrefs.SetInt("cf_level_show_rate", value);
        }

        public static string CacheMail
        {
            get => PlayerPrefs.GetString("cache_mail", null);
            set => PlayerPrefs.SetString("cache_mail", value);
        }

        public static string CFGoldCompleteLevel
        {
            get => PlayerPrefs.GetString("cf_gold_complete_level", "");
            set => PlayerPrefs.SetString("cf_gold_complete_level", value);
        }

        public static string CFAutoQualityLevel
        {
            get => PlayerPrefs.GetString("cf_auto_quality_level", "");
            set => PlayerPrefs.SetString("cf_auto_quality_level", value);
        }

        public static bool CF_SmallBundleAcive
        {
            get => PlayerPrefs.GetInt("cf_small_bundle_active", 0) > 0;
            set => PlayerPrefs.SetInt("cf_small_bundle_active", value ? 1 : 0);
        }

        public static bool CF_ShowBreakAds
        {
            get => PlayerPrefs.GetInt("cf_show_break_ads", 1) > 0;
            set => PlayerPrefs.SetInt("cf_show_break_ads", value ? 1 : 0);
        }

        public static int CFLevelShowBanner
        {
            get => PlayerPrefs.GetInt("cf_level_show_banner", 11);
            set => PlayerPrefs.SetInt("cf_level_show_banner", value);
        }

        public static bool CFShowFullFirstTimeFail
        {
            get => PlayerPrefs.GetInt("cf_show_full_first_time_fail", 1) == 1;
            set => PlayerPrefs.SetInt("cf_show_full_first_time_fail", value ? 1 : 0);
        }

        public static string CF_AppIconName
        {
            get => PlayerPrefs.GetString("cf_app_icon_name", "default");
            set => PlayerPrefs.SetString("cf_app_icon_name", value);
        }
    }
}