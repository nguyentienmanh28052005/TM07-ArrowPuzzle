using System;
using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;

public class CheatUI : MonoBehaviour
{
    [Header("Hack")]
    [SerializeField] private InputField levelInputField;
    [SerializeField] private InputField recordInputField;
    [SerializeField] private InputField levelFromInputField;
    [SerializeField] private InputField levelToInputField;

    [SerializeField] private Button nextLevelBtn;
    [SerializeField] private Button backLevelBtn;
    [SerializeField] private Button completeLevelBtn;

    [SerializeField] private Button addGoldBtn;
    [SerializeField] private Button subtractGoldBtn;
    [SerializeField] private Button addStarBtn;
    [SerializeField] private Button subtractStarBtn;
    [SerializeField] private InputField curveIndexInputField;
    [SerializeField] private InputField customFolderInputField;
    [SerializeField] private Toggle customizeCurveToggle;

    [SerializeField] private Slider sfxSlider;

    [SerializeField] private Text statsText;

    [Header("Segment Test")]
    [SerializeField] private Dropdown campaignDropdown;
    [SerializeField] private Dropdown countryDropdown;
    [SerializeField] private Button fetchSegmentBtn;

    private void Start()
    {
        if (fetchSegmentBtn != null && campaignDropdown != null && countryDropdown != null)
        {
            fetchSegmentBtn.onClick.AddListener(() =>
            {
#if TEST_CAMPAIGN_COUNTRY
                string campaign = campaignDropdown.options.Count > 0 ? campaignDropdown.options[campaignDropdown.value].text : "";
                string country = countryDropdown.options.Count > 0 ? countryDropdown.options[countryDropdown.value].text : "";
                if (ExperimentSegmentHandler.Instance != null)
                {
                    ExperimentSegmentHandler.Instance.SetTestOverridesAndFetch(campaign, country);
                }
#else
                Debug.LogWarning("TEST_CAMPAIGN_COUNTRY define is not active!");
#endif
            });
        }

        nextLevelBtn.onClick.AddListener(() =>
        {
            OnClickNextLevel();
        });
        backLevelBtn.onClick.AddListener(OnClickBackLevel);
        completeLevelBtn.onClick.AddListener(OnClickCompleteLevel);
        addGoldBtn.onClick.AddListener(OnClickAddGold);
        subtractGoldBtn.onClick.AddListener(OnClickSubtractGold);
        addStarBtn.onClick.AddListener(OnClickAddStar);
        subtractStarBtn.onClick.AddListener(OnClickSubtractStar);

        sfxSlider.value = AudioManager.Instance.Ratio_Sound;
        sfxSlider.onValueChanged.AddListener((x) =>
        {
            AudioManager.Instance.Ratio_Sound = x;
        });

        if (customizeCurveToggle != null)
        {
            customizeCurveToggle.isOn = PlayerPrefs.GetInt("CustomizeCurve", 0) == 1;
            customizeCurveToggle.onValueChanged.AddListener((isOn) =>
            {
                PlayerPrefs.SetInt("CustomizeCurve", isOn ? 1 : 0);
                PlayerPrefs.Save();
            });
        }

        if (curveIndexInputField != null)
        {
            curveIndexInputField.text = PlayerPrefs.GetInt("CurveIndex", 4).ToString();
            curveIndexInputField.onValueChanged.AddListener((val) =>
            {
                if (int.TryParse(val, out int index))
                {
                    PlayerPrefs.SetInt("CurveIndex", index);
                    PlayerPrefs.Save();
                }
            });
        }

        if (customFolderInputField != null)
        {
            customFolderInputField.text = PlayerPrefs.GetString("CustomLevelFolder", "");
            customFolderInputField.onValueChanged.AddListener((val) =>
            {
                PlayerPrefs.SetString("CustomLevelFolder", val);
                PlayerPrefs.Save();
            });
        }
    }
    
        private void OnClickCompleteLevel()
    {
        LevelManager.Instance.OnComplete();
    }

    private void OnClickAddGold()
    {
        GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "hack", new[] { new DataResource(RES_type.GOLD, 1000) }, DataManager.Level);
    }

    private void OnClickSubtractGold()
    {
        GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "hack", new[] { new DataResource(RES_type.GOLD, -1000) }, DataManager.Level);
    }
    private void OnClickAddStar()
    {
        GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "hack", new[] { new DataResource(RES_type.Star, 1000) }, DataManager.Level);
    }

    private void OnClickSubtractStar()
    {
        GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "hack", new[] { new DataResource(RES_type.Star, -1000) }, DataManager.Level);
    }

    public void OnClickNextLevel(int add = 0)
    {
        if (int.TryParse(levelInputField.text, out int level))
        {
            GameRes.SetLevel(Level_type.Normal, level + add);
            if (!string.IsNullOrEmpty(LevelConfig.CacheLevelIndex) && (DataManager.Level - 1) % LevelConfig.CountLevelRandom == 0)
            {
                LevelConfig.CacheLevelIndex = string.Empty;
            }
            LevelRemoteManager.Instance.LoadLevelCache(GameRes.GetLevel());

            DataManager.Instance.AdsOrDeathInLevel = 0;
            DataManager.Instance.AdsPerLevelPlay = 0;
            DataManager.Instance.ConsecutiveLose = 0;
            DataManager.Instance.ConsecutivePlay = 0;
            PlayerPrefsUtil.LastIndexFailShowAds = 0;
            PlayerPrefs.SetInt("reduce_death", 0);
            GameManager.Instance.NextLevel();
        }
        else
        {
            GameRes.SetLevel(Level_type.Normal, GameRes.GetLevel() + 1 + add);
            if (!string.IsNullOrEmpty(LevelConfig.CacheLevelIndex) && (DataManager.Level - 1) % LevelConfig.CountLevelRandom == 0)
            {
                LevelConfig.CacheLevelIndex = string.Empty;
            }
            LevelRemoteManager.Instance.LoadLevelCache(GameRes.GetLevel());

            DataManager.Instance.AdsOrDeathInLevel = 0;
            DataManager.Instance.AdsPerLevelPlay = 0;
            DataManager.Instance.ConsecutiveLose = 0;
            DataManager.Instance.ConsecutivePlay = 0;
            PlayerPrefsUtil.LastIndexFailShowAds = 0;
            PlayerPrefs.SetInt("reduce_death", 0);
            GameManager.Instance.NextLevel();
        }
        if (add != 0)
        {
            levelInputField.text = GameRes.GetLevel().ToString();
        }
    }

    private void OnClickBackLevel()
    {
        DataManager.Instance.AdsOrDeathInLevel = 0;
        DataManager.Instance.AdsPerLevelPlay = 0;
        DataManager.Instance.ConsecutiveLose = 0;
        DataManager.Instance.ConsecutivePlay = 0;
        PlayerPrefsUtil.LastIndexFailShowAds = 0;
        PlayerPrefs.SetInt("reduce_death", 0);

        GameRes.SetLevel(Level_type.Normal, GameRes.GetLevel() - 1);
        LevelRemoteManager.Instance.LoadLevelCache(GameRes.GetLevel());
        GameManager.Instance.NextLevel();
    }
}
